using System.Collections.Generic;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using Enterprise.ZArchitecture.Business;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

class AcceptDocumentResponseUnpackingStrategyTest : InterchangeUnpackingStrategyBaseTest
{
	const string TestFilesFolder = "Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles";

	const string ExpectedApplicationReference = "20ce80cf-ebce-40ae-b880-716ddc4138e1";

	public void TestUnpack_AcceptDocumentResponseCorrectBody()
		=> UnpackResponseAndCheckResult(
			"AcceptDocumentResponseCorrectBody",
			ExpectedUnpackResult.Success(
				expectedLogs: [(Events.InterchangeAcknowledged, "AcceptDocumentResponse acknowledgement is processed")]));

	public void TestUnpack_AcceptDocumentResponseWithEmptySysRef()
		=> UnpackResponseAndCheckResult(
			"AcceptDocumentResponseWithEmptySysRef",
			ExpectedUnpackResult.Failure("Interchange processing failed because AcceptDocumentResponse doesn't contain sysRef value."));

	protected override IReadOnlyCollection<XmlQualifiedName> ExpectedSupportedXmlNodes
		=> [Constants.PUESC.DocumentHandlingPort.XmlNodes.AcceptDocumentResponse];

	protected override IXmlInterchangeUnpackingStrategy CreateUnpackingStrategy()
		=> new AcceptDocumentResponseUnpackingStrategy(new DataProviderFactory(RecognizableMessages.All));

	void UnpackResponseAndCheckResult(string testCaseName, ExpectedUnpackResult expectedResult) => CombineAssertions(() =>
	{
		var responseXml = InterchangeTestHelper.GetTestFile($"{TestFilesFolder}.{testCaseName}.xml");
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(responseXml, ApplicationCode.PLCustoms);

		using var responseBodyXmlReader = responseInterchange.GetBodyXmlReader();
		var unpackResult = UnpackingStrategy.Unpack(responseInterchange, requestMessage.Interchange, requestMessage, responseBodyXmlReader, ServiceLog);

		unpackResult.Assert(
			expectedResult,
			logExpectedOnByDefault: new([requestMessage, responseInterchange]),
			assertionsPrefix: testCaseName);
		if (expectedResult.ExpectedSuccess)
		{
			AssertEquals("Validating request message application reference", ExpectedApplicationReference, requestMessage.EM_ApplicationReference);
		}
	});
}
