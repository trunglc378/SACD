using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SACD.Application.Common;
using SACD.Application.Contracts;
using SACD.Application.Services;
using SACD.Domain.Entities;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Rpa;
using SACD.Infrastructure.Services;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Text;
using SACD.Infrastructure.Transfer;
using SACD.Infrastructure.Word;

namespace SACD.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private SqliteDatabaseContext _dbContext = null!;
        private IFileStore _fileStore = null!;
        private IWorkspaceService _workspaceService = null!;
        private IIntakeService _intakeService = null!;
        private ITextService _textService = null!;
        private IMetadataService _metadataService = null!;
        private IWorkflowService _workflowService = null!;
        private IProposalService _proposalService = null!;
        private IDeliveryService _deliveryService = null!;
        private ISearchService _searchService = null!;
        private IFactService _factService = null!;
        private IReportService _reportService = null!;
        private IRecorderService _recorderService = null!;
        private IRunnerService _runnerService = null!;
        private IOperationsService _operationsService = null!;

        private ActorContext _actor = null!;

        // Workspace Props
        private string _workspacePath = @"E:\Project\SACD\workspace_data";
        private string _statusMessage = "Sẵn sàng khởi động.";
        private string _machineProfile = "Intake";
        private bool _isWorkspaceOpen;

        public string WorkspacePath { get => _workspacePath; set => SetProperty(ref _workspacePath, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public string MachineProfile { get => _machineProfile; set => SetProperty(ref _machineProfile, value); }
        public bool IsWorkspaceOpen { get => _isWorkspaceOpen; set => SetProperty(ref _isWorkspaceOpen, value); }

        // Collections
        public ObservableCollection<ImportItemResult> ImportedDocuments { get; } = new();
        public ObservableCollection<TextBlockDto> DocumentTextBlocks { get; } = new();
        public ObservableCollection<TaskItemDto> TasksList { get; } = new();
        public ObservableCollection<SearchResultDto> SearchResults { get; } = new();
        public ObservableCollection<FactResult> VerifiedFacts { get; } = new();
        public ObservableCollection<AuditItemDto> AuditEvents { get; } = new();

        // Search Keyword
        private string _searchKeyword = "";
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }

        // Selected Document / Task
        private ImportItemResult? _selectedDocument;
        public ImportItemResult? SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                if (SetProperty(ref _selectedDocument, value) && value != null)
                {
                    _ = LoadDocumentBlocksAsync(value.DocumentId);
                }
            }
        }

        private TaskItemDto? _selectedTask;
        public TaskItemDto? SelectedTask { get => _selectedTask; set => SetProperty(ref _selectedTask, value); }

        // Report Fields
        private string _reportTitle = "Báo cáo tổng hợp tình hình văn bản tháng 10/2026";
        private string _reportExportPath = @"E:\Project\SACD\exports";
        private string _lastExportStatus = "";
        public string ReportTitle { get => _reportTitle; set => SetProperty(ref _reportTitle, value); }
        public string ReportExportPath { get => _reportExportPath; set => SetProperty(ref _reportExportPath, value); }
        public string LastExportStatus { get => _lastExportStatus; set => SetProperty(ref _lastExportStatus, value); }

        // Commands
        public ICommand OpenWorkspaceCommand { get; }
        public ICommand ImportFilesCommand { get; }
        public ICommand ReviewTextCommand { get; }
        public ICommand CreateTaskCommand { get; }
        public ICommand CompleteTaskCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ExportDraftReportCommand { get; }
        public ICommand ExportSubmissionReportCommand { get; }
        public ICommand RunRpaMacroCommand { get; }
        public ICommand BackupWorkspaceCommand { get; }

        public MainViewModel()
        {
            OpenWorkspaceCommand = new RelayCommand(async () => await OpenWorkspaceAsync());
            ImportFilesCommand = new RelayCommand(async () => await ImportFilesAsync(), () => IsWorkspaceOpen);
            ReviewTextCommand = new RelayCommand(async () => await ReviewTextAsync(), () => SelectedDocument != null);
            CreateTaskCommand = new RelayCommand(async () => await CreateTaskAsync(), () => IsWorkspaceOpen);
            CompleteTaskCommand = new RelayCommand(async () => await CompleteTaskAsync(), () => SelectedTask != null);
            SearchCommand = new RelayCommand(async () => await ExecuteSearchAsync(), () => IsWorkspaceOpen);
            ExportDraftReportCommand = new RelayCommand(async () => await ExportReportAsync("Draft"), () => IsWorkspaceOpen);
            ExportSubmissionReportCommand = new RelayCommand(async () => await ExportReportAsync("Submission"), () => IsWorkspaceOpen);
            RunRpaMacroCommand = new RelayCommand(async () => await RunRpaMacroAsync(), () => IsWorkspaceOpen);
            BackupWorkspaceCommand = new RelayCommand(async () => await BackupWorkspaceAsync(), () => IsWorkspaceOpen);

            InitializeServices(WorkspacePath);
        }

        private void InitializeServices(string path)
        {
            _dbContext = new SqliteDatabaseContext(path);
            _fileStore = new LocalFileStore(path);
            _actor = ActorContext.Create(Environment.UserName, "ws-desktop-01");

            _workspaceService = new WorkspaceService(_dbContext);
            _intakeService = new IntakeService(_dbContext, _fileStore);
            _textService = new TextService(_dbContext, _fileStore, new DocumentTextExtractor());
            _metadataService = new MetadataService(_dbContext);
            _workflowService = new WorkflowService(_dbContext);
            _proposalService = new ProposalService(_dbContext);
            _deliveryService = new DeliveryService(_dbContext);
            _searchService = new SearchService(_dbContext);
            _factService = new FactService(_dbContext);
            _reportService = new ReportService(_dbContext, _fileStore, new WordReportGenerator());
            _recorderService = new RecorderService(_dbContext);
            _runnerService = new RunnerService(_dbContext, new RpaEngine());
            _operationsService = new OperationsService(_dbContext);
        }

        public async Task OpenWorkspaceAsync()
        {
            try
            {
                StatusMessage = "Đang mở không gian làm việc...";
                InitializeServices(WorkspacePath);

                var res = await _workspaceService.OpenAsync(new WorkspaceOpenRequest(WorkspacePath, "Standard", 1), _actor);
                if (res.Success)
                {
                    IsWorkspaceOpen = true;
                    MachineProfile = res.Value!.MachineProfile;
                    StatusMessage = $"Đã mở kho: {res.Value.WorkspaceId} (SID: {_actor.WindowsSid}, Mode: {MachineProfile})";
                    await RefreshDataAsync();
                }
                else
                {
                    StatusMessage = $"Lỗi mở kho: {res.Message} [{res.Code}]";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }
        }

        public async Task RefreshDataAsync()
        {
            if (!IsWorkspaceOpen) return;

            // Load Deadlines / Tasks
            var deadlineRes = await _workflowService.EvaluateDeadlinesAsync(new DeadlineQuery(null, null, 24), _actor);
            if (deadlineRes.Success && deadlineRes.Value != null)
            {
                TasksList.Clear();
                foreach (var t in deadlineRes.Value.Tasks)
                {
                    TasksList.Add(t);
                }
            }

            // Load Audit
            var auditRes = await _operationsService.QueryAuditAsync(new AuditQuery(null, null, null, null, null, 20), _actor);
            if (auditRes.Success && auditRes.Value != null)
            {
                AuditEvents.Clear();
                foreach (var a in auditRes.Value.Events)
                {
                    AuditEvents.Add(a);
                }
            }
        }

        private async Task ImportFilesAsync()
        {
            try
            {
                // Create a sample demo document to import
                string sampleFile = Path.Combine(WorkspacePath, "VanBan_Den_Sample.txt");
                if (!File.Exists(sampleFile))
                {
                    await File.WriteAllTextAsync(sampleFile, "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\nĐộc lập - Tự do - Hạnh phúc\n\nQUYẾT ĐỊNH XỬ LÝ VĂN BẢN VÀ THỜI HẠN BÁO CÁO\nSố: 15/QĐ-UBND\nNgày ban hành: 15/10/2026\nĐơn vị nhận: Phòng Tổng hợp Nghiệp vụ\nNội dung: Hoàn thành tổng hợp tài liệu và số liệu trước ngày 20/10/2026.\n");
                }

                StatusMessage = "Đang nhập tệp và kiểm tra toàn vẹn SHA256...";
                var res = await _intakeService.ImportAsync(new ImportRequest(
                    new[] { sampleFile },
                    null,
                    "File",
                    "Thủ công từ máy A",
                    1
                ), _actor);

                if (res.Success && res.Value != null)
                {
                    foreach (var item in res.Value.Items)
                    {
                        ImportedDocuments.Add(item);
                        // Trigger extraction automatically
                        await _textService.ExtractAsync(new ExtractRequest(item.DocumentId, "BuiltIn"), _actor);
                    }
                    SelectedDocument = ImportedDocuments.LastOrDefault();
                    StatusMessage = $"Đã nhập thành công hồ sơ {res.Value.BusinessCode} ({res.Value.Items.Count} tệp).";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi nhập tệp: {ex.Message}";
            }
        }

        private async Task LoadDocumentBlocksAsync(string documentId)
        {
            try
            {
                DocumentTextBlocks.Clear();
                var res = await _textService.ExtractAsync(new ExtractRequest(documentId, "BuiltIn"), _actor);
                if (res.Success && res.Value != null)
                {
                    foreach (var b in res.Value.Blocks)
                    {
                        DocumentTextBlocks.Add(b);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi trích xuất: {ex.Message}";
            }
        }

        private async Task ReviewTextAsync()
        {
            if (SelectedDocument == null) return;
            try
            {
                StatusMessage = "Đang xác nhận đối chiếu nội dung...";
                var res = await _textService.SaveReviewAsync(new TextReviewRequest(
                    SelectedDocument.DocumentId,
                    SelectedDocument.DocumentId, // using expected revision
                    DocumentTextBlocks.ToList(),
                    new[] { 1 },
                    "Đã kiểm tra đối chiếu văn bản đầy đủ"
                ), _actor);

                if (res.Success)
                {
                    StatusMessage = "Văn bản đã chuyển trạng thái [Reviewed] có căn cứ.";
                }
                else
                {
                    StatusMessage = $"Lỗi duyệt: {res.Message}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi duyệt: {ex.Message}";
            }
        }

        private async Task CreateTaskAsync()
        {
            try
            {
                string dosId = ImportedDocuments.FirstOrDefault()?.DocumentId ?? Guid.NewGuid().ToString("N");
                var res = await _workflowService.SaveTaskAsync(new TaskRequest(
                    DossierId: dosId,
                    TaskId: null,
                    ExpectedRowVersion: 0,
                    AssigneeVersionId: null,
                    Name: $"Soạn thảo báo cáo định kỳ {DateTime.Now:dd/MM}",
                    DueKind: "Date",
                    DueDate: DateTime.Now.AddDays(3).ToString("yyyy-MM-dd"),
                    DueUtc: null,
                    EvidenceId: null
                ), _actor);

                if (res.Success)
                {
                    StatusMessage = "Đã tạo nhiệm vụ và tính toán thời hạn.";
                    await RefreshDataAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tạo nhiệm vụ: {ex.Message}";
            }
        }

        private async Task CompleteTaskAsync()
        {
            if (SelectedTask == null) return;
            try
            {
                var res = await _workflowService.CompleteAsync(new CompleteTaskRequest(
                    SelectedTask.Id,
                    SelectedTask.RowVersion,
                    null,
                    "Đã hoàn thành theo chỉ đạo"
                ), _actor);

                if (res.Success)
                {
                    StatusMessage = $"Nhiệm vụ {SelectedTask.Name} đã hoàn thành.";
                    await RefreshDataAsync();
                }
                else
                {
                    StatusMessage = $"Lỗi: {res.Message}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi hoàn thành nhiệm vụ: {ex.Message}";
            }
        }

        private async Task ExecuteSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchKeyword)) return;
            try
            {
                SearchResults.Clear();
                var res = await _searchService.SearchAsync(new SearchRequest(SearchKeyword, null, null, null, 1, 20), _actor);
                if (res.Success && res.Value != null)
                {
                    foreach (var r in res.Value.Results)
                    {
                        SearchResults.Add(r);
                    }
                    StatusMessage = $"Tìm thấy {res.Value.TotalCount} kết quả khớp từ khóa '{SearchKeyword}'.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tìm kiếm: {ex.Message}";
            }
        }

        private async Task ExportReportAsync(string mode)
        {
            try
            {
                Directory.CreateDirectory(ReportExportPath);
                StatusMessage = $"Đang xuất báo cáo dạng {mode} bằng OpenXML...";

                // Create draft first
                var draftRes = await _reportService.CreateDraftAsync(new ReportRequest(
                    ReportId: null,
                    Name: ReportTitle,
                    TemplateVersionId: "TPL-DEFAULT",
                    PeriodStart: DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"),
                    PeriodEnd: DateTime.Now.ToString("yyyy-MM-dd"),
                    SourceRevisionIds: Array.Empty<string>(),
                    AggregationIds: Array.Empty<string>(),
                    SectionTexts: new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "I. Tình hình tiếp nhận", $"Tổng số văn bản đã tiếp nhận và lập hồ sơ trong kỳ: {ImportedDocuments.Count}." },
                        { "II. Theo dõi hạn và tiến độ", $"Số nhiệm vụ đang xử lý: {TasksList.Count(t => t.State != "Completed")}, đã hoàn thành: {TasksList.Count(t => t.State == "Completed")}." },
                        { "III. Đề xuất kiến nghị", "Đề nghị tiếp tục duy trì quy trình kiểm tra toàn vẹn và đối soát dữ liệu trên máy cách ly." }
                    }
                ), _actor);

                if (!draftRes.Success || draftRes.Value == null)
                {
                    StatusMessage = $"Lỗi tạo nháp báo cáo: {draftRes.Message}";
                    return;
                }

                string revId = draftRes.Value.RevisionId;

                if (mode == "Submission")
                {
                    // Review to allow submission export per BR-30
                    await _reportService.ReviewAsync(new ReviewRequest(
                        revId, 1, Array.Empty<string>(), "{\"checked\":true}", "Đã rà soát đủ điều kiện trình lãnh đạo"
                    ), _actor);
                }

                var expRes = await _reportService.ExportAsync(new ExportRequest(
                    revId, mode, ReportExportPath, true
                ), _actor);

                if (expRes.Success && expRes.Value != null)
                {
                    LastExportStatus = $"Đã xuất: {Path.GetFileName(expRes.Value.OutputPath)}\nSHA256: {expRes.Value.OutputHash}";
                    StatusMessage = $"Xuất báo cáo Word ({mode}) thành công!";
                }
                else
                {
                    StatusMessage = $"Lỗi xuất báo cáo: {expRes.Message}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }
        }

        private async Task RunRpaMacroAsync()
        {
            try
            {
                StatusMessage = "Đang chạy kịch bản tự động hóa nhập liệu QLVB...";

                // Create script
                var pubRes = await _recorderService.PublishScriptAsync(new PublishScriptRequest(
                    ScriptId: "SCRIPT-QLVB-01",
                    Name: "Tự động nhập số văn bản và trích yếu vào QLVB",
                    Steps: new[]
                    {
                        new ScriptStepDto(1, "WaitWindow", null, "{\"window\":\"Quản lý văn bản\"}", 5000, "None"),
                        new ScriptStepDto(2, "PauseForUser", null, "{\"prompt\":\"Kiểm tra biểu mẫu trước khi ghi\"}", 10000, "None"),
                        new ScriptStepDto(3, "Click", null, "{\"button\":\"Lưu\"}", 3000, "None")
                    },
                    ProfileJson: "{}"
                ), _actor);

                if (pubRes.Success && pubRes.Value != null)
                {
                    var runRes = await _runnerService.RunAsync(new RunRequest(
                        ScriptVersionId: pubRes.Value.ScriptVersionId,
                        IdempotencyKey: Guid.NewGuid().ToString("N"),
                        VariableBindings: new System.Collections.Generic.Dictionary<string, string>(),
                        StepByStepMode: false
                    ), _actor);

                    if (runRes.Success && runRes.Value != null)
                    {
                        StatusMessage = $"Kịch bản hoàn tất: Trạng thái {runRes.Value.State} (Bước {runRes.Value.CurrentStep}).";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi RPA: {ex.Message}";
            }
        }

        private async Task BackupWorkspaceAsync()
        {
            try
            {
                string backupDir = Path.Combine(WorkspacePath, "backups");
                StatusMessage = "Đang tạo bản sao lưu snapshot kho...";
                var res = await _operationsService.BackupAsync(new BackupRequest(backupDir, "Sao lưu định kỳ"), _actor);
                if (res.Success && res.Value != null)
                {
                    StatusMessage = $"Sao lưu thành công: {Path.GetFileName(res.Value.BackupPath)} (SHA256: {res.Value.ManifestHash[..8]}...)";
                    await RefreshDataAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi sao lưu: {ex.Message}";
            }
        }
    }
}
