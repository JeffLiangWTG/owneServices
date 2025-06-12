using System.Xml;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.BT.Helpers
{
    public class OrchestrationHelpers
    {
        public static string CreateSoapRequestMessage(XmlDocument input, string messageNumber, string recipientId)
        {
            var service = GetService(input);
            var address = GetAddress(input);
            var to = GetTo(service, recipientId, address);
            var action = GetAction(service);
            var badgeIdentifier = GetBadge_Identifier(input);
            var tradeId = GetTraderID(input);
            var body = GetBody(input);
            body = body.Replace("<MetaData", @"<MetaData xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms""");

            var output =
                $@"<S:Envelope xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/03/addressing"" xmlns:ebi=""http://www.myvan.descartes.com/ebi/2004/r1"" xmlns:S=""http://www.w3.org/2003/05/soap-envelope"" xmlns:pnt=""http://www.myvan.descartes.com/pnt/2018/r1"">
	<S:Header>
		<wsa:From>
			<wsa:Address>urn:duns:{address}</wsa:Address>
		</wsa:From>
		<wsa:To>{to}</wsa:To>
		<wsa:Action>{action}</wsa:Action>
		<ebi:Sequence>
			<ebi:MessageNumber>{messageNumber}</ebi:MessageNumber>
		</ebi:Sequence>
		<pnt:CSPInformation>
			<pnt:X-Badge-Identifier>{badgeIdentifier}</pnt:X-Badge-Identifier>
			<pnt:SoftwareVendor>WTG</pnt:SoftwareVendor>
			<pnt:TraderID>{tradeId}</pnt:TraderID>
		</pnt:CSPInformation>
	</S:Header>
  <S:Body>
    {body}
  </S:Body>
</S:Envelope>";
            return output;
        }

        static string GetAddress(XmlDocument input)
        {
            var address = GetValueFromXml(input,
                "//*[local-name()='GBCustoms']/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='Credentials']/*[local-name()='PartyID']");
            
            return address;
        }

        static string GetTo(string service, string recipientId, string partyId)
        {
            switch (service)
            {
                case "PentantACA":
                    return "urn:zz:PENTANT_INV";
                default:
                    if ("GBCustomsTest-Pentant".Equals(recipientId) && "211430898".Equals(partyId))
                        return "urn:zz:PENTANT_TDR";

                    return "urn:zz:PENTANT_CDS";

            }
        }

        static string GetAction(string service)
        {
            switch (service)
            {
                case "PentantACA":
                    return "urn:myvan:INVREQ";
                case "AmendDeclaration":
                    return "urn:myvan:CUSDECA";
                case "CancelDeclaration":
                    return "urn:myvan:CUSDECX";
                case "PentantArrival":
                    return "urn:myvan:CUSDECAN";
                case "ExportInventoryConsolidation":
                    return "urn:myvan:INVECREQ";
                case "ExportInventoryMovement":
                    return "urn:myvan:INVEMREQ";
                case "ExportInventoryQuery":
                    return "urn:myvan:INVEQREQ";
                default:
                    return "urn:myvan:CUSDEC";
            }
        }

        static string GetBadge_Identifier(XmlDocument input)
        {
            var rawBadge = GetValueFromXml(input, "//*[local-name()='GBCustoms']/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='Credentials']/*[local-name()='Badge']");
            return rawBadge.Length == 3 ? string.Concat("PNT", rawBadge) : rawBadge;
        }

        static string GetEORI(string credentialKey)
        {
            var result = string.Empty;

            const string pattern = @"\.(.*)\.";
            var match = System.Text.RegularExpressions.Regex.Match(credentialKey, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                result = match.Groups[1].Value;
            }

            return result.Trim();
        }

        static string GetService(XmlDocument input)
        {
            return GetValueFromXml(input, "//*[local-name()='GBCustoms']/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='Service']");
        }

        static string GetTraderID(XmlDocument input)
        {
            var credentialsKey = GetValueFromXml(input, "//*[local-name()='GBCustoms']/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='Credentials']/@Key");
            if (!string.IsNullOrEmpty(credentialsKey))
            {
                return GetEORI(credentialsKey);
            }
            return string.Empty;
        }

        static string GetBody(XmlDocument input)
        {
            var nodes = input.SelectNodes("//*[local-name()='GBCustoms']/*[local-name()='Body']");
            return nodes?[0]?.InnerXml ?? string.Empty;
        }

        static string GetValueFromXml(XmlDocument input, string xpath)
        {
            var nodes = input.SelectNodes(xpath);
            return nodes?[0]?.InnerXml ?? string.Empty;
        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                           