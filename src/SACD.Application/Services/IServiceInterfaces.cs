using System.Threading;
using System.Threading.Tasks;
using SACD.Application.Common;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;

namespace SACD.Application.Services
{
    public interface IWorkspaceService
    {
        Task<OperationResult<WorkspaceInfo>> OpenAsync(WorkspaceOpenRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<WorkspaceInfo>> InitializeNewAsync(string directoryPath, string machineProfile, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<bool>> CheckLockAsync(string directoryPath, CancellationToken ct = default);
    }

    public interface IIntakeService
    {
        Task<OperationResult<ImportBatchResult>> ImportAsync(ImportRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ImportBatchResult>> ReceiveMailAsync(MailIntakeRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface ITextService
    {
        Task<OperationResult<TextRevisionResult>> ExtractAsync(ExtractRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<TextRevisionResult>> OcrAsync(OcrRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<TextRevisionResult>> SaveReviewAsync(TextReviewRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IMetadataService
    {
        Task<OperationResult<MetadataResult>> SaveMetadataAsync(MetadataRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<string>> ClassifyAsync(ClassificationRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IPrintService
    {
        Task<OperationResult<PrintAttemptInfo>> PrepareAsync(PrintRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<PrintAttemptInfo>> SubmitAsync(string attemptId, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<PrintAttemptInfo>> ConfirmAsync(PrintConfirmation confirmation, ActorContext actor, CancellationToken ct = default);
    }

    public interface ITransferService
    {
        Task<OperationResult<TransferResult>> PrepareAsync(TransferPrepareRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<TransferResult>> ImportAsync(TransferImportRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IWorkflowService
    {
        Task<OperationResult<TaskResult>> SaveTaskAsync(TaskRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<DeadlinePage>> EvaluateDeadlinesAsync(DeadlineQuery query, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<TaskResult>> CompleteAsync(CompleteTaskRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IExternalEntryService
    {
        Task<OperationResult<ExternalEntryResult>> FillAsync(ExternalFillRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ExternalEntryResult>> ReconcileAsync(ReconciliationRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IProposalService
    {
        Task<OperationResult<ProposalResult>> CreateAsync(ProposalRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ProposalResult>> SubmitAsync(SubmitProposalRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<string>> RecordDecisionAsync(DecisionRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IDeliveryService
    {
        Task<OperationResult<DeliveryResult>> DeliverAsync(DeliveryRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<DeliveryResult>> RecordReceiptAsync(ReceiptRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface ISearchService
    {
        Task<OperationResult<SearchPage>> SearchAsync(SearchRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<HistoryPage>> HistoryAsync(HistoryRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<bool>> ArchiveAsync(ArchiveRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IFactService
    {
        Task<OperationResult<FactResult>> ConfirmAsync(FactRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<DedupResult>> ResolveDuplicateAsync(DedupRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<AggregateResult>> AggregateAsync(AggregateRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IReportService
    {
        Task<ReportStatistics> GetStatisticsAsync(string periodStart, string periodEnd, CancellationToken ct = default);
        Task<OperationResult<TemplateResult>> ValidateTemplateAsync(TemplateRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ReportRevisionInfo>> CreateDraftAsync(ReportRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ReportRevisionInfo>> ReviewAsync(ReviewRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ExportInfo>> ExportAsync(ExportRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IRecorderService
    {
        Task<OperationResult<ScriptDraftInfo>> StartAsync(RecordRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ScriptDraftInfo>> StopAsync(string scriptId, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<ScriptVersionInfo>> PublishScriptAsync(PublishScriptRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IRunnerService
    {
        Task<OperationResult<RunInfo>> RunAsync(RunRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<RunInfo>> ControlAsync(RunControlRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IOperationsService
    {
        Task<OperationResult<ConfigInfo>> SaveConfigAsync(ConfigRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<AuditPage>> QueryAuditAsync(AuditQuery query, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<BackupInfo>> BackupAsync(BackupRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<RestoreInfo>> RestoreAsync(RestoreRequest request, ActorContext actor, CancellationToken ct = default);
    }

    public interface IAiAssistant
    {
        Task<OperationResult<AiProposal>> ProposeAsync(AiRequest request, ActorContext actor, CancellationToken ct = default);
        bool IsEnabled { get; }
    }

    public interface IDirectoryService
    {
        Task<OperationResult<DirectoryVersionInfo>> PublishAsync(DirectoryRequest request, ActorContext actor, CancellationToken ct = default);
        Task<OperationResult<bool>> RetireAsync(string entryId, ActorContext actor, CancellationToken ct = default);
    }
}
