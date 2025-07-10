using System.Collections.Generic;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

class FaultInterchangeUnpackingStrategyTest : InterchangeUnpackingStrategyBaseTest
{
	const string CommonTestFilesFolder = "Enterprise.Customs.PL.Business.Testing.Message.TestFiles";
	const string DocumentHandlingPortTestFilesFolder = "Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles";

	public void TestSoapFault1_1()
		=> UnpackResponseAndCheckResult(CommonTestFilesFolder, "SoapFault1.1");

	public void TestSoapFault1_2()
		=> UnpackResponseAndCheckResult(CommonTestFilesFolder, "SoapFault1.2");

	public void TestBusinessErrorFault()
		=> UnpackResponseAndCheckResult(DocumentHandlingPortTestFilesFolder, "BusinessErrorFault");

	public void TestTechErrorFault()
		=> UnpackResponseAndCheckResult(DocumentHandlingPortTestFilesFolder, "TechErrorFault");

	protected virtual string ApplicationCode => ApplicationCodes.PLCustoms;

	protected override IReadOnlyCollection<XmlQualifiedName> ExpectedSupportedXmlNodes => [
		UniversalMessaging.Event.RootNode,
		SoapHelper.FaultNodeV1dot1,
		SoapHelper.FaultNodeV1dot2,
		PUESC.DocumentHandlingPort.XmlNodes.BusinessErrorFault,
		PUESC.DocumentHandlingPort.XmlNodes.TechErrorFault,
	];

	protected virtual BusinessObject CreateTransmitMessageLinkedObject() => Factory.New<JobDeclaration>();

	void UnpackResponseAndCheckResult(string testFilesFolder, string testCaseName) => CombineAssertions(() =>
	{
		var responseXml = InterchangeTestHelper.GetTestFile($"{testFilesFolder}.{testCaseName}.xml");
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(responseXml, ApplicationCode);
		requestMessage.EM_LinkedObject = CreateTransmitMessageLinkedObject();

		var expectedFaultMessage = new ExpectedMessage(requestMessage)
		{
			ApplicationCode = ApplicationCode,
			SubType = EDIMessageSubType.Fault,
			ReceiveTransmit = ReceiveTransmitList.Codes.Receive,
			Status = EDIMessageStatusList.Codes.Queued,
			ApplicationReference = null,
			Text = responseXml,
		};
		var expectedUnpackResult = ExpectedUnpackResult.Success(
			expectedMessages: [expectedFaultMessage],
			expectedLogs: [(
				Events.InterchangeAcknowledged,
				$"Created fault message  for transmit message {requestMessage.EM_MessageNum}.",
				ExpectedOn: new(responseInterchange))]);

		using var responseBodyXmlReader = responseInterchange.GetBodyXmlReader();
		var unpackResult = UnpackingStrategy.Unpack(responseInterchange, requestMessage.Interchange, requestMessage, responseBodyXmlReader, ServiceLog);

		unpackResult.Assert(expectedUnpackResult, assertionsPrefix: testCaseName);
	});

	protected override IXmlInterchangeUnpackingStrategy CreateUnpackingStrategy()
		=> new FaultInterchangeUnpackingStrategy(new DataProviderFactory(RecognizableMessages.All));
}
