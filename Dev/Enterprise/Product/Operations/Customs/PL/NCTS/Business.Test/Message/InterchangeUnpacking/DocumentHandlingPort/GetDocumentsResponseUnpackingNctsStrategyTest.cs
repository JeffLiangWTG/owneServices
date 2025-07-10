using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class GetDocumentsResponseUnpackingNctsStrategyTest : GetDocumentsResponseUnpackingStrategyTestBase
{
	const string TestFilesFolder = "Enterprise.Customs.PL.NCTS.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles";

	public void TestProcessForValidInterchange()
	{
		CreateTestOutboundMessageWithLinkedObject("850cce3f-0091-4c24-8177-87ffa8266d7f");
		CreateTestOutboundMessageWithLinkedObject("1f27f64f-8793-4775-9c87-f7d8b9440c77");

		var transmitMessage = Factory.New<EDIMessage>();
		transmitMessage.EM_ApplicationCode = "PLN";
		transmitMessage.EM_MessageType = EUJobMessageTypeList.Codes.NctsDeparture;
		transmitMessage.EM_MessageSubType = "015";
		transmitMessage.EM_MessageNum = "23IE015001X156";
		transmitMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage.EM_Status = EDIMessage.Status.Sent;
		transmitMessage.EM_GB = ZGuid.NewZGuid();

		transmitMessage.EM_LinkedObject = Factory.New<NctsDepartureMovementHeader>();

		var expectedMessage1 = new ExpectedMessage
		{
			ApplicationCode = ApplicationCode.PLCustomsNCTS,
			Type = EUJobMessageTypeList.Codes.NctsDeparture,
			SubType = "056",
			Num = "23IE056000073P",
			RootNode = new XmlQualifiedName("IE056PL", "http://www.mf.gov.pl/schematy/NCTS2/IE056/2021/03/"),
		};
		var expectedMessage2 = new ExpectedMessage
		{
			ApplicationCode = ApplicationCode.PLCustomsNCTS,
			Type = EUJobMessageTypeList.Codes.NctsDeparture,
			SubType = "NPP",
			ApplicationReference = "b3166eee-1d6e-4b00-960a-343a7f849d14",
			RootNode = new XmlQualifiedName("Dokument", "http://crd.gov.pl/xml/schematy/UPO/2008/05/09/"),
		};
		var expectedMessage3 = new ExpectedMessage
		{
			ApplicationCode = ApplicationCode.PLCustomsNCTS,
			Type = EUJobMessageTypeList.Codes.NctsDeparture,
			SubType = "NPP",
			ApplicationReference = "bcf9cb6a-edbf-4469-b5a8-42b278999955",
			RootNode = new XmlQualifiedName("Dokument", "http://crd.gov.pl/xml/schematy/UPO/2008/05/09/"),
		};
		var expectedResult = ExpectedUnpackResult.Success(
			expectedMessages: [expectedMessage1, expectedMessage2, expectedMessage3],
			[(Events.InterchangeInProgress, "PLN:DEP/NPP message was created from document [1]=NPP.xml."),
			(Events.InterchangeInProgress, "PLN:DEP/NPP message was created from document [2]=NPP.xml."),
			(Events.InterchangeInProgress, "PLN:DEP/056 message was created from document [3]=IE056.xml."),
			(Events.ErrorReport, LogType.Error, "Transmit messages not found: OdrzucenieKomunikatu_23OdrzKom-666.xml, OdrzucenieKomunikatu_23OdrzKom-P11372.xml."),
			(Events.InterchangeAcknowledged, LogType.Information, "GetDocumentsResponse acknowledgements of UPP, IE056PL is processed, created 3 messages.")]);

		var cusPollingTransaction = GetDocumentsResponseDocumentUnpackingTestHelper.CreateCusPollingTransactionWithStaffAndGlbExternalPassword(Factory, CusPollingTransactionTypes.PLC);
		var responseXml = InterchangeTestHelper.GetTestFile($"{TestFilesFolder}.GetDocumentsResponse.xml");
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(responseXml, ApplicationCode.PLCustomsNCTS);
		requestMessage.EM_MessageType = EdiMessageMessageType.CusPollingTransaction;
		requestMessage.EM_LinkedObject = cusPollingTransaction;

		using var responseBodyXmlReader = responseInterchange.GetBodyXmlReader();
		var unpackResult = UnpackingStrategy.Unpack(responseInterchange, requestMessage.Interchange, requestMessage, responseBodyXmlReader, ServiceLog);
		unpackResult.Assert(
			expectedResult,
			logExpectedOnByDefault: new([responseInterchange]));
		return;

		void CreateTestOutboundMessageWithLinkedObject(string applicationReference)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_ApplicationReference = applicationReference;
			message.EM_ApplicationCode = "PLN";
			message.EM_MessageType = "DEP";

			var linkedObject = Factory.NewWithValidTestData<NctsHeader>();
			message.EM_LinkedObject = linkedObject;
		}
	}
}
