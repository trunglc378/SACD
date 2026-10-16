using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Services;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Word;

namespace SACD.Desktop.ViewModels;

public sealed class CorrespondenceViewModel : ViewModelBase, IDisposable
{
    private readonly CorrespondenceService service;
    private readonly SqliteDatabaseContext db;
    private readonly DispatcherTimer timer = new() { Interval=TimeSpan.FromMinutes(1) };
    private CorrespondenceCase? selected;
    private string status="Chọn hoặc tiếp nhận một hồ sơ để bắt đầu.", filter="", note="", text="", alert="";
    private bool busy, scanned;
    public ObservableCollection<CorrespondenceCase> Cases { get; } = new();
    public ObservableCollection<CaseHistory> History { get; } = new();
    public CorrespondenceCase? SelectedCase { get=>selected; set { if(SetProperty(ref selected,value)) LoadSelection(); } }
    public string Filter {get=>filter;set=>SetProperty(ref filter,value);}
    public string Note {get=>note;set=>SetProperty(ref note,value);}
    public string Status {get=>status;set=>SetProperty(ref status,value);}
    public string TextPreview {get=>text;set=>SetProperty(ref text,value);}
    public string DeadlineAlert {get=>alert;set=>SetProperty(ref alert,value);}
    public bool PackageScanned {get=>scanned;set=>SetProperty(ref scanned,value);}
    public string Profile {get;}
    public string RoleGuide => Profile=="Intake"
        ? "MÁY 1: Kiểm tra email nội bộ → tải tệp → tiếp nhận → kiểm tra và lưu thông tin → xác nhận → in bản giấy → xuất USB."
        : "MÁY 2: Nhập gói USB → điền QLVB → đối soát đã lưu → tạo/in phiếu → trình ký → cập nhật duyệt → giao → ký nhận → theo dõi hạn.";
    public string NextStep => SelectedCase?.State switch
    {
        "Received"=>"Bước 2: mở tệp gốc, đối chiếu nội dung, nhập thông tin bên dưới rồi Lưu thông tin và Xác nhận.",
        "Reviewed"=>"Bước 3: mở tệp gốc để in. Sau khi kiểm tra bản giấy, nhập căn cứ và bấm Xác nhận đã in.",
        "Printed"=>"Bước 4–5: xuất gói ZIP ra USB an toàn; mang USB và bản giấy sang Máy 2.",
        "Imported"=>"Bước 6: nhập dữ liệu vào QLVB (thủ công hoặc RPA đã cấu hình). Lưu trong QLVB, nhập mã/số đến rồi Đối soát đã lưu.",
        "Registered"=>"Bước 6.2–7: tạo phiếu Word có mã QR, kiểm tra/in phiếu, ghép với bản giấy rồi xác nhận đã trình lãnh đạo.",
        "Submitted"=>"Bước 8–9: chờ phiếu đã ký. Nhập người duyệt, người/đội được giao, hạn và căn cứ quyết định rồi Cập nhật đã duyệt.",
        "Approved"=>"Bước 10: chuyển bản giấy cho đúng người/đội được giao; nhập căn cứ và xác nhận Đã giao.",
        "Delivered"=>"Bước 11–12: sau khi người nhận ký, nhập căn cứ chữ ký rồi Xác nhận ký nhận.",
        "Acknowledged"=>"Theo dõi hạn. Chỉ xác nhận hoàn thành sau khi có kết quả xử lý thực tế và căn cứ.",
        "Completed"=>"Hồ sơ đã hoàn thành. Có thể tra cứu mã hồ sơ và lịch sử giao nhận.", _=>"Chọn hồ sơ trong danh sách."
    };
    public ICommand ActionCommand {get;}
    public CorrespondenceViewModel(SqliteDatabaseContext db,ActorContext actor,string profile)
    {
        this.db=db; service=new(db,actor);Profile=profile;
        ActionCommand=new RelayCommand(async p=>await Execute(p?.ToString()??""),_=>!busy);
        timer.Tick+=(_,_)=>RefreshDeadlines();timer.Start();Reload();
    }
    private void Reload(string? id=null)
    {
        id??=SelectedCase?.Id;Cases.Clear();
        foreach(var row in service.List().Where(x=>string.IsNullOrWhiteSpace(Filter)||x.Code.Contains(Filter.Trim(),StringComparison.OrdinalIgnoreCase)||x.Number.Contains(Filter.Trim(),StringComparison.OrdinalIgnoreCase)))Cases.Add(row);
        SelectedCase=Cases.FirstOrDefault(x=>x.Id==id)??Cases.FirstOrDefault();RefreshDeadlines();
    }
    private void LoadSelection()
    {
        History.Clear();TextPreview="";
        if(SelectedCase is {} row)
        {
            foreach(var h in service.History(row.Id))History.Add(h);
            TextPreview=string.Join(Environment.NewLine,service.ReadText(row).Blocks.Select(b=>b.RawTextNfc));
        }
        OnPropertyChanged(nameof(NextStep));
    }
    private void RefreshDeadlines()
    {
        System.Windows.Data.CollectionViewSource.GetDefaultView(Cases).Refresh();
        var rows=service.List();var now=DateTime.UtcNow;
        int overdue=rows.Count(x=>x.State!="Completed"&&CorrespondenceService.Deadline(x,now).StartsWith("Quá hạn"));
        int soon=rows.Count(x=>CorrespondenceService.Deadline(x,now).StartsWith("Sắp hết hạn"));
        DeadlineAlert=overdue==0 ? (soon==0?"":$"Sắp hết hạn trong 3 ngày: {soon} hồ sơ.") : $"CẦN XỬ LÝ: {overdue} hồ sơ quá hạn; {soon} hồ sơ sắp hết hạn.";
    }
    private async Task Execute(string action)
    {
        busy=true;CommandManager.InvalidateRequerySuggested();
        try
        {
            if(action=="Refresh"){Reload();Status="Đã tải lại hồ sơ và hạn xử lý.";return;}
            if(action=="Receive")
            {
                var picker=new OpenFileDialog{Filter="Văn bản (*.txt;*.docx;*.pdf)|*.txt;*.docx;*.pdf",Title="Chọn văn bản đã tải từ email nội bộ"};
                if(picker.ShowDialog()!=true)return;
                var received=await service.ReceiveAsync(picker.FileName);Reload(received.Id);Status="Đã tiếp nhận. Điền thông tin và đối chiếu tệp gốc trước khi xác nhận.";return;
            }
            if(action=="Import")
            {
                var picker=new OpenFileDialog{Filter="Gói SACD (*.zip)|*.zip"};if(picker.ShowDialog()!=true)return;
                var imported=await service.ImportAsync(picker.FileName,PackageScanned);Reload(imported.Id);Status="Đã nhập hồ sơ và thông tin từ Máy 1. Không cần nhập lại từ đầu.";return;
            }
            var row=SelectedCase??throw new InvalidOperationException("Chọn một hồ sơ trước.");
            if(action=="Save"){row=await service.SaveAsync(row);Reload(row.Id);Status="Đã lưu thông tin. Nếu là Máy 1, tiếp tục đối chiếu rồi Xác nhận.";return;}
            if(action=="Open")
            {
                // Restore the extension only in the selected local view directory; never change the hash object.
                var folder=Path.Combine(db.WorkspaceDirectory,"view",row.Id);Directory.CreateDirectory(folder);
                var path=Path.Combine(folder,Path.GetFileName(row.OriginalName));
                File.Copy(new LocalFileStore(db.WorkspaceDirectory).GetObjectAbsolutePath(row.ObjectHash),path,true);
                Process.Start(new ProcessStartInfo(path){UseShellExecute=true});Status="Đã mở bản sao tệp gốc. In trong ứng dụng đọc, rồi xác nhận kết quả tại SACD.";return;
            }
            if(action=="Export")
            {
                var picker=new SaveFileDialog{Filter="Gói SACD (*.zip)|*.zip",FileName=row.Code+".zip",OverwritePrompt=true};if(picker.ShowDialog()!=true)return;
                if(File.Exists(picker.FileName))throw new InvalidOperationException("Chọn tên gói mới để giữ bản đã xuất.");
                await service.ExportAsync(row,picker.FileName);Reload(row.Id);Status="Đã xuất gói chứa tệp gốc, thông tin và checksum. Chuyển vật lý qua USB an toàn.";return;
            }
            if(action=="Slip")
            {
                if(Profile!="Management" || row.State!="Registered")throw new InvalidOperationException("Đối soát hồ sơ đã lưu trong QLVB trước khi tạo phiếu trình.");
                var picker=new SaveFileDialog{Filter="Phiếu Word (*.docx)|*.docx",FileName="Phieu_"+row.Code+".docx"};if(picker.ShowDialog()!=true)return;
                ProcessingSlipGenerator.Generate(picker.FileName,row);Process.Start(new ProcessStartInfo(picker.FileName){UseShellExecute=true});Status="Đã tạo phiếu Word và QR mã hồ sơ. Kiểm tra mẫu, in và trình lãnh đạo; SACD chưa ghi nhận đã duyệt.";return;
            }
            if(action=="Rpa")
            {
                if(Profile!="Management" || row.State!="Imported")throw new InvalidOperationException("Chọn hồ sơ chờ nhập QLVB ở Máy 2.");
                var picker=new OpenFileDialog{Filter="Cấu hình QLVB (*.json)|*.json",Title="Chọn mapping đã khảo sát cho QLVB thực tế"};if(picker.ShowDialog()!=true)return;
                string mappingHash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(picker.FileName))).ToLowerInvariant();
                await service.RecordAssistanceAsync(row,"RPA bắt đầu","Mapping SHA256: "+mappingHash+"; chưa lưu QLVB.");
                try { await Task.Run(()=>ControlledFormFiller.Fill(picker.FileName,row)); }
                catch {await service.RecordAssistanceAsync(row,"RPA cần đối soát","Dừng do lỗi; kiểm tra biểu mẫu thực tế trước khi thử lại.");throw;}
                await service.RecordAssistanceAsync(row,"RPA điền xong","Đã đọc lại giá trị; dừng trước Lưu.");
                Status="RPA đã điền và đọc lại các trường. Kiểm tra trên QLVB, tự bấm Lưu rồi nhập mã/số đến để đối soát.";return;
            }
            row=await service.AdvanceAsync(row,action,Note);Reload(row.Id);Note="";
            Status="Đã ghi nhận: "+row.StateLabel+". "+NextStep;
        }
        catch(Exception ex){Status="Chưa thực hiện: "+ex.Message;}
        finally{busy=false;CommandManager.InvalidateRequerySuggested();}
    }
    public void Dispose()=>timer.Stop();
}
