using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Automation;
using System.Text.Json;
using SACD.Desktop;
using SACD.Desktop.ViewModels;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Services;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var app=new System.Windows.Application();int code=1;
        app.Startup+=async (_,_)=>
        {
            var root=Path.Combine(Path.GetTempPath(),"SACD_UI_"+Guid.NewGuid().ToString("N"));
            try
            {
                using var vm=new MainViewModel{WorkspacePath=root};
                var window=new MainWindow{DataContext=vm};
                await vm.OpenWorkspaceAsync();
                Check(vm.IsWorkspaceOpen,"Không mở được kho: "+vm.StatusMessage);
                vm.ImportFilesCommand.Execute(null);
                await Until(()=>vm.ImportedDocuments.Count>0 && vm.DocumentTextBlocks.Count>0);
                vm.ReviewTextCommand.Execute(null);
                await Until(()=>vm.StatusMessage.Contains("[Reviewed]"));
                vm.RunRpaMacroCommand.Execute(null);
                await Until(()=>vm.StatusMessage.StartsWith("RPA mô phỏng:"));
                vm.RunRpaMacroCommand.Execute(null);
                await Until(()=>vm.StatusMessage.StartsWith("RPA mô phỏng:"));
                Check(!vm.StatusMessage.Contains("UNIQUE"),"Lỗi phiên bản RPA lặp lại");
                using var context=new SqliteDatabaseContext(root);
                var workflow=new CorrespondenceService(context,ActorContext.Create("S-TEST","smoke"));
                string source=Path.Combine(root,"fixture.txt");await File.WriteAllTextAsync(source,"VĂN BẢN KIỂM THỬ GIẢ LẬP\nSố: 123/CV\nNội dung thử quy trình.");
                var row=await workflow.ReceiveAsync(source);
                row=await workflow.SaveAsync(row with {Number="123/CV",DocumentType="Công văn",IssuedDate="2026-10-16",Issuer="Đơn vị giả lập A",Recipient="Đơn vị giả lập B",Summary="Văn bản kiểm thử quy trình",Classification="Không mật",ProposedReceiver="Đội thử nghiệm"});
                var desk=vm.CaseDesk!;desk.ActionCommand.Execute("Refresh");desk.SelectedCase=desk.Cases.Single();
                desk.Note="Đã đối chiếu dữ liệu thử nghiệm";desk.ActionCommand.Execute("Confirm");
                await Until(()=>desk.SelectedCase?.State=="Reviewed");
                var tabs=Find<TabControl>((DependencyObject)window.Content)!;tabs.SelectedIndex=1;
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                var grid=(FrameworkElement)window.Content;
                grid.Measure(new Size(1366,768));grid.Arrange(new Rect(0,0,1366,768));grid.UpdateLayout();
                Check(Find<CorrespondenceView>(grid)!=null,"Thiếu giao diện quy trình");
                var png=new PngBitmapEncoder();var bitmap=new RenderTargetBitmap(1366,768,96,96,PixelFormats.Pbgra32);bitmap.Render(grid);png.Frames.Add(BitmapFrame.Create(bitmap));
                string output=Path.GetFullPath(args.FirstOrDefault()??Path.Combine("TestResults","workflow-ui.png"));Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                using(var stream=File.Create(output))png.Save(stream);
                // Exercise the actual Windows UIA adapter against a synthetic QLVB form.
                var panel=new StackPanel();var fields=new Dictionary<string,TextBox>();
                foreach(var key in new[]{"Number","Summary","Issuer","DueDate"})
                {var box=new TextBox();AutomationProperties.SetAutomationId(box,key);panel.Children.Add(box);fields[key]=box;}
                bool saved=false;var save=new Button{Content="Lưu"};save.Click+=(_,_)=>saved=true;panel.Children.Add(save);
                var target=new Window{Title="SACD QLVB TEST FIXTURE",Content=panel,Width=350,Height=250,Left=-10000,Top=-10000,ShowInTaskbar=false};
                target.Show();
                string mapping=Path.Combine(root,"mapping.json");
                await File.WriteAllTextAsync(mapping,JsonSerializer.Serialize(new ControlledFormFiller.Mapping(Environment.ProcessPath!,target.Title,fields.Keys.ToDictionary(k=>k,k=>k))));
                await Task.Run(()=>ControlledFormFiller.Fill(mapping,row));
                Check(fields["Number"].Text==row.Number && fields["Issuer"].Text==row.Issuer,"RPA điền sai dữ liệu");
                Check(!saved,"RPA đã bấm Lưu ngoài ý muốn");
                foreach(var box in fields.Values)box.Text="";
                var wrong=fields.Keys.ToDictionary(k=>k,k=>k);wrong["Issuer"]="KHONG_TON_TAI";
                await File.WriteAllTextAsync(mapping,JsonSerializer.Serialize(new ControlledFormFiller.Mapping(Environment.ProcessPath!,target.Title,wrong)));
                bool rejected=false;
                try{await Task.Run(()=>ControlledFormFiller.Fill(mapping,row));}catch(InvalidOperationException){rejected=true;}
                Check(rejected&&fields.Values.All(x=>x.Text==""),"Selector sai phải bị chặn trước khi điền bất kỳ trường nào");
                target.Close();
                Console.WriteLine("PASS: mở kho, tiếp nhận, duyệt revision, RPA mô phỏng 2 lần, giao diện quy trình, UIA điền biểu mẫu giả lập và dừng trước Lưu.");
                Console.WriteLine(output);window.Close();code=0;
            }
            catch(Exception ex){Console.Error.WriteLine(ex);}
            finally{try{Directory.Delete(root,true);}catch{}app.Shutdown();}
        };
        app.Run();return code;
    }
    private static void Check(bool value,string message){if(!value)throw new Exception(message);}
    private static async Task Until(Func<bool> condition)
    {for(int i=0;i<100;i++){if(condition())return;await Task.Delay(50);}throw new TimeoutException("Thao tác UI chưa hoàn tất.");}
    private static T? Find<T>(DependencyObject parent) where T:DependencyObject
    {if(parent is T match)return match;for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var found=Find<T>(VisualTreeHelper.GetChild(parent,i));if(found!=null)return found;}return null;}
}
