using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Messaging.Business.EDIMessage;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using EnterpriseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.PL.ServiceTasks.Testing;

[TestedType(typeof(MessageProcessingService))]
sealed class MessageProcessingServiceTest : BranchMessageProcessorServiceTest<MessageProcessingService>
{
	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", ServiceTaskCodeList.Codes.MessageProcessor, hostedServiceAttribute.Code);
			AssertEquals("Description", ServiceTaskCodeList.Descriptions.MessageProcessor, hostedServiceAttribute.Description);
			AssertEquals("Category", ApplicationCode.PLCustoms, hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", CountryCodes.Poland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestSettingEnvironment()
	{
		SetupDataForTesting();
		Factory.Save();

		var service = new MessageProcessingService();
		InitialiseTaskSchedule(service);

		ServiceTaskTestHelper.AssertNoEnvironmentExceptionThrown(ServiceTaskCodeList.Codes.MessageProcessor, canRunInAnyBranch: true, service);
	}

	public void TestPreProcessingMessage()
	{
		var message = CreateMessage(ApplicationCode.PLCustoms, Status.Queued);
		var serviceTask = CreateServiceTask();

		InitialiseTaskSchedule(serviceTask);
		RunTaskSchedule(serviceTask);

		var testMessage = NewFactory().Load<EnterpriseEDIMessage>(message.PK);
		AssertEquals("EM_Status should be PND", Status.Pending, testMessage.EM_Status);
	}

	public void TestPreProcessingNCTSMessage()
	{
		var message = CreateMessage(ApplicationCode.PLCustomsNCTS, Status.Queued);
		var serviceTask = CreateServiceTask();

		InitialiseTaskSchedule(serviceTask);
		RunTaskSchedule(serviceTask);

		var testMessage = NewFactory().Load<EnterpriseEDIMessage>(message.PK);
		AssertEquals("EM_Status should be PND", Status.Pending, testMessage.EM_Status);
	}

	public void TestProcessingNCTSMessage()
	{
		var message = CreateMessage(ApplicationCode.PLCustomsNCTS, Status.PreProcessedOK);
		var serviceTask = CreateServiceTask();

		InitialiseTaskSchedule(serviceTask);
		RunTaskSchedule(serviceTask);

		var testMessage = NewFactory().Load<EnterpriseEDIMessage>(message.PK);
		AssertEquals("EM_Status should be PPS", Status.Received, testMessage.EM_Status);
	}

	public void TestPreProcessingExitControlMessage()
	{
		var message = CreateMessage(ApplicationCode.PLCustomsExitControl, Status.Queued);
		var serviceTask = CreateServiceTask();

		InitialiseTaskSchedule(serviceTask);
		RunTaskSchedule(serviceTask);

		var testMessage = NewFactory().Load<EnterpriseEDIMessage>(message.PK);
		AssertEquals("EM_Status should be PND", Status.Pending, testMessage.EM_Status);
	}

	public void TestProcessingExitControlMessage()
	{
		var message = CreateMessage(ApplicationCode.PLCustomsExitControl, Status.PreProcessedOK);
		var serviceTask = CreateServiceTask();

		InitialiseTaskSchedule(serviceTask);
		RunTaskSchedule(serviceTask);

		var testMessage = NewFactory().Load<EnterpriseEDIMessage>(message.PK);
		AssertEquals("EM_Status should be PPS", Status.Received, testMessage.EM_Status);
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[] {
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Pre-Processing Import/Export",
			predicates:
			[
				"EM_Status=QUE",
				"EM_ReceiveTransmit=RCV",
				"EM_IsActive=Y",
				"EM_ApplicationCode=PLC",
				"EM_HeldUntilDate IS PASTORNULL"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Pre-Processing NCTS",
			predicates:
			[
				"EM_Status=QUE",
				"EM_ReceiveTransmit=RCV",
				"EM_IsActive=Y",
				"EM_ApplicationCode=PLN",
				"EM_HeldUntilDate IS PASTORNULL"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Pre-Processing Exit Control",
			predicates:
			[
				"EM_Status=QUE",
				"EM_ReceiveTransmit=RCV",
				"EM_IsActive=Y",
				"EM_ApplicationCode=PLX",
				"EM_HeldUntilDate IS PASTORNULL"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Processing Import/Export",
			predicates:
			[
				"EM_Status=PPS",
				"EM_ReceiveTransmit=RCV",
				"EM_IsActive=Y",
				"EM_ApplicationCode=PLC",
				"EM_HeldUntilDate IS PASTORNULL"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Processing NCTS",
			predicates:
			[
				"EM_Status=PPS",
				"EM_ReceiveTransmit=RCV",
				"EM_IsActive=Y",
				"EM_ApplicationCode=PLN",
				"EM_HeldUntilDate IS PASTORNULL"
			]),
		new TaskNudgeInformationForTest(
			table: "EDIMessage",
			queueName: "Polish Customs Message Processing Exit Control",
			predicates:
			[
				"EM_Status=PPS",
				"EM_ReceiveTransmit=RCV",
				"EM_IsActive=Y",
				"EM_ApplicationCode=PLX",
				"EM_HeldUntilDate IS PASTORNULL"
			]),
	};

	protected override MessageProcessingService CreateServiceTask()
	{
		var incomingMessagesProcessingRouterMock = new Mock<IncomingMessagesProcessingLegacyRouter>(new MessageProcessorFactoryLegacyResolver()) { CallBase = true };

		var messageProcessorMock = new Mock<BranchCustomsApplicationTypeMessageProcessor>(incomingMessagesProcessingRouterMock.Object.Logger) { CallBase = true };
		messageProcessorMock.Protected().Setup<string>("MessageFriendlyNameCore").Returns("Processor for MessageProcessingService Test");
		messageProcessorMock.Protected().Setup<string>("ApplicationCodeCore").Returns(ApplicationCode.PLCustoms);
		messageProcessorMock.Protected().Setup("PreProcessMessageCore", ItExpr.IsAny<EnterpriseEDIMessage>())
			.Callback((EnterpriseEDIMessage message) =>
			{
				if (message.EM_Status == Status.Queued)
				{
					message.EM_Status = Status.Pending;
				}
			});
		messageProcessorMock.Protected().Setup("ProcessMessageCore", ItExpr.IsAny<EnterpriseEDIMessage>())
			.Callback((EnterpriseEDIMessage message) =>
			{
				if (message.EM_Status == Status.PreProcessedOK)
				{
					message.EM_Status = Status.Received;
				}
			});
		incomingMessagesProcessingRouterMock.Setup(x => x.GetApplicationTypeProcessorCore(It.IsAny<EnterpriseEDIMessage>())).Returns(messageProcessorMock.Object);

		var serviceTaskMock = new Mock<MessageProcessingService> { CallBase = true };
		serviceTaskMock.Protected().Setup<BranchCustomsMessageProcessor>("GetNewBranchCustomsMessageProcessor").Returns(incomingMessagesProcessingRouterMock.Object);
		serviceTaskMock.Object.ServiceLogger = new TestServiceLogger();

		return serviceTaskMock.Object;
	}

	protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
	{
		var message = CreateMessage(ApplicationCode.PLCustoms, Status.PreProcessedOK);

		return new BranchMessageProcessorServiceTestHelperData()
		{
			MessagePK = message.PK
		};
	}

	Business.EDIMessage CreateMessage(ZString applicationCode, ZString messageStatus)
	{
		var message = Factory.New<Business.EDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		message.EM_Status = messageStatus;
		message.EM_MessageText = ZString.Empty;
		Factory.Save();

		return message;
	}
}
