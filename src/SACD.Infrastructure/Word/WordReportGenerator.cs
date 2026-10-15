using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocText = DocumentFormat.OpenXml.Wordprocessing.Text;
using SACD.Domain.Entities;
using SACD.Domain.ValueObjects;

namespace SACD.Infrastructure.Word
{
    public interface IWordReportGenerator
    {
        Task<(string FilePath, string Sha256Hash)> GenerateReportDocxAsync(
            string outputPath,
            ReportRevisionEntity reportRevision,
            IReadOnlyList<ReportSectionEntity> sections,
            IReadOnlyList<ClaimEntity> claims,
            IReadOnlyList<FactVersionEntity> facts,
            string exportMode, // Draft or Submission
            CancellationToken ct = default);
    }

    public class WordReportGenerator : IWordReportGenerator
    {
        public async Task<(string FilePath, string Sha256Hash)> GenerateReportDocxAsync(
            string outputPath,
            ReportRevisionEntity reportRevision,
            IReadOnlyList<ReportSectionEntity> sections,
            IReadOnlyList<ClaimEntity> claims,
            IReadOnlyList<FactVersionEntity> facts,
            string exportMode,
            CancellationToken ct = default)
        {
            string? dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // Generate Word document using OpenXML
            using (var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Title
                var titlePara = body.AppendChild(new Paragraph());
                var titleRun = titlePara.AppendChild(new Run());
                titleRun.AppendChild(new DocText("BÁO CÁO TỔNG HỢP VĂN BẢN VÀ SỐ LIỆU NGHIỆP VỤ"));
                titleRun.RunProperties = new RunProperties(
                    new Bold(),
                    new FontSize { Val = "32" },
                    new Color { Val = "003366" }
                );

                // Subtitle / Mode Banner
                var modePara = body.AppendChild(new Paragraph());
                var modeRun = modePara.AppendChild(new Run());
                if (exportMode == "Draft")
                {
                    modeRun.AppendChild(new DocText("[ BẢN NHÁP / DỰ THẢO - CHƯA PHÊ DUYỆT ]"));
                    modeRun.RunProperties = new RunProperties(
                        new Bold(),
                        new Italic(),
                        new FontSize { Val = "22" },
                        new Color { Val = "CC0000" }
                    );
                }
                else
                {
                    modeRun.AppendChild(new DocText("[ BẢN TRÌNH LÃNH ĐẠO CHÍNH THỨC ]"));
                    modeRun.RunProperties = new RunProperties(
                        new Bold(),
                        new FontSize { Val = "22" },
                        new Color { Val = "006600" }
                    );
                }

                // Metadata block
                var metaPara = body.AppendChild(new Paragraph());
                var metaRun = metaPara.AppendChild(new Run());
                metaRun.AppendChild(new DocText($"Phiên bản báo cáo: v{reportRevision.VersionNo} | Ngày lập: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC | Trạng thái: {reportRevision.State}"));
                metaRun.RunProperties = new RunProperties(new Italic(), new FontSize { Val = "18" });

                // Empty line
                body.AppendChild(new Paragraph());

                // Sections
                foreach (var section in sections)
                {
                    var secHeader = body.AppendChild(new Paragraph());
                    var secHeaderRun = secHeader.AppendChild(new Run());
                    secHeaderRun.AppendChild(new DocText($"{section.Ordinal}. {section.SectionKey.ToUpperInvariant()}"));
                    secHeaderRun.RunProperties = new RunProperties(new Bold(), new FontSize { Val = "24" });

                    var secContent = body.AppendChild(new Paragraph());
                    var secContentRun = secContent.AppendChild(new Run());
                    secContentRun.AppendChild(new DocText(section.Text));
                    secContentRun.RunProperties = new RunProperties(new FontSize { Val = "20" });

                    body.AppendChild(new Paragraph());
                }

                // Table of Claims & Evidence
                if (claims.Count > 0)
                {
                    var claimHeader = body.AppendChild(new Paragraph());
                    var claimHeaderRun = claimHeader.AppendChild(new Run());
                    claimHeaderRun.AppendChild(new DocText("DANH MỤC CĂN CỨ VÀ NHẬN ĐỊNH (CLAIMS & EVIDENCE)"));
                    claimHeaderRun.RunProperties = new RunProperties(new Bold(), new FontSize { Val = "22" });

                    var table = body.AppendChild(new Table());
                    var tblProps = new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = BorderValues.Single, Size = 4 },
                            new BottomBorder { Val = BorderValues.Single, Size = 4 },
                            new LeftBorder { Val = BorderValues.Single, Size = 4 },
                            new RightBorder { Val = BorderValues.Single, Size = 4 },
                            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 2 },
                            new InsideVerticalBorder { Val = BorderValues.Single, Size = 2 }
                        )
                    );
                    table.AppendChild(tblProps);

                    // Header row
                    var headerRow = table.AppendChild(new TableRow());
                    AddCell(headerRow, "STT", true);
                    AddCell(headerRow, "Loại nhận định", true);
                    AddCell(headerRow, "Nội dung nhận định", true);
                    AddCell(headerRow, "Kiểm chứng", true);

                    int claimIdx = 1;
                    foreach (var claim in claims)
                    {
                        var row = table.AppendChild(new TableRow());
                        AddCell(row, claimIdx++.ToString(), false);
                        AddCell(row, claim.Kind == "Factual" ? "Số liệu/Sự kiện" : "Bình luận nghiệp vụ", false);
                        AddCell(row, claim.Text, false);
                        AddCell(row, claim.Checked == 1 ? "Đã đối chiếu nguồn" : "Chưa kiểm tra", false);
                    }

                    body.AppendChild(new Paragraph());
                }

                // Facts Table if any
                if (facts.Count > 0)
                {
                    var factHeader = body.AppendChild(new Paragraph());
                    var factHeaderRun = factHeader.AppendChild(new Run());
                    factHeaderRun.AppendChild(new DocText("BẢNG TỔNG HỢP SỐ LIỆU ĐÃ XÁC NHẬN"));
                    factHeaderRun.RunProperties = new RunProperties(new Bold(), new FontSize { Val = "22" });

                    var table = body.AppendChild(new Table());
                    table.AppendChild(new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = BorderValues.Single, Size = 4 },
                            new BottomBorder { Val = BorderValues.Single, Size = 4 },
                            new LeftBorder { Val = BorderValues.Single, Size = 4 },
                            new RightBorder { Val = BorderValues.Single, Size = 4 },
                            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 2 },
                            new InsideVerticalBorder { Val = BorderValues.Single, Size = 2 }
                        )
                    ));

                    var headerRow = table.AppendChild(new TableRow());
                    AddCell(headerRow, "Chỉ tiêu", true);
                    AddCell(headerRow, "Kỳ báo cáo", true);
                    AddCell(headerRow, "Giá trị", true);
                    AddCell(headerRow, "Trạng thái", true);

                    foreach (var fact in facts)
                    {
                        var row = table.AppendChild(new TableRow());
                        AddCell(row, fact.LogicalId, false);
                        AddCell(row, $"{fact.PeriodStart} - {fact.PeriodEnd}", false);
                        AddCell(row, fact.ValueDecimalText ?? "N/A", false);
                        AddCell(row, fact.Status, false);
                    }

                    body.AppendChild(new Paragraph());
                }

                // Signatures footer block
                var sigPara = body.AppendChild(new Paragraph());
                sigPara.AppendChild(new Run(new DocText("Người lập báo cáo: _____________________    Lãnh đạo phê duyệt: _____________________"))
                {
                    RunProperties = new RunProperties(new Italic(), new FontSize { Val = "20" })
                });

                mainPart.Document.Save();
            }

            // Compute hash
            string hash;
            using (var sha = SHA256.Create())
            await using (var fs = File.OpenRead(outputPath))
            {
                byte[] hashBytes = await sha.ComputeHashAsync(fs, ct);
                hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }

            return (outputPath, hash);
        }

        private static void AddCell(TableRow row, string text, bool isHeader)
        {
            var cell = row.AppendChild(new TableCell());
            var p = cell.AppendChild(new Paragraph());
            var r = p.AppendChild(new Run(new DocText(text)));
            if (isHeader)
            {
                r.RunProperties = new RunProperties(new Bold());
            }
        }
    }
}
