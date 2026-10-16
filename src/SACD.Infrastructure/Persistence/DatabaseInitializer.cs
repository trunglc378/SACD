using System;
using Microsoft.Data.Sqlite;

namespace SACD.Infrastructure.Persistence
{
    public static class DatabaseInitializer
    {
        public const string DdlScript = @"
PRAGMA foreign_keys=ON;

CREATE TABLE IF NOT EXISTS Workspace (
 id TEXT NOT NULL PRIMARY KEY,
 ownerSid TEXT NOT NULL,
 machineProfile TEXT NOT NULL CHECK(machineProfile IN ('Intake','Management','Test')),
 schemaVersion INTEGER NOT NULL CHECK(schemaVersion>0),
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS FileObject (
 hash TEXT NOT NULL PRIMARY KEY CHECK(length(hash)=64),
 relativePath TEXT NOT NULL UNIQUE,
 sizeBytes INTEGER NOT NULL CHECK(sizeBytes>=0),
 mediaType TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Dossier (
 id TEXT NOT NULL PRIMARY KEY,
 businessCode TEXT NOT NULL UNIQUE,
 state TEXT NOT NULL CHECK(state IN ('Received','Checking','Prepared','AwaitingDecision','Approved','Returned','Rejected','Delivered','Acknowledged','InProgress','Completed','Archived')),
 rowVersion INTEGER NOT NULL DEFAULT 1 CHECK(rowVersion>0),
 receivedUtc TEXT NOT NULL,
 archivedUtc TEXT
);

CREATE TABLE IF NOT EXISTS Document (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 objectHash TEXT NOT NULL REFERENCES FileObject(hash),
 originalName TEXT NOT NULL,
 sourceKind TEXT NOT NULL CHECK(sourceKind IN ('Mail','File','Scan','Transfer','Manual')),
 sourceRef TEXT,
 currentRevisionId TEXT,
 status TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(dossierId,objectHash),
 FOREIGN KEY(currentRevisionId) REFERENCES TextRevision(id) DEFERRABLE INITIALLY DEFERRED
);

CREATE TABLE IF NOT EXISTS TextRevision (
 id TEXT NOT NULL PRIMARY KEY,
 documentId TEXT NOT NULL REFERENCES Document(id),
 versionNo INTEGER NOT NULL CHECK(versionNo>0),
 method TEXT NOT NULL,
 reviewState TEXT NOT NULL CHECK(reviewState IN ('NeedsReview','Partial','Reviewed')),
 reviewerSid TEXT,
 reviewedUtc TEXT,
 createdUtc TEXT NOT NULL,
 UNIQUE(documentId,versionNo),
 CHECK(reviewState<>'Reviewed' OR (reviewerSid IS NOT NULL AND reviewedUtc IS NOT NULL))
);

CREATE TABLE IF NOT EXISTS TextBlock (
 id TEXT NOT NULL PRIMARY KEY,
 revisionId TEXT NOT NULL REFERENCES TextRevision(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=1),
 locatorJson TEXT NOT NULL,
 extractedText TEXT NOT NULL,
 rawTextNfc TEXT NOT NULL,
 searchText TEXT NOT NULL,
 reviewed INTEGER NOT NULL DEFAULT 0 CHECK(reviewed IN (0,1)),
 UNIQUE(revisionId,ordinal),
 UNIQUE(revisionId,id)
);

CREATE TABLE IF NOT EXISTS EvidenceRef (
 id TEXT NOT NULL PRIMARY KEY,
 revisionId TEXT NOT NULL,
 blockId TEXT NOT NULL,
 startOffset INTEGER NOT NULL CHECK(startOffset>=0),
 endOffset INTEGER NOT NULL,
 quoteHash TEXT NOT NULL CHECK(length(quoteHash)=64),
 createdUtc TEXT NOT NULL,
 CHECK(endOffset>startOffset),
 FOREIGN KEY(revisionId,blockId) REFERENCES TextBlock(revisionId,id)
);

CREATE TABLE IF NOT EXISTS MetadataVersion (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 fieldKey TEXT NOT NULL,
 versionNo INTEGER NOT NULL CHECK(versionNo>0),
 valueJson TEXT,
 status TEXT NOT NULL CHECK(status IN ('Unknown','Proposed','Confirmed','Rejected')),
 evidenceId TEXT REFERENCES EvidenceRef(id),
 manualReason TEXT,
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(dossierId,fieldKey,versionNo),
 CHECK(status<>'Confirmed' OR (valueJson IS NOT NULL AND evidenceId IS NOT NULL))
);

CREATE TABLE IF NOT EXISTS DirectoryEntryVersion (
 id TEXT NOT NULL PRIMARY KEY,
 logicalId TEXT NOT NULL,
 versionNo INTEGER NOT NULL CHECK(versionNo>0),
 kind TEXT NOT NULL,
 code TEXT NOT NULL,
 name TEXT NOT NULL,
 parentVersionId TEXT REFERENCES DirectoryEntryVersion(id),
 active INTEGER NOT NULL CHECK(active IN (0,1)),
 createdUtc TEXT NOT NULL,
 UNIQUE(logicalId,versionNo),
 UNIQUE(kind,code,versionNo)
);

CREATE TABLE IF NOT EXISTS ClassificationRecord (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 versionNo INTEGER NOT NULL CHECK(versionNo>0),
 classificationJson TEXT NOT NULL,
 milestoneDate TEXT,
 evidenceId TEXT REFERENCES EvidenceRef(id),
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(dossierId,versionNo)
);

CREATE TABLE IF NOT EXISTS TaskItem (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 name TEXT NOT NULL,
 assigneeVersionId TEXT REFERENCES DirectoryEntryVersion(id),
 state TEXT NOT NULL CHECK(state IN ('Draft','Assigned','InProgress','Completed','Cancelled','Reopened')),
 dueKind TEXT NOT NULL CHECK(dueKind IN ('Unknown','Date','Instant')),
 dueDate TEXT,
 dueUtc TEXT,
 completedUtc TEXT,
 rowVersion INTEGER NOT NULL DEFAULT 1,
 CHECK((dueKind='Unknown' AND dueDate IS NULL AND dueUtc IS NULL) OR (dueKind='Date' AND dueDate IS NOT NULL AND dueUtc IS NULL) OR (dueKind='Instant' AND dueUtc IS NOT NULL AND dueDate IS NULL))
);

CREATE TABLE IF NOT EXISTS TaskEvent (
 id TEXT NOT NULL PRIMARY KEY,
 taskId TEXT NOT NULL REFERENCES TaskItem(id),
 sequenceNo INTEGER NOT NULL,
 kind TEXT NOT NULL,
 payloadJson TEXT NOT NULL,
 evidenceId TEXT REFERENCES EvidenceRef(id),
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(taskId,sequenceNo)
);

CREATE TABLE IF NOT EXISTS WorkflowEvent (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 sequenceNo INTEGER NOT NULL,
 fromState TEXT NOT NULL,
 toState TEXT NOT NULL,
 reason TEXT NOT NULL,
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(dossierId,sequenceNo)
);

CREATE TABLE IF NOT EXISTS TemplateVersion (
 id TEXT NOT NULL PRIMARY KEY,
 logicalId TEXT NOT NULL,
 versionNo INTEGER NOT NULL,
 objectHash TEXT NOT NULL REFERENCES FileObject(hash),
 mappingJson TEXT NOT NULL,
 mappingHash TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(logicalId,versionNo)
);

CREATE TABLE IF NOT EXISTS ProposalVersion (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 logicalId TEXT NOT NULL,
 versionNo INTEGER NOT NULL,
 recipientVersionId TEXT NOT NULL REFERENCES DirectoryEntryVersion(id),
 contentJson TEXT NOT NULL,
 templateVersionId TEXT REFERENCES TemplateVersion(id),
 state TEXT NOT NULL CHECK(state IN ('Draft','Submitted','Superseded')),
 submittedUtc TEXT,
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(logicalId,versionNo)
);

CREATE TABLE IF NOT EXISTS DecisionRecord (
 id TEXT NOT NULL PRIMARY KEY,
 proposalVersionId TEXT NOT NULL REFERENCES ProposalVersion(id),
 outcome TEXT NOT NULL CHECK(outcome IN ('Approved','Rejected','ReturnedForEdit','Superseded')),
 decisionMakerVersionId TEXT NOT NULL REFERENCES DirectoryEntryVersion(id),
 decidedUtc TEXT NOT NULL,
 evidenceId TEXT NOT NULL REFERENCES EvidenceRef(id),
 supersedesId TEXT REFERENCES DecisionRecord(id),
 recordedBySid TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS DeliveryRecord (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 decisionId TEXT NOT NULL REFERENCES DecisionRecord(id),
 recipientVersionId TEXT NOT NULL REFERENCES DirectoryEntryVersion(id),
 snapshotJson TEXT NOT NULL,
 deliveredUtc TEXT NOT NULL,
 recordedBySid TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ReceiptRecord (
 id TEXT NOT NULL PRIMARY KEY,
 deliveryId TEXT NOT NULL REFERENCES DeliveryRecord(id),
 receiverVersionId TEXT NOT NULL REFERENCES DirectoryEntryVersion(id),
 signedUtc TEXT NOT NULL,
 evidenceId TEXT NOT NULL REFERENCES EvidenceRef(id),
 exceptionReason TEXT,
 recordedBySid TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS MappingVersion (
 id TEXT NOT NULL PRIMARY KEY,
 logicalId TEXT NOT NULL,
 versionNo INTEGER NOT NULL,
 kind TEXT NOT NULL,
 payloadJson TEXT NOT NULL,
 contentHash TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(logicalId,versionNo)
);

CREATE TABLE IF NOT EXISTS ExternalEntryAttempt (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT NOT NULL REFERENCES Dossier(id),
 mappingVersionId TEXT NOT NULL REFERENCES MappingVersion(id),
 idempotencyKey TEXT NOT NULL UNIQUE,
 snapshotHash TEXT NOT NULL,
 state TEXT NOT NULL CHECK(state IN ('Prepared','Executing','AwaitingUser','Verified','Failed','NeedsReconciliation','Cancelled')),
 externalRef TEXT,
 reconciledBySid TEXT,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS PrintAttempt (
 id TEXT NOT NULL PRIMARY KEY,
 dossierId TEXT REFERENCES Dossier(id),
 objectHash TEXT NOT NULL REFERENCES FileObject(hash),
 printerProfileJson TEXT NOT NULL,
 state TEXT NOT NULL CHECK(state IN ('Prepared','Submitted','ConfirmedPrinted','Failed','Unknown','Cancelled')),
 spoolJobId TEXT,
 confirmedBySid TEXT,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS TransferPackage (
 id TEXT NOT NULL PRIMARY KEY,
 originWorkspaceId TEXT NOT NULL,
 schemaVersion INTEGER NOT NULL,
 manifestHash TEXT NOT NULL,
 state TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 importedUtc TEXT
);

CREATE TABLE IF NOT EXISTS TransferItem (
 packageId TEXT NOT NULL REFERENCES TransferPackage(id),
 relativePath TEXT NOT NULL,
 sizeBytes INTEGER NOT NULL CHECK(sizeBytes>=0),
 contentHash TEXT NOT NULL,
 entityType TEXT NOT NULL,
 originEntityId TEXT NOT NULL,
 originVersion TEXT NOT NULL,
 PRIMARY KEY(packageId,relativePath),
 UNIQUE(packageId,entityType,originEntityId,originVersion)
);

CREATE TABLE IF NOT EXISTS ImportMap (
 originWorkspaceId TEXT NOT NULL,
 entityType TEXT NOT NULL,
 originEntityId TEXT NOT NULL,
 originVersion TEXT NOT NULL,
 contentHash TEXT NOT NULL,
 localEntityId TEXT NOT NULL,
 packageId TEXT NOT NULL REFERENCES TransferPackage(id),
 PRIMARY KEY(originWorkspaceId,entityType,originEntityId,originVersion)
);

CREATE TABLE IF NOT EXISTS MetricDefinitionVersion (
 id TEXT NOT NULL PRIMARY KEY,
 logicalId TEXT NOT NULL,
 versionNo INTEGER NOT NULL,
 name TEXT NOT NULL,
 unitCode TEXT NOT NULL,
 kind TEXT NOT NULL CHECK(kind IN ('Flow','Stock','Ratio','Cumulative')),
 ruleJson TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(logicalId,versionNo)
);

CREATE TABLE IF NOT EXISTS FactVersion (
 id TEXT NOT NULL PRIMARY KEY,
 logicalId TEXT NOT NULL,
 versionNo INTEGER NOT NULL,
 metricVersionId TEXT NOT NULL REFERENCES MetricDefinitionVersion(id),
 sourceRevisionId TEXT NOT NULL REFERENCES TextRevision(id),
 sourceNumberText TEXT,
 numberFormat TEXT,
 valueStatus TEXT NOT NULL CHECK(valueStatus IN ('Known','Missing')),
 valueDecimalText TEXT,
 periodStart TEXT NOT NULL,
 periodEnd TEXT NOT NULL,
 periodKind TEXT NOT NULL,
 scopeJson TEXT NOT NULL,
 status TEXT NOT NULL CHECK(status IN ('Proposed','Confirmed','Rejected')),
 reviewerSid TEXT,
 reviewedUtc TEXT,
 createdUtc TEXT NOT NULL,
 UNIQUE(logicalId,versionNo),
 CHECK(periodStart<=periodEnd),
 CHECK((valueStatus='Known' AND valueDecimalText IS NOT NULL) OR (valueStatus='Missing' AND valueDecimalText IS NULL)),
 CHECK(status<>'Confirmed' OR (reviewerSid IS NOT NULL AND reviewedUtc IS NOT NULL))
);

CREATE TABLE IF NOT EXISTS FactEvidence (
 factVersionId TEXT NOT NULL REFERENCES FactVersion(id),
 evidenceId TEXT NOT NULL REFERENCES EvidenceRef(id),
 PRIMARY KEY(factVersionId,evidenceId)
);

CREATE TABLE IF NOT EXISTS DedupDecisionVersion (
 id TEXT NOT NULL PRIMARY KEY,
 logicalId TEXT NOT NULL,
 versionNo INTEGER NOT NULL,
 canonicalFactVersionId TEXT REFERENCES FactVersion(id),
 state TEXT NOT NULL CHECK(state IN ('Resolved','Unresolved')),
 reason TEXT NOT NULL,
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(logicalId,versionNo),
 CHECK(state<>'Resolved' OR canonicalFactVersionId IS NOT NULL)
);

CREATE TABLE IF NOT EXISTS DedupMember (
 decisionVersionId TEXT NOT NULL REFERENCES DedupDecisionVersion(id),
 factVersionId TEXT NOT NULL REFERENCES FactVersion(id),
 role TEXT NOT NULL CHECK(role IN ('Canonical','Equivalent','Excluded','Unresolved')),
 PRIMARY KEY(decisionVersionId,factVersionId)
);

CREATE TABLE IF NOT EXISTS AggregationRun (
 id TEXT NOT NULL PRIMARY KEY,
 metricVersionId TEXT NOT NULL REFERENCES MetricDefinitionVersion(id),
 queryJson TEXT NOT NULL,
 resultJson TEXT NOT NULL,
 snapshotHash TEXT NOT NULL,
 state TEXT NOT NULL CHECK(state IN ('Valid','NeedsReview','Failed')),
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS AggregationContribution (
 aggregationId TEXT NOT NULL REFERENCES AggregationRun(id),
 factVersionId TEXT NOT NULL REFERENCES FactVersion(id),
 groupKey TEXT NOT NULL,
 operation TEXT NOT NULL,
 contributionDecimalText TEXT,
 dedupDecisionVersionId TEXT REFERENCES DedupDecisionVersion(id),
 PRIMARY KEY(aggregationId,groupKey,factVersionId,operation)
);

CREATE TABLE IF NOT EXISTS Report (
 id TEXT NOT NULL PRIMARY KEY,
 name TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ReportRevision (
 id TEXT NOT NULL PRIMARY KEY,
 reportId TEXT NOT NULL REFERENCES Report(id),
 versionNo INTEGER NOT NULL,
 templateVersionId TEXT NOT NULL REFERENCES TemplateVersion(id),
 snapshotJson TEXT NOT NULL,
 state TEXT NOT NULL CHECK(state IN ('Draft','Reviewed')),
 isStale INTEGER NOT NULL DEFAULT 0 CHECK(isStale IN (0,1)),
 reviewerSid TEXT,
 reviewedUtc TEXT,
 checklistJson TEXT,
 createdUtc TEXT NOT NULL,
 UNIQUE(reportId,versionNo),
 CHECK(state<>'Reviewed' OR (reviewerSid IS NOT NULL AND reviewedUtc IS NOT NULL AND checklistJson IS NOT NULL))
);

CREATE TABLE IF NOT EXISTS ReportSection (
 id TEXT NOT NULL PRIMARY KEY,
 reportRevisionId TEXT NOT NULL REFERENCES ReportRevision(id),
 sectionKey TEXT NOT NULL,
 ordinal INTEGER NOT NULL,
 text TEXT NOT NULL,
 UNIQUE(reportRevisionId,sectionKey)
);

CREATE TABLE IF NOT EXISTS Claim (
 id TEXT NOT NULL PRIMARY KEY,
 sectionId TEXT NOT NULL REFERENCES ReportSection(id),
 kind TEXT NOT NULL CHECK(kind IN ('Factual','Commentary')),
 text TEXT NOT NULL,
 checked INTEGER NOT NULL DEFAULT 0 CHECK(checked IN (0,1)),
 commentaryReason TEXT,
 CHECK(kind<>'Commentary' OR commentaryReason IS NOT NULL)
);

CREATE TABLE IF NOT EXISTS ClaimEvidence (
 claimId TEXT NOT NULL REFERENCES Claim(id),
 evidenceId TEXT NOT NULL REFERENCES EvidenceRef(id),
 PRIMARY KEY(claimId,evidenceId)
);

CREATE TABLE IF NOT EXISTS ReportSource (
 reportRevisionId TEXT NOT NULL REFERENCES ReportRevision(id),
 sourceRevisionId TEXT NOT NULL REFERENCES TextRevision(id),
 sourceHash TEXT NOT NULL,
 PRIMARY KEY(reportRevisionId,sourceRevisionId)
);

CREATE TABLE IF NOT EXISTS ExportRecord (
 id TEXT NOT NULL PRIMARY KEY,
 reportRevisionId TEXT NOT NULL REFERENCES ReportRevision(id),
 mode TEXT NOT NULL CHECK(mode IN ('Draft','Submission')),
 outputHash TEXT NOT NULL,
 outputPath TEXT NOT NULL,
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Script (
 id TEXT NOT NULL PRIMARY KEY,
 name TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ScriptVersion (
 id TEXT NOT NULL PRIMARY KEY,
 scriptId TEXT NOT NULL REFERENCES Script(id),
 versionNo INTEGER NOT NULL,
 state TEXT NOT NULL CHECK(state IN ('Draft','Published','Retired')),
 profileJson TEXT NOT NULL,
 contentHash TEXT NOT NULL,
 mappingVersionId TEXT REFERENCES MappingVersion(id),
 createdUtc TEXT NOT NULL,
 UNIQUE(scriptId,versionNo)
);

CREATE TABLE IF NOT EXISTS ScriptStep (
 id TEXT NOT NULL PRIMARY KEY,
 scriptVersionId TEXT NOT NULL REFERENCES ScriptVersion(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>0),
 action TEXT NOT NULL CHECK(action IN ('WaitWindow','WaitElement','Invoke','SetValue','Click','DoubleClick','Scroll','Move','KeyChord','TypeVariable','Assert','PauseForUser')),
 selectorJson TEXT,
 paramsJson TEXT NOT NULL,
 timeoutMs INTEGER NOT NULL CHECK(timeoutMs>0 AND timeoutMs<=180000),
 retryPolicy TEXT NOT NULL CHECK(retryPolicy IN ('None','ReadOnlyBounded')),
 UNIQUE(scriptVersionId,ordinal)
);

CREATE TABLE IF NOT EXISTS RpaRun (
 id TEXT NOT NULL PRIMARY KEY,
 scriptVersionId TEXT NOT NULL REFERENCES ScriptVersion(id),
 externalAttemptId TEXT REFERENCES ExternalEntryAttempt(id),
 idempotencyKey TEXT NOT NULL UNIQUE,
 inputSnapshotHash TEXT NOT NULL,
 state TEXT NOT NULL CHECK(state IN ('Queued','Running','Paused','AwaitingUser','Succeeded','Failed','Cancelled','Interrupted','NeedsReconciliation')),
 currentStep INTEGER NOT NULL DEFAULT 0,
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS RpaStepRun (
 id TEXT NOT NULL PRIMARY KEY,
 runId TEXT NOT NULL REFERENCES RpaRun(id),
 stepId TEXT NOT NULL REFERENCES ScriptStep(id),
 attemptNo INTEGER NOT NULL CHECK(attemptNo>0),
 state TEXT NOT NULL CHECK(state IN ('Prepared','Dispatched','Verified','Failed','Cancelled','Unknown','AwaitingUser')),
 observedJson TEXT,
 startedUtc TEXT NOT NULL,
 endedUtc TEXT,
 UNIQUE(runId,stepId,attemptNo)
);

CREATE TABLE IF NOT EXISTS ToolVersion (
 id TEXT NOT NULL PRIMARY KEY,
 kind TEXT NOT NULL,
 versionLabel TEXT NOT NULL,
 manifestJson TEXT NOT NULL,
 manifestHash TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ConfigVersion (
 id TEXT NOT NULL PRIMARY KEY,
 kind TEXT NOT NULL,
 versionNo INTEGER NOT NULL,
 payloadJson TEXT NOT NULL,
 contentHash TEXT NOT NULL,
 createdUtc TEXT NOT NULL,
 UNIQUE(kind,versionNo)
);

CREATE TABLE IF NOT EXISTS Job (
 id TEXT NOT NULL PRIMARY KEY,
 kind TEXT NOT NULL,
 entityId TEXT NOT NULL,
 idempotencyKey TEXT NOT NULL UNIQUE,
 state TEXT NOT NULL CHECK(state IN ('Queued','Running','Partial','Succeeded','Failed','Cancelled','Interrupted')),
 checkpointJson TEXT,
 errorCode TEXT,
 createdUtc TEXT NOT NULL,
 updatedUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS AuditEvent (
 id TEXT NOT NULL PRIMARY KEY,
 sequenceNo INTEGER NOT NULL UNIQUE,
 actorSid TEXT NOT NULL,
 action TEXT NOT NULL,
 objectType TEXT NOT NULL,
 objectId TEXT NOT NULL,
 revisionId TEXT,
 resultCode TEXT NOT NULL,
 correlationId TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS BackupRecord (
 id TEXT NOT NULL PRIMARY KEY,
 manifestHash TEXT NOT NULL,
 relativeOrSelectedPath TEXT NOT NULL,
 state TEXT NOT NULL CHECK(state IN ('Creating','Verified','Failed','Cancelled')),
 drillState TEXT NOT NULL CHECK(drillState IN ('NotRun','Passed','Failed')),
 actorSid TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS AiInvocation (
 id TEXT NOT NULL PRIMARY KEY,
 configVersionId TEXT NOT NULL REFERENCES ConfigVersion(id),
 modelHash TEXT NOT NULL,
 sourceIdsJson TEXT NOT NULL,
 inputTokens INTEGER NOT NULL CHECK(inputTokens>=0),
 outputTokens INTEGER NOT NULL CHECK(outputTokens>=0),
 state TEXT NOT NULL,
 createdUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ReportAggregation (
 reportRevisionId TEXT NOT NULL REFERENCES ReportRevision(id),
 aggregationId TEXT NOT NULL REFERENCES AggregationRun(id),
 PRIMARY KEY(reportRevisionId,aggregationId)
);

CREATE INDEX IF NOT EXISTS IX_Document_Dossier ON Document(dossierId);
CREATE INDEX IF NOT EXISTS IX_TextRevision_Document ON TextRevision(documentId,versionNo);
CREATE INDEX IF NOT EXISTS IX_Evidence_Block ON EvidenceRef(revisionId,blockId);
CREATE INDEX IF NOT EXISTS IX_Task_Due ON TaskItem(state,dueDate,dueUtc);
CREATE INDEX IF NOT EXISTS IX_Workflow_Dossier ON WorkflowEvent(dossierId,sequenceNo);
CREATE INDEX IF NOT EXISTS IX_Fact_Metric ON FactVersion(metricVersionId,status,periodStart,periodEnd);
CREATE INDEX IF NOT EXISTS IX_Run_State ON RpaRun(state,createdUtc);
CREATE INDEX IF NOT EXISTS IX_Audit_Filter ON AuditEvent(createdUtc,actorSid,action);

CREATE VIRTUAL TABLE IF NOT EXISTS TextSearch USING fts5(
 documentId UNINDEXED,
 revisionId UNINDEXED,
 blockId UNINDEXED,
 rawTextNfc UNINDEXED,
 searchText
);

CREATE TABLE IF NOT EXISTS CorrespondenceCase (
 id TEXT PRIMARY KEY, documentId TEXT NOT NULL UNIQUE REFERENCES Document(id),
 payloadJson TEXT NOT NULL, rowVersion INTEGER NOT NULL CHECK(rowVersion>0)
);
CREATE TABLE IF NOT EXISTS CorrespondenceCaseEvent (
 id TEXT PRIMARY KEY, caseId TEXT NOT NULL REFERENCES CorrespondenceCase(id),
 action TEXT NOT NULL, actor TEXT NOT NULL, atUtc TEXT NOT NULL, note TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS CorrespondenceCaseImport (
 originWorkspaceId TEXT NOT NULL, originCaseId TEXT NOT NULL, originVersion INTEGER NOT NULL,
 contentHash TEXT NOT NULL, localCaseId TEXT NOT NULL REFERENCES CorrespondenceCase(id),
 PRIMARY KEY(originWorkspaceId,originCaseId)
);
";

        public static void InitializeDatabase(SqliteConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            using var cmdPragma = connection.CreateCommand();
            cmdPragma.CommandText = "PRAGMA foreign_keys=ON;";
            cmdPragma.ExecuteNonQuery();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = DdlScript;
            cmd.ExecuteNonQuery();
        }
    }
}
