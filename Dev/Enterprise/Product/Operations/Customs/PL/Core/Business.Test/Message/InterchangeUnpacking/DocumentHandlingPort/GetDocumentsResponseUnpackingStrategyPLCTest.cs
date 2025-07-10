using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GetDocumentsResponseUnpackingStrategyPLCTest : GetDocumentsResponseUnpackingStrategyTestBase
{
	const string TestFilesFolder = "Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles";

	public void TestProcessForValidInterchange() => CombineAssertions(() =>
	{
		var transmitMessage1 = Factory.New<EDIMessage>();
		transmitMessage1.EM_ApplicationCode = ApplicationCode.PLCustoms;
		transmitMessage1.EM_MessageType = EUJobMessageTypeList.Codes.Export;
		transmitMessage1.EM_MessageSubType = "525";
		transmitMessage1.EM_MessageNum = "230010732";
		transmitMessage1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage1.EM_Status = EDIMessage.Status.Sent;
		transmitMessage1.EM_GB = ZGuid.NewZGuid();
		transmitMessage1.EM_LinkedObject = Factory.New<CusEntryHeader>();

		var transmitMessage2 = Factory.New<EDIMessage>();
		transmitMessage2.EM_ApplicationCode = ApplicationCode.PLCustoms;
		transmitMessage2.EM_MessageType = EUJobMessageTypeList.Codes.Export;
		transmitMessage2.EM_MessageSubType = "525";
		transmitMessage2.EM_MessageNum = "230010733";
		transmitMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage2.EM_Status = EDIMessage.Status.Sent;
		transmitMessage2.EM_GB = ZGuid.NewZGuid();
		transmitMessage2.EM_LinkedObject = Factory.New<CusEntryHeader>();

		var expectedMessage1 = new ExpectedMessage
		{
			ApplicationCode = ApplicationCode.PLCustoms,
			Type = EUJobMessageTypeList.Codes.Export,
			SubType = "529",
			Num = "23IE529-P10732",
			RootNode = new XmlQualifiedName("CC529C", "http://www.mf.gov.pl/xsd/AES/CC529C.xsd"),
		};
		var expectedMessage2 = new ExpectedMessage
		{
			ApplicationCode = ApplicationCode.PLCustoms,
			Type = EUJobMessageTypeList.Codes.Export,
			SubType = "528",
			Num = "23IE528-P11182",
			RootNode = new XmlQualifiedName("CC528C", "http://www.mf.gov.pl/xsd/AES/CC528C.xsd"),
		};
		var expectedResult = ExpectedUnpackResult.Success(
			expectedMessages: [expectedMessage1, expectedMessage2],
			[(Events.InterchangeInProgress, "PLC:EXP/529 message was created from document [0]=CC529C_23IE529-P10732.xml."),
			(Events.InterchangeInProgress, "PLC:EXP/528 message was created from document [1]=CC528C_23IE528-P11182.xml."),
			(Events.InterchangeAcknowledged, LogType.Information, "GetDocumentsResponse acknowledgements of IE529, IE528 is processed, created 2 messages."),
			(Events.ErrorReport, LogType.Error, "Transmit message not found: OdrzucenieKomunikatu_23OdrzKom-P12527.xml."),
			(Events.ErrorReport, LogType.Error, "Xml parse error: Document number 3."),
			(Events.ErrorReport, LogType.Error, "Not supported content: XmlParseError.xml, NotSupportedMessage.xml, EmptyIdentification.xml."),
				(Events.ErrorReport, LogType.Error, "Message have too long identification: TooLongIdentification.xml.")]);

		var cusPollingTransaction = GetDocumentsResponseDocumentUnpackingTestHelper.CreateCusPollingTransactionWithStaffAndGlbExternalPassword(Factory, CusPollingTransactionTypes.PLC);
		var responseXml = InterchangeTestHelper.GetTestFile($"{TestFilesFolder}.GetDocumentsResponseTest.xml");
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(responseXml, ApplicationCode.PLCustoms);
		requestMessage.EM_MessageType = EdiMessageMessageType.CusPollingTransaction;
		requestMessage.EM_LinkedObject = cusPollingTransaction;

		using var responseBodyXmlReader = responseInterchange.GetBodyXmlReader();
		var unpackResult = UnpackingStrategy.Unpack(responseInterchange, requestMessage.Interchange, requestMessage, responseBodyXmlReader, ServiceLog);
		unpackResult.Assert(
			expectedResult,
			logExpectedOnByDefault: new([responseInterchange]));
	});
}
