using System.Collections.Generic;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts.DataProviders;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class GetDocumentsResponseUnpackingStrategyTestBase : InterchangeUnpackingStrategyBaseTest
{
	protected override IReadOnlyCollection<XmlQualifiedName> ExpectedSupportedXmlNodes
		=> [Constants.PUESC.DocumentHandlingPort.XmlNodes.GetDocumentsResponse];

	protected override IXmlInterchangeUnpackingStrategy CreateUnpackingStrategy()
		=> new GetDocumentsResponseUnpackingStrategy(new DataProviderFactory(RecognizableMessages.All));
}
