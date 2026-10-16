using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SACD.Domain.Entities;

namespace SACD.Infrastructure.Text
{
    public static class VietnameseTextNormalizer
    {
        private static readonly string[] VietnameseSigns = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };

        public static string NormalizeToNfc(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Normalize(NormalizationForm.FormC);
        }

        public static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            string normalized = NormalizeToNfc(input);
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                {
                    normalized = normalized.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
                }
            }
            return normalized;
        }

        public static string PrepareSearchText(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;
            string nfc = NormalizeToNfc(rawText);
            string unaccented = RemoveDiacritics(nfc);
            // Combine both so that searching with or without accents finds the text
            return $"{nfc} {unaccented}";
        }
    }

    public interface IDocumentTextExtractor
    {
        Task<IReadOnlyList<TextBlockEntity>> ExtractBlocksAsync(string filePath, string revisionId, CancellationToken ct = default, string? originalExtension = null);
    }

    public class DocumentTextExtractor : IDocumentTextExtractor
    {
        public async Task<IReadOnlyList<TextBlockEntity>> ExtractBlocksAsync(string filePath, string revisionId, CancellationToken ct = default, string? originalExtension = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File to extract does not exist.", filePath);

            string ext = (originalExtension ?? Path.GetExtension(filePath)).ToLowerInvariant();
            if (ext == ".docx")
            {
                return ExtractFromDocx(filePath, revisionId);
            }
            else if (ext == ".txt")
            {
                return await ExtractFromTxtAsync(filePath, revisionId, ct);
            }
            else
            {
                // Default fallback / PDF simulated text extractor
                return await ExtractFromGenericOrPdfAsync(filePath, revisionId, ct);
            }
        }

        private static IReadOnlyList<TextBlockEntity> ExtractFromDocx(string filePath, string revisionId)
        {
            var blocks = new List<TextBlockEntity>();
            int ordinal = 1;

            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                foreach (var para in body.Elements<Paragraph>())
                {
                    string text = para.InnerText;
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    string nfc = VietnameseTextNormalizer.NormalizeToNfc(text);
                    blocks.Add(new TextBlockEntity
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        RevisionId = revisionId,
                        Ordinal = ordinal++,
                        LocatorJson = $"{{\"type\":\"paragraph\",\"index\":{ordinal - 1}}}",
                        ExtractedText = text,
                        RawTextNfc = nfc,
                        SearchText = VietnameseTextNormalizer.PrepareSearchText(nfc),
                        Reviewed = 0
                    });
                }
            }

            if (blocks.Count == 0)
            {
                blocks.Add(new TextBlockEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RevisionId = revisionId,
                    Ordinal = 1,
                    LocatorJson = "{\"type\":\"document\",\"index\":1}",
                    ExtractedText = "(Văn bản rỗng hoặc không có đoạn văn bản khả dụng)",
                    RawTextNfc = "(Văn bản rỗng hoặc không có đoạn văn bản khả dụng)",
                    SearchText = VietnameseTextNormalizer.PrepareSearchText("Văn bản rỗng"),
                    Reviewed = 0
                });
            }

            return blocks;
        }

        private static async Task<IReadOnlyList<TextBlockEntity>> ExtractFromTxtAsync(string filePath, string revisionId, CancellationToken ct)
        {
            var blocks = new List<TextBlockEntity>();
            var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8, ct);
            int ordinal = 1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string nfc = VietnameseTextNormalizer.NormalizeToNfc(line);
                blocks.Add(new TextBlockEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RevisionId = revisionId,
                    Ordinal = ordinal++,
                    LocatorJson = $"{{\"type\":\"line\",\"lineNumber\":{i + 1}}}",
                    ExtractedText = line,
                    RawTextNfc = nfc,
                    SearchText = VietnameseTextNormalizer.PrepareSearchText(nfc),
                    Reviewed = 0
                });
            }

            if (blocks.Count == 0)
            {
                blocks.Add(new TextBlockEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RevisionId = revisionId,
                    Ordinal = 1,
                    LocatorJson = "{\"type\":\"file\",\"index\":1}",
                    ExtractedText = "(Tệp văn bản rỗng)",
                    RawTextNfc = "(Tệp văn bản rỗng)",
                    SearchText = VietnameseTextNormalizer.PrepareSearchText("Tệp văn bản rỗng"),
                    Reviewed = 0
                });
            }

            return blocks;
        }

        private static async Task<IReadOnlyList<TextBlockEntity>> ExtractFromGenericOrPdfAsync(string filePath, string revisionId, CancellationToken ct)
        {
            var blocks = new List<TextBlockEntity>();
            byte[] bytes = await File.ReadAllBytesAsync(filePath, ct);

            // Attempt to parse text streams if ASCII/UTF8 strings are embedded
            string rawContent = Encoding.UTF8.GetString(bytes);
            var stringBuilder = new StringBuilder();
            int page = 1;
            int ordinal = 1;

            using (var reader = new StringReader(rawContent))
            {
                string? line;
                while ((line = await reader.ReadLineAsync(ct)) != null)
                {
                    // Clean printable text
                    var clean = new StringBuilder();
                    foreach (char c in line)
                    {
                        if (!char.IsControl(c) || c == '\t') clean.Append(c);
                    }
                    string text = clean.ToString().Trim();
                    if (text.Length > 10)
                    {
                        string nfc = VietnameseTextNormalizer.NormalizeToNfc(text);
                        blocks.Add(new TextBlockEntity
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            RevisionId = revisionId,
                            Ordinal = ordinal++,
                            LocatorJson = $"{{\"type\":\"page\",\"pageNumber\":{page}}}",
                            ExtractedText = text,
                            RawTextNfc = nfc,
                            SearchText = VietnameseTextNormalizer.PrepareSearchText(nfc),
                            Reviewed = 0
                        });
                        if (ordinal % 20 == 0) page++;
                    }
                }
            }

            if (blocks.Count == 0)
            {
                string fallback = $"[Trích xuất từ tệp: {Path.GetFileName(filePath)}] Dung lượng: {bytes.Length} bytes.";
                string nfc = VietnameseTextNormalizer.NormalizeToNfc(fallback);
                blocks.Add(new TextBlockEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RevisionId = revisionId,
                    Ordinal = 1,
                    LocatorJson = "{\"type\":\"page\",\"pageNumber\":1}",
                    ExtractedText = fallback,
                    RawTextNfc = nfc,
                    SearchText = VietnameseTextNormalizer.PrepareSearchText(nfc),
                    Reviewed = 0
                });
            }

            return blocks;
        }
    }
}
