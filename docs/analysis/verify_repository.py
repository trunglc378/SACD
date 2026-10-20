"""Read-only repository review; SQLite fixtures run only in memory.

This does not build or execute SACD/.NET. Results describe documentation,
historical TRX evidence, and SQL behavior on Python's SQLite runtime.
Run: python -X utf8 docs/analysis/verify_repository.py
"""
import json
import re
import sqlite3
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
BASELINE = "293323cd020800232d9102ace9d6c90ac6fa0b64"


def read(relative):
    return subprocess.check_output(["git", "show", BASELINE + ":" + relative], cwd=str(ROOT)).decode("utf-8-sig")


def main():
    result = {
        "scope": "Historical baseline only; current behavior is verified by .NET regression tests. In-memory SQL probes, not C# execution.",
        "commit": BASELINE,
        "sqlite_runtime": sqlite3.sqlite_version,
    }
    markdown_files = [ROOT/p for p in subprocess.check_output(["git", "ls-tree", "-r", "--name-only", BASELINE, "--", "docs"], cwd=str(ROOT), text=True).splitlines() if p.endswith(".md")]
    missing = []
    # Scan original documentation, not the new review report itself.
    markdown_files = [p for p in markdown_files if "analysis" not in p.relative_to(ROOT).parts]
    for path in markdown_files:
        for match in re.finditer(r"\[[^\]]*\]\(([^)]+)\)", read(path.relative_to(ROOT).as_posix())):
            target = match.group(1).strip().strip("<>")
            if re.match(r"^[a-zA-Z][a-zA-Z0-9+.-]*:", target) or target.startswith("#"):
                continue
            target = unquote(target.split("#", 1)[0])
            original_target = (path.parent / target).resolve().relative_to(ROOT).as_posix()
            available = subprocess.run(["git", "cat-file", "-e", BASELINE + ":" + original_target], cwd=str(ROOT), capture_output=True).returncode == 0
            if target and not available:
                missing.append({"source": path.relative_to(ROOT).as_posix(), "target": target})
    result["documentation"] = {
        "markdown_files_in_docs_excluding_analysis": len(markdown_files),
        "missing_relative_link_occurrences": len(missing),
        "missing_targets_unique": sorted(set(x["target"] for x in missing)),
        "missing_links": missing,
    }
    result["xunit_fact_methods_in_source"] = sum(
        len(re.findall(r"\[Fact\]", read(p)))
        for p in subprocess.check_output(["git", "ls-tree", "-r", "--name-only", BASELINE, "--", "tests/SACD.Tests"], cwd=str(ROOT), text=True).splitlines() if p.endswith(".cs")
    )
    trx_path = ROOT / "TestResults/SACD.Tests.trx"
    trx = ET.parse(str(trx_path)).getroot()
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    counters = trx.find("t:ResultSummary/t:Counters", ns)
    times = trx.find("t:Times", ns)
    result["historical_trx"] = {
        "file": trx_path.relative_to(ROOT).as_posix(),
        "counters": counters.attrib if counters is not None else None,
        "times": times.attrib if times is not None else None,
        "fresh_run": False,
    }

    source = read("src/SACD.Infrastructure/Persistence/DatabaseInitializer.cs")
    ddl = re.search(r'public const string DdlScript = @"(.*?)";', source, re.S).group(1)
    ddl = ddl.replace('""', '"')
    db = sqlite3.connect(":memory:")
    db.executescript(ddl)
    tables = [r[0] for r in db.execute("SELECT name FROM sqlite_master WHERE type='table'")]
    declared_tables = re.findall(r"CREATE TABLE IF NOT EXISTS (\w+)", ddl)
    result["schema"] = {
        "declared_business_tables": len(declared_tables),
        "actual_tables_including_fts_and_shadow": len(tables),
        "integrity_check": db.execute("PRAGMA integrity_check").fetchone()[0],
        "foreign_keys": db.execute("PRAGMA foreign_keys").fetchone()[0],
    }
    db.executescript("""
INSERT INTO FileObject VALUES ('aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa', 'objects/a', 1, 'text/plain', '2026-10-20T00:00:00Z');
INSERT INTO Dossier(id,businessCode,state,receivedUtc) VALUES ('dos','HS-TEST','Received','2026-10-20T00:00:00Z');
INSERT INTO Document(id,dossierId,objectHash,originalName,sourceKind,status,createdUtc) VALUES ('doc','dos','aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa','test.txt','File','Imported','2026-10-20T00:00:00Z');
INSERT INTO TextRevision(id,documentId,versionNo,method,reviewState,createdUtc) VALUES ('rev-old','doc',1,'Extract','NeedsReview','2026-10-20T00:00:00Z'),('rev-new','doc',2,'Extract','NeedsReview','2026-10-20T00:00:00Z');
UPDATE Document SET currentRevisionId='rev-new' WHERE id='doc';
INSERT INTO TextSearch VALUES ('doc','rev-old','block-old','obsoleteword','obsoleteword'),('doc','rev-new','block-new','currentword','currentword');
INSERT INTO MetricDefinitionVersion(id,logicalId,versionNo,name,unitCode,kind,ruleJson,createdUtc) VALUES ('metric','METRIC',1,'Test','DOC','Stock','{}','2026-10-20T00:00:00Z');
INSERT INTO FactVersion(id,logicalId,versionNo,metricVersionId,sourceRevisionId,valueStatus,valueDecimalText,periodStart,periodEnd,periodKind,scopeJson,status,reviewerSid,reviewedUtc,createdUtc) VALUES ('fact-old','F-OLD',1,'metric','rev-new','Known','10','2026-10-01','2026-10-10','Month','{}','Confirmed','TEST','2026-10-20T00:00:00Z','2026-10-20T00:00:00Z'),('fact-new','F-NEW',1,'metric','rev-new','Known','25','2026-10-01','2026-10-20','Month','{}','Confirmed','TEST','2026-10-20T00:00:00Z','2026-10-20T00:00:00Z');
INSERT INTO TemplateVersion(id,logicalId,versionNo,objectHash,mappingJson,mappingHash,createdUtc) VALUES ('tpl','TPL',1,'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa','{}','test','2026-10-20T00:00:00Z');
INSERT INTO Report VALUES ('report','Test report','2026-10-20T00:00:00Z');
INSERT INTO ReportRevision(id,reportId,versionNo,templateVersionId,snapshotJson,state,isStale,createdUtc) VALUES ('report-rev','report',1,'tpl','{}','Draft',0,'2026-10-20T00:00:00Z');
INSERT INTO ReportSection VALUES ('section','report-rev','I',1,'Test');
INSERT INTO Claim(id,sectionId,kind,text,checked) VALUES ('claim','section','Factual','Test claim',0);
""")
    services = read("src/SACD.Infrastructure/Services/ServicesPart2.cs")
    search_region = services.split("public class SearchService", 1)[1].split("public async Task<OperationResult<HistoryPage>>", 1)[0]
    search_sql = re.search(r'cmd.CommandText = @"(.*?)";', search_region, re.S).group(1)
    old_hits = db.execute(search_sql, {"keyword": '"obsoleteword"*', "limit": 20, "offset": 0}).fetchall()
    assert len(old_hits) == 1 and old_hits[0][-1] == 1
    review_region = services.split("public async Task<OperationResult<ReportRevisionInfo>> ReviewAsync", 1)[1].split("public async Task<OperationResult<ExportInfo>>", 1)[0]
    review_sql = re.search(r'cmd.CommandText = @"(.*?)";', review_region, re.S).group(1)
    db.execute(review_sql, {"sid": "TEST", "now": "2026-10-20T00:00:00Z", "chk": "{}", "id": "report-rev"})
    reviewed = db.execute("SELECT state,isStale FROM ReportRevision WHERE id='report-rev'").fetchone()
    checked = db.execute("SELECT checked FROM Claim WHERE id='claim'").fetchone()[0]
    assert reviewed == ("Reviewed", 0) and checked == 0
    aggregate_region = services.split("public async Task<OperationResult<AggregateResult>> AggregateAsync", 1)[1].split("// CMP-13", 1)[0]
    stock_sql = re.search(r'cmd.CommandText = "(SELECT valueDecimalText.*?)";', aggregate_region, re.S).group(1)
    old_first = [db.execute(stock_sql, {"id": fid}).fetchone()[0] for fid in ["fact-old", "fact-new"]]
    new_first = [db.execute(stock_sql, {"id": fid}).fetchone()[0] for fid in ["fact-new", "fact-old"]]
    assert old_first[0] != new_first[0]
    result["sql_probes"] = {
        "source_search_query_returns_obsolete_revision": {"hits": len(old_hits), "returned_revision": old_hits[0][-1], "current_revision": 2},
        "source_review_update_allows_unchecked_claim": {"state": reviewed[0], "is_stale": reviewed[1], "claim_checked": checked, "claim_evidence_count": db.execute("SELECT count(*) FROM ClaimEvidence").fetchone()[0]},
        "confirmed_facts_without_evidence_allowed_by_schema": db.execute("SELECT count(*) FROM FactVersion f WHERE status='Confirmed' AND NOT EXISTS(SELECT 1 FROM FactEvidence e WHERE e.factVersionId=f.id)").fetchone()[0],
        "source_stock_selection_depends_on_request_order": {"old_then_new_selected": old_first[0], "new_then_old_selected": new_first[0], "expected_latest": "25"},
        "foreign_key_check_after_fixtures": list(db.execute("PRAGMA foreign_key_check")),
    }
    db.close()
    output = ROOT / "docs/analysis/2026-10-20-verification.json"
    output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "documentation"}, ensure_ascii=False, indent=2))
    print("Missing relative links:", len(missing), "; see", output)


if __name__ == "__main__":
    main()
