using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodeList = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(PLMessagePacker))]
sealed class PLMessagePackerTest : UniversalCustomsEDIMessagePackerTest<PLMessagePacker>
{
	public void TestPack()
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new PLMessagePacker();
		var testMessage = CreateMessage("TestMessage");

		var errorMsg = messagePacker.Pack(testMessage, interchange, logger);
		AssertNotNull(interchange);
		AssertEquals(ZString.Empty, errorMsg);
		AssertInterchange(interchange, testMessage);
	}

	public override void TestCorrectSubscribeToUCPSubscribers() => CombineAssertions(() =>
	{
		foreach (var applicationCode in ApplicationCodes)
		{
			AssertType<PLMessagePacker>(applicationCode, ObjectFactory.Get<IUCPSubscribersProvider>().GetMessagePacker(applicationCode));
		}
	});

	protected override string ApplicationCode => ApplicationCodeList.PLCustoms;

	static string[] ApplicationCodes => [
		ApplicationCodeList.PLCustoms,
		ApplicationCodeList.PLCustomsNCTS,
		ApplicationCodeList.PLCustomsExitControl,
	];

	EDIMessage CreateMessage(string messageBody = "", bool isTest = true)
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = ApplicationCode;
		message.EM_MessageType = "XXX";
		message.EM_MessageSubType = "YYY";
		message.EM_MessageOwner = "ZZZ";
		message.EM_MessageText = messageBody;
		message.EM_IsTestMessage = isTest;
		message.EM_LinkedObject = Factory.New<CusEntryHeader>();

		return message;
	}

	void AssertInterchange(EDIInterchange interchange, EDIMessage message)
	{
		AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
		AssertEquals("EI_InterchangeNum", ZString.Empty, interchange.EI_InterchangeNum);
		AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals("EI_IsActive", expected: true, interchange.EI_IsActive);
		AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
		AssertEquals("EI_To", CustomsDestinationCodes.PlCustomsTest, interchange.EI_To);
		AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
		AssertEquals("EI_ApplicationCode", message.EM_ApplicationCode, interchange.EI_ApplicationCode);
		AssertEquals("EI_InterchangeType", message.EM_MessageType, interchange.EI_InterchangeType);
		AssertNotEquals("EI_BodyText is filled", ZString.Empty, interchange.EI_BodyText);
	}
}
