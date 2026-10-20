using System.Text.Json;
using SACD.Domain.Entities;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SACD.Infrastructure.Text;

internal static class PdfTextReader
{
    public static IReadOnlyList<TextBlockEntity> Extract(string file,string revision,CancellationToken ct)
    {
        using var pdf=PdfDocument.Open(file);
        if(pdf.NumberOfPages>100)throw new InvalidDataException("PDF vượt giới hạn 100 trang.");
        var blocks=new List<TextBlockEntity>();
        foreach(var page in pdf.GetPages())
        {
            ct.ThrowIfCancellationRequested();
            string text=ContentOrderTextExtractor.GetText(page).Trim();
            bool needsOcr=string.IsNullOrWhiteSpace(text);
            if(needsOcr)text=$"[Trang {page.Number}: chưa có lớp chữ; cần OCR/đối chiếu, chưa được xác nhận toàn văn.]";
            string nfc=VietnameseTextNormalizer.NormalizeToNfc(text);
            blocks.Add(new TextBlockEntity
            {
                Id=Guid.NewGuid().ToString("N"),RevisionId=revision,Ordinal=page.Number,
                LocatorJson=needsOcr?JsonSerializer.Serialize(new{type="page",pageNumber=page.Number,needsOcr=true}):JsonSerializer.Serialize(new{type="page",pageNumber=page.Number}),
                ExtractedText=text,RawTextNfc=nfc,SearchText=needsOcr?"":VietnameseTextNormalizer.PrepareSearchText(nfc),Reviewed=0
            });
        }
        if(blocks.Count==0)throw new InvalidDataException("PDF không có trang.");
        return blocks;
    }
}
