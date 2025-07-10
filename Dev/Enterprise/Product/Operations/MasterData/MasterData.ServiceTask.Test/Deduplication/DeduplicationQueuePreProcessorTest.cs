using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterData.ServiceTask.Deduplication;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterData.ServiceTask.Test
{
	public class DeduplicationQueuePreProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			var sql = "TRUNCATE TABLE PatternMatchingResult";
			Db.Connection.ExecuteNonQuery(sql);
		}

		public void TestNonDirtyRecords()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var queRecord = Factory.New<PatternMatchingResult>();
			queRecord.PMT_MasterPK = testOrg.PK;
			queRecord.PMT_Status = PatternMatchingResult.StatusCodes.Queued;
			queRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			queRecord.PMT_FoundTimeUtc = DateTime.Today;
			queRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			DeduplicationQueuePreProcessor.Process(new DummyLogger());

			var resultedPMTs = new BusinessObjectFactory().Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testOrg.PK));
			AssertEquals("There should be still one PMT record with this master Org.", 1, resultedPMTs.Length);
			AssertEquals("PMT_Status should be converted to QUP", PatternMatchingResult.StatusCodes.QueuedForProcessing, resultedPMTs[0].PMT_Status);
		}

		public void TestDirtyRecords()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();

			for (int i = 0; i <= 100; i++)
			{
				var contact = testOrg.Contacts.AddNew();
				contact.OC_ContactName = "Brandon" + i;
			}

			var queRecord = Factory.New<PatternMatchingResult>();
			queRecord.PMT_MasterPK = testOrg.PK;
			queRecord.PMT_Status = PatternMatchingResult.StatusCodes.Queued;
			queRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			queRecord.PMT_FoundTimeUtc = DateTime.Today;
			queRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			DeduplicationQueuePreProcessor.Process(new DummyLogger());

			var resultedPMTs = new BusinessObjectFactory().Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testOrg.PK));
			AssertEquals("There should be still one PMT record with this master Org.", 1, resultedPMTs.Length);
			AssertEquals("PMT_Status should be converted to EXC", PatternMatchingResult.StatusCodes.Excluded, resultedPMTs[0].PMT_Status);
		}

		public void TestDirtyRecordsWithExistingEXC()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();

			for (int i = 0; i <= 100; i++)
			{
				var contact = testOrg.Contacts.AddNew();
				contact.OC_ContactName = "Brandon" + i;
			}

			var queRecord = Factory.New<PatternMatchingResult>();
			queRecord.PMT_MasterPK = testOrg.PK;
			queRecord.PMT_Status = PatternMatchingResult.StatusCodes.Queued;
			queRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			queRecord.PMT_FoundTimeUtc = DateTime.Today;
			queRecord.PMT_GS_NKExcludeBy = "~BP";

			var excRecord = Factory.New<PatternMatchingResult>();
			excRecord.PMT_MasterPK = testOrg.PK;
			excRecord.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
			excRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			excRecord.PMT_FoundTimeUtc = DateTime.Today;

			Factory.Save();

			var logger = new TestServiceLogger();
			DeduplicationQueuePreProcessor.Process(logger);

			AssertEquals("No SqlException should be thrown and logged", 0, logger.Count);

			var resultedPMTs = new BusinessObjectFactory().Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testOrg.PK));
			AssertEquals("There should be only one PMT record with this master Org.", 1, resultedPMTs.Length);
			AssertEquals("PMT_Status should be set to EXC", PatternMatchingResult.StatusCodes.Excluded, resultedPMTs[0].PMT_Status);
		}

		public void TestErrorReportingOnSqlException()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var dropColumnSql = @"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE NAME = N'NR_RC__PMT_FoundTimeUtc') DROP INDEX NR_RC__PMT_FoundTimeUtc ON PatternMatchingResult
ALTER TABLE dbo.PatternMatchingResult DROP COLUMN PMT_FoundTimeUtc";
			Db.Connection.ExecuteNonQuery(dropColumnSql);

			var insetQueRecordSql = $@"
INSERT INTO dbo.PatternMatchingResult (PMT_PK, PMT_MasterPK, PMT_MasterTableCode,  PMT_TargetPK, PMT_TargetTableCode, PMT_Status, PMT_GS_NKExcludeBy, PMT_ScorePercent)
VALUES (NEWID(), '{testOrg.PK}', 'OH', NULL, '', 'QUE', '', 0)";
			Db.Connection.ExecuteNonQuery(insetQueRecordSql);

			AssertExceptionThrown<SqlException>(() => DeduplicationQueuePreProcessor.Process(new DummyLogger()));
		}

		public void TestDeleteDirtyPatternMatchingResults()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testPer = Factory.NewWithValidTestData<GlbPerson>();
			var result1 = PatternMatchingResult.CreatePatternMatchingResult(OrgHeaderSchema.Constants.Prefix, testOrg.PK, Factory, PatternMatchingResult.StatusCodes.QueuedForProcessing);
			var result2 = PatternMatchingResult.CreatePatternMatchingResult(GlbPersonSchema.Constants.Prefix, testPer.PK, Factory, PatternMatchingResult.StatusCodes.QueuedForProcessing);
			var result3 = PatternMatchingResult.CreatePatternMatchingResult(OrgHeaderSchema.Constants.Prefix, ZGuid.NewZGuid(), Factory, PatternMatchingResult.StatusCodes.QueuedForProcessing);
			var result4 = PatternMatchingResult.CreatePatternMatchingResult(GlbPersonSchema.Constants.Prefix, ZGuid.NewZGuid(), Factory, PatternMatchingResult.StatusCodes.QueuedForProcessing);
			Factory.Save();

			var logger = new TestServiceLogger();
			DeduplicationQueuePreProcessor.Process(logger);

			CombineAssertions("Dirty Pattern Matching Results Been Deleted", () =>
			{
				AssertEquals(true, Factory.ExistsInDatabase(PatternMatchingResultSchema.Constants.TableName, new ZQuery(PatternMatchingResultSchema.PK, result1.PK)));
				AssertEquals(true, Factory.ExistsInDatabase(PatternMatchingResultSchema.Constants.TableName, new ZQuery(PatternMatchingResultSchema.PK, result2.PK)));
				AssertEquals(false, Factory.ExistsInDatabase(PatternMatchingResultSchema.Constants.TableName, new ZQuery(PatternMatchingResultSchema.PK, result3.PK)));
				AssertEquals(false, Factory.ExistsInDatabase(PatternMatchingResultSchema.Constants.TableName, new ZQuery(PatternMatchingResultSchema.PK, result4.PK)));
			});
		}
	}
}
