using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class StatusLogManagerTest : TestCaseWithFactory
	{
		public void TestAddARejectLogIfNecessary()
		{
			StmALog rejectedEvent = entry.Logs.AddNew(Events.DeclarationRejected);
			AssertEquals("PreCondition: Not Cancelled", false, rejectedEvent.SL_IsCancelled);

			IStatusList list = new ImportMessageStatusList();
			AssertNull(logManager.AddARejectLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ImportMessageStatusList.Codes.ClearEntrySummaryReplace, list));

			StmALog log1 = logManager.AddARejectLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ImportMessageStatusList.Codes.ErrorEntrySummaryReplace, list);
			AssertNotNull("one log added", log1);
			AssertEquals(ImportMessageStatusList.Codes.ErrorEntrySummaryReplace, log1.SL_Reference);
			AssertEquals("Not Cancelled", false, rejectedEvent.SL_IsCancelled);
			AssertEquals("HasARejectLog", true, logManager.HasARejectLog(ImportMessageStatusList.GetErrorStatusForEntrySummary()));

			StmALog log2 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ImportMessageStatusList.Codes.ClearEntrySummaryReplace, list);
			AssertNotNull("one log added", log2);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryReplace, log2.SL_Reference);
			AssertEquals("Cancelled", true, rejectedEvent.SL_IsCancelled);
			AssertEquals("Cancelled", true, log1.SL_IsCancelled);
			AssertEquals("HasARejectLog", false, logManager.HasARejectLog(ImportMessageStatusList.GetErrorStatusForEntrySummary()));
		}

		public void TestAddALogIfNecessary()
		{
			StmALog withdrawnStatusEvent = entry.Logs.AddNew(Events.DeclarationCancellationApproved);
			AssertEquals("PreCondition: Not Cancelled", false, withdrawnStatusEvent.SL_IsCancelled);
			AssertNull(logManager.AddALogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal, null));
			IStatusList list = new ImportMessageStatusList();
			StmALog log1 = logManager.AddALogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ErrorDepartureOriginal, list);
			AssertNotNull("one log added", log1);
			AssertEquals(ImportMessageStatusList.Codes.ErrorDepartureOriginal, log1.SL_Reference);
			AssertEquals("Not Cancelled", false, withdrawnStatusEvent.SL_IsCancelled);

			log1 = logManager.AddALogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal, list);
			AssertNotNull("one log added", log1);
			AssertEquals(ImportMessageStatusList.Codes.ClearDepartureOriginal, log1.SL_Reference);
			AssertEquals("Not Cancelled", true, withdrawnStatusEvent.SL_IsCancelled);
		}

		public void TestExcludeEstimatedLogs()
		{
			entry.Logs.AddNew(Events.CustomsEntryStatus, ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, new ZDateTimeOffset(2013, 6, 6), true);
			AssertEquals(false, logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearEntrySummaryOriginal }));
			entry.Logs.AddNew(Events.CustomsEntryStatus, ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, new ZDateTimeOffset(2013, 6, 6), false);
			AssertEquals(true, logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearEntrySummaryOriginal }));
		}

		public void TestAddAClearLogIfNecessary()
		{
			StmALog log1 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal, new ImportMessageStatusList());
			AssertNotNull("one log added", log1);
			AssertEquals(ImportMessageStatusList.Codes.ClearDepartureOriginal, log1.SL_Reference);
			AssertEquals("HasAClearLog", true, logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearDepartureOriginal }));
			AssertEquals("HasAClearInBondDeparture", true, logManager.HasAClearInBondDeparture);
			AssertEquals("HasAClearInBondArrival", false, logManager.HasAClearInBondArrival);
			AssertEquals("HasAClearInBondExportation", false, logManager.HasAClearInBondExportation);

			StmALog log2 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureAmendment, ImportMessageStatusList.Codes.ErrorDepartureAmendment, new ImportMessageStatusList());
			AssertNull("no change", log2);

			StmALog log3 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingArrival, ImportMessageStatusList.Codes.ClearArrival, new ImportMessageStatusList());
			AssertNotNull("should have added clear Arrival", log3);

			AssertEquals("HasAClearLog", true, logManager.HasAClearArrivalExportBTATransmissionLog(new string[] { ImportMessageStatusList.Codes.ClearArrival }));
			AssertEquals("HasAClearInBondDeparture", true, logManager.HasAClearInBondDeparture);
			AssertEquals("HasAClearInBondArrival", true, logManager.HasAClearInBondArrival);
			AssertEquals("HasAClearInBondExportation", false, logManager.HasAClearInBondExportation);

			StmALog log4 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw, ImportMessageStatusList.Codes.ClearDepartureWithdraw, new ImportMessageStatusList());
			AssertNotNull("should have added clear withdrawal", log4);
			AssertEquals("log1 should be cancelled", true, log1.SL_IsCancelled);
			AssertEquals("log2 should have been cancelled", true, log3.SL_IsCancelled);

			AssertEquals("HasAClearLog", false, logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearArrival }));
			AssertEquals("HasAClearInBondDeparture", false, logManager.HasAClearInBondDeparture);
			AssertEquals("HasAClearInBondArrival", false, logManager.HasAClearInBondArrival);
			AssertEquals("HasAClearInBondExportation", false, logManager.HasAClearInBondExportation);

			StmALog log5 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal, new ImportMessageStatusList());
			AssertNotNull("one log added", log5);
			AssertEquals("A withdrawal log should have been cancelled", true, log4.SL_IsCancelled);
		}

		public void TestAddAClearLogIfNecessary_ENS()
		{
			StmALog log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, new ImportMessageStatusList());
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, log.SL_Reference);
			Assert(logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearEntrySummaryOriginal }));

			log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings, new ImportMessageStatusList());
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings, log.SL_Reference);
			Assert(logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings }));

			log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings, new ImportMessageStatusList());
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings, log.SL_Reference);
			Assert(logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings }));

			log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ImportMessageStatusList.Codes.ClearEntrySummaryReplace, new ImportMessageStatusList());
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryReplace, log.SL_Reference);
			Assert(logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearEntrySummaryReplace }));

			log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings, new ImportMessageStatusList());
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings, log.SL_Reference);
			Assert(logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings }));

			log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings, new ImportMessageStatusList());
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings, log.SL_Reference);
			Assert(logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings }));

			log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete, ImportMessageStatusList.Codes.ClearEntrySummaryDelete, new ImportMessageStatusList());
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryDelete, log.SL_Reference);
			Assert(!logManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearEntrySummaryDelete }));
			Assert(logManager.HasAWithdrawnLog);
		}

		public void TestGetFromJobBranchCurrentTime()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";

			var branch1 = Factory.New<GlbBranch>();
			var branch2 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "USPHL";
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "USLAX";
			branch2.GB_GC = company1.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";
			Factory.Save();

			using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				entry.Declaration.JE_GB = branch1.PK;

				var localTimeBranch1 = ZDateTime.Now;
				var utcTimeNow = DateTime.UtcNow;
				var centralimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
				var centralTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow, centralimeInfo);
				AssertEquals("Branch1 Time Zone", centralTimeZone.TimeOfDay.Hours, localTimeBranch1.TimeOfDay.Hours);

				using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
				{
					var localTimeBranch2 = ZDateTime.Now;
					var utcTimeNow2 = DateTime.UtcNow;

					StmALog log = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, new ImportMessageStatusList());
					AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, log.SL_Reference);

					var pacificTimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
					var pacificTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow2, pacificTimeInfo);
					AssertEquals("Branch2 Time Zone", pacificTimeZone.TimeOfDay.Hours, localTimeBranch2.TimeOfDay.Hours);

					AssertEquals("Wrote by Job Branch Time", localTimeBranch1.TimeOfDay.Hours, log.SL_EventTime.TimeOfDay.Hours);
				}
			}
		}
		CusEntryHeader entry;
		StatusLogManager logManager;

		protected override void SetUp()
		{
			base.SetUp();
			entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			logManager = new StatusLogManager(entry.Logs, entry.Declaration?.Branch);
		}
	}
}
