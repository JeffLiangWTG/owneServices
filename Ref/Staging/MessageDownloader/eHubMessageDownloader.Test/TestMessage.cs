namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader.Test
{
	static class TestMessage
	{
		internal const string NormalStructuredMessage = @"
<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery"">
<Header>
	<SenderID>LFSSINMEL</SenderID>
	<RecipientID>CUSTOMS_DATA_REPO</RecipientID>
	<InterchangeType>RDM</InterchangeType>
	<InterchangeNumber>1126</InterchangeNumber>
</Header>
<Body>
	<RefDbRepoMessage>
		<Source>SG</Source>
		<SubSource>CLASET</SubSource>
		<ContentType>XML</ContentType>
		<Data>TradenetResponse xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse"" xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2""</Data>
    </RefDbRepoMessage>
</Body>
</ns0:GenericMessageInterchange>
";

		internal const string BadStructuredMessage = @"
<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery"">
<Header>
	<SenderID>LFSSINMEL</SenderID>
	<RecipientID>CUSTOMS_DATA_REPO</RecipientID>
	<InterchangeType>RDM</InterchangeType>
	<InterchangeNumber>1126</InterchangeNumber>
</Header>
<Body>
	<RefDbRepoMessage>
		<Source>SG</Source>
		<SubSource>CLASET</SubSource>
		<ContentType>XML</ContentType>
		<Data>&lt;TradenetResponse xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse"" xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration""
/cbc:Code&gt;&lt;cbc:Code&gt;85198191&lt;/cbc:Code&gt;&lt;cac:Description&gt;&lt;cbc:FreeText&gt;OTHER WASTE SOUND RECORDING OR REPRODUCING APPARATUS USING MAGNETIC, OPTICAL OR SEMICONDUCTOR MEDIA NOT ELSEWHERE SPECIFIED  FOR
";
	}
}
