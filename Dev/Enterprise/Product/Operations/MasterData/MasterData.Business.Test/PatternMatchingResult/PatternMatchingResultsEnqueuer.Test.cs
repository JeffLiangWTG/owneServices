using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingResultsEnqueuerTest : TestCaseWithFactory
	{
		public void TestQueueOrgHeaderForDeduplicationProcessing()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var existingResultQuery = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, org.TablePrefix)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, new string[] { PatternMatchingResult.StatusCodes.Queued, PatternMatchingResult.StatusCodes.QueuedForProcessing });

			PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(org, Factory);

			var queryResults = Factory.Load<PatternMatchingResult>(existingResultQuery);
			AssertEquals(1, queryResults.Length);
			AssertEquals(User.ServiceUserCode, queryResults[0].PMT_GS_NKExcludeBy);
			AssertEquals(PatternMatchingResult.StatusCodes.Queued, queryResults[0].PMT_Status);

			AssertNoExceptionThrown(() => PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(org, Factory));
			AssertEquals("Queuing method should not create a new record", 1, Factory.Load<PatternMatchingResult>(existingResultQuery).Length);
		}

		public void TestQueueOrgHeaderForDeduplicationProcessingWhenQUPExisted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var existingQUPRecord = Factory.New<PatternMatchingResult>();
			existingQUPRecord.PMT_MasterPK = org.PK;
			existingQUPRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			existingQUPRecord.PMT_GS_NKExcludeBy = User.ServiceUserCode;
			existingQUPRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			existingQUPRecord.PMT_FoundTimeUtc = DateTime.Today;

			Factory.Save();

			var existingResultQuery = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, org.TablePrefix)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, new string[] { PatternMatchingResult.StatusCodes.Queued, PatternMatchingResult.StatusCodes.QueuedForProcessing });

			PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(org, Factory);

			var queryResults = Factory.Load<PatternMatchingResult>(existingResultQuery);
			AssertEquals(1, queryResults.Length);
			AssertEquals(User.ServiceUserCode, queryResults[0].PMT_GS_NKExcludeBy);
			AssertEquals(PatternMatchingResult.StatusCodes.Queued, queryResults[0].PMT_Status);
		}

		public void TestQueueOrgHeaderForDeduplicationProcessingWhenQUEExisted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var existingQUPRecord = Factory.New<PatternMatchingResult>();
			existingQUPRecord.PMT_MasterPK = org.PK;
			existingQUPRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			existingQUPRecord.PMT_GS_NKExcludeBy = User.ServiceUserCode;
			existingQUPRecord.PMT_Status = PatternMatchingResult.StatusCodes.Queued;
			existingQUPRecord.PMT_FoundTimeUtc = DateTime.Today;

			Factory.Save();

			var existingResultQuery = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, org.TablePrefix)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, new string[] { PatternMatchingResult.StatusCodes.Queued, PatternMatchingResult.StatusCodes.QueuedForProcessing });

			PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(org, Factory);

			var queryResults = Factory.Load<PatternMatchingResult>(existingResultQuery);
			AssertEquals(1, queryResults.Length);
			AssertEquals(User.ServiceUserCode, queryResults[0].PMT_GS_NKExcludeBy);
			AssertEquals(PatternMatchingResult.StatusCodes.Queued, queryResults[0].PMT_Status);
		}

		public void TestQueueGlbPersonForDeduplicationProcessing()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			var existingResultQuery = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, person.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, person.TablePrefix)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, new string[] { PatternMatchingResult.StatusCodes.Queued, PatternMatchingResult.StatusCodes.QueuedForProcessing });

			PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(person, Factory);

			var queryResults = Factory.Load<PatternMatchingResult>(existingResultQuery);
			AssertEquals(1, queryResults.Length);
			AssertEquals(User.ServiceUserCode, queryResults[0].PMT_GS_NKExcludeBy);
			AssertEquals(PatternMatchingResult.StatusCodes.QueuedForProcessing, queryResults[0].PMT_Status);

			AssertNoExceptionThrown(() => PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(person, Factory));
			AssertEquals("Queuing method should not create a new record", 1, Factory.Load<PatternMatchingResult>(existingResultQuery).Length);
		}

		public void TestQueueGlbPersonForDeduplicationProcessingWhenQUPExisted()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var existingQUPRecord = Factory.New<PatternMatchingResult>();
			existingQUPRecord.PMT_MasterPK = person.PK;
			existingQUPRecord.PMT_MasterTableCode = GlbPersonSchema.Constants.Prefix;
			existingQUPRecord.PMT_GS_NKExcludeBy = User.ServiceUserCode;
			existingQUPRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			existingQUPRecord.PMT_FoundTimeUtc = DateTime.Today;

			Factory.Save();

			var existingResultQuery = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, person.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, person.TablePrefix)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, new string[] { PatternMatchingResult.StatusCodes.Queued, PatternMatchingResult.StatusCodes.QueuedForProcessing });

			PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(person, Factory);

			var queryResults = Factory.Load<PatternMatchingResult>(existingResultQuery);
			AssertEquals(1, queryResults.Length);
			AssertEquals(User.ServiceUserCode, queryResults[0].PMT_GS_NKExcludeBy);
			AssertEquals(PatternMatchingResult.StatusCodes.QueuedForProcessing, queryResults[0].PMT_Status);
		}

		public void TestQueueMasterForDeduplicationNotProcessingWhenMasterPkIsLocked()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection.TryGetLock((PatternMatchingConstants.AppLockKey + "," + org.PK.ToString()).ToUpperInvariant(), out var appLock));
				using (appLock)
				{
					PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(org, Factory);
					AssertEquals(0, Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK)).Length);
				}
			}
		}

		public void TestEnqueuerWillNotTriggerDuplicateKeyRowError()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var exsitingPatternMatchingResult = Factory.New<PatternMatchingResult>();
			exsitingPatternMatchingResult.PMT_MasterPK = org.PK;
			exsitingPatternMatchingResult.PMT_GS_NKExcludeBy = User.ServiceUserCode;
			exsitingPatternMatchingResult.PMT_FoundTimeUtc = DateTime.Now;
			exsitingPatternMatchingResult.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
			exsitingPatternMatchingResult.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;

			Factory.Save();

			PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(org, Factory);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestEnqueuerWillSaveQueuedRecordWithoutSavingCurrentFactory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK));
			AssertEquals(0, patternMatchingResults.Length);

			PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(org, Factory);

			Assert(!org.IsInDatabase);

			patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK));
			AssertEquals(1, patternMatchingResults.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.Queued, patternMatchingResults[0].PMT_Status);

			var person = Factory.NewWithValidTestData<GlbPerson>();

			patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, person.PK));
			AssertEquals(0, patternMatchingResults.Length);

			PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(person, Factory);

			Assert(!person.IsInDatabase);

			patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, person.PK));
			AssertEquals(1, patternMatchingResults.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.QueuedForProcessing, patternMatchingResults[0].PMT_Status);
		}
	}
}
