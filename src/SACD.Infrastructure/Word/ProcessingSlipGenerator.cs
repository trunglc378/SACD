using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QRCoder;
using SACD.Application.Contracts;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace SACD.Infrastructure.Word;

public static class ProcessingSlipGenerator
{
    public static void Generate(string path, CorrespondenceCase row)
    {
        // QR contains only the stable business code, never document text or a cloud URL.
        byte[] qr = PngByteQRCodeHelper.GetQRCode(row.Code, QRCodeGenerator.ECCLevel.Q, 8);
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new Document(new Body()); var body = main.Document.Body!;
        void Line(string text,bool bold=false) => body.Append(new Paragraph(new Run(
            new RunProperties(new RunFonts{Ascii="Times New Roman",HighAnsi="Times New Roman"},new Bold{Val=bold},new FontSize{Val="26"}),new DocumentFormat.OpenXml.Wordprocessing.Text(text){Space=SpaceProcessingModeValues.Preserve})));
        Line("PHIẾU ĐỀ XUẤT XỬ LÝ VĂN BẢN",true);
        Line("Mẫu tham khảo — cần đối chiếu mẫu phiếu được đơn vị phê duyệt.");
        Line("Mã tra cứu: "+row.Code);Line("Số văn bản: "+row.Number);Line("Loại: "+row.DocumentType);
        Line("Ngày ban hành: "+row.IssuedDate);Line("Đơn vị ban hành: "+row.Issuer);
        Line("Đơn vị nhận: "+row.Recipient);Line("Người ký: "+row.Signer);
        Line("Độ mật theo xác nhận: "+row.Classification);Line("Trích yếu: "+row.Summary);
        Line("Công việc liên quan: "+row.RelatedWork);Line("Hạn xử lý: "+(row.DueDate.Length==0?"Chưa xác định":row.DueDate));
        Line("Cán bộ đề xuất giao: "+row.ProposedReceiver);
        Line("Ý kiến lãnh đạo: ..................................................................");
        Line("Người/đội được giao: ..................................................................");
        Line("Lãnh đạo ký duyệt: .................................. Ngày: ..................");
        Line("Người nhận ký: ........................................ Ngày: ..................");
        var image=main.AddImagePart(ImagePartType.Png);using(var stream=new MemoryStream(qr)) image.FeedData(stream);
        string rel=main.GetIdOfPart(image);
        body.Append(new Paragraph(new Run(new Drawing(new DW.Inline(
            new DW.Extent{Cx=1100000,Cy=1100000},new DW.DocProperties{Id=1,Name="Mã hồ sơ QR"},
            new A.Graphic(new A.GraphicData(new PIC.Picture(
                new PIC.NonVisualPictureProperties(new PIC.NonVisualDrawingProperties{Id=0,Name="QR"},new PIC.NonVisualPictureDrawingProperties()),
                new PIC.BlipFill(new A.Blip{Embed=rel},new A.Stretch(new A.FillRectangle())),
                new PIC.ShapeProperties(new A.Transform2D(new A.Offset{X=0,Y=0},new A.Extents{Cx=1100000,Cy=1100000}),
                    new A.PresetGeometry(new A.AdjustValueList()){Preset=A.ShapeTypeValues.Rectangle})))
                {Uri="http://schemas.openxmlformats.org/drawingml/2006/picture"})) ))));
        main.Document.Save();
    }
}
