using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.NL.ServiceTasks.Testing;

[TestedType(typeof(MessageReceiverServiceTask))]
sealed class MessageReceiverServiceTaskTest : ServiceTaskTestCase<MessageReceiverServiceTask>
{
	public void TestEmptyTextInterchange() => CombineAssertions(() =>
	{
		var interchange = CreateInterchangeAndRunService(ZString.Empty);

		AssertEquals("No messages should be created", 0, interchange.ContainedMessages.Count);
	});

	public void TestServiceAttributes()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var uniqueAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Category", MessagingServiceTask.MessageServiceTaskCategory, uniqueAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Netherlands, uniqueAttribute.RequiresCompanyInCountry);
			Assert("CanRunInAnyBranch", uniqueAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, uniqueAttribute.AllowsMultipleInstances);
			AssertEquals("Code", "NLR", uniqueAttribute.Code);
			AssertEquals("Description", "NL Customs Message Retriever", uniqueAttribute.Description);
			AssertEquals("Type", typeof(MessageReceiverServiceTask), uniqueAttribute.Type);
			AssertEquals("MinimumPeriod", "60Seconds", uniqueAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "15minutes", uniqueAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestInitialiseSchedule()
	{
		var testTask = new MessageReceiverServiceTask();
		InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					"NL Customs interchanges inbound",
					EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.NLCustoms),
			};
		}
	}

	EDIInterchange CreateInterchangeAndRunService(ZString interchangeBodyText)
	{
		var interchange = Factory.NewWithValidTestData<EDIInterchange>();
		interchange.EI_ApplicationCode = "NLC";
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_IsActive = true;
		interchange.ForceDeprecatedNTextUsageForTesting = true;
		interchange.EI_BodyText = string.Empty;
		Factory.Save();

		var service = new MessageReceiverServiceTask();
		InitialiseTaskSchedule(service);
		using (Env.Instance.TemporaryServiceTaskContext(MessageReceiverServiceTask.Code, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(service.RunTask);
		}

		return interchange;
	}
}
