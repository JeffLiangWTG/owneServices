using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOEDIMessage))]
sealed class NOEDIMessageTest : EDIMessageTest<NOEDIMessage>
{
	protected override bool ExpectedIsMessageInterpretationSetterSupported => true;

	public void TestMessageDefaults()
	{
		var message = Factory.New<NOEDIMessage>();
		AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.NOCustoms, message.EM_ApplicationCode);
	}

	public void TestPrettier()
	{
		var message = Factory.New<NOEDIMessage>();
		AssertType<NOEDIMessagePrettier>(message.Prettier);
	}

	public void TestMessageInterpretation_SetterShouldCheckIsMessageInterpretationSetterSupported() => CombineAssertions(() =>
	{
		var messageMock = Factory.NewMoq<NOEDIMessage>();
		AssertNoExceptionThrown("When setting initial value", () => messageMock.Object.EM_MessageInterpretation = "message v1");

		messageMock.SetupGet(x => x.EM_MessageText).Returns("EDI MESSAGE");
		messageMock.SetupGet(x => x.IsMessageInterpretationSetterSupported).Returns(false);
		AssertExceptionThrown<NotSupportedException>("When setter is not supported", () => messageMock.Object.EM_MessageInterpretation = "message v2");

		messageMock.SetupGet(x => x.IsMessageInterpretationSetterSupported).Returns(true);
		AssertNoExceptionThrown("When setter is supported", () => messageMock.Object.EM_MessageInterpretation = "message v3");
	});

	public void TestTypeDecider()
	{
		AssertType<NOEDIMessageTypeDecider>(NOEDIMessage.TypeDecider);
	}

	[TestDate(2024, 06, 01, 09, 30, 00)]
	public void TestNumberFountainNumbersAndFillInPlaceHolders()
	{
		const string bgmReference = "1112223332024060100000101";
		const string headerTemplate = "CUSDEC:1:902:UN:NEP-I+<<JOB REFERENCE NUMBER PLACE HOLDER>>'";
		const string headerFinal = $"CUSDEC:1:902:UN:NEP-I+{bgmReference}'";
		var declaration = SetupDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		var messageSendingObject = new MessageSendingObject(entryHeader);
		var wrapper = new CUSDECMessageDataProviderWrapper(messageSendingObject);
		var generatedEdiMessage = new CUSDECMessageBuilder(wrapper, Common.MessageBuilders.MessageSubTypes.Create).GenerateMessage();
		var serializedMessageText = generatedEdiMessage.EM_MessageText;

		CombineAssertions(() =>
		{
			AssertNotNullOrEmpty("Serialized message not null or empty", serializedMessageText);
			AssertEquals("Placeholder existing for header", expected: true, serializedMessageText.Contains(headerTemplate));

			entryHeader.CH_BGMReference = bgmReference;
			generatedEdiMessage = new CUSDECMessageBuilder(wrapper, Common.MessageBuilders.MessageSubTypes.Create).GenerateMessage();
			serializedMessageText = generatedEdiMessage.EM_MessageText;
			AssertEquals("Final data for header", expected: true, serializedMessageText.Contains(headerFinal));
		});
	}

	public void TestAdditionalBusinessObjectFetchStrategies()
	{
		var message = Factory.New<NOEDIMessage>();
		AssertType<Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy>(message.GetFetchStrategies().Single());
	}

	JobDeclaration SetupDeclaration()
	{
		var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		jobDeclaration.JE_CustomsProfile = "1234-CODE1";
		jobDeclaration.JE_CustomsOffice = "NO000000";
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = cusEntryHeader.MergedLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		var orgCusCode = declarant.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.OrganizationNumber;
		orgCusCode.OK_CustomsRegNo = "111222333";
		jobDeclaration.SetDeclarant(declarant);

		return jobDeclaration;
	}
}
