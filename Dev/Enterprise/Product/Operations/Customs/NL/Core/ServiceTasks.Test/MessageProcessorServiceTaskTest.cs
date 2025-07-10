using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.NL.ServiceTasks.Testing;

[TestedType(typeof(MessageProcessorServiceTask))]
sealed class MessageProcessorServiceTaskTest : BranchMessageProcessorServiceTest<MessageProcessorServiceTask>
{
	static readonly ZString messageTypeForTesting = "TST";

	public void TestHostedServiceRequirement()
	{
		var hostedServiceRequirementMethod = typeof(MessagingServiceTask).GetMethods().FirstOrDefault(p => Attribute.IsDefined(p, typeof(HostedServiceRequirementAttribute)));
		AssertNotNull("Method with Attribute HostedServiceRequirement should exist on MessagingServiceTask", hostedServiceRequirementMethod);

		var nlCompany = Factory.NewWithValidTestData<GlbCompany>();
		nlCompany.GC_RN_NKCountryCode = "NL";
		nlCompany.GC_IsActive = true;
		var nlBranch = Factory.NewWithValidTestData<GlbBranch>();
		nlBranch.GB_GC = nlCompany.PK;
		nlBranch.GB_RN_NKCountryCode = "NL";
		nlBranch.GB_IsActive = true;
		Factory.Save();

		CombineAssertions(() =>
		{
			var combinations = new List<(string submissionType, bool requiredExpected)> { ("BLT", true), ("BTH", true), ("BIT", true), ("ITF", false) };
			foreach (var combination in combinations)
			{
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(nlCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { SubmissionType = combination.submissionType }))
				{
					AssertEquals($"Registry Setting Submission Type: {combination.submissionType}", combination.requiredExpected ? string.Empty : "There is no BLT configured in the Registry.", hostedServiceRequirementMethod.Invoke(null, null));
				}
			}
		});
	}

	public void TestProcessMessagesInAnyBranch()
	{
		var test = SetupDataForTesting();
		Factory.Save();

		var service = new MessageProcessorServiceTaskForTest();
		InitialiseTaskSchedule(service);
		using (Env.Instance.TemporaryServiceTaskContext(MessageProcessorServiceTask.Code, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(service.RunTask);
		}

		var factory = new BusinessObjectFactory();
		var messages = factory.Load<NLEDIMessage>(test.MessagePK);
		AssertEquals("EDI Message - Status", NLEDIMessage.Status.Received, messages.EM_Status);
	}

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "NLP", hostedServiceAttribute.Code);
			AssertEquals("Description", "NL Customs Message Processor", hostedServiceAttribute.Description);
			AssertEquals("Category", "NLC", hostedServiceAttribute.Category);
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Netherlands, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
		});
	}

	protected override MessageProcessorServiceTask CreateServiceTask() => new MessageProcessorServiceTaskForTest();

	protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "NEW";
		company.GC_RN_NKCountryCode = "AU";
		var newBranch = company.Branches.AddNew();
		newBranch.GB_Code = "NEW";

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";

		var nlmessage = Factory.New<EDIMessage>();
		nlmessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NLCustoms;
		nlmessage.EM_MessageType = messageTypeForTesting;
		nlmessage.EM_MessageSubType = NLIncomingMessageSubTypeList.Codes.CCEXTA;
		nlmessage.EM_MessageNum = "1";
		nlmessage.EM_ReceiveTransmit = NLEDIMessage.Direction.Receive;
		nlmessage.EM_Status = NLEDIMessage.Status.Queued;
		nlmessage.EM_IsActive = true;
		nlmessage.EM_MessageText = "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WcoTypeCode>CCEXTA</WcoTypeCode><CommunicationMetaData><ApplicationReferenceId>TestReferenceABC</ApplicationReferenceId><CommunicationsAgreementId>325656</CommunicationsAgreementId><Recipient><Id>00000001</Id></Recipient><Sender><Id>00000002</Id></Sender></CommunicationMetaData><Response><Declaration><ExpirationDateTime formatCode=\"102\">20200612</ExpirationDateTime><FunctionalReferenceId>TestReferenceABC</FunctionalReferenceId></Declaration></Response></MetaData>";
		nlmessage.EM_GB = newBranch.PK;

		Factory.Save();
		return new BranchMessageProcessorServiceTestHelperData()
		{
			MessagePK = nlmessage.PK
		};
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					"NL Customs Response Messages",
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NLCustoms,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
			};
		}
	}

	sealed class MessageProcessorServiceTaskForTest : MessageProcessorServiceTask
	{
		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new NLCBranchCustomsMessageProcessorForTest();
	}

	sealed class NLCBranchCustomsMessageProcessorForTest : NLCBranchCustomsMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
		{
			var result = base.GetMessageProcessorsCore();
			result.Add(new BaseMessageProcessorForTest(Logger));
			return result;
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			return new BaseMessageProcessorForTest(Logger);
		}
	}

	sealed class BaseMessageProcessorForTest : BranchCustomsApplicationTypeMessageProcessor
	{
		public BaseMessageProcessorForTest(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "MessageProcessingServiceTestBaseMessageProcessorForTest";

		protected sealed override string ApplicationCodeCore => ApplicationCodeList.Codes.NLCustoms;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			message.EM_Status = EDIMessageStatusList.Codes.Received;
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { messageTypeForTesting };
	}
}
