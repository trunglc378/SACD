namespace SACD.Application.Contracts;

public record CorrespondenceCase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentId { get; set; } = "";
    public string DossierId { get; set; } = "";
    public string Code { get; set; } = "";
    public string OriginalName { get; set; } = "";
    public string ObjectHash { get; set; } = "";
    public string Number { get; set; } = "";
    public string DocumentType { get; set; } = "";
    public string IssuedDate { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Recipient { get; set; } = "";
    public string Signer { get; set; } = "";
    public string Summary { get; set; } = "";
    public string RelatedWork { get; set; } = "";
    public string Classification { get; set; } = "Chưa xác định";
    public string DeclassificationBasis { get; set; } = "";
    public string DueDate { get; set; } = "";
    public string ProposedReceiver { get; set; } = "";
    public string ApprovedReceiver { get; set; } = "";
    public string Leader { get; set; } = "";
    public string DecisionEvidence { get; set; } = "";
    public string ReceiptEvidence { get; set; } = "";
    public string ExternalRef { get; set; } = "";
    public string State { get; set; } = "Received";
    public int RowVersion { get; set; } = 1;
    public string CompletedUtc { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public string DeadlineLabel
    {
        get
        {
            if(State=="Completed")
            {
                if(DueDate.Length==0 || !DateTime.TryParse(CompletedUtc,null,System.Globalization.DateTimeStyles.RoundtripKind,out var completed))
                    return "Đã hoàn thành (chưa xác định hạn)";
                return SACD.Domain.ValueObjects.DeadlineCalculator.Evaluate("Date",DueDate,null,null,completed.ToUniversalTime(),TimeSpan.Zero).IsOverdue
                    ? "Hoàn thành quá hạn" : "Hoàn thành đúng hạn";
            }
            return SACD.Domain.ValueObjects.DeadlineCalculator.Evaluate(DueDate.Length==0?"Unknown":"Date",DueDate,null,null,DateTime.UtcNow,TimeSpan.FromDays(3)).DisplayText;
        }
    }
    [System.Text.Json.Serialization.JsonIgnore]
    public string StateLabel => State switch
    {
        "Received" => "M1 · Chờ kiểm tra", "Reviewed" => "M1 · Đã xác nhận", "Printed" => "M1 · Đã in, sẵn sàng USB",
        "Imported" => "M2 · Chờ nhập QLVB", "Registered" => "M2 · Đã đối soát QLVB", "Submitted" => "Chờ lãnh đạo duyệt",
        "Approved" => "Đã duyệt, chờ giao", "Delivered" => "Đã giao, chờ ký nhận", "Acknowledged" => "Đã ký nhận, đang xử lý",
        "Completed" => "Đã hoàn thành", _ => State
    };
}

public record CaseHistory(string Action, string Actor, string AtUtc, string Note);

