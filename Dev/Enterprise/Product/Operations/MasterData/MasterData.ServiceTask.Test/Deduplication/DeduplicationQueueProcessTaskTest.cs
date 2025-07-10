using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterData.ServiceTask.Deduplication;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterData.ServiceTask.Test
{
	[TestedType(typeof(DeduplicationQueueProcessTask))]
	public class DeduplicationQueueProcessTaskTest : ServiceTaskTestCase<DeduplicationQueueProcessTask>
	{
		protected override void SetUpCore()
		{
			var sql = "TRUNCATE TABLE PatternMatchingResult";
			Db.Connection.ExecuteNonQuery(sql);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttribute()
		{
			var attribute = GetHostedServiceAttributes().SingleOrDefault();

			AssertNotNull(attribute);
			AssertEquals(false, attribute.AllowsMultipleInstances);
			AssertEquals(true, attribute.IsMandatory);
			AssertEquals(true, attribute.CanRunInAnyBranch);
			AssertEquals("DQP", attribute.Code);
			AssertEquals("De-duplication Queue Processing Service", attribute.Description);
			AssertEquals("30minutes", attribute.DefaultScheduleRunEvery);
		}

		public void TestInitialiseSchedule()
		{
			var testTask = new DeduplicationQueueProcessTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("ScheduleTask - IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("ScheduleTask - TaskPeriodCount", 30, taskSchedule.Recurrence.Period);
			AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("DailyStartTime - Default", ZDateTime.Empty, taskSchedule.Recurrence.CalcDailyStartTimeUtc);
		}

		public void TestRun_WithOrganisationHasDuplicates()
		{
			// Arrange
			var sourceTable = OrgHeaderSchema.Constants.Prefix;
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();
			var target1PK = Guid.NewGuid();
			var target2PK = Guid.NewGuid();

			var outputs = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, target1PK, 0.99),
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, target2PK, 0.99)
			};

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(masterPK, outputs);
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();
			populaterObject.ServiceLogger = mockLogger.Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));
			AssertEquals(2, factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK)).Length);

			foreach (var output in outputs)
			{
				AssertEquals(1, factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK).AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, output.TargetPK)).Length);
				AssertEquals(1, factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, output.TargetPK).AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, testMaster.PK)).Length);
			}

			mockLogger.Verify(mock => mock.Log(LogType.Information, $"'Potential Duplicate' status has been created for master OH: {masterPK} and target OH: {target1PK}"), Times.Once);
		}

		public void TestRun_WithOrganisationHasNoDuplicate()
		{
			// Arrange
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(masterPK, Enumerable.Empty<DuplicateDetectorProviderOutput>());
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();
			populaterObject.ServiceLogger = mockLogger.Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));

			var queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK));
			AssertEquals(1, queryResult.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.NoDuplicates, queryResult[0].PMT_Status);

			mockLogger.Verify(mock => mock.Log(LogType.Information, $"'No Duplicates' status has been created for OH: {masterPK}"));
		}

		public void TestRun_WithPersonHasDuplicates()
		{
			// Arrange
			var sourceTable = GlbPersonSchema.Constants.Prefix;
			var testMaster = Factory.NewWithValidTestData<GlbPerson>();
			var masterPK = testMaster.PK.ToGuid();

			var outputs = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, Guid.NewGuid(), 0.99),
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, Guid.NewGuid(), 0.99)
			};

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithPersonOutputs(masterPK, outputs);
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			populaterObject.ServiceLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>().Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = GlbPersonSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));
			AssertEquals(2, factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK)).Length);

			foreach (var output in outputs)
			{
				AssertEquals(1, factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK).AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, output.TargetPK)).Length);
				AssertEquals(1, factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, output.TargetPK).AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, testMaster.PK)).Length);
			}
		}

		public void TestRun_WithPersonHasNoDuplicate()
		{
			// Arrange
			var testMaster = Factory.NewWithValidTestData<GlbPerson>();
			var masterPK = testMaster.PK.ToGuid();

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithPersonOutputs(masterPK, Enumerable.Empty<DuplicateDetectorProviderOutput>());
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			populaterObject.ServiceLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>().Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = GlbPersonSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));

			var queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK));
			AssertEquals(1, queryResult.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.NoDuplicates, queryResult[0].PMT_Status);
		}

		public void TestRun_WithExistingNDURecord()
		{
			// Arrange
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(masterPK, Enumerable.Empty<DuplicateDetectorProviderOutput>());
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			populaterObject.ServiceLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>().Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			var nduRecord = Factory.New<PatternMatchingResult>();
			nduRecord.PMT_MasterPK = testMaster.PK;
			nduRecord.PMT_Status = PatternMatchingResult.StatusCodes.NoDuplicates;
			nduRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			nduRecord.PMT_FoundTimeUtc = DateTime.Today;

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));
			AssertEquals(null, factory.Load<PatternMatchingResult>(nduRecord.PK));

			var queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK));
			AssertEquals(1, queryResult.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.NoDuplicates, queryResult[0].PMT_Status);
		}

		public void TestRun_WithExistingPDURecord()
		{
			// Arrange
			var sourceTable = GlbPersonSchema.Constants.Prefix;
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();
			var targetPK = Guid.NewGuid();

			var outputs = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, targetPK, 0.99),
			};

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(masterPK, outputs);
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			populaterObject.ServiceLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>().Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			var pduRecord = Factory.New<PatternMatchingResult>();
			pduRecord.PMT_MasterPK = testMaster.PK;
			pduRecord.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
			pduRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			pduRecord.PMT_FoundTimeUtc = DateTime.Today;
			pduRecord.PMT_TargetTableCode = OrgHeaderSchema.Constants.Prefix;
			pduRecord.PMT_TargetPK = targetPK;
			pduRecord.PMT_ScorePercent = 99;

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));
			AssertEquals(null, factory.Load<PatternMatchingResult>(pduRecord.PK));

			var queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK));
			AssertEquals(1, queryResult.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.PotentialDuplicate, queryResult[0].PMT_Status);
			AssertEquals(targetPK, queryResult[0].PMT_TargetPK);

			queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, targetPK));
			AssertEquals(1, queryResult.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.PotentialDuplicate, queryResult[0].PMT_Status);
			AssertEquals(testMaster.PK, queryResult[0].PMT_TargetPK);
		}

		public void TestRun_DoesNotIncludeLockedBizOInProcessing()
		{
			var sourceTable = OrgHeaderSchema.Constants.Prefix;
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var masterPK = org.PK.ToGuid();
			var targetPK = Guid.NewGuid();

			var outputs = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, targetPK, 0.99),
			};

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(masterPK, outputs);
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			populaterObject.ServiceLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>().Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = org.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			PatternMatchingResult[] CheckPatternMatchingResult()
			{
				var query = new ZQuery(PatternMatchingResultSchema.PMT_TargetPK, targetPK);

				query.AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PotentialDuplicate);

				return Factory.Load<PatternMatchingResult>(query);
			}

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection.TryGetLock((PatternMatchingConstants.AppLockKey + "," + org.PK.ToString()).ToUpperInvariant(), out var appLock));
				using (appLock)
				{
					// Act
					populaterObject.RunTask(CancellationToken.None);

					// Assert
					AssertEquals(0, CheckPatternMatchingResult().Length);
				}

				// Act
				populaterObject.RunTask(CancellationToken.None);

				// Assert
				AssertEquals(1, CheckPatternMatchingResult().Length);
			}
		}

		public void TestRun_WithTimeout()
		{
			// Arrange
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(masterPK, Enumerable.Empty<DuplicateDetectorProviderOutput>());
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();

			populaterObject.ServiceLogger = mockLogger.Object;
			mockProviderNoDuplicates.Setup(x => x.LastRunStatus).Returns(DuplicationStatus.Timeout);

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));

			var queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK));
			AssertEquals(1, queryResult.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.Error, queryResult[0].PMT_Status);

			mockLogger.Verify(mock => mock.Log(LogType.Warning, $"Duplicate detection for OH: {masterPK} took longer than expected"));
		}

		public void TestRun_WithUnexpectedException()
		{
			// Arrange
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(masterPK, Enumerable.Empty<DuplicateDetectorProviderOutput>());
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();

			populaterObject.ServiceLogger = mockLogger.Object;
			mockProviderNoDuplicates.Setup(x => x.LastRunStatus).Returns(DuplicationStatus.ErrorOccurred);

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));

			var queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK));
			AssertEquals(1, queryResult.Length);
			AssertEquals(PatternMatchingResult.StatusCodes.Error, queryResult[0].PMT_Status);

			mockLogger.Verify(mock => mock.Log(LogType.Warning, $"An error occurred while duplicate detection for OH: {masterPK}"));
		}

		public void TestRun_WithError()
		{
			// Arrange
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().ThrowsException(masterPK, new Exception());
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();

			populaterObject.ServiceLogger = mockLogger.Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			var queryResult = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, testMaster.PK));
			AssertEquals(0, queryResult.Length);

			mockLogger.Verify(mock => mock.Log(LogType.Warning, $"De-duplication error caused with OH: {masterPK}"));
			AssertStartsWith("Expect DeveloperNotificationException", "Unhandled error", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestRun_WithPairOfPotentialDuplicatesInSameBatch()
		{
			// Arrange
			var sourceTable = OrgHeaderSchema.Constants.Prefix;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org1PK = org1.PK.ToGuid();
			var org2PK = org2.PK.ToGuid();

			var outputs1 = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, org1PK, sourceTable, org2PK, 0.99),
			};

			var outputs2 = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, org2PK, sourceTable, org1PK, 0.99),
			};

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(org1PK, outputs1);
			mockProviderNoDuplicates.WithOutputs(org2PK, outputs2);
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();
			populaterObject.ServiceLogger = mockLogger.Object;

			var qupRecord1 = Factory.New<PatternMatchingResult>();
			qupRecord1.PMT_MasterPK = org1.PK;
			qupRecord1.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord1.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord1.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord1.PMT_GS_NKExcludeBy = "~BP";

			var qupRecord2 = Factory.New<PatternMatchingResult>();
			qupRecord2.PMT_MasterPK = org2.PK;
			qupRecord2.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord2.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord2.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord2.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord1.PK));
			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord2.PK));

			var org1Result = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org1.PK));
			AssertEquals(1, org1Result.Length);
			AssertEquals(org1Result[0].PMT_TargetPK, org2.PK);

			var org2Result = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org2.PK));
			AssertEquals(1, org2Result.Length);
			AssertEquals(org2Result[0].PMT_TargetPK, org1.PK);
		}

		public void TestRun_WithPotentialDuplicatesFoundAndTargetHasNoDuplicationBefore()
		{
			var sourceTable = OrgHeaderSchema.Constants.Prefix;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org1PK = org1.PK.ToGuid();
			var org2PK = org2.PK.ToGuid();

			var outputs = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, org1PK, sourceTable, org2PK, 0.99),
			};

			var mockProviderNoDuplicates = DuplicateDetectorProviderMockBuilder.New<IDuplicateDetectorProvider>().WithOutputs(org1PK, outputs);
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates.Object);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();
			populaterObject.ServiceLogger = mockLogger.Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = org1.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			var nduRecord = Factory.New<PatternMatchingResult>();
			nduRecord.PMT_MasterPK = org2.PK;
			nduRecord.PMT_Status = PatternMatchingResult.StatusCodes.NoDuplicates;
			nduRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			nduRecord.PMT_FoundTimeUtc = DateTime.Today;

			Factory.Save();

			// Act
			populaterObject.RunTask(CancellationToken.None);

			// Assert
			var factory = new BusinessObjectFactory();

			AssertEquals(null, factory.Load<PatternMatchingResult>(qupRecord.PK));
			AssertEquals(null, factory.Load<PatternMatchingResult>(nduRecord.PK));

			var org1Result = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org1.PK));
			AssertEquals(1, org1Result.Length);
			AssertEquals(org1Result[0].PMT_TargetPK, org2.PK);
			AssertEquals(org1Result[0].PMT_Status, PatternMatchingResult.StatusCodes.PotentialDuplicate);

			var org2Result = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org2.PK));
			AssertEquals(1, org2Result.Length);
			AssertEquals(org2Result[0].PMT_TargetPK, org1.PK);
			AssertEquals(org2Result[0].PMT_Status, PatternMatchingResult.StatusCodes.PotentialDuplicate);
		}

		public void TestRun_NotLoadAllPatternMatchingResults()
		{
			var batchSize = OrganisationsDataRegistry.Instance.QueueProcessingBatchSize.Value;
			var batcher1 = new DeduplicationQueueProcessBatcherForTest<OrgHeader>(null, null, OrgHeaderSchema.PK, OrgHeaderSchema.Constants.Prefix);
			var query = batcher1.GetQueryExposed();
			AssertContains($"OH_PK IN (SELECT TOP {batchSize} PMT_MasterPK FROM dbo.PatternMatchingResult WHERE PMT_Status = 'QUP' /* Parameterised value literalised by ZNonPersistentDataQuery */ AND PMT_MasterTableCode = @MasterTableCode)", query.GetAsCompleteSQLStatement(OrgHeaderSchema.Constants.TableName, false));
			AssertContains("PMT_MasterTableCode = 'OH'", query.LiteralTextSqlFormatted);

			var batcher2 = new DeduplicationQueueProcessBatcherForTest<GlbPerson>(null, null, GlbPersonSchema.PK, GlbPersonSchema.Constants.Prefix);
			query = batcher2.GetQueryExposed();
			AssertContains($"PER_PK IN (SELECT TOP {batchSize} PMT_MasterPK FROM dbo.PatternMatchingResult WHERE PMT_Status = 'QUP' /* Parameterised value literalised by ZNonPersistentDataQuery */ AND PMT_MasterTableCode = @MasterTableCode)", query.GetAsCompleteSQLStatement(GlbPersonSchema.Constants.TableName, false));
			AssertContains("PMT_MasterTableCode = 'PER'", query.LiteralTextSqlFormatted);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCanRunInAnyBranch()
		{
			var initialUserContext = Env.CurrentUserContext;
			var sourceTable = OrgHeaderSchema.Constants.Prefix;
			var testMaster = Factory.NewWithValidTestData<OrgHeader>();
			var masterPK = testMaster.PK.ToGuid();
			var target1PK = Guid.NewGuid();
			var target2PK = Guid.NewGuid();

			var outputs = new List<DuplicateDetectorProviderOutput>
			{
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, target1PK, 0.99),
				new DuplicateDetectorProviderOutput(sourceTable, masterPK, sourceTable, target2PK, 0.99)
			};

			var mockProviderNoDuplicates = new DuplicateDetectorProviderWithThresholdOverrides();
			var populaterObject = new DeduplicationQueueProcessTask(mockProviderNoDuplicates);
			var mockLogger = DuplicateDetectorProviderMockBuilder.New<ILogger>();
			populaterObject.ServiceLogger = mockLogger.Object;

			var qupRecord = Factory.New<PatternMatchingResult>();
			qupRecord.PMT_MasterPK = testMaster.PK;
			qupRecord.PMT_Status = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			qupRecord.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			qupRecord.PMT_FoundTimeUtc = DateTime.Today;
			qupRecord.PMT_GS_NKExcludeBy = "~BP";

			Factory.Save();

			try
			{
				using (EnvProxy.Instance.TemporaryServiceTaskContext("DQP", true))
				{
					populaterObject.RunTask();
				}
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
				AssertEquals("No Errors", false, ErrorReporter.LastMessageReported.Contains("Direct access to Env.CurrentBranch is not allowed from service tasks"));
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						PatternMatchingResultSchema.Constants.TableName,
						DeduplicationQueueProcessTask.FriendlyName,
						PatternMatchingResultSchema.Constants.PMT_Status + "=" + PatternMatchingResult.StatusCodes.Queued),
				};
			}
		}

		#region Implementation

		class DeduplicationQueueProcessBatcherForTest<T> : DeduplicationQueueProcessBatcher<T> where T : BusinessObject, IDeduplicatable
		{
			internal DeduplicationQueueProcessBatcherForTest(IDuplicateDetectorProvider duplicateDetectorProvider, Func<T, IEnumerable<DuplicateDetectorProviderOutput>> detectDuplicates, SchemaPKColumn schemaPKColumn, string parentTableCode) : base(duplicateDetectorProvider, detectDuplicates, schemaPKColumn, parentTableCode)
			{
			}

			public ZQuery GetQueryExposed() => GetQuery();
		}

		#endregion
	}
}
