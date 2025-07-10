using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using EDIMessage = Enterprise.Customs.PL.Business.EDIMessage;

namespace Enterprise.Customs.PL.ServiceTasks.Testing;

[TestedType(typeof(MessageSendingService))]
sealed class MessageSendingServiceTest : ServiceTaskTestCase<MessageSendingService>
{
	public void TestHostedServiceAttributeIsOnlyOne()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
	}

	public void TestHostedServiceAttributeProperties()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "PLS", hostedServiceAttribute.Code);
			AssertEquals("Description", "Polish Customs Message Sender", hostedServiceAttribute.Description);
			AssertEquals("Category", ApplicationCode.PLCustoms, hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Poland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
			AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestInitialiseSchedule()
	{
		var serviceTask = new MessageSendingService();
		InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("Task is active", ZBool.True, taskSchedule.SST_Active);
			Assert("Task period is defined in minutes", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("Task period count equals to default", 15, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly is not defined", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("DailyStartTime is empty", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	public void TestRunTaskExport()
	{
		var message = CreateNewMessageAndSetTestCertificate(messageType: EUJobMessageTypeList.Codes.Export);

		AssertMessageSent(message);
	}

	public void TestRunTaskImport()
	{
		var message = CreateNewMessageAndSetTestCertificate(messageType: EUJobMessageTypeList.Codes.Import);

		AssertMessageSent(message);
	}

	public void TestRunTaskCusPollingTransaction()
	{
		var message = CreateNewMessageAndSetTestCertificate(messageType: EdiMessageMessageType.CusPollingTransaction);

		AssertMessageSent(message);
	}

	public void TestRunTaskArrival()
	{
		var message = CreateNewMessageAndSetTestCertificate(messageType: EUJobMessageTypeList.Codes.NctsArrivalNotification, applicationCode: ApplicationCode.PLCustomsNCTS);

		AssertMessageSent(message);
	}

	public void TestRunTaskDeparture()
	{
		var message = CreateNewMessageAndSetTestCertificate(messageType: EUJobMessageTypeList.Codes.NctsDeparture, applicationCode: ApplicationCode.PLCustomsNCTS);

		AssertMessageSent(message);
	}

	public void TestRunTaskIncorrectApplicationCode()
	{
		const string incorrectApplicationCode = "ERR";
		var message = CreateNewMessageAndSetTestCertificate();
		message.EM_ApplicationCode = incorrectApplicationCode;

		AssertMessageNotSent(message);
	}

	public void TestRunTaskIncorrectDirection()
	{
		const string incorrectDirection = "ERR";
		var message = CreateNewMessageAndSetTestCertificate();
		message.EM_ReceiveTransmit = incorrectDirection;

		AssertMessageNotSent(message);
	}

	public void TestRunTaskIncorrectStatus()
	{
		const string incorrectStatus = "ZYX";
		var message = CreateNewMessageAndSetTestCertificate();
		message.EM_Status = incorrectStatus;

		AssertMessageNotSent(message);
	}

	public void TestRunTaskInInactive()
	{
		var message = CreateNewMessageAndSetTestCertificate();
		message.EM_IsActive = false;

		AssertMessageNotSent(message);
	}

	EDIMessage CreateNewMessageAndSetTestCertificate(string messageType = EUJobMessageTypeList.Codes.Import, string applicationCode = ApplicationCode.PLCustoms)
	{
		const string testMessageText = "<msg>Test Message</msg>";
		var testLinkedObject = Factory.NewWithValidTestData<JobDeclaration>().CustomsEntryHeaders.AddNew();

		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_MessageType = messageType;
		message.EM_ReceiveTransmit = Messaging.Business.EDIInterchange.Direction.Transmit;
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		message.EM_IsTestMessage = true;
		message.EM_IsActive = true;
		message.EM_LinkedObject = testLinkedObject;
		message.EM_MessageText = testMessageText;

		var staffPassword = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		staffPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		staffPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		Factory.Save();

		return message;
	}

	void AssertMessageSent(EDIMessage message)
	{
		SaveAndSend(message);
		CombineAssertions("Message is sent and linked", () =>
		{
			AssertEquals("message Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertNotNull("message Interchange", message.Interchange);
		});
	}

	void AssertMessageNotSent(EDIMessage message)
	{
		SaveAndSend(message);
		CombineAssertions("Message is not sent and is not linked", () =>
		{
			AssertNotEquals("message Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertNull("message Interchange", message.Interchange);
		});
	}

	void SaveAndSend(EDIMessage message)
	{
		Factory.Save();
		var service = new MessageSendingService();
		InitialiseTaskSchedule(service);
		ServiceTaskTestHelper.AssertNoEnvironmentExceptionThrown(ServiceTaskCodeList.Codes.MessageSender, true, service);

		message.Reload();
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[] {
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Sender - Import",
			predicates:
			[
				"EM_IsActive=Y",
				"EM_HeldUntilDate IS PASTORNULL",
				"EM_ApplicationCode=PLC",
				"EM_MessageType=IMP",
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Sender - Export",
			predicates:
			[
				"EM_IsActive=Y",
				"EM_HeldUntilDate IS PASTORNULL",
				"EM_ApplicationCode=PLC",
				"EM_MessageType=EXP",
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Sender - CusPollingTransaction",
			predicates:
			[
				"EM_IsActive=Y",
				"EM_HeldUntilDate IS PASTORNULL",
				"EM_ApplicationCode=PLC",
				"EM_MessageType=CPT",
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Sender - Arrival",
			predicates:
			[
				"EM_IsActive=Y",
				"EM_HeldUntilDate IS PASTORNULL",
				"EM_ApplicationCode=PLN",
				"EM_MessageType=ARN",
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Sender - Departure",
			predicates:
			[
				"EM_IsActive=Y",
				"EM_HeldUntilDate IS PASTORNULL",
				"EM_ApplicationCode=PLN",
				"EM_MessageType=DEP",
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Sender - Exit Control",
			predicates:
			[
				"EM_IsActive=Y",
				"EM_HeldUntilDate IS PASTORNULL",
				"EM_ApplicationCode=PLX",
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Sender - Attachment",
			predicates:
			[
				"EM_IsActive=Y",
				"EM_HeldUntilDate IS PASTORNULL",
				"EM_ApplicationCode=PLM",
				"EM_MessageType=ATT",
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX"
			]),
	};
}
