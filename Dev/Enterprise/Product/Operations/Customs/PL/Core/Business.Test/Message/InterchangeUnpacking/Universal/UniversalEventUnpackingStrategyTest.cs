using System.Collections.Generic;
using System.Xml;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(UniversalEventUnpackingStrategy))]
sealed class UniversalEventUnpackingStrategyTest : InterchangeUnpackingStrategyBaseTest
{
	const string TestFilesFolder = "Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.Universal.TestFiles";

	protected override IReadOnlyCollection<XmlQualifiedName> ExpectedSupportedXmlNodes
		=> [new("UniversalEvent", "http://www.cargowise.com/Schemas/Universal/2012/11")];

	protected override IXmlInterchangeUnpackingStrategy CreateUnpackingStrategy()
		=> new UniversalEventUnpackingStrategy();

	public void TestUnpack() => CombineAssertions(() =>
	{
		var interchangeXml = InterchangeTestHelper.GetTestFile($"{TestFilesFolder}.UniversalInterchangeWithRejectionEvent.xml");
		var expectedMessageXml = InterchangeTestHelper.GetTestFile($"{TestFilesFolder}.UniversalEventRejection.xml");
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(interchangeXml, ApplicationCode.PLCustoms);
		requestMessage.EM_LinkedObject = Factory.New<CusEntryHeader>();

		var expectedFaultMessage = new ExpectedMessage(requestMessage)
		{
			SubType = EDIMessageSubType.UniversalRejection,
			ReceiveTransmit = ReceiveTransmitList.Codes.Receive,
			Status = EDIMessageStatusList.Codes.Queued,
			ApplicationReference = null,
			Text = expectedMessageXml,
		};
		var expectedUnpackResult = ExpectedUnpackResult.Success(
			expectedMessages: [expectedFaultMessage],
			expectedLogs: [(
				Events.InterchangeAcknowledged,
				$"Created fault message  for transmit message {requestMessage.EM_MessageNum}.",
				ExpectedOn: new(responseInterchange))]);

		using var responseBodyXmlReader = responseInterchange.GetBodyXmlReader();
		var unpackResult = UnpackingStrategy.Unpack(responseInterchange, requestMessage.Interchange, requestMessage, responseBodyXmlReader, ServiceLog);
		unpackResult.Assert(expectedUnpackResult);
	});
}
