using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Definitions.LeaveEngine;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.HRM.Common;
using Enterprise.HRM.Common.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	[TestedType(typeof(StaffRegionProcessServiceTask))]
	class StaffRegionProcessServiceTaskTest : ServiceTaskTestCase<StaffRegionProcessServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void AssertDeltaHourAndAccrual(string message, HrlBalanceTransaction transaction, ZDecimal deltaValueHours, ZDateTimeOffset accrual)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals(nameof(transaction.LLT_DeltaValueHours), deltaValueHours, transaction.LLT_DeltaValueHours);
				AssertEquals(nameof(transaction.LLT_Accrual), accrual, transaction.LLT_Accrual);
			});
		}

		void AssertBalanceTransaction(HrlBalanceTransaction transaction, GlbStaff staff, HrlProcessingRun run, ZString leaveType, ZDateTimeOffset accrual, ZString comment, ZDecimal deltaValueHours, string transactionType, ZDateTimeOffset? forfeiture = null)
		{
			CombineAssertions(() =>
			{
				AssertEquals(staff.PK, transaction.LLT_GS_Staff);
				AssertEquals(leaveType, transaction.LLT_LeaveType);
				AssertEquals(accrual, transaction.LLT_Accrual);
				AssertEquals(comment, transaction.LLT_Comment);
				AssertEquals(run.PK, transaction.LLT_LLR_ProcessingRun);
				AssertEquals(deltaValueHours, transaction.LLT_DeltaValueHours);
				AssertEquals(transactionType, transaction.LLT_TransactionType);

				if (forfeiture != null)
				{
					AssertEquals(forfeiture, transaction.LLT_Forfeiture);
				}
			});
		}

		[TestDate(2023, 10, 5)]
		public void TestSetsAccrualDateTimeToMidnightOfAccrualDate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BBY";
			staff.GS_EmploymentDate = new ZDate(2023, 1, 1);

			var tz = Factory.NewWithValidTestData<GlbStaffTimezone>();
			tz.GSZ_GS_Staff = staff.PK;
			tz.GSZ_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);
			tz.GSZ_R3_NKTimeZoneSetName = "Asia/Chongqing";

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "CN";

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run.LLR_RN_NKCountry = "CN";
			run.LLR_SystemCreateTimeUtc = new DateTime(2023, 1, 1);
			run.LLR_ProcessTo = new ZDate(2023, 2, 1);

			CreateStaffPolicy(staff, policy, new DateTime(2023, 1, 1), null);

			Factory.Save();

			var balanceResults = new BalanceTransaction { EffectiveDate = new DateTime(2023, 2, 1), LeaveType = "ANN", NonForfeitureHours = 10 };
			var forecastData = new LeaveForecasterResult { BalanceTransactions = new[] { balanceResults } };
			var mockForecaster = new Mock<ILeaveForecaster>(MockBehavior.Strict);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2023, 2, 1), null, false, true)).Returns(forecastData);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2023, 2, 1), null, true, true)).Returns(forecastData);

			var logs = new List<string>();
			var logger = new DummyLogger();
			logger.OnLog += (o, e) => logs.Add($"{e.Type}: {e.Message}");

			var task = new StaffRegionProcessServiceTask(mockForecaster.Object) { ServiceLogger = logger };
			var staffToProcess = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run));
			AssertCollectionContains("PRE: Staff will be processed", "BBY", staffToProcess.Select(s => s.GS_Code));

			task.RunTask();

			var transactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run.PK));
			AssertEquals(2, transactions.Length);

			var transaction = transactions[0];
			CombineAssertions(string.Join(System.Environment.NewLine, logs), () =>
			{
				AssertEquals(nameof(transaction.LLT_TransactionType), "PRO", transaction.LLT_TransactionType);
				AssertEquals(nameof(transaction.LLT_DeltaValueHours), 10m, transaction.LLT_DeltaValueHours);
				AssertEquals(nameof(transaction.LLT_Accrual), new ZDateTimeOffset(2023, 2, 1, 23, 59, 59, TimeSpan.FromHours(8)), transaction.LLT_Accrual);
			});

			var fprTransaction = transactions[1];
			CombineAssertions(string.Join(System.Environment.NewLine, logs), () =>
			{
				AssertEquals(nameof(fprTransaction.LLT_TransactionType), "FPR", fprTransaction.LLT_TransactionType);
				AssertEquals(nameof(fprTransaction.LLT_DeltaValueHours), 10m, fprTransaction.LLT_DeltaValueHours);
				AssertEquals(nameof(fprTransaction.LLT_Accrual), new ZDateTimeOffset(2023, 2, 1, 23, 59, 59, TimeSpan.FromHours(8)), fprTransaction.LLT_Accrual);
			});
		}

		[TestDate(2023, 10, 5)]
		public void TestSkipProcessingIfOnlyNeedsReprocessing()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BBY";
			staff.GS_EmploymentDate = new ZDate(2023, 1, 1);

			var tz = Factory.NewWithValidTestData<GlbStaffTimezone>();
			tz.GSZ_GS_Staff = staff.PK;
			tz.GSZ_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);
			tz.GSZ_R3_NKTimeZoneSetName = "Asia/Chongqing";

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "CN";

			var previousRun = Factory.NewWithValidTestData<HrlProcessingRun>();
			previousRun.LLR_Status = LeaveProcessingRunStatusCodes.Completed;
			previousRun.LLR_RN_NKCountry = "CN";
			previousRun.LLR_SystemCreateTimeUtc = new DateTime(2023, 1, 1);
			previousRun.LLR_ProcessTo = new ZDate(2023, 2, 1);

			var previousRunResult = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			previousRunResult.LLT_LLR_ProcessingRun = previousRun.PK;
			previousRunResult.LLT_GS_Staff = staff.PK;
			previousRunResult.LLT_DeltaValueHours = 5;
			previousRunResult.LLT_Accrual = new ZDateTimeOffset(2023, 2, 1, 23, 59, 59, TimeSpan.FromHours(8));
			previousRunResult.LLT_LeaveType = "ANN";
			previousRunResult.LLT_TransactionType = "PRO";
			previousRunResult.LLT_SystemCreateTimeUtc = new ZDateTime(2023, 1, 15);

			var balanceAffectingChange = Factory.NewWithValidTestData<HrlBalanceAffectingLog>();
			balanceAffectingChange.LLB_GS_Staff = staff.PK;
			balanceAffectingChange.LLB_EffectiveDate = new ZDateTimeOffset(2023, 1, 15, 8, 0, 0, TimeSpan.FromHours(8));
			balanceAffectingChange.LLB_Reference = "Leave Approved";
			balanceAffectingChange.LLB_SystemCreateTimeUtc = new ZDateTime(2023, 1, 16);

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run.LLR_RN_NKCountry = "CN";
			run.LLR_SystemCreateTimeUtc = new DateTime(2023, 1, 1);
			run.LLR_ProcessTo = new ZDate(2023, 2, 1);

			CreateStaffPolicy(staff, policy, new DateTime(2023, 1, 1), null);

			Factory.Save();

			var balanceResults = new BalanceTransaction { EffectiveDate = new DateTime(2023, 2, 1), LeaveType = "ANN", NonForfeitureHours = 10 };
			var forecastData = new LeaveForecasterResult { BalanceTransactions = new[] { balanceResults } };
			var mockForecaster = new Mock<ILeaveForecaster>(MockBehavior.Strict);
			mockForecaster.Setup(m => m.Forecast("BBY", It.IsAny<DateTime>(), It.IsAny<DateTime?>(), false, true)).Returns(forecastData);
			mockForecaster.Setup(m => m.Forecast("BBY", It.IsAny<DateTime>(), It.IsAny<DateTime?>(), true, true)).Returns(forecastData);

			var logs = new List<string>();
			var logger = new DummyLogger();
			logger.OnLog += (o, e) => logs.Add($"{e.Type}: {e.Message}");

			var task = new StaffRegionProcessServiceTask(mockForecaster.Object) { ServiceLogger = logger };
			var staffToProcess = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run));
			AssertCollectionContains("PRE: Staff will be processed", "BBY", staffToProcess.Select(s => s.GS_Code));

			task.RunTask();

			var transactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run.PK));
			AssertEquals(2, transactions.Length);

			var transaction = transactions[0];
			CombineAssertions(string.Join(System.Environment.NewLine, logs), () =>
			{
				AssertEquals(nameof(transaction.LLT_TransactionType), "REP", transaction.LLT_TransactionType);
				AssertEquals(nameof(transaction.LLT_DeltaValueHours), 5m, transaction.LLT_DeltaValueHours);
				AssertEquals(nameof(transaction.LLT_Accrual), new ZDateTimeOffset(2023, 2, 1, 23, 59, 59, TimeSpan.FromHours(8)), transaction.LLT_Accrual);
			});

			var frrTransaction = transactions[1];
			CombineAssertions(string.Join(System.Environment.NewLine, logs), () =>
			{
				AssertEquals(nameof(frrTransaction.LLT_TransactionType), "FRR", frrTransaction.LLT_TransactionType);
				AssertEquals(nameof(frrTransaction.LLT_DeltaValueHours), 10m, frrTransaction.LLT_DeltaValueHours);
				AssertEquals(nameof(frrTransaction.LLT_Accrual), new ZDateTimeOffset(2023, 2, 1, 23, 59, 59, TimeSpan.FromHours(8)), frrTransaction.LLT_Accrual);
			});
		}

		#region OldestProcessingRunQuery

		[TestDate(2023, 10, 5)]
		public void TestOldestProcessingRunQueryCanGetInProgressProcessingRun()
		{
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run1.LLR_SystemCreateTimeUtc = new DateTime(2020, 5, 1);

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_Status = LeaveProcessingRunStatusCodes.InProgress;
			run2.LLR_SystemCreateTimeUtc = new DateTime(2020, 4, 1);

			var run3 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3.LLR_Status = LeaveProcessingRunStatusCodes.Completed;
			run3.LLR_SystemCreateTimeUtc = new DateTime(2020, 3, 1);

			var run4 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4.LLR_Status = LeaveProcessingRunStatusCodes.InProgressWithErrors;
			run4.LLR_SystemCreateTimeUtc = new DateTime(2020, 2, 1);

			var run5 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5.LLR_Status = LeaveProcessingRunStatusCodes.CompletedWithErrors;
			run5.LLR_SystemCreateTimeUtc = new DateTime(2020, 1, 1);

			Factory.Save();

			var task = new StaffRegionProcessServiceTask();

			var processingRun = Factory.LoadTop1<HrlProcessingRun>(task.OldestProcessingRunQuery);

			AssertNotNull(processingRun);
			AssertEquals(run2.PK, processingRun.PK);
		}

		[TestDate(2023, 10, 5)]
		public void TestOldestProcessingRunQueryCanGetQueuedProcessingRun()
		{
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_Status = LeaveProcessingRunStatusCodes.InProgress;
			run1.LLR_SystemCreateTimeUtc = new DateTime(2020, 5, 1);

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run2.LLR_SystemCreateTimeUtc = new DateTime(2020, 4, 1);

			var run3 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3.LLR_Status = LeaveProcessingRunStatusCodes.Completed;
			run3.LLR_SystemCreateTimeUtc = new DateTime(2020, 3, 1);

			var run4 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4.LLR_Status = LeaveProcessingRunStatusCodes.InProgressWithErrors;
			run4.LLR_SystemCreateTimeUtc = new DateTime(2020, 2, 1);

			var run5 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5.LLR_Status = LeaveProcessingRunStatusCodes.CompletedWithErrors;
			run5.LLR_SystemCreateTimeUtc = new DateTime(2020, 1, 1);

			Factory.Save();

			var task = new StaffRegionProcessServiceTask();

			var processingRun = Factory.LoadTop1<HrlProcessingRun>(task.OldestProcessingRunQuery);

			AssertNotNull(processingRun);
			AssertEquals(run2.PK, processingRun.PK);
		}

		[TestDate(2023, 10, 5)]
		public void TestOldestProcessingRunQueryCanGetProcessingRunByTime()
		{
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_Status = LeaveProcessingRunStatusCodes.InProgress;
			run1.LLR_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run2.LLR_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);

			var run3 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3.LLR_Status = LeaveProcessingRunStatusCodes.InProgress;
			run3.LLR_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);

			var run4 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run4.LLR_SystemCreateTimeUtc = ZDateTime.Now;

			var run5 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5.LLR_Status = LeaveProcessingRunStatusCodes.InProgress;
			run5.LLR_SystemCreateTimeUtc = ZDateTime.Now.AddDays(4);

			Factory.Save();

			var task = new StaffRegionProcessServiceTask();

			var processingRun = Factory.LoadTop1<HrlProcessingRun>(task.OldestProcessingRunQuery);

			AssertNotNull(processingRun);
			AssertEquals(run4.PK, processingRun.PK);
		}

		#endregion

		#region GetStaffQueueInProcessingRunQuery

		#region Last Accrual And Policy

		[TestDate(2023, 10, 5)]
		public void TestGetStaffQueueInProcessingRunQuery_StaffHasNoBalanceTransaction_UseEmploymentDateAsLastAccrual_ProcessExpiredPolicy()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "US";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run_ExpiredPolicy_ProcessToTimeLowerThanPolicyLowerBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeLowerThanPolicyLowerBound.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeLowerThanPolicyLowerBound.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddDays(-1);

			var run_ExpiredPolicy_ProcessToTimeEqualsPolicyLowerBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeEqualsPolicyLowerBound.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeEqualsPolicyLowerBound.LLR_ProcessTo = ZDate.Today.AddYears(-2);

			var run_ExpiredPolicy_ProcessToTimeWithinPolicyTimeRange = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeWithinPolicyTimeRange.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeWithinPolicyTimeRange.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddMonths(-6);

			var run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound.LLR_ProcessTo = ZDate.Today.AddYears(-1);

			var run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddDays(1);

			Factory.Save();

			var result0 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeLowerThanPolicyLowerBound));
			var result1 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeEqualsPolicyLowerBound));
			var result2 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeWithinPolicyTimeRange));
			var result3 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound));
			var result4 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound));

			CombineAssertions(() =>
			{
				AssertEquals("Cannot get staff when the process run time is below the policy start time.", 0, result0.Length);
				AssertEquals("Can get staff when the process run time is equal to the policy start time.", 1, result1.Length);
				AssertEquals("Can get staff when the process run time is within the policy time range.", 1, result2.Length);
				AssertEquals("Can get staff when the process run time is equal to the policy end time.", 1, result3.Length);
				AssertEquals("Can get staff when the process run time is higher than the policy end time.", 1, result4.Length);

				AssertEquals("Query result 1 is the staff.", staff.PK, result1[0].PK);
				AssertEquals("Query result 2 is the staff.", staff.PK, result2[0].PK);
				AssertEquals("Query result 3 is the staff.", staff.PK, result3[0].PK);
				AssertEquals("Query result 4 is the staff.", staff.PK, result4[0].PK);
			});
		}

		[TestDate(2023, 10, 5)]
		public void TestGetStaffQueueInProcessingRunQuery_StaffHasNoBalanceTransaction_UseEmploymentDateAsLastAccrual_ProcessEffectivePolicy()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "US";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run_EffectivePolicy_ProcessToTimeLowerThanPolicyLowerBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeLowerThanPolicyLowerBound.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeLowerThanPolicyLowerBound.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddDays(-1);

			var run_EffectivePolicy_ProcessToTimeEqualsPolicyLowerBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeEqualsPolicyLowerBound.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeEqualsPolicyLowerBound.LLR_ProcessTo = ZDate.Today.AddYears(-1);

			var run_EffectivePolicy_ProcessToTimeLargerThanPolicyLowerBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeLargerThanPolicyLowerBound.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeLargerThanPolicyLowerBound.LLR_ProcessTo = ZDate.Today.AddMonths(-6);

			var run_EffectivePolicy_ProcessToTimeInFuture = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeInFuture.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeInFuture.LLR_ProcessTo = ZDate.Today.AddYears(1);

			Factory.Save();

			var result0 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeLowerThanPolicyLowerBound));
			var result1 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeEqualsPolicyLowerBound));
			var result2 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeLargerThanPolicyLowerBound));
			var result3 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeInFuture));

			CombineAssertions(() =>
			{
				AssertEquals("Cannot get staff when the process run time is below the policy start time.", 0, result0.Length);
				AssertEquals("Can get staff when the process run time is equal to the policy start time.", 1, result1.Length);
				AssertEquals("Can get staff when the process run time is larger than the policy start time.", 1, result2.Length);
				AssertEquals("Can get staff when the process run time is in the future.", 1, result3.Length);

				AssertEquals("Query result 1 is the staff.", staff.PK, result1[0].PK);
				AssertEquals("Query result 2 is the staff.", staff.PK, result2[0].PK);
				AssertEquals("Query result 3 is the staff.", staff.PK, result3[0].PK);
			});
		}

		[TestDate(2023, 10, 5)]
		public void TestGetStaffQueueInProcessingRunQuery_StaffHasBalanceTransaction_UseLargestAccrualAsLastAccrual_ProcessExpiredPolicy()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "US";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(6);

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(7);

			var balanceTransaction1 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction1.LLT_GS_Staff = staff.PK;
			balanceTransaction1.LLT_Accrual = ZDateTimeOffset.Today.AddYears(-2).AddMonths(6);
			balanceTransaction1.LLT_LLR_ProcessingRun = run1.PK;

			var balanceTransaction2 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction2.LLT_GS_Staff = staff.PK;
			balanceTransaction2.LLT_Accrual = ZDateTimeOffset.Today.AddYears(-2).AddMonths(7);
			balanceTransaction2.LLT_LLR_ProcessingRun = run2.PK;

			var run_ExpiredPolicy_ProcessToTimeLowerThanLastAccrual = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeLowerThanLastAccrual.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeLowerThanLastAccrual.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(7).AddDays(-1);

			var run_ExpiredPolicy_ProcessToTimeEqualsLastAccrual = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeEqualsLastAccrual.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeEqualsLastAccrual.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(7);

			var run_ExpiredPolicy_ProcessToTimeWithinAccrualAndPolicyTimeRange = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeWithinAccrualAndPolicyTimeRange.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeWithinAccrualAndPolicyTimeRange.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(10);

			var run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound.LLR_ProcessTo = ZDate.Today.AddYears(-1);

			var run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound.LLR_RN_NKCountry = "US";
			run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddDays(1);

			Factory.Save();

			var result0 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeLowerThanLastAccrual));
			var result1 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeEqualsLastAccrual));
			var result2 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeWithinAccrualAndPolicyTimeRange));
			var result3 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeEqualsPolicyUpperBound));
			var result4 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_ExpiredPolicy_ProcessToTimeLargerThanPolicyUpperBound));

			CombineAssertions(() =>
			{
				AssertEquals("Cannot get staff when the process run time is less than the last accrual time.", 0, result0.Length);
				AssertEquals("Can get staff when the process run time is equal to the last accrual time.", 1, result1.Length);
				AssertEquals("Can get staff when the process run time is within time range [LastAccrual, PolicyEndTime].", 1, result2.Length);
				AssertEquals("Can get staff when the process run time is equal to the policy end time.", 1, result3.Length);
				AssertEquals("Can get staff when the process run time is higher than the policy end time.", 1, result4.Length);

				AssertEquals("Query result 1 is the staff.", staff.PK, result1[0].PK);
				AssertEquals("Query result 2 is the staff.", staff.PK, result2[0].PK);
				AssertEquals("Query result 3 is the staff.", staff.PK, result3[0].PK);
				AssertEquals("Query result 4 is the staff.", staff.PK, result4[0].PK);
			});
		}

		[TestDate(2023, 10, 5)]
		public void TestGetStaffQueueInProcessingRunQuery_StaffHasBalanceTransaction_UseLargestAccrualAsLastAccrual_ProcessEffectivePolicy()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "US";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "AU";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-6);

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "AU";
			run2.LLR_ProcessTo = ZDate.Today.AddMonths(-7);

			var balanceTransaction1 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction1.LLT_GS_Staff = staff.PK;
			balanceTransaction1.LLT_Accrual = ZDateTimeOffset.Today.AddMonths(-6);
			balanceTransaction1.LLT_LLR_ProcessingRun = run1.PK;

			var balanceTransaction2 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction2.LLT_GS_Staff = staff.PK;
			balanceTransaction2.LLT_Accrual = ZDateTimeOffset.Today.AddMonths(-7);
			balanceTransaction2.LLT_LLR_ProcessingRun = run2.PK;

			var run_EffectivePolicy_ProcessToTimeLowerThanLastAccrual = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeLowerThanLastAccrual.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeLowerThanLastAccrual.LLR_ProcessTo = ZDate.Today.AddMonths(-6).AddDays(-1);

			var run_EffectivePolicy_ProcessToTimeEqualsLastAccrual = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeEqualsLastAccrual.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeEqualsLastAccrual.LLR_ProcessTo = ZDate.Today.AddMonths(-6);

			var run_EffectivePolicy_ProcessToTimeLargerThanLastAccrual = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeLargerThanLastAccrual.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeLargerThanLastAccrual.LLR_ProcessTo = ZDate.Today.AddMonths(-6).AddDays(1);

			var run_EffectivePolicy_ProcessToTimeInFuture = Factory.NewWithValidTestData<HrlProcessingRun>();
			run_EffectivePolicy_ProcessToTimeInFuture.LLR_RN_NKCountry = "AU";
			run_EffectivePolicy_ProcessToTimeInFuture.LLR_ProcessTo = ZDate.Today.AddYears(1);

			Factory.Save();

			var result0 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeLowerThanLastAccrual));
			var result1 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeEqualsLastAccrual));
			var result2 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeLargerThanLastAccrual));
			var result3 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run_EffectivePolicy_ProcessToTimeInFuture));

			CombineAssertions(() =>
			{
				AssertEquals("Cannot get staff when the process run time is less than the last accrual time.", 0, result0.Length);
				AssertEquals("Can get staff when the process run time is equal to the last accrual time.", 1, result1.Length);
				AssertEquals("CCan get staff when the process run time is larger than the last accrual time.", 1, result2.Length);
				AssertEquals("Can get staff when the process run time is in the future.", 1, result3.Length);

				AssertEquals("Query result 1 is the staff.", staff.PK, result1[0].PK);
				AssertEquals("Query result 2 is the staff.", staff.PK, result2[0].PK);
				AssertEquals("Query result 3 is the staff.", staff.PK, result3[0].PK);
			});
		}

		#endregion

		#region Multiple Processing Runs

		[TestDate(2023, 10, 5)]
		public void TestGetStaffQueueInProcessingRunQuery_MultipleProcessRuns_ProcessToMustLargerThanTheLastAccrual()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			staff1.GS_EmploymentDate = ZDate.Today.AddYears(-3);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";
			staff2.GS_EmploymentDate = ZDate.Today.AddYears(-3);
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "TS3";
			staff3.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff1, policy, ZDateTimeOffset.Today.AddYears(-3), null);
			_ = CreateStaffPolicy(staff2, policy, ZDateTimeOffset.Today.AddYears(-3), null);
			_ = CreateStaffPolicy(staff3, policy, ZDateTimeOffset.Today.AddYears(-3), null);

			var run0 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run0.LLR_RN_NKCountry = "US";
			run0.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(1);
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(3);
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddMonths(6);

			var balanceTransaction11 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction11.LLT_GS_Staff = staff1.PK;
			balanceTransaction11.LLT_Accrual = ZDateTimeOffset.Today.AddYears(-2).AddMonths(1);
			balanceTransaction11.LLT_LLR_ProcessingRun = run0.PK;

			var balanceTransaction22 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction22.LLT_GS_Staff = staff2.PK;
			balanceTransaction22.LLT_Accrual = ZDateTimeOffset.Today.AddYears(-2).AddMonths(3);
			balanceTransaction22.LLT_LLR_ProcessingRun = run1.PK;

			var balanceTransaction33 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction33.LLT_GS_Staff = staff3.PK;
			balanceTransaction33.LLT_Accrual = ZDateTimeOffset.Today.AddYears(-2).AddMonths(6);
			balanceTransaction33.LLT_LLR_ProcessingRun = run2.PK;

			Factory.Save();

			var result0 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run0));
			var result1 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run1));
			var result2 = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run2));

			AssertEquals(0, result0.Length);
			AssertEquals(1, result1.Length);
			AssertEquals(2, result2.Length);

			AssertEquals(staff1.PK, result1[0].PK);
			AssertContainsExactElementsInAnyOrder(new[] { staff1.PK, staff2.PK }, result2.Select(s => s.PK));

			CombineAssertions(() =>
			{
				AssertEquals("run 0 cannot get staff", 0, result0.Length);
				AssertEquals("run 1 can get 1 staff", 1, result1.Length);
				AssertEquals("run 2 can get 2 staff", 2, result2.Length);

				AssertEquals("Query result 1 is the staff 1.", staff1.PK, result1[0].PK);
				AssertContainsExactElementsInAnyOrder("Query result 2 contains staff 1 and staff 2.", new[] { staff1.PK, staff2.PK }, result2.Select(s => s.PK));
			});
		}

		[TestDate(2023, 10, 5)]
		public void TestGetStaffQueueInProcessingRunQuery_MultipleProcessRuns_GetStaffsByCountry()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			staff1.GS_EmploymentDate = ZDate.Today.AddYears(-3);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";
			staff2.GS_EmploymentDate = ZDate.Today.AddYears(-3);
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "TS3";
			staff3.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "US";
			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff1, policy1, ZDateTimeOffset.Today.AddYears(-3), null);

			_ = CreateStaffPolicy(staff2, policy1, ZDateTimeOffset.Today.AddYears(-2), null);
			_ = CreateStaffPolicy(staff2, policy2, ZDateTimeOffset.Today.AddYears(-1), null);

			_ = CreateStaffPolicy(staff3, policy2, ZDateTimeOffset.Today, null);

			var run1_us = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1_us.LLR_RN_NKCountry = "US";
			run1_us.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddDays(-1);
			var run2_us = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2_us.LLR_RN_NKCountry = "US";
			run2_us.LLR_ProcessTo = ZDate.Today.AddYears(-2);
			var run3_us = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3_us.LLR_RN_NKCountry = "US";
			run3_us.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddDays(1);
			var run4_us = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4_us.LLR_RN_NKCountry = "US";
			run4_us.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddDays(-1);
			var run5_us = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5_us.LLR_RN_NKCountry = "US";
			run5_us.LLR_ProcessTo = ZDate.Today.AddYears(-1);
			var run6_us = Factory.NewWithValidTestData<HrlProcessingRun>();
			run6_us.LLR_RN_NKCountry = "US";
			run6_us.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddDays(1);
			var run7_us = Factory.NewWithValidTestData<HrlProcessingRun>();
			run7_us.LLR_RN_NKCountry = "US";
			run7_us.LLR_ProcessTo = ZDate.Today.AddYears(1);

			var run1_au = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1_au.LLR_RN_NKCountry = "AU";
			run1_au.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddDays(-1);
			var run2_au = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2_au.LLR_RN_NKCountry = "AU";
			run2_au.LLR_ProcessTo = ZDate.Today.AddYears(-2);
			var run3_au = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3_au.LLR_RN_NKCountry = "AU";
			run3_au.LLR_ProcessTo = ZDate.Today.AddYears(-2).AddDays(1);
			var run4_au = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4_au.LLR_RN_NKCountry = "AU";
			run4_au.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddDays(-1);
			var run5_au = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5_au.LLR_RN_NKCountry = "AU";
			run5_au.LLR_ProcessTo = ZDate.Today.AddYears(-1);
			var run6_au = Factory.NewWithValidTestData<HrlProcessingRun>();
			run6_au.LLR_RN_NKCountry = "AU";
			run6_au.LLR_ProcessTo = ZDate.Today.AddYears(-1).AddDays(1);
			var run7_au = Factory.NewWithValidTestData<HrlProcessingRun>();
			run7_au.LLR_RN_NKCountry = "AU";
			run7_au.LLR_ProcessTo = ZDate.Today.AddYears(1);

			Factory.Save();

			var result0_us = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run1_us));
			var result1_us = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run2_us));
			var result2_us = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run3_us));
			var result3_us = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run4_us));
			var result4_us = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run5_us));
			var result5_us = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run6_us));
			var result6_us = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run7_us));

			var result0_au = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run1_au));
			var result1_au = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run2_au));
			var result2_au = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run3_au));
			var result3_au = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run4_au));
			var result4_au = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run5_au));
			var result5_au = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run6_au));
			var result6_au = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run7_au));

			CombineAssertions(() =>
			{
				AssertEquals("US run 0 can get 1 staff.", 1, result0_us.Length);
				AssertEquals("US run 1 can get 2 staff.", 2, result1_us.Length);
				AssertEquals("US run 2 can get 2 staff.", 2, result2_us.Length);
				AssertEquals("US run 3 can get 2 staff.", 2, result3_us.Length);
				AssertEquals("US run 4 can get 2 staff.", 2, result4_us.Length);
				AssertEquals("US run 5 can get 2 staff.", 2, result5_us.Length);
				AssertEquals("US run 6 can get 2 staff.", 2, result6_us.Length);

				AssertContainsExactElementsInAnyOrder("US result 0 is staff 1.", new[] { staff1.PK }, result0_us.Select(s => s.PK));
				AssertContainsExactElementsInAnyOrder("US result 1 contains staff 1 and staff 2.", new[] { staff1.PK, staff2.PK }, result1_us.Select(s => s.PK));
				AssertContainsExactElementsInAnyOrder("US result 2 contains staff 1 and staff 2.", new[] { staff1.PK, staff2.PK }, result2_us.Select(s => s.PK));
				AssertContainsExactElementsInAnyOrder("US result 3 contains staff 1 and staff 2.", new[] { staff1.PK, staff2.PK }, result3_us.Select(s => s.PK));
				AssertContainsExactElementsInAnyOrder("US result 4 contains staff 1 and staff 2.", new[] { staff1.PK, staff2.PK }, result4_us.Select(s => s.PK));
				AssertContainsExactElementsInAnyOrder("US result 5 contains staff 1 and staff 2.", new[] { staff1.PK, staff2.PK }, result5_us.Select(s => s.PK));
				AssertContainsExactElementsInAnyOrder("US result 6 contains staff 1 and staff 2.", new[] { staff1.PK, staff2.PK }, result6_us.Select(s => s.PK));

				AssertEquals("AU run 0 cannot get staff.", 0, result0_au.Length);
				AssertEquals("AU run 1 cannot get staff.", 0, result1_au.Length);
				AssertEquals("AU run 2 cannot get staff.", 0, result2_au.Length);
				AssertEquals("AU run 3 cannot get staff.", 0, result3_au.Length);
				AssertEquals("US run 4 can get 1 staff.", 1, result4_au.Length);
				AssertEquals("US run 5 can get 1 staff.", 1, result5_au.Length);
				AssertEquals("US run 6 can get 2 staff.", 2, result6_au.Length);

				AssertEquals("AU result 4 is staff 2.", staff2.PK, result4_au[0].PK);
				AssertEquals("AU result 5 is staff 2.", staff2.PK, result5_au[0].PK);
				AssertContainsExactElementsInAnyOrder("AU result 6 contains staff 1 and staff 2.", new[] { staff2.PK, staff3.PK }, result6_au.Select(s => s.PK));
			});
		}

		#endregion

		[TestDate(2023, 10, 5)]
		public void TestGetStaffQueueInProcessingRunQuery_OnlyGetsNoneProcessedStaffs()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			staff1.GS_EmploymentDate = ZDate.Today.AddYears(-3);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";
			staff2.GS_EmploymentDate = ZDate.Today.AddYears(-3);
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "TS3";
			staff3.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff1, policy, ZDateTimeOffset.Today.AddYears(-3), null);
			_ = CreateStaffPolicy(staff2, policy, ZDateTimeOffset.Today.AddYears(-3), null);
			_ = CreateStaffPolicy(staff3, policy, ZDateTimeOffset.Today.AddYears(-3), null);

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddYears(-1);

			var balanceTransaction = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceTransaction.LLT_GS_Staff = staff1.PK;
			balanceTransaction.LLT_LLR_ProcessingRun = run.PK;

			Factory.Save();

			var staffs = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run));

			AssertContainsExactElementsInAnyOrder(new[] { staff2.PK, staff3.PK }, staffs.Select(s => s.PK));
		}

		#endregion

		#region ProcessStaff

		public void TestInitialiseTask()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		GlbStaffHoliday CreateStaffHoliday(GlbStaff staff, ZDateTime startTime, ZDateTime endTime)
		{
			var holiday = Factory.NewWithValidTestData<GlbStaffHoliday>();

			holiday.GA_GS = staff.PK;
			holiday.GA_StartTime = startTime;
			holiday.GA_EndTime = endTime;

			return holiday;
		}

		HrlBalanceAffectingQueue CreateBalanceAffectingQueue(GlbStaff staff, ZDateTimeOffset effectiveDate)
		{
			var balanceAffectingQueue = Factory.NewWithValidTestData<HrlBalanceAffectingQueue>();

			balanceAffectingQueue.LLQ_EffectiveDate = effectiveDate;
			balanceAffectingQueue.LLQ_GS_Staff = staff.PK;

			return balanceAffectingQueue;
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Success()
		{
			//process a staff successfully
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Debug|Processing TSS succeed to {processTo}
Information|Batched [1] staff were successfully processed.
Information|Processing Run {run.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			//Test balance transaction records
			var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

			AssertEquals(4, balanceTransactions.Length);

			var proTransactions1 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == ZDateTimeOffset.Empty &&
				x.LLT_TransactionType == BalanceTransactionTypes.Processing).FirstOrDefault();
			var proTransactions2 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)) &&
				x.LLT_TransactionType == BalanceTransactionTypes.Processing).FirstOrDefault();

			var fprTransactions1 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == ZDateTimeOffset.Empty &&
				x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).FirstOrDefault();

			var fprTransactions2 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)) &&
				x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).FirstOrDefault();

			AssertNotNull(proTransactions1);
			AssertNotNull(proTransactions2);
			AssertNotNull(fprTransactions1);
			AssertNotNull(fprTransactions2);

			AssertBalanceTransaction(proTransactions1, staff, run, "LT1", processTo, "Processing", 10.1m, "PRO");
			AssertBalanceTransaction(proTransactions2, staff, run, "LT1", processTo, "Processing", 234.5m, "PRO");
			AssertBalanceTransaction(fprTransactions1, staff, run, "LT1", processTo, "Processing", 10.1m, "FPR");
			AssertBalanceTransaction(fprTransactions2, staff, run, "LT1", processTo, "Processing", 234.5m, "FPR");

			//Test no error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));

			AssertEquals(0, balanceAffectingLogs.Length);

			//Test the processing run is deactivated
			var process2 = new StaffRegionProcessServiceTask();
			var logger2 = new TestServiceLogger();
			process2.ServiceLogger = logger2;
			process2.RunTask();

			var expectedLog2 = $@"Information|No processable processing run found.
";
			AssertEquals(expectedLog2, logger2.ToString());
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_AdjustmentCreatedLaterThanAccrual()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BBY";
			staff.GS_EmploymentDate = new ZDate(2022, 12, 1);

			var tz = Factory.NewWithValidTestData<GlbStaffTimezone>();
			tz.GSZ_GS_Staff = staff.PK;
			tz.GSZ_EffectiveDate = new ZDateTimeOffset(2022, 12, 1);
			tz.GSZ_R3_NKTimeZoneSetName = "Asia/Chongqing";

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "CN";

			CreateStaffPolicy(staff, policy, new DateTime(2022, 12, 1), null);

			// t1 should have been included in the previous REP (t0), not included in this REP
			var t1 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			t1.LLT_GS_Staff = staff.PK;
			t1.LLT_LeaveType = "ANN";
			t1.LLT_TransactionType = "MAN";
			t1.LLT_DeltaValueHours = 3;
			t1.LLT_Accrual = new ZDateTimeOffset(2023, 1, 1, 23, 59, 59, TimeSpan.FromHours(8));
			t1.LLT_SystemCreateTimeUtc = new DateTime(2022, 12, 1);

			var t0 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			t0.LLT_GS_Staff = staff.PK;
			t0.LLT_LeaveType = "ANN";
			t0.LLT_TransactionType = "REP";
			t0.LLT_DeltaValueHours = 3;
			t0.LLT_Accrual = new ZDateTimeOffset(2023, 1, 1, 23, 59, 59, TimeSpan.FromHours(8));
			t0.LLT_SystemCreateTimeUtc = new DateTime(2023, 1, 1);

			// t2 should have not been included in previous PRO, but will be included in this REP
			var t2 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			t2.LLT_GS_Staff = staff.PK;
			t2.LLT_LeaveType = "ANN";
			t2.LLT_TransactionType = "MAN";
			t2.LLT_DeltaValueHours = 5;
			t2.LLT_Accrual = new ZDateTimeOffset(2023, 1, 1, 23, 59, 59, TimeSpan.FromHours(8));
			t2.LLT_SystemCreateTimeUtc = new DateTime(2023, 1, 5);

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run.LLR_RN_NKCountry = "CN";
			run.LLR_SystemCreateTimeUtc = new DateTime(2023, 1, 1);
			run.LLR_ProcessTo = new ZDate(2023, 2, 1);

			// create an HrlBalanceAffectingQueue to trigger REP
			var queue = Factory.NewWithValidTestData<HrlBalanceAffectingQueue>();
			queue.LLQ_GS_Staff = staff.PK;

			Factory.Save();

			var balanceResults1 = new BalanceTransaction { EffectiveDate = new DateTime(2023, 1, 1), LeaveType = "ANN", NonForfeitureHours = 8 };
			var forecastData1 = new LeaveForecasterResult { BalanceTransactions = new[] { balanceResults1 } };
			var balanceResults2 = new BalanceTransaction { EffectiveDate = new DateTime(2023, 2, 1), LeaveType = "ANN", NonForfeitureHours = 15 };
			var forecastData2 = new LeaveForecasterResult { BalanceTransactions = new[] { balanceResults2 } };

			var mockForecaster = new Mock<ILeaveForecaster>(MockBehavior.Strict);
			// forecastData1 include both t1 and t2
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2023, 1, 1, 23, 59, 59), null, false, true))
				.Returns(forecastData1);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2023, 2, 1), new DateTime(2023, 1, 1), false, true))
				.Returns(forecastData2);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2023, 1, 1, 23, 59, 59), null, true, true))
				.Returns(forecastData1);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2023, 2, 1), new DateTime(2023, 1, 1), true, true))
				.Returns(forecastData2);

			var logs = new List<string>();
			var logger = new DummyLogger();
			logger.OnLog += (o, e) => logs.Add($"{e.Type}: {e.Message}");
			var task = new StaffRegionProcessServiceTask(mockForecaster.Object) { ServiceLogger = logger };

			var staffToProcess = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run));
			AssertCollectionContains("PRE: Staff will be processed", "BBY", staffToProcess.Select(s => s.GS_Code));

			task.RunTask();

			#region Assert
			var transactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run.PK));
			AssertEquals(4, transactions.Length);

			var proTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.Processing).FirstOrDefault();
			var repTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.Reprocessing).FirstOrDefault();
			var fprTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).FirstOrDefault();
			var frrTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.FinanceReprocessing).FirstOrDefault();

			AssertNotNull(proTransactions);
			AssertNotNull(repTransactions);
			AssertNotNull(fprTransactions);
			AssertNotNull(frrTransactions);

			var reprocessingAccrualDate = new ZDateTimeOffset(2023, 1, 1, 23, 59, 59, TimeSpan.FromHours(8));
			var processingAccrualDate = new ZDateTimeOffset(2023, 2, 1, 23, 59, 59, TimeSpan.FromHours(8));

			AssertDeltaHourAndAccrual(string.Join(System.Environment.NewLine, logs), proTransactions, 7m, processingAccrualDate);
			AssertDeltaHourAndAccrual(string.Join(System.Environment.NewLine, logs), repTransactions, 5m, reprocessingAccrualDate);
			AssertDeltaHourAndAccrual(string.Join(System.Environment.NewLine, logs), fprTransactions, 7m, processingAccrualDate);
			AssertDeltaHourAndAccrual(string.Join(System.Environment.NewLine, logs), frrTransactions, 8m, reprocessingAccrualDate);

			#endregion
		}

		[TestDate(2024, 7, 4)]
		public void TestProcessSingleStaff_Adjustment()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BBY";
			staff.GS_EmploymentDate = new ZDate(2024, 7, 1);

			var tz = Factory.NewWithValidTestData<GlbStaffTimezone>();
			tz.GSZ_GS_Staff = staff.PK;
			tz.GSZ_EffectiveDate = new ZDateTimeOffset(2024, 7, 1);
			tz.GSZ_R3_NKTimeZoneSetName = "Asia/Chongqing";

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "CN";

			CreateStaffPolicy(staff, policy, new DateTime(2024, 7, 1), null);

			var t0 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			t0.LLT_GS_Staff = staff.PK;
			t0.LLT_LeaveType = "ANN";
			t0.LLT_TransactionType = "PRO";
			t0.LLT_DeltaValueHours = 80;
			t0.LLT_Accrual = new ZDateTimeOffset(2024, 8, 31, 23, 59, 59, TimeSpan.FromHours(8));
			t0.LLT_SystemCreateTimeUtc = new DateTime(2024, 7, 4);

			// t1 should have been included in the previous PRO, not included in this REP
			var t1 = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			t1.LLT_GS_Staff = staff.PK;
			t1.LLT_LeaveType = "ANN";
			t1.LLT_TransactionType = "MAN";
			t1.LLT_DeltaValueHours = 7;
			t1.LLT_Accrual = new ZDateTimeOffset(2024, 9, 10, 23, 59, 59, TimeSpan.FromHours(8));
			t1.LLT_SystemCreateTimeUtc = new DateTime(2024, 7, 4);

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_Status = LeaveProcessingRunStatusCodes.Queued;
			run.LLR_RN_NKCountry = "CN";
			run.LLR_SystemCreateTimeUtc = new DateTime(2024, 7, 4);
			run.LLR_ProcessTo = new ZDate(2024, 9, 30);

			// create an HrlBalanceAffectingQueue to trigger REP
			var queue = Factory.NewWithValidTestData<HrlBalanceAffectingQueue>();
			queue.LLQ_GS_Staff = staff.PK;

			Factory.Save();

			var balanceResults0 = new BalanceTransaction { EffectiveDate = new DateTime(2024, 8, 31), LeaveType = "ANN", NonForfeitureHours = 87m };
			var forecastData0 = new LeaveForecasterResult { BalanceTransactions = new[] { balanceResults0 } };
			var balanceResults1 = new BalanceTransaction { EffectiveDate = new DateTime(2024, 9, 10), LeaveType = "ANN", NonForfeitureHours = 91.44m };
			var forecastData1 = new LeaveForecasterResult { BalanceTransactions = new[] { balanceResults1 } };
			var balanceResults2 = new BalanceTransaction { EffectiveDate = new DateTime(2024, 9, 30), LeaveType = "ANN", NonForfeitureHours = 100.33m };
			var forecastData2 = new LeaveForecasterResult { BalanceTransactions = new[] { balanceResults2 } };

			var mockForecaster = new Mock<ILeaveForecaster>(MockBehavior.Strict);

			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2024, 8, 31, 23, 59, 59), null, false, true))
				.Returns(forecastData0);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2024, 9, 10, 23, 59, 59), null, false, true))
				.Returns(forecastData1);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2024, 9, 30), It.IsAny<DateTime?>(), false, true))
				.Returns(forecastData2);

			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2024, 8, 31, 23, 59, 59), null, true, true))
				.Returns(forecastData0);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2024, 9, 10, 23, 59, 59), null, true, true))
				.Returns(forecastData1);
			mockForecaster.Setup(m => m.Forecast("BBY", new DateTime(2024, 9, 30), It.IsAny<DateTime?>(), true, true))
				.Returns(forecastData2);

			var logs = new List<string>();
			var logger = new DummyLogger();
			logger.OnLog += (o, e) => logs.Add($"{e.Type}: {e.Message}");
			var task = new StaffRegionProcessServiceTask(mockForecaster.Object) { ServiceLogger = logger };

			var staffToProcess = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run));
			AssertCollectionContains("PRE: Staff will be processed", "BBY", staffToProcess.Select(s => s.GS_Code));

			task.RunTask();

			#region Assert
			var transactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run.PK));
			AssertEquals(4, transactions.Length);

			var proTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.Processing).FirstOrDefault();
			var repTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.Reprocessing).FirstOrDefault();
			var fprTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).FirstOrDefault();
			var frrTransactions = transactions.Where(x => x.LLT_TransactionType == BalanceTransactionTypes.FinanceReprocessing).FirstOrDefault();

			AssertNotNull(proTransactions);
			AssertNotNull(repTransactions);
			AssertNotNull(fprTransactions);
			AssertNotNull(frrTransactions);

			var processingAccrualDate = new ZDateTimeOffset(2024, 9, 30, 23, 59, 59, TimeSpan.FromHours(8));
			var reprocessingAccrualDate = new ZDateTimeOffset(2024, 9, 10, 23, 59, 59, TimeSpan.FromHours(8));

			AssertDeltaHourAndAccrual(BalanceTransactionTypes.Processing, proTransactions, 8.89m, processingAccrualDate);
			AssertDeltaHourAndAccrual(BalanceTransactionTypes.Reprocessing, repTransactions, 11.44m, reprocessingAccrualDate);
			AssertDeltaHourAndAccrual(BalanceTransactionTypes.FinanceProcessing, fprTransactions, 8.89m, processingAccrualDate);
			AssertDeltaHourAndAccrual(BalanceTransactionTypes.FinanceReprocessing, frrTransactions, 91.44m, reprocessingAccrualDate);

			#endregion
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Success_WithLargeDeltaValueHours()
		{
			//process a staff successfully
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			RunServiceTask(staff, out var _, deltaAmount: 9999.9999999m);

			//Test balance transaction records
			var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

			AssertEquals(4, balanceTransactions.Length);

			Assert(balanceTransactions.Where(x => x.LLT_DeltaValueHours == 10.1m && x.LLT_TransactionType == BalanceTransactionTypes.Processing).Any());
			Assert(balanceTransactions.Where(x => x.LLT_DeltaValueHours == 9999.9999999m && x.LLT_TransactionType == BalanceTransactionTypes.Processing).Any());

			Assert(balanceTransactions.Where(x => x.LLT_DeltaValueHours == 10.1m && x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).Any());
			Assert(balanceTransactions.Where(x => x.LLT_DeltaValueHours == 9999.9999999m && x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).Any());

			//Test no error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));

			AssertEquals(0, balanceAffectingLogs.Length);

			//Test the processing run is deactivated
			var process2 = new StaffRegionProcessServiceTask();
			var logger2 = new TestServiceLogger();
			process2.ServiceLogger = logger2;
			process2.RunTask();

			var expectedLog2 = $@"Information|No processable processing run found.
";
			AssertEquals(expectedLog2, logger2.ToString());
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Success_NewRunWithSmallerDate_DoNotProcessStaff()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			RunServiceTask(staff, out var _);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo.AddDays(-1);

			Factory.Save();

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run2.PK}.
Information|Processing Run {run2.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK));
			AssertEquals(0, balanceTransactions.Length);
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Success_NewRunWithLargerDate_ProcessStaff()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-18);
			var processTo1 = new ZDateTimeOffset(run1.LLR_ProcessTo.Year, run1.LLR_ProcessTo.Month, run1.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staff, out var _);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo.AddDays(1);
			var processTo2 = new ZDateTimeOffset(run2.LLR_ProcessTo.Year, run2.LLR_ProcessTo.Month, run2.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run2.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Debug|Processing TSS succeed to {processTo2}
Information|Batched [1] staff were successfully processed.
Information|Processing Run {run2.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK));
			AssertEquals(4, balanceTransactions.Length);

			var proTransactions1 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == ZDateTimeOffset.Empty &&
				x.LLT_TransactionType == "PRO").FirstOrDefault();
			var proTransactions2 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)) &&
				x.LLT_TransactionType == "PRO").FirstOrDefault();

			var fprTransactions1 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == ZDateTimeOffset.Empty &&
				x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).FirstOrDefault();

			var fprTransactions2 = balanceTransactions.Where(x =>
				x.LLT_Forfeiture == new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)) &&
				x.LLT_TransactionType == BalanceTransactionTypes.FinanceProcessing).FirstOrDefault();

			AssertNotNull(proTransactions1);
			AssertNotNull(proTransactions2);
			AssertNotNull(fprTransactions1);
			AssertNotNull(fprTransactions2);

			AssertBalanceTransaction(proTransactions1, staff, run2, "LT1", processTo2, "Processing", 0m, "PRO");
			AssertBalanceTransaction(proTransactions2, staff, run2, "LT1", processTo2, "Processing", 0m, "PRO");
			AssertBalanceTransaction(fprTransactions1, staff, run2, "LT1", processTo2, "Processing", 0m, "FPR");
			AssertBalanceTransaction(fprTransactions2, staff, run2, "LT1", processTo2, "Processing", 0m, "FPR");
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Success_NewRunWithSameDate_DoNotProcessStaff()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			RunServiceTask(staff, out var _);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo;

			Factory.Save();

			var staffToProcess = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run2));
			AssertContainsExactElementsInAnyOrder("PRE: Will consider TSS", new[] { "TSS" }, staffToProcess.Select(s => s.GS_Code));

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run2.PK}.
Information|Batched [1] staff to process
Information|Skip TSS
Information|Batched [1] staff were successfully processed.
Information|Processing Run {run2.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK));
			AssertEquals(0, balanceTransactions.Length);
		}

		void RunServiceTask(GlbStaff staff, out TestServiceLogger logger, TestServiceTaskType testServiceTaskType = TestServiceTaskType.ServiceTask, bool mockForecast = true, decimal deltaAmount = 234.5m)
			=> RunServiceTask(new GlbStaff[] { staff }, out logger, testServiceTaskType, mockForecast, deltaAmount);

		void RunServiceTask(GlbStaff[] staffs, out TestServiceLogger logger, TestServiceTaskType testServiceTaskType = TestServiceTaskType.ServiceTask, bool mockForecast = true, decimal deltaAmount = 234.5m)
		{
			var serviceTask = GetServiceTask(testServiceTaskType);

			var forecastDates = GetForecastDates(mockForecast);

			logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;

			var clientFactoryMock = MockForecastResponse(staffs, forecastDates, deltaAmount, Factory);
			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("SRP", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		DateTime[] GetForecastDates(bool mockForecast)
		{
			var processingRunQuery = new ZQuery(HrlProcessingRunSchema.LLR_Status,
			new[]
			{
				LeaveProcessingRunStatusCodes.Queued,
				LeaveProcessingRunStatusCodes.InProgress
			})
			{
				OrderBy = $"{nameof(HrlProcessingRunSchema.LLR_SystemCreateTimeUtc)} ASC"
			};

			var processRuns = Factory.Load<HrlProcessingRun>(processingRunQuery);
			var forecastDates = mockForecast ? processRuns.Select(r => r.LLR_ProcessTo.ToDateTime()).ToArray() : null;
			return forecastDates;
		}

		StaffRegionProcessServiceTask GetServiceTask(TestServiceTaskType testServiceTaskType)
		{
			switch (testServiceTaskType)
			{
				case TestServiceTaskType.ServiceTaskForBatchTest:
					return new StaffRegionProcessServiceTaskForBatchTest();

				case TestServiceTaskType.ServiceTaskForTestWithFactory:
					return new StaffRegionProcessServiceTaskForTest(Factory);

				default:
					return new StaffRegionProcessServiceTask();
			}
		}

		enum TestServiceTaskType
		{
			ServiceTaskForBatchTest,
			ServiceTaskForTestWithFactory,
			ServiceTask,
		}

		static Mock<IGlowServiceClientFactory> MockForecastResponse(GlbStaff[] staffs, DateTime[] forecastDates, decimal deltaAmount, BusinessObjectFactory factory)
		{
			// ILeaveForecaster is already an interface... why mock all the way down to http? sadists

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);

			foreach (var staff in staffs)
			{
				for (var i = 0; i < forecastDates?.Length; i++)
				{
					var lastAccrualDate = i == 0 ? (DateTime?)null : forecastDates[i - 1];
					var accrualDate = forecastDates[i];

					var glbStaffHolidaysFilter = new ZQuery(GlbStaffHolidaySchema.GA_GS, staff.PK);
					glbStaffHolidaysFilter.AddToFilter(JoinCondition.And, GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, accrualDate);
					var glbStaffHolidays = factory.Load<GlbStaffHoliday>(glbStaffHolidaysFilter);

					var returnData = GetLeaveForecasterResponse(deltaAmount, glbStaffHolidays, accrualDate, lastAccrualDate);

					var urlAccrual = $"api/leave/forecast?staffCode={staff.GS_Code}&accrualDate={accrualDate:yyyy-MM-dd}&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false";
					clientMock.Setup(c => c.GetAsync(urlAccrual)).Returns(
						Task.FromResult(
							new HttpResponseMessage()
							{
								Content = new StringContent(JsonConvert.SerializeObject(returnData))
							}));

					clientMock.Setup(c => c.GetAsync($"api/leave/forecast?staffCode={staff.GS_Code}&accrualDate={accrualDate:yyyy-MM-dd}&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=true")).Returns(
						Task.FromResult(
							new HttpResponseMessage()
							{
								Content = new StringContent(JsonConvert.SerializeObject(returnData))
							}));

					if (lastAccrualDate != null)
					{
						var urlAccrualWithlastAccrualDate = $"api/leave/forecast?staffCode={staff.GS_Code}&accrualDate={accrualDate:yyyy-MM-dd}&onlyIncludeRequestedSnapshots=true&lastAccrualDate={lastAccrualDate:yyyy-MM-dd}&leaveOnAccrualDate=false";
						clientMock.Setup(c => c.GetAsync(urlAccrualWithlastAccrualDate)).Returns(
							Task.FromResult(
								new HttpResponseMessage()
								{
									Content = new StringContent(JsonConvert.SerializeObject(returnData))
								}));

						clientMock.Setup(c => c.GetAsync($"api/leave/forecast?staffCode={staff.GS_Code}&accrualDate={accrualDate:yyyy-MM-dd}&onlyIncludeRequestedSnapshots=true&lastAccrualDate={lastAccrualDate:yyyy-MM-dd}&leaveOnAccrualDate=true")).Returns(
							Task.FromResult(
								new HttpResponseMessage()
								{
									Content = new StringContent(JsonConvert.SerializeObject(returnData))
								}));
					}
				}
			}

			return clientFactoryMock;
		}

		static LeaveForecasterResult GetLeaveForecasterResponse(decimal deltaAmount, GlbStaffHoliday[] holidays, DateTime accrualDate, DateTime? lastAccrualDate)
		{
			var leaveForecasterResult = new LeaveForecasterResult
			{
				BalanceTransactions = new List<BalanceTransaction>
				{
					new BalanceTransaction() {
						EffectiveDate = accrualDate,
						ForfeitureBuckets = new List<PendingForfeiture> { new PendingForfeiture { Amount = deltaAmount, Name = "PFF1", Expiry = ZDateTimeOffset.Today.AddYears(1).ToDateTime() } },
						LeaveType = "LT1",
						NonForfeitureHours = 10.1m,
					}
				}
			};

			if (holidays?.Length > 0)
			{
				var leaveProcessedList = new List<LeaveProcessed>();

				foreach (var holiday in holidays)
				{
					var holidayStartTime = holiday.GA_StartTime.ToDateTime();
					var holidayEndTime = holiday.GA_EndTime.ToDateTime();

					leaveProcessedList.Add(new LeaveProcessed
					{
						LeaveType = "Annual",
						TotalHours = 120,
						TotalProcessedHours = 80,
						HoursProcessedInThisPeriod = 20,
						ProcessedFrom = GetProcessedFrom(lastAccrualDate, holidayStartTime, holidayEndTime),
						ProcessedTo = accrualDate > holidayEndTime ? holidayEndTime : accrualDate,
						ProcessedDate = ZDateTime.Today.ToDateTime(),
						AccrualDate = accrualDate,
						FreeText = holiday.PK.ToString(),
					});
				}

				leaveForecasterResult.LeaveProcessed = leaveProcessedList;
			}

			return leaveForecasterResult;
		}

		static DateTime GetProcessedFrom(DateTime? lastAccrualDate, DateTime holidayStartTime, DateTime holidayEndTime)
		{
			if (lastAccrualDate.HasValue)
			{
				var accrualProcessFromTime = lastAccrualDate.Value.AddDays(1);
				if (accrualProcessFromTime > holidayStartTime && accrualProcessFromTime < holidayEndTime)
				{
					return accrualProcessFromTime;
				}
			}

			return holidayStartTime;
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Failed()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staff, out var logger, mockForecast: false);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Error|Processing TSS Error: Error getting response from api/leave/forecast?staffCode=TSS&accrualDate=2022-04-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Information|Processing TSS
Error|Processing TSS Error: Error getting response from api/leave/forecast?staffCode=TSS&accrualDate=2022-04-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Information|Processing TSS
Error|Processing TSS Error: Error getting response from api/leave/forecast?staffCode=TSS&accrualDate=2022-04-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Error|Processing TSS Failed
Error|[0] staff processed successfully, [1] consecutive staff had errors. Batch processing failed.
Information|Processing Run {run.PK} is COR.
";
			AssertEquals(expectedLog, logger.ToString());

			//Test balance transactions
			var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

			AssertEquals(1, balanceTransactions.Length);

			AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
			AssertEquals("ERR", balanceTransactions[0].LLT_LeaveType);
			AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
			AssertEquals(processTo, balanceTransactions[0].LLT_Accrual);
			AssertEquals(run.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
			AssertEquals("Error: Row could not be processed, it will be processed in the next run", balanceTransactions[0].LLT_Comment);
			AssertEquals(0m, balanceTransactions[0].LLT_DeltaValueHours);
			AssertEquals("ERR", balanceTransactions[0].LLT_TransactionType);

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));

			AssertEquals(1, balanceAffectingLogs.Length);
			AssertEquals(1, balanceAffectingQueues.Length);

			AssertEquals(staff.PK, balanceAffectingLogs[0].LLB_GS_Staff);
			AssertEquals(processTo, balanceAffectingLogs[0].LLB_EffectiveDate);
			AssertEquals("Error getting response from ap", balanceAffectingLogs[0].LLB_Reference);

			AssertEquals(staff.PK, balanceAffectingQueues[0].LLQ_GS_Staff);
			AssertEquals(processTo, balanceAffectingQueues[0].LLQ_EffectiveDate);
			AssertEquals("Error getting response from ap", balanceAffectingQueues[0].LLQ_Reference);

			var process = new StaffRegionProcessServiceTask();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			process.RunTask();

			expectedLog = $@"Information|No processable processing run found.
";
			AssertEquals(expectedLog, logger.ToString());
		}

		public void TestProcessSingleStaff_Failed_NewRunWithSmallerDate_DoNotProcessStaff()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			RunServiceTask(staff, out var _);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo.AddDays(-1);

			Factory.Save();

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run2.PK}.
Information|Processing Run {run2.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK));
			AssertEquals(0, balanceTransactions.Length);
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Failed_NewRunWithLargerDate_ReprocessingStaff()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-18);
			var processTo1 = new ZDateTimeOffset(run1.LLR_ProcessTo.Year, run1.LLR_ProcessTo.Month, run1.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staff, out var _, mockForecast: false);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo.AddDays(1);
			var processTo2 = new ZDateTimeOffset(run2.LLR_ProcessTo.Year, run2.LLR_ProcessTo.Month, run2.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run2.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Debug|Reprocessing TSS to {processTo1}.
Debug|Processing TSS succeed to {processTo2}
Information|Batched [1] staff were successfully processed.
Information|Processing Run {run2.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK));
			AssertEquals(FormatTransactions(balanceTransactions), 8, balanceTransactions.Length);

			AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
			AssertEquals("LT1", balanceTransactions[0].LLT_LeaveType);
			AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
			AssertEquals(processTo1, balanceTransactions[0].LLT_Accrual);
			AssertEquals(run2.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
			AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[0].LLT_Comment);
			AssertEquals(10.1m, balanceTransactions[0].LLT_DeltaValueHours);
			AssertEquals("REP", balanceTransactions[0].LLT_TransactionType);

			AssertEquals(staff.PK, balanceTransactions[1].LLT_GS_Staff);
			AssertEquals("LT1", balanceTransactions[1].LLT_LeaveType);
			AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[1].LLT_Forfeiture);
			AssertEquals(processTo2, balanceTransactions[1].LLT_Accrual);
			AssertEquals(run2.PK, balanceTransactions[1].LLT_LLR_ProcessingRun);
			AssertEquals("Processing", balanceTransactions[1].LLT_Comment);
			AssertEquals(0m, balanceTransactions[1].LLT_DeltaValueHours);
			AssertEquals("PRO", balanceTransactions[1].LLT_TransactionType);

			AssertEquals(staff.PK, balanceTransactions[4].LLT_GS_Staff);
			AssertEquals("LT1", balanceTransactions[4].LLT_LeaveType);
			AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[4].LLT_Forfeiture);
			AssertEquals(processTo1, balanceTransactions[4].LLT_Accrual);
			AssertEquals(run2.PK, balanceTransactions[4].LLT_LLR_ProcessingRun);
			AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[4].LLT_Comment);
			AssertEquals(234.5m, balanceTransactions[4].LLT_DeltaValueHours);
			AssertEquals("REP", balanceTransactions[4].LLT_TransactionType);

			AssertEquals(staff.PK, balanceTransactions[5].LLT_GS_Staff);
			AssertEquals("LT1", balanceTransactions[5].LLT_LeaveType);
			AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[5].LLT_Forfeiture);
			AssertEquals(processTo2, balanceTransactions[5].LLT_Accrual);
			AssertEquals(run2.PK, balanceTransactions[5].LLT_LLR_ProcessingRun);
			AssertEquals("Processing", balanceTransactions[5].LLT_Comment);
			AssertEquals(0m, balanceTransactions[5].LLT_DeltaValueHours);
			AssertEquals("PRO", balanceTransactions[5].LLT_TransactionType);

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));

			AssertEquals(1, balanceAffectingLogs.Length);
			AssertEquals(0, balanceAffectingQueues.Length);
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_Failed_NewRunWithSameDate_ReprocessStaff()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-18);
			var processTo = new ZDateTimeOffset(run1.LLR_ProcessTo.Year, run1.LLR_ProcessTo.Month, run1.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staff, out var _, mockForecast: false);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo;

			Factory.Save();

			//Test new run can get the error staff
			var staffs = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run2));
			AssertEquals(1, staffs.Length);

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run2.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Debug|Reprocessing TSS to {processTo}.
Debug|Processing TSS succeed to {processTo}
Information|Batched [1] staff were successfully processed.
Information|Processing Run {run2.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK));
			AssertEquals(FormatTransactions(balanceTransactions), 4, balanceTransactions.Length);

			AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
			AssertEquals("LT1", balanceTransactions[0].LLT_LeaveType);
			AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
			AssertEquals(processTo, balanceTransactions[0].LLT_Accrual);
			AssertEquals(run2.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
			AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[0].LLT_Comment);
			AssertEquals(10.1m, balanceTransactions[0].LLT_DeltaValueHours);
			AssertEquals("REP", balanceTransactions[0].LLT_TransactionType);

			AssertEquals(staff.PK, balanceTransactions[2].LLT_GS_Staff);
			AssertEquals("LT1", balanceTransactions[2].LLT_LeaveType);
			AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[2].LLT_Forfeiture);
			AssertEquals(processTo, balanceTransactions[2].LLT_Accrual);
			AssertEquals(run2.PK, balanceTransactions[2].LLT_LLR_ProcessingRun);
			AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[2].LLT_Comment);
			AssertEquals(234.5m, balanceTransactions[2].LLT_DeltaValueHours);
			AssertEquals("REP", balanceTransactions[2].LLT_TransactionType);

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));

			AssertEquals(1, balanceAffectingLogs.Length);
			AssertEquals(0, balanceAffectingQueues.Length);
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessSingleStaff_SaveProcessingLogsForLeaveRecords()
		{
			//process a staff successfully
			var date = new DateTime(2023, 10, 15, 0, 0, 0);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = date.AddYears(-3);

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy, date.AddYears(-3), null);

			var staffHoliday1 = CreateStaffHoliday(staff, new DateTime(2023, 11, 5, 0, 0, 0), new DateTime(2023, 12, 5, 0, 0, 0));
			var staffHoliday2 = CreateStaffHoliday(staff, new DateTime(2023, 12, 5, 0, 0, 0), new DateTime(2024, 1, 5, 0, 0, 0));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "AU";
			run.LLR_ProcessTo = (ZDate)date.AddMonths(3);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(11));

			Factory.Save();

			RunServiceTask(staff, out var logger);

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Debug|Processing TSS succeed to {processTo}
Information|Batched [1] staff were successfully processed.
Information|Processing Run {run.PK} is COM.
";
			AssertEquals(expectedLog, logger.ToString());

			//Test leave processed logs
			var offset = StaffTimezoneOffsetProvider.GetOffset(Factory, staff.PK, new DateTime(2023, 11, 5));
			var leaveProcessedLogs = Factory.Load<HrlLeaveProcessedLog>(new ZDBOnlyQuery(typeof(HrlLeaveProcessedLog)));

			AssertEquals(2, leaveProcessedLogs.Length);

			AssertEquals(20m, leaveProcessedLogs[0].LLD_ProcessedThisPeriodHours);
			AssertEquals(new ZDateTimeOffset(2023, 11, 5, 0, 0, 0, offset), leaveProcessedLogs[0].LLD_ProcessedFrom);
			AssertEquals(new ZDateTimeOffset(2023, 12, 5, 0, 0, 0, offset), leaveProcessedLogs[0].LLD_ProcessedTo);
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[0].LLD_SupersededOn);
			AssertEquals(staffHoliday1.PK, leaveProcessedLogs[0].LLD_GA_StaffHoliday);
			AssertEquals(run.PK, leaveProcessedLogs[0].LLD_LLR_ProcessingRun);

			AssertEquals(20m, leaveProcessedLogs[1].LLD_ProcessedThisPeriodHours);
			AssertEquals(new ZDateTimeOffset(2023, 12, 5, 0, 0, 0, offset), leaveProcessedLogs[1].LLD_ProcessedFrom);
			AssertEquals(new ZDateTimeOffset(2024, 1, 5, 0, 0, 0, offset), leaveProcessedLogs[1].LLD_ProcessedTo);
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[1].LLD_SupersededOn);
			AssertEquals(staffHoliday2.PK, leaveProcessedLogs[1].LLD_GA_StaffHoliday);
			AssertEquals(run.PK, leaveProcessedLogs[1].LLD_LLR_ProcessingRun);
		}

		public void TestProcessSingleStaff_SaveSingleLogForLeaveRecord()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy, ZDateTimeOffset.Today.AddYears(-3), null);

			var staffHoliday = CreateStaffHoliday(staff, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(-1));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "AU";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-1);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "AU";
			run2.LLR_ProcessTo = ZDate.Today;

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Third Run
			var run3 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3.LLR_RN_NKCountry = "AU";
			run3.LLR_ProcessTo = ZDate.Today.AddMonths(1);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Forth Run
			var run4 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4.LLR_RN_NKCountry = "AU";
			run4.LLR_ProcessTo = ZDate.Today.AddMonths(2);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Fifth Run
			var run5 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5.LLR_RN_NKCountry = "AU";
			run5.LLR_ProcessTo = ZDate.Today.AddMonths(3);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Test leave processed logs
			var leaveProcessedLogs = Factory.Load<HrlLeaveProcessedLog>(new ZDBOnlyQuery(typeof(HrlLeaveProcessedLog))).OrderBy(x => x.LLD_SystemCreateTimeUtc).ToArray();

			AssertEquals(1, leaveProcessedLogs.Length);

			//run2 processes leave
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[0].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[0].LLD_GA_StaffHoliday);
			AssertEquals(staffHoliday.GA_StartTime, leaveProcessedLogs[0].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(staffHoliday.GA_EndTime, leaveProcessedLogs[0].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run2.PK, leaveProcessedLogs[0].LLD_LLR_ProcessingRun);
		}

		public void TestProcessSingleStaff_MultipleLeaverLogSpanningAccruals()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy, ZDateTimeOffset.Today.AddYears(-3), null);

			var staffHoliday = CreateStaffHoliday(staff, ZDate.Today.AddDays(-10), ZDate.Today.AddDays(50));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "AU";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-1);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "AU";
			run2.LLR_ProcessTo = ZDate.Today;

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Third Run
			var run3 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3.LLR_RN_NKCountry = "AU";
			run3.LLR_ProcessTo = ZDate.Today.AddMonths(1);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Forth Run
			var run4 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4.LLR_RN_NKCountry = "AU";
			run4.LLR_ProcessTo = ZDate.Today.AddMonths(2);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Fifth Run
			var run5 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5.LLR_RN_NKCountry = "AU";
			run5.LLR_ProcessTo = ZDate.Today.AddMonths(3);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Test leave processed logs
			var leaveProcessedLogs = Factory.Load<HrlLeaveProcessedLog>(new ZDBOnlyQuery(typeof(HrlLeaveProcessedLog))).OrderBy(x => x.LLD_SystemCreateTimeUtc).ToArray();

			AssertEquals(3, leaveProcessedLogs.Length);

			//run2 processes the first part of the leave
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[0].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[0].LLD_GA_StaffHoliday);
			AssertEquals(staffHoliday.GA_StartTime, leaveProcessedLogs[0].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(run2.LLR_ProcessTo, leaveProcessedLogs[0].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run2.PK, leaveProcessedLogs[0].LLD_LLR_ProcessingRun);

			//run3 processes the second part of the leave
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[1].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[1].LLD_GA_StaffHoliday);
			AssertEquals(run2.LLR_ProcessTo.AddDays(1), leaveProcessedLogs[1].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(run3.LLR_ProcessTo, leaveProcessedLogs[1].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run3.PK, leaveProcessedLogs[1].LLD_LLR_ProcessingRun);

			//run4 processes the last part of the leave
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[2].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[2].LLD_GA_StaffHoliday);
			AssertEquals(run3.LLR_ProcessTo.AddDays(1), leaveProcessedLogs[2].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(staffHoliday.GA_EndTime, leaveProcessedLogs[2].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run4.PK, leaveProcessedLogs[2].LLD_LLR_ProcessingRun);
		}

		public void TestProcessSingleStaff_SupersedeProcessingLogsOnReprocess()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy, ZDateTimeOffset.Today.AddYears(-3), null);

			var staffHoliday1 = CreateStaffHoliday(staff, ZDate.Today.AddMonths(-4), ZDate.Today.AddMonths(-4));
			var staffHoliday2 = CreateStaffHoliday(staff, ZDate.Today.AddMonths(-2), ZDate.Today.AddMonths(-2));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "AU";
			run1.LLR_ProcessTo = ZDate.Today;

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			_ = CreateBalanceAffectingQueue(staff, ZDateTimeOffset.Today.AddMonths(-3));
			_ = CreateBalanceAffectingQueue(staff, ZDateTimeOffset.Today.AddMonths(-2));

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "AU";
			run2.LLR_ProcessTo = ZDate.Today.AddMonths(1);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Test balance affection queue
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));
			AssertEquals(0, balanceAffectingQueues.Length);

			//Test leave processed logs
			var leaveProcessedLogs = Factory.Load<HrlLeaveProcessedLog>(new ZDBOnlyQuery(typeof(HrlLeaveProcessedLog))).OrderBy(x => x.LLD_SystemCreateTimeUtc).ToArray();

			AssertEquals(3, leaveProcessedLogs.Length);

			//Holiday 1 is processed by run1
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[0].LLD_SupersededOn);
			AssertEquals(staffHoliday1.PK, leaveProcessedLogs[0].LLD_GA_StaffHoliday);
			AssertEquals(staffHoliday1.GA_StartTime, leaveProcessedLogs[0].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(staffHoliday1.GA_EndTime, leaveProcessedLogs[0].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run1.PK, leaveProcessedLogs[0].LLD_LLR_ProcessingRun);

			//Holiday 2 is processed by run 1 and superseded by run2
			AssertNotEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[1].LLD_SupersededOn);
			AssertEquals(staffHoliday2.PK, leaveProcessedLogs[1].LLD_GA_StaffHoliday);
			AssertEquals(staffHoliday2.GA_StartTime, leaveProcessedLogs[1].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(staffHoliday2.GA_EndTime, leaveProcessedLogs[1].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run1.PK, leaveProcessedLogs[1].LLD_LLR_ProcessingRun);

			//Holiday 2 has a new processed log from run2
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[2].LLD_SupersededOn);
			AssertEquals(staffHoliday2.PK, leaveProcessedLogs[2].LLD_GA_StaffHoliday);
			AssertEquals(staffHoliday2.GA_StartTime, leaveProcessedLogs[2].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(staffHoliday2.GA_EndTime, leaveProcessedLogs[2].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run2.PK, leaveProcessedLogs[2].LLD_LLR_ProcessingRun);
		}

		public void TestProcessSingleStaff_SupersedeMultipleLeaverLogSpanningAccrualsOnReprocess()
		{
			//Init Data
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy = Factory.NewWithValidTestData<HrlPolicy>();
			policy.LLP_RN_NKCountry = "AU";

			_ = CreateStaffPolicy(staff, policy, ZDateTimeOffset.Today.AddYears(-3), null);

			var staffHoliday = CreateStaffHoliday(staff, ZDate.Today.AddDays(-10), ZDate.Today.AddDays(50));

			//First Run
			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "AU";
			run1.LLR_ProcessTo = ZDate.Today.AddMonths(-1);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Second Run
			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "AU";
			run2.LLR_ProcessTo = ZDate.Today;

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Third Run
			_ = CreateBalanceAffectingQueue(staff, ZDateTimeOffset.Today.AddMonths(-3));

			var run3 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run3.LLR_RN_NKCountry = "AU";
			run3.LLR_ProcessTo = ZDate.Today.AddMonths(1);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Forth Run
			_ = CreateBalanceAffectingQueue(staff, ZDateTimeOffset.Today.AddMonths(-2));

			var run4 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run4.LLR_RN_NKCountry = "AU";
			run4.LLR_ProcessTo = ZDate.Today.AddMonths(2);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Fifth Run
			_ = CreateBalanceAffectingQueue(staff, ZDateTimeOffset.Today.AddMonths(-1));

			var run5 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run5.LLR_RN_NKCountry = "AU";
			run5.LLR_ProcessTo = ZDate.Today.AddMonths(3);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Sixth Run
			var run6 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run6.LLR_RN_NKCountry = "AU";
			run6.LLR_ProcessTo = ZDate.Today.AddMonths(4);

			Factory.Save();

			RunServiceTask(staff, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForTestWithFactory);

			//Test leave processed logs
			var leaveProcessedLogs = Factory.Load<HrlLeaveProcessedLog>(new ZDBOnlyQuery(typeof(HrlLeaveProcessedLog))).OrderBy(x => x.LLD_SystemCreateTimeUtc).ToArray();

			AssertEquals(4, leaveProcessedLogs.Length);

			//run2 processes the first part of the leave and been superseded by run3
			AssertNotEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[0].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[0].LLD_GA_StaffHoliday);
			AssertEquals(staffHoliday.GA_StartTime, leaveProcessedLogs[0].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(run2.LLR_ProcessTo, leaveProcessedLogs[0].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run2.PK, leaveProcessedLogs[0].LLD_LLR_ProcessingRun);

			//run3 processes the leave again and been superseded by run4
			AssertNotEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[1].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[1].LLD_GA_StaffHoliday);
			AssertEquals(run2.LLR_ProcessTo.AddDays(1).ToDateTime(), leaveProcessedLogs[1].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(run3.LLR_ProcessTo, leaveProcessedLogs[1].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run3.PK, leaveProcessedLogs[1].LLD_LLR_ProcessingRun);

			//run4 processes the leave again and been superseded by run5
			AssertNotEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[2].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[2].LLD_GA_StaffHoliday);
			AssertEquals(run3.LLR_ProcessTo.AddDays(1).ToDateTime(), leaveProcessedLogs[2].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(staffHoliday.GA_EndTime, leaveProcessedLogs[2].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run4.PK, leaveProcessedLogs[2].LLD_LLR_ProcessingRun);

			//run5 processes the leave again
			AssertEquals(ZDateTimeOffset.Empty, leaveProcessedLogs[3].LLD_SupersededOn);
			AssertEquals(staffHoliday.PK, leaveProcessedLogs[3].LLD_GA_StaffHoliday);
			AssertEquals(staffHoliday.GA_StartTime, leaveProcessedLogs[3].LLD_ProcessedFrom.ToDateTime());
			AssertEquals(staffHoliday.GA_EndTime, leaveProcessedLogs[3].LLD_ProcessedTo.ToDateTime());
			AssertEquals(run5.PK, leaveProcessedLogs[3].LLD_LLR_ProcessingRun);
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessMultipleStaff()
		{
			var staffCount = 10;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "TS" + i;
				staff.GS_EmploymentDate = ZDate.Today.AddYears(-5 + i);

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddYears(1);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staffs, out var logger);

			var processedStaffs = staffs.Where(s => s.GS_EmploymentDate < run.LLR_ProcessTo);

			//Test log
			var expectedLog = $"Information|Batched [{processedStaffs.Count()}] staff to process";
			AssertContains(expectedLog, logger.ToString());

			foreach (var staff in processedStaffs)
			{
				expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Processing {staff.GS_Code} succeed to {processTo}
";
				AssertContains(expectedLog, logger.ToString());

				//Test balance transactions
				var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_GS_Staff, staff.PK);

				var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

				AssertEquals(4, balanceTransactions.Length);

				AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
				AssertEquals("LT1", balanceTransactions[0].LLT_LeaveType);
				AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
				AssertEquals(processTo, balanceTransactions[0].LLT_Accrual);
				AssertEquals(run.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
				AssertEquals("Processing", balanceTransactions[0].LLT_Comment);
				AssertEquals(10.1m, balanceTransactions[0].LLT_DeltaValueHours);
				AssertEquals("PRO", balanceTransactions[0].LLT_TransactionType);

				AssertEquals(staff.PK, balanceTransactions[2].LLT_GS_Staff);
				AssertEquals("LT1", balanceTransactions[2].LLT_LeaveType);
				AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[2].LLT_Forfeiture);
				AssertEquals(processTo, balanceTransactions[2].LLT_Accrual);
				AssertEquals(run.PK, balanceTransactions[2].LLT_LLR_ProcessingRun);
				AssertEquals("Processing", balanceTransactions[2].LLT_Comment);
				AssertEquals(234.5m, balanceTransactions[2].LLT_DeltaValueHours);
				AssertEquals("PRO", balanceTransactions[2].LLT_TransactionType);
			}

			expectedLog = $@"Information|Batched [{processedStaffs.Count()}] staff were successfully processed.
Information|Processing Run {run.PK} is COM.";
			AssertContains(expectedLog, logger.ToString());

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));

			AssertEquals(0, balanceAffectingLogs.Length);

			//Test run service again
			var process = new StaffRegionProcessServiceTask();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			process.RunTask();

			expectedLog = $@"Information|No processable processing run found.
";
			AssertEquals(expectedLog, logger.ToString());

			//Test no more staff in queue
			staffs = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run));
			AssertEquals(0, staffs.Length);
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessMultipleStaff_ConsecutiveFailures_ProcessFailed()
		{
			var staffCount = 10;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "TS" + i;
				staff.GS_EmploymentDate = ZDateTime.Today.AddYears(-5 + i);

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddYears(1);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staffs, out var logger, mockForecast: false);

			//Test log
			var expectedLog = $@"Information|Batched [6] staff to process
";
			AssertContains(expectedLog, logger.ToString());

			//Test balance transactions
			var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

			AssertEquals(3, balanceTransactions.Length);

			foreach (var transaction in balanceTransactions)
			{
				var staff = staffs.First(s => s.PK == transaction.LLT_GS_Staff);

				expectedLog = $@"Information|Processing {staff.GS_Code}
Error|Processing {staff.GS_Code} Error: Error getting response from api/leave/forecast?staffCode={staff.GS_Code}&accrualDate=2024-10-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Information|Processing {staff.GS_Code}
Error|Processing {staff.GS_Code} Error: Error getting response from api/leave/forecast?staffCode={staff.GS_Code}&accrualDate=2024-10-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Information|Processing {staff.GS_Code}
Error|Processing {staff.GS_Code} Error: Error getting response from api/leave/forecast?staffCode={staff.GS_Code}&accrualDate=2024-10-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Error|Processing {staff.GS_Code} Failed";

				AssertContains(expectedLog, logger.ToString());

				AssertEquals("ERR", transaction.LLT_LeaveType);
				AssertEquals(ZDateTimeOffset.Empty, transaction.LLT_Forfeiture);
				AssertEquals(processTo, transaction.LLT_Accrual);
				AssertEquals(run.PK, transaction.LLT_LLR_ProcessingRun);
				AssertEquals("Error: Row could not be processed, it will be processed in the next run", transaction.LLT_Comment);
				AssertEquals(0m, transaction.LLT_DeltaValueHours);
				AssertEquals("ERR", transaction.LLT_TransactionType);
			}

			expectedLog = $@"Error|[0] staff processed successfully, [3] consecutive staff had errors. Batch processing failed.
Information|Processing Run {run.PK} is COR.
";

			AssertContains(expectedLog, logger.ToString());

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));

			AssertEquals(3, balanceAffectingLogs.Length);
			AssertEquals(3, balanceAffectingQueues.Length);

			foreach (var balanceAffectingLog in balanceAffectingLogs)
			{
				AssertEquals(processTo, balanceAffectingLog.LLB_EffectiveDate);
				AssertEquals("Error getting response from ap", balanceAffectingLog.LLB_Reference);
			}

			foreach (var balanceAffectingQueue in balanceAffectingQueues)
			{
				AssertEquals(processTo, balanceAffectingQueue.LLQ_EffectiveDate);
				AssertEquals("Error getting response from ap", balanceAffectingQueue.LLQ_Reference);
			}
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessMultipleStaff_OnlyTwoStaffFailed_ProcessComplated()
		{
			var staffCount = 10;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "TS" + i;
				staff.GS_EmploymentDate = ZDate.Today;

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddYears(2);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(11));
			Factory.Save();

			var succeedStaffs = staffs.Where(s => s.GS_Code != "TS5" && s.GS_Code != "TS6").ToArray();
			RunServiceTask(succeedStaffs, out var logger);

			//Test log
			var expectedLog = $@"Information|Batched [10] staff to process
";
			AssertContains(expectedLog, logger.ToString());

			//Test balance transactions
			var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

			AssertEquals(34, balanceTransactions.Length);

			foreach (var staff in staffs)
			{
				var transactions = balanceTransactions.Where(t => t.LLT_GS_Staff == staff.PK).ToArray();

				if (staff.GS_Code != "TS5" && staff.GS_Code != "TS6")
				{
					expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Processing {staff.GS_Code} succeed to {processTo}
";
					AssertContains(expectedLog, logger.ToString());

					AssertEquals(4, transactions.Length);

					AssertEquals(staff.PK, transactions[0].LLT_GS_Staff);
					AssertEquals("LT1", transactions[0].LLT_LeaveType);
					AssertEquals(ZDateTimeOffset.Empty, transactions[0].LLT_Forfeiture);
					AssertEquals(processTo, transactions[0].LLT_Accrual);
					AssertEquals(run.PK, transactions[0].LLT_LLR_ProcessingRun);
					AssertEquals("Processing", transactions[0].LLT_Comment);
					AssertEquals(10.1m, transactions[0].LLT_DeltaValueHours);
					AssertEquals("PRO", transactions[0].LLT_TransactionType);

					AssertEquals(staff.PK, transactions[2].LLT_GS_Staff);
					AssertEquals("LT1", transactions[2].LLT_LeaveType);
					AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), transactions[2].LLT_Forfeiture);
					AssertEquals(processTo, transactions[2].LLT_Accrual);
					AssertEquals(run.PK, transactions[2].LLT_LLR_ProcessingRun);
					AssertEquals("Processing", transactions[2].LLT_Comment);
					AssertEquals(234.5m, transactions[2].LLT_DeltaValueHours);
					AssertEquals("PRO", transactions[2].LLT_TransactionType);
				}
				else
				{
					expectedLog = $@"Information|Processing {staff.GS_Code}
Error|Processing {staff.GS_Code} Error: Error getting response from api/leave/forecast?staffCode={staff.GS_Code}&accrualDate=2025-10-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Information|Processing {staff.GS_Code}
Error|Processing {staff.GS_Code} Error: Error getting response from api/leave/forecast?staffCode={staff.GS_Code}&accrualDate=2025-10-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Information|Processing {staff.GS_Code}
Error|Processing {staff.GS_Code} Error: Error getting response from api/leave/forecast?staffCode={staff.GS_Code}&accrualDate=2025-10-05&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false.
Error|Processing {staff.GS_Code} Failed";

					AssertContains(expectedLog, logger.ToString());

					AssertEquals(1, transactions.Length);

					AssertEquals("ERR", transactions[0].LLT_LeaveType);
					AssertEquals(ZDateTimeOffset.Empty, transactions[0].LLT_Forfeiture);
					AssertEquals(processTo, transactions[0].LLT_Accrual);
					AssertEquals(run.PK, transactions[0].LLT_LLR_ProcessingRun);
					AssertEquals("Error: Row could not be processed, it will be processed in the next run", transactions[0].LLT_Comment);
					AssertEquals(0m, transactions[0].LLT_DeltaValueHours);
					AssertEquals("ERR", transactions[0].LLT_TransactionType);
				}
			}

			expectedLog = $@"Information|Batched [8] staff were successfully processed.
Error|[2] staff had errors.
Information|Processing Run {run.PK} is COR.
";

			AssertContains(expectedLog, logger.ToString());

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));

			AssertEquals(2, balanceAffectingLogs.Length);
			AssertEquals(2, balanceAffectingQueues.Length);

			foreach (var balanceAffectingLog in balanceAffectingLogs)
			{
				AssertEquals(processTo, balanceAffectingLog.LLB_EffectiveDate);
				AssertEquals("Error getting response from ap", balanceAffectingLog.LLB_Reference);
			}

			foreach (var balanceAffectingQueue in balanceAffectingQueues)
			{
				AssertEquals(processTo, balanceAffectingQueue.LLQ_EffectiveDate);
				AssertEquals("Error getting response from ap", balanceAffectingQueue.LLQ_Reference);
			}
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessMultipleStaff_HasFailedStaff_Reprocesse_SameDate()
		{
			var staffCount = 10;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "TS" + i;
				staff.GS_EmploymentDate = ZDate.Today;

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddYears(2);

			Factory.Save();

			var succeedStaffs = staffs.Where(s => s.GS_Code != "TS5" && s.GS_Code != "TS6").ToArray();
			RunServiceTask(succeedStaffs, out var _);

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo;
			var processTo = new ZDateTimeOffset(run2.LLR_ProcessTo.Year, run2.LLR_ProcessTo.Month, run2.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(11));

			Factory.Save();

			RunServiceTask(staffs, out var logger);

			//Test log
			var expectedLog = $@"Information|Batched [10] staff to process
";
			AssertContains(expectedLog, logger.ToString());

			foreach (var staff in staffs)
			{
				var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_GS_Staff, staff.PK);
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK);
				var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

				if (staff.GS_Code != "TS5" && staff.GS_Code != "TS6")
				{
					expectedLog = $@"Information|Skip {staff.GS_Code}
";
					AssertContains(expectedLog, logger.ToString());

					AssertEquals(0, balanceTransactions.Length);
				}
				else
				{
					expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Reprocessing {staff.GS_Code} to {processTo}.
";
					AssertContains(expectedLog, logger.ToString());

					AssertEquals(FormatTransactions(balanceTransactions), 4, balanceTransactions.Length);

					AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[0].LLT_LeaveType);
					AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
					AssertEquals(processTo, balanceTransactions[0].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
					AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[0].LLT_Comment);
					AssertEquals(10.1m, balanceTransactions[0].LLT_DeltaValueHours);
					AssertEquals("REP", balanceTransactions[0].LLT_TransactionType);

					AssertEquals(staff.PK, balanceTransactions[2].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[2].LLT_LeaveType);
					AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[2].LLT_Forfeiture);
					AssertEquals(processTo, balanceTransactions[2].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[2].LLT_LLR_ProcessingRun);
					AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[2].LLT_Comment);
					AssertEquals(234.5m, balanceTransactions[2].LLT_DeltaValueHours);
					AssertEquals("REP", balanceTransactions[2].LLT_TransactionType);
				}
			}

			expectedLog = $@"Information|Batched [10] staff were successfully processed.
Information|Processing Run {run2.PK} is COM.
";

			AssertContains(expectedLog, logger.ToString());

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));

			AssertEquals(2, balanceAffectingLogs.Length);
			AssertEquals(0, balanceAffectingQueues.Length);
		}

		static string FormatTransactions(IEnumerable<HrlBalanceTransaction> transactions)
			=> string.Join(System.Environment.NewLine, transactions.Select(t => $"{t.Staff.GS_Code} - {t.LLT_LeaveType} - {t.LLT_TransactionType}: {t.LLT_Comment}").OrderBy(l => l));

		[TestDate(2023, 10, 5)]
		public void TestProcessMultipleStaff_HasFailedStaff_Reprocesse_LargerDate()
		{
			var staffCount = 10;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "TS" + i;
				staff.GS_EmploymentDate = ZDate.Today;

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddYears(2);
			var processTo1 = new ZDateTimeOffset(run1.LLR_ProcessTo.Year, run1.LLR_ProcessTo.Month, run1.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(11));

			Factory.Save();

			var succeedStaffs = staffs.Where(s => s.GS_Code != "TS5" && s.GS_Code != "TS6").ToArray();
			RunServiceTask(succeedStaffs, out var _);

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = ZDate.Today.AddYears(2).AddDays(1);
			var processTo2 = new ZDateTimeOffset(run2.LLR_ProcessTo.Year, run2.LLR_ProcessTo.Month, run2.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(11));

			Factory.Save();

			RunServiceTask(staffs, out var logger);

			//Test log
			var expectedLog = $@"Information|Batched [10] staff to process
";
			AssertContains(expectedLog, logger.ToString());

			foreach (var staff in staffs)
			{
				var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_GS_Staff, staff.PK);
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK);
				var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

				if (staff.GS_Code != "TS5" && staff.GS_Code != "TS6")
				{
					expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Processing {staff.GS_Code} succeed to {processTo2}
";
					AssertContains(expectedLog, logger.ToString());

					AssertEquals(4, balanceTransactions.Length);

					AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[0].LLT_LeaveType);
					AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
					AssertEquals(processTo2, balanceTransactions[0].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
					AssertEquals("Processing", balanceTransactions[0].LLT_Comment);
					AssertEquals(0m, balanceTransactions[0].LLT_DeltaValueHours);
					AssertEquals("PRO", balanceTransactions[0].LLT_TransactionType);

					AssertEquals(staff.PK, balanceTransactions[2].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[2].LLT_LeaveType);
					AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[2].LLT_Forfeiture);
					AssertEquals(processTo2, balanceTransactions[2].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[2].LLT_LLR_ProcessingRun);
					AssertEquals("Processing", balanceTransactions[2].LLT_Comment);
					AssertEquals(0m, balanceTransactions[2].LLT_DeltaValueHours);
					AssertEquals("PRO", balanceTransactions[2].LLT_TransactionType);
				}
				else
				{
					expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Reprocessing {staff.GS_Code} to {processTo1}.
Debug|Processing {staff.GS_Code} succeed to {processTo2}
";
					AssertContains(expectedLog, logger.ToString());

					AssertEquals(8, balanceTransactions.Length);

					AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[0].LLT_LeaveType);
					AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
					AssertEquals(processTo1, balanceTransactions[0].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
					AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[0].LLT_Comment);
					AssertEquals(10.1m, balanceTransactions[0].LLT_DeltaValueHours);
					AssertEquals("REP", balanceTransactions[0].LLT_TransactionType);

					AssertEquals(staff.PK, balanceTransactions[1].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[1].LLT_LeaveType);
					AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[1].LLT_Forfeiture);
					AssertEquals(processTo2, balanceTransactions[1].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[1].LLT_LLR_ProcessingRun);
					AssertEquals("Processing", balanceTransactions[1].LLT_Comment);
					AssertEquals(0m, balanceTransactions[1].LLT_DeltaValueHours);
					AssertEquals("PRO", balanceTransactions[1].LLT_TransactionType);

					AssertEquals(staff.PK, balanceTransactions[4].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[4].LLT_LeaveType);
					AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[4].LLT_Forfeiture);
					AssertEquals(processTo1, balanceTransactions[4].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[4].LLT_LLR_ProcessingRun);
					AssertEquals("Reprocessed due to changes: Error getting response from ap", balanceTransactions[4].LLT_Comment);
					AssertEquals(234.5m, balanceTransactions[4].LLT_DeltaValueHours);
					AssertEquals("REP", balanceTransactions[4].LLT_TransactionType);

					AssertEquals(staff.PK, balanceTransactions[5].LLT_GS_Staff);
					AssertEquals("LT1", balanceTransactions[5].LLT_LeaveType);
					AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[5].LLT_Forfeiture);
					AssertEquals(processTo2, balanceTransactions[5].LLT_Accrual);
					AssertEquals(run2.PK, balanceTransactions[5].LLT_LLR_ProcessingRun);
					AssertEquals("Processing", balanceTransactions[5].LLT_Comment);
					AssertEquals(0m, balanceTransactions[5].LLT_DeltaValueHours);
					AssertEquals("PRO", balanceTransactions[5].LLT_TransactionType);
				}
			}

			expectedLog = $@"Information|Batched [10] staff were successfully processed.
Information|Processing Run {run2.PK} is COM.
";

			AssertContains(expectedLog, logger.ToString());

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));

			AssertEquals(2, balanceAffectingLogs.Length);
			AssertEquals(0, balanceAffectingQueues.Length);
		}

		#region Batch Staffs

		[TestDate(2023, 10, 5)]
		public void TestProcessBatchStaffs()
		{
			var staffCount = 9;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "T" + i.ToString("00");
				staff.GS_EmploymentDate = ZDate.Today.AddYears(-5);

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddYears(1);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staffs, out var logger, testServiceTaskType: TestServiceTaskType.ServiceTaskForBatchTest);

			//Test log
			var expectedLog = $"Information|Batched [5] staff to process";
			AssertContains(expectedLog, logger.ToString());

			expectedLog = $"Information|Batched [4] staff to process";
			AssertContains(expectedLog, logger.ToString());

			foreach (var staff in staffs)
			{
				expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Processing {staff.GS_Code} succeed to {processTo}
";
				AssertContains(expectedLog, logger.ToString());

				//Test balance transactions
				var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_GS_Staff, staff.PK);

				var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

				AssertEquals(4, balanceTransactions.Length);

				AssertEquals(staff.PK, balanceTransactions[0].LLT_GS_Staff);
				AssertEquals("LT1", balanceTransactions[0].LLT_LeaveType);
				AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[0].LLT_Forfeiture);
				AssertEquals(processTo, balanceTransactions[0].LLT_Accrual);
				AssertEquals(run.PK, balanceTransactions[0].LLT_LLR_ProcessingRun);
				AssertEquals("Processing", balanceTransactions[0].LLT_Comment);
				AssertEquals(10.1m, balanceTransactions[0].LLT_DeltaValueHours);
				AssertEquals("PRO", balanceTransactions[0].LLT_TransactionType);

				AssertEquals(staff.PK, balanceTransactions[1].LLT_GS_Staff);
				AssertEquals("LT1", balanceTransactions[1].LLT_LeaveType);
				AssertEquals(ZDateTimeOffset.Empty, balanceTransactions[1].LLT_Forfeiture);
				AssertEquals(processTo, balanceTransactions[1].LLT_Accrual);
				AssertEquals(run.PK, balanceTransactions[1].LLT_LLR_ProcessingRun);
				AssertEquals("Processing", balanceTransactions[1].LLT_Comment);
				AssertEquals(10.1m, balanceTransactions[1].LLT_DeltaValueHours);
				AssertEquals(BalanceTransactionTypes.FinanceProcessing, balanceTransactions[1].LLT_TransactionType);

				AssertEquals(staff.PK, balanceTransactions[2].LLT_GS_Staff);
				AssertEquals("LT1", balanceTransactions[2].LLT_LeaveType);
				AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[3].LLT_Forfeiture);
				AssertEquals(processTo, balanceTransactions[2].LLT_Accrual);
				AssertEquals(run.PK, balanceTransactions[2].LLT_LLR_ProcessingRun);
				AssertEquals("Processing", balanceTransactions[2].LLT_Comment);
				AssertEquals(234.5m, balanceTransactions[2].LLT_DeltaValueHours);
				AssertEquals("PRO", balanceTransactions[2].LLT_TransactionType);

				AssertEquals(staff.PK, balanceTransactions[3].LLT_GS_Staff);
				AssertEquals("LT1", balanceTransactions[3].LLT_LeaveType);
				AssertEquals(new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), balanceTransactions[3].LLT_Forfeiture);
				AssertEquals(processTo, balanceTransactions[3].LLT_Accrual);
				AssertEquals(run.PK, balanceTransactions[3].LLT_LLR_ProcessingRun);
				AssertEquals("Processing", balanceTransactions[3].LLT_Comment);
				AssertEquals(234.5m, balanceTransactions[3].LLT_DeltaValueHours);
				AssertEquals(BalanceTransactionTypes.FinanceProcessing, balanceTransactions[3].LLT_TransactionType);
			}

			expectedLog = $@"Information|Batched [5] staff were successfully processed.";
			AssertContains(expectedLog, logger.ToString());

			expectedLog = $@"Information|Batched [4] staff were successfully processed.
Information|Processing Run {run.PK} is COM.";
			AssertContains(expectedLog, logger.ToString());

			//Test error records
			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));

			AssertEquals(0, balanceAffectingLogs.Length);

			//Test run service again
			var process2 = new StaffRegionProcessServiceTask();
			logger = new TestServiceLogger();
			process2.ServiceLogger = logger;
			process2.RunTask();

			expectedLog = $@"Information|No processable processing run found.
";
			AssertEquals(expectedLog, logger.ToString());

			//Test no staff in queue
			staffs = Factory.Load<GlbStaff>(StaffRegionProcessServiceTask.GetStaffQueueInProcessingRunQuery(run));
			AssertEquals(0, staffs.Length);
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessBatchStaffs_OneBatch_Failed()
		{
			var staffCount = 5;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "T" + i.ToString("00");
				staff.GS_EmploymentDate = ZDate.Today.AddYears(-5);

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddYears(1);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staffs, out var logger, testServiceTaskType: TestServiceTaskType.ServiceTaskForBatchTest, mockForecast: false);

			var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

			AssertEquals(3, balanceTransactions.Length);

			foreach (var balanceTransaction in balanceTransactions)
			{
				AssertEquals("ERR", balanceTransaction.LLT_LeaveType);
				AssertEquals(ZDateTimeOffset.Empty, balanceTransaction.LLT_Forfeiture);
				AssertEquals(processTo, balanceTransaction.LLT_Accrual);
				AssertEquals(run.PK, balanceTransaction.LLT_LLR_ProcessingRun);
				AssertEquals("Error: Row could not be processed, it will be processed in the next run", balanceTransaction.LLT_Comment);
				AssertEquals(0m, balanceTransaction.LLT_DeltaValueHours);
				AssertEquals("ERR", balanceTransaction.LLT_TransactionType);
			}

			var expectedLog = $"Information|Batched [5] staff to process";
			AssertContains(expectedLog, logger.ToString());

			expectedLog = $@"Error|[0] staff processed successfully, [3] consecutive staff had errors. Batch processing failed.
Information|Processing Run {run.PK} is COR.
";
			AssertContains(expectedLog, logger.ToString());
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessBatchStaffs_TwoBatches_Failed()
		{
			var staffCount = 9;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "T" + i.ToString("00");
				staff.GS_EmploymentDate = ZDateTime.Today.AddYears(-5);

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, staff.GS_EmploymentDate.ToDateTime().AddYears(3), null);
				_ = CreateStaffPolicy(staff, policy2, staff.GS_EmploymentDate.ToDateTime().AddYears(1), staff.GS_EmploymentDate.ToDateTime().AddYears(3));
			}

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddYears(1);
			var processTo = new ZDateTimeOffset(run.LLR_ProcessTo.Year, run.LLR_ProcessTo.Month, run.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staffs, out var logger, testServiceTaskType: TestServiceTaskType.ServiceTaskForBatchTest, mockForecast: false);

			var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
			var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

			AssertEquals(6, balanceTransactions.Length);

			foreach (var balanceTransaction in balanceTransactions)
			{
				AssertEquals("ERR", balanceTransaction.LLT_LeaveType);
				AssertEquals(ZDateTimeOffset.Empty, balanceTransaction.LLT_Forfeiture);
				AssertEquals(processTo, balanceTransaction.LLT_Accrual);
				AssertEquals(run.PK, balanceTransaction.LLT_LLR_ProcessingRun);
				AssertEquals("Error: Row could not be processed, it will be processed in the next run", balanceTransaction.LLT_Comment);
				AssertEquals(0m, balanceTransaction.LLT_DeltaValueHours);
				AssertEquals("ERR", balanceTransaction.LLT_TransactionType);
			}

			var expectedLog = $"Information|Batched [5] staff to process";
			AssertContains(expectedLog, logger.ToString());

			expectedLog = $@"Error|[0] staff processed successfully, [3] consecutive staff had errors. Batch processing failed.
";
			AssertContains(expectedLog, logger.ToString());

			expectedLog = $@"Error|[0] staff processed successfully, [3] consecutive staff had errors. Batch processing failed.
Error|2 batch(es) processed. 2 consecutive batch(es) had failed. Processing Run failed.
Information|Processing Run {run.PK} is COR.
";
			AssertContains(expectedLog, logger.ToString());
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessBatchStaffs_TwoBatches_Failed_Reprocess()
		{
			var staffCount = 6;

			var staffs = new GlbStaff[staffCount];
			for (var i = 0; i < staffCount; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "T" + i.ToString("00");
				staff.GS_EmploymentDate = ZDate.Today;

				staffs[i] = staff;
			}

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			foreach (var staff in staffs)
			{
				_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(3).ToDateTime(), null);
				_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(1).ToDateTime(), ZDateTimeOffset.Today.AddYears(3).ToDateTime());
			}

			var run1 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run1.LLR_RN_NKCountry = "US";
			run1.LLR_ProcessTo = ZDate.Today.AddYears(1);
			var processTo1 = new ZDateTimeOffset(run1.LLR_ProcessTo.Year, run1.LLR_ProcessTo.Month, run1.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(10));

			Factory.Save();

			RunServiceTask(staffs, out var _, testServiceTaskType: TestServiceTaskType.ServiceTaskForBatchTest, mockForecast: false);

			var balanceAffectingLogs = Factory.Load<HrlBalanceAffectingLog>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingLog)));
			var reprocessStaffs = balanceAffectingLogs.Select(l => l.LLB_GS_Staff).ToList();

			var run2 = Factory.NewWithValidTestData<HrlProcessingRun>();
			run2.LLR_RN_NKCountry = "US";
			run2.LLR_ProcessTo = run1.LLR_ProcessTo.AddMonths(1);
			var processTo2 = new ZDateTimeOffset(run2.LLR_ProcessTo.Year, run2.LLR_ProcessTo.Month, run2.LLR_ProcessTo.Day, 23, 59, 59, TimeSpan.FromHours(11));

			Factory.Save();

			RunServiceTask(staffs, out var logger, testServiceTaskType: TestServiceTaskType.ServiceTaskForBatchTest);

			//Test log
			var expectedLog = $"Information|Batched [5] staff to process";
			AssertContains(expectedLog, logger.ToString());

			expectedLog = $"Information|Batched [1] staff to process";
			AssertContains(expectedLog, logger.ToString());

			foreach (var staff in staffs)
			{
				var query = new ZDBOnlyQuery(typeof(HrlBalanceTransaction));
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_GS_Staff, staff.PK);
				query.AddToFilter(HrlBalanceTransactionSchema.LLT_LLR_ProcessingRun, run2.PK);
				var balanceTransactions = Factory.Load<HrlBalanceTransaction>(query);

				if (reprocessStaffs.Contains(staff.PK))
				{
					expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Reprocessing {staff.GS_Code} to {processTo1}.
Debug|Processing {staff.GS_Code} succeed to {processTo2}
";
					AssertContains(expectedLog, logger.ToString());

					AssertEquals(8, balanceTransactions.Length);

					AssertBalanceTransaction(balanceTransactions[0], staff, run2, "LT1", processTo1, "Reprocessed due to changes: Error getting response from ap", 10.1m, "REP", ZDateTimeOffset.Empty);
					AssertBalanceTransaction(balanceTransactions[1], staff, run2, "LT1", processTo2, "Processing", 0m, "PRO", ZDateTimeOffset.Empty);
					AssertBalanceTransaction(balanceTransactions[4], staff, run2, "LT1", processTo1, "Reprocessed due to changes: Error getting response from ap", 234.5m, "REP", new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)));
					AssertBalanceTransaction(balanceTransactions[5], staff, run2, "LT1", processTo2, "Processing", 0m, "PRO", new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)));
				}
				else
				{
					expectedLog = $@"Information|Processing {staff.GS_Code}
Debug|Processing {staff.GS_Code} succeed to {processTo2}
";
					AssertContains(expectedLog, logger.ToString());

					AssertEquals(4, balanceTransactions.Length);

					AssertBalanceTransaction(balanceTransactions[0], staff, run2, "LT1", processTo2, "Processing", 10.1m, "PRO", ZDateTimeOffset.Empty);
					AssertBalanceTransaction(balanceTransactions[1], staff, run2, "LT1", processTo2, "Processing", 10.1m, "FPR", ZDateTimeOffset.Empty);
					AssertBalanceTransaction(balanceTransactions[2], staff, run2, "LT1", processTo2, "Processing", 234.5m, "PRO", new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)));
					AssertBalanceTransaction(balanceTransactions[3], staff, run2, "LT1", processTo2, "Processing", 234.5m, "FPR", new ZDateTimeOffset(2024, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)));
				}
			}

			expectedLog = $@"Information|Batched [5] staff were successfully processed.";
			AssertContains(expectedLog, logger.ToString());

			expectedLog = $@"Information|Batched [1] staff were successfully processed.
Information|Processing Run {run2.PK} is COM.";
			AssertContains(expectedLog, logger.ToString());

			//Test error records
			var balanceAffectingQueues = Factory.Load<HrlBalanceAffectingQueue>(new ZDBOnlyQuery(typeof(HrlBalanceAffectingQueue)));
			AssertEquals(0, balanceAffectingQueues.Length);
		}

		class StaffRegionProcessServiceTaskForBatchTest : StaffRegionProcessServiceTask
		{
			public sealed override int BatchSize => 5;
		}

		class StaffRegionProcessServiceTaskForTest : StaffRegionProcessServiceTask
		{
			public StaffRegionProcessServiceTaskForTest(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			protected override BusinessObjectFactory GetStaffQueueFactory() => Factory;
		}

		[TestDate(2023, 10, 5)]
		public void TestProcessStaff_TheReturnedAPIDataIsWrong_TryProcessStaffThreeTimes()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			var staffPolicy00 = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1).ToDateTime(), null);
			var staffPolicy01 = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2).ToDateTime(), ZDateTimeOffset.Today.AddYears(-1).ToDateTime());

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var response = new HttpResponseMessage();
			using (response.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(response));

				var process = new StaffRegionProcessServiceTask();
				var logger = new TestServiceLogger();
				process.ServiceLogger = logger;
				process.RunTask();

				var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Error|Processing TSS Error: Object reference not set to an instance of an object.
Information|Processing TSS
Error|Processing TSS Error: Cannot access a disposed object.
Object name: 'System.Net.Http.StringContent'.
Information|Processing TSS
Error|Processing TSS Error: Cannot access a disposed object.
Object name: 'System.Net.Http.StringContent'.
Error|Processing TSS Failed
Error|[0] staff processed successfully, [1] consecutive staff had errors. Batch processing failed.
Information|Processing Run {run.PK} is COR.
";
				AssertEquals(expectedLog, logger.ToString());
			}
		}

		#endregion

		public void TestProcessStaff_HTTPError_TryProcessStaffThreeTimes()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			var staffPolicy00 = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			var staffPolicy01 = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var response = new HttpResponseMessage();
			using (response.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>())).Throws(new HttpRequestException("Dummy HttpException"));

				var process = new StaffRegionProcessServiceTask();
				var logger = new TestServiceLogger();
				process.ServiceLogger = logger;
				process.RunTask();

				var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Error|Processing TSS Error: Dummy HttpException
Information|Processing TSS
Error|Processing TSS Error: Dummy HttpException
Information|Processing TSS
Error|Processing TSS Error: Dummy HttpException
Error|Processing TSS Failed
Error|[0] staff processed successfully, [1] consecutive staff had errors. Batch processing failed.
Information|Processing Run {run.PK} is COR.
";
				AssertEquals(expectedLog, logger.ToString());
			}
		}

		public void TestProcessStaff_HTTPError_DeserializeError()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			var staffPolicy00 = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			var staffPolicy01 = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var returnData = Array.Empty<BalanceTransaction>();

			var response = new HttpResponseMessage();
			using (response.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(
					Task.FromResult(
						new HttpResponseMessage()
						{
							Content = new StringContent(JsonConvert.SerializeObject(returnData))
						}));

				var process = new StaffRegionProcessServiceTask();
				var logger = new TestServiceLogger();
				process.ServiceLogger = logger;
				process.RunTask();

				var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Error|Processing TSS Error: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'CargoWise.Definitions.LeaveEngine.LeaveForecasterResult' because the type requires a JSON object (e.g. {{""name"":""value""}}) to deserialize correctly.
To fix this error either change the JSON to a JSON object (e.g. {{""name"":""value""}}) or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array.
Path '', line 1, position 1.";

				AssertContains(expectedLog, logger.ToString());
			}
		}
		#endregion

		public void TestTryGetTaskLock()
		{
			var run = Factory.NewWithValidTestData<HrlProcessingRun>();

			Assert("A lock can be acquired", StaffRegionProcessServiceTask.TryGetTaskLock(Db.Connection, run, out var @lock));

			using (@lock)
			{
				Assert("lock is holding", @lock.IsHoldingLock());
			}

			Assert("lock is released", !@lock.IsHoldingLock());
		}

		public void TestProcessStaff_SqlLockLostException_TryRunProcessingRunThreeTimes()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			var staffPolicy00 = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			var staffPolicy01 = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var response = new HttpResponseMessage();
			using (response.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>())).Throws(new SqlLockLostException());

				var process = new StaffRegionProcessServiceTask();
				var logger = new TestServiceLogger();
				process.ServiceLogger = logger;
				process.RunTask();

				var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Warning|SqlLockLostException: Operation is not valid due to the current state of the object.
Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Warning|SqlLockLostException: Operation is not valid due to the current state of the object.
Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Warning|SqlLockLostException: Operation is not valid due to the current state of the object.
";
				AssertEquals(expectedLog, logger.ToString());
			}
		}

		public void TestProcessSingleStaff_NoGlowServiceClient()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSS";
			staff.GS_EmploymentDate = ZDate.Today.AddYears(-3);

			var policy1 = Factory.NewWithValidTestData<HrlPolicy>();
			policy1.LLP_RN_NKCountry = "AU";

			var policy2 = Factory.NewWithValidTestData<HrlPolicy>();
			policy2.LLP_RN_NKCountry = "US";

			_ = CreateStaffPolicy(staff, policy1, ZDateTimeOffset.Today.AddYears(-1), null);
			_ = CreateStaffPolicy(staff, policy2, ZDateTimeOffset.Today.AddYears(-2), ZDateTimeOffset.Today.AddYears(-1));

			var run = Factory.NewWithValidTestData<HrlProcessingRun>();
			run.LLR_RN_NKCountry = "US";
			run.LLR_ProcessTo = ZDate.Today.AddMonths(-18);

			Factory.Save();

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, String.Empty);

			var process = new StaffRegionProcessServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			process.RunTask();

			//Test log
			var expectedLog = $@"Information|Start Processing Run {run.PK}.
Information|Batched [1] staff to process
Information|Processing TSS
Error|Processing TSS Error: Glow Service Uri is not set.
Information|Processing TSS
Error|Processing TSS Error: Glow Service Uri is not set.
Information|Processing TSS
Error|Processing TSS Error: Glow Service Uri is not set.
Error|Processing TSS Failed
Error|[0] staff processed successfully, [1] consecutive staff had errors. Batch processing failed.
Information|Processing Run {run.PK} is COR.
";
			AssertEquals(expectedLog, logger.ToString());
		}

		HrlStaffPolicy CreateStaffPolicy(GlbStaff staff, HrlPolicy policy, ZDateTimeOffset effectiveDate, ZDateTimeOffset? effectiveEndDate)
		{
			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = policy.PK;
			staffPolicy.LLS_EffectiveDate = effectiveDate;

			if (effectiveEndDate != null)
			{
				staffPolicy.LLS_AutoEffectiveEndDate = (ZDateTimeOffset)effectiveEndDate;
			}

			return staffPolicy;
		}
	}
}
