using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using InterchangeStatus = Enterprise.Messaging.Business.EDIInterchange.Status;

namespace Enterprise.Customs.PL.ServiceTasks.Testing;

[TestedType(typeof(CusPollingTransactionProcessingService))]
sealed class CusPollingTransactionProcessingServiceTest : ServiceTaskTestCase<CusPollingTransactionProcessingService>
{
	public void TestHostedServiceAttributeIsOnlyOne()
		=> AssertEquals("Expected single attribute", 1, GetHostedServiceAttributes().Length);

	public void TestHostedServiceAttributeProperties()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "PCT", hostedServiceAttribute.Code);
			AssertEquals("Description", "Polish CusPollingTransaction Processor", hostedServiceAttribute.Description);
			AssertEquals("Category", ApplicationCode.PLCustoms, hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Poland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "1minute", hostedServiceAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestInitialiseSchedule()
	{
		var serviceTask = new CusPollingTransactionProcessingService();
		InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("Task is active", ZBool.True, taskSchedule.SST_Active);
			Assert("Task period is defined in minutes", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("Task period count equals to default", 1, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly is not defined", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("DailyStartTime is empty", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	[TestDate(2025, 3, 25, 10, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestNothingHappensWhenNowAboveNextAttempt()
		=> CusPollingTransactionTestHelper.AssertNothingHappensWhenNowAboveNextAttempt(Factory, RunServiceProcessing);

	[TestDate(2025, 3, 25, 10, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestMessageSentWhenNowEqualToNextAttempt()
		=> CusPollingTransactionTestHelper.AssertMessageSentWhenNowEqualToNextAttempt(Factory, RunServiceProcessing);

	[TestDate(2025, 3, 25, 10, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestMessageSentWhenNowBelowNextAttempt()
		=> CusPollingTransactionTestHelper.AssertMessageSentWhenNowBelowNextAttempt(Factory, RunServiceProcessing);

	[TestDate(2025, 3, 25, 10, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestBacklogsCreatedWhenNowBelowNextAttemptMinusOneHour()
		=> CusPollingTransactionTestHelper.AssertBacklogsCreatedWhenNowBelowNextAttemptMinusOneHour(Factory, RunServiceProcessing);

	[TestDate(2025, 3, 25, 10, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeReceived()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeReceived(Factory, RunServiceProcessing);

	[TestDate(2025, 3, 25, 10, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeFailed_FAL()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeFailed(Factory, RunServiceProcessing, InterchangeStatus.Failed);

	[TestDate(2025, 3, 25, 10, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeFailed_ERR()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeFailed(Factory, RunServiceProcessing, InterchangeStatus.Error);

	void RunServiceProcessing(GlbBranch branch)
	{
		var service = new CusPollingTransactionProcessingService();
		InitialiseAndRunTaskSchedule(service);
		AssertEquals("No service processing errors", 0, ErrorReporter.TotalErrorCount);
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
}
