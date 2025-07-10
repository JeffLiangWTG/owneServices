using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.NO.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EDIFactMessagePacker))]
sealed class EDIFactMessagePackerTest : UniversalCustomsEDIMessagePackerTest<EDIFactMessagePacker>
{
	public void TestPack_Parameters() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When message is null", () => MessagePacker.Pack(null, interchange, loggingInformation));
		AssertExceptionThrown<ArgumentNullException>("When Linked Object is null", () => MessagePacker.Pack(message, null, loggingInformation));

		var entryLine = Factory.New<CusEntryLine>();
		message.EM_LinkedObject = entryLine;
		AssertExceptionThrown<ArgumentNullException>("When Linked Object is not a CusEntryHeader", () => MessagePacker.Pack(message, null, loggingInformation));

		message.EM_LinkedObject = cusEntryHeader;
		AssertExceptionThrown<ArgumentNullException>("When interchange is null", () => MessagePacker.Pack(message, null, loggingInformation));
		AssertExceptionThrown<ArgumentNullException>("When logger is null", () => MessagePacker.Pack(message, interchange, null));
	});

	public void TestPack_ForNewMessage()
	{
		var declaration = GetNewJobDeclaration(JobMessageTypeList.Codes.Export);
		declaration.CustomsEntryHeaders.Add(cusEntryHeader);
		message.EM_LinkUniqueID = cusEntryHeader.PK;
		message.EM_LinkedObject = cusEntryHeader;
		message.EM_MessageNum = "1234";

		var result = MessagePacker.Pack(message, interchange, loggingInformation);

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Output String", result);
			AssertEquals("EI_InterchangeType", message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals("EI_ApplicationCode", "NOC", interchange.EI_ApplicationCode);
			AssertEquals("EI_HeaderText", $"{{\"custom.NO.FileName\":\"E1234.txt\"}}", interchange.EI_HeaderText);
			AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
		});
	}

	[TestDate(2024, 11, 04)]
	public void TestPack_HeaderAndFooter()
	{
		var registry = NOCustomsDataRegistry.Instance;
		var declaration = GetNewJobDeclaration(JobMessageTypeList.Codes.Import);
		var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		_ = CreateAuthorisationRecord(declarantOrgHeader, CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration, "NUMBER1");
		declaration.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;

		declaration.CustomsEntryHeaders.Add(cusEntryHeader);
		message.EM_LinkUniqueID = cusEntryHeader.PK;
		message.EM_LinkedObject = cusEntryHeader;
		message.EM_MessageNum = "1234";

		registry.EnableTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		AssertPack_HeaderAndFooter(
			"When TestMessages is disabled",
			"UNB+UNOA:1+NUMBER1:NEP+NO000101:NEP+20241104:0000+20241104000001'",
			"UNZ+1+20241104000001'");

		interchange.ContainedMessages.RemoveAll();
		message.EM_MessageText = "SOME_SAMPLE_MESSAGE_TEXT";
		registry.EnableTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		AssertPack_HeaderAndFooter(
			"When TestMessages is enabled",
			"UNB+UNOA:1+NUMBER1:NEP+NO000119:NEP+20241104:0000+20241104000002++++++2'",
			"UNZ+1+20241104000002'");
	}

	void AssertPack_HeaderAndFooter(string testMessage, string expectedUNBSegment, string expectedUNZSegment)
	{
		var result = MessagePacker.Pack(message, interchange, loggingInformation);
		var interchangeBodyText = interchange.EI_BodyText.ToString();
		AssertEquals("[Pre-condition]: Interchanbe Body Text", false, interchangeBodyText.IsNullOrEmpty());

		CombineAssertions($"{testMessage}", () =>
		{
			var segments = interchangeBodyText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

			var actualUNBSegment = segments.FirstOrDefault(s => s.StartsWith("UNB"));
			var actualUNZSegment = segments.FirstOrDefault(s => s.StartsWith("UNZ"));
			AssertEquals("UNB", expectedUNBSegment, actualUNBSegment);
			AssertEquals("UNZ", expectedUNZSegment, actualUNZSegment);
		});
	}

	protected override string ApplicationCode => ApplicationCodeList.Codes.NOCustoms;

	protected override void SetUp()
	{
		base.SetUp();

		message = Factory.NewWithValidTestData<EDIMessage>();
		message.EM_MessageText = "SOME_SAMPLE_MESSAGE_TEXT";
		interchange = Factory.New<EDIInterchange>();
		loggingInformation = Mock.Of<LoggingInformation>();
		cusEntryHeader = Factory.New<CusEntryHeader>();
	}

	EDIMessage message;
	EDIInterchange interchange;
	LoggingInformation loggingInformation;
	CusEntryHeader cusEntryHeader;

	IUniversalCustomsEDIMessagePacker MessagePacker => messagePacker ??= new EDIFactMessagePacker();
	IUniversalCustomsEDIMessagePacker messagePacker;

	JobDeclaration GetNewJobDeclaration(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		return declaration;
	}

	CusAuthorisationHeader CreateAuthorisationRecord(OrgHeader orgHeader, ZString type, ZString number, string countryCode = Core.Constants.CountryCodes.Norway)
	{
		var result = orgHeader.Factory.New<CusAuthorisationHeader>();
		result.CPH_OH_PermitHolder = orgHeader.PK;
		result.CPH_Type = type;
		result.CPH_RN_NKCountryCode = countryCode;
		result.CPH_StartDate = ZDate.Today.AddDays(-1);
		result.CPH_EndDate = ZDate.Today.AddDays(1);
		result.CPH_Number = number;
		return result;
	}
}
