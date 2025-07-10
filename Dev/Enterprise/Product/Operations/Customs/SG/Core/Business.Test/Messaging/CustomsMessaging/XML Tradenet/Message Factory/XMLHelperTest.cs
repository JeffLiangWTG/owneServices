using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	class XMLHelperTest : TestCaseWithFactory
	{
		public void TestMakeDataAlwaysInUppercase()
		{
			AssertMultilineASCIIEquals(expected, XMLHelper.MakeDataAlwaysInUppercase(xml));
		}

		const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# interchange number place holder #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># senders reference place holder #</cbc:SenderID>
  <cbc:RecipientID># recipient reference place holder #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <out:OutwardUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>cnl</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <cac:Cancellation>
        <cac:CancellationHeader>
          <cbc:MessageReference>wtgb00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID># msgno placeholder #</cbc:ID>
            <cbc:Date># message date time create place holder #</cbc:Date>
            <cbc:SequenceNumeric># unique batch number place holder #</cbc:SequenceNumeric>
          </cac:UniqueReferenceNumber>
          <cbc:DeclarantID># senders reference place holder #</cbc:DeclarantID>
          <cbc:CommonAccessReference>outupd</cbc:CommonAccessReference>
          <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
          <cbc:CancellationReasonCode />
        </cac:CancellationHeader>
        <cac:DeclarantParty>
          <cac:PersonInformation>
            <cbc:CodeValue />
            <cbc:Name>test sg4 broker</cbc:Name>
          </cac:PersonInformation>
          <cbc:Telephone />
        </cac:DeclarantParty>
      </cac:Cancellation>
    </out:OutwardUpdate>
  </InboundMessage>
</TradenetDeclaration>";
		const string expected = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <out:OutwardUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>CNL</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <cac:Cancellation>
        <cac:CancellationHeader>
          <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID># MSGNO PLACEHOLDER #</cbc:ID>
            <cbc:Date># MESSAGE DATE TIME CREATE PLACE HOLDER #</cbc:Date>
            <cbc:SequenceNumeric># UNIQUE BATCH NUMBER PLACE HOLDER #</cbc:SequenceNumeric>
          </cac:UniqueReferenceNumber>
          <cbc:DeclarantID># SENDERS REFERENCE PLACE HOLDER #</cbc:DeclarantID>
          <cbc:CommonAccessReference>OUTUPD</cbc:CommonAccessReference>
          <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
          <cbc:CancellationReasonCode />
        </cac:CancellationHeader>
        <cac:DeclarantParty>
          <cac:PersonInformation>
            <cbc:CodeValue />
            <cbc:Name>TEST SG4 BROKER</cbc:Name>
          </cac:PersonInformation>
          <cbc:Telephone />
        </cac:DeclarantParty>
      </cac:Cancellation>
    </out:OutwardUpdate>
  </InboundMessage>
</TradenetDeclaration>";
	}
}
