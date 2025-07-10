using System;
using System.Linq;
using Enterprise.Customs.SG.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	public class TradenetDeclarationMessageBuilderTest : BaseTradenetMessageBuilderTest<TradenetDeclaration>
	{
		public override void TestCreateTradeNetMessageParent()
		{
			var builder = GetBuilder();
			var declaration = builder.CreateTradeNetMessageParent();
			AssertType<TradenetDeclaration>(declaration);
		}

		[TestDate(2020, 01, 03, 12, 30, 50)]
		public override void TestGetMessageContent()
		{
			var builder = GetBuilder();
			var declaration = builder.CreateTradeNetMessageParent();
			var inboundMessage = declaration.InboundMessage = new TradenetDeclarationInboundMessage();
			inboundMessage.InPayment = new InPayment()
			{ Header = new Header { DeclarantID = "DABCD2020" } };
			var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <ipt:InPayment>
      <ipt:Header>
        <cbc:DeclarantID>DABCD2020</cbc:DeclarantID>
      </ipt:Header>
    </ipt:InPayment>
  </InboundMessage>
</TradenetDeclaration>";
			var actualMessage = builder.GetMessageContent(declaration);
			Assert("actualMessage is not expected", XmlUtil.CompareXmlElements(expectedMessage, actualMessage));
		}

		[TestDate(2020, 01, 03, 12, 30, 50)]
		public void TestGetOriginalMessage()
		{
			IMessageFactory builder = GetBuilder();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var inpOriginal = builder.GetOriginalMessage(EntryHeader);
			AssertType<INPDEC>(inpOriginal);
			Assert("ExpectedINPDEC is not expected", XmlUtil.CompareXmlElements(ExpectedINPDEC, inpOriginal.MessageText));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var iptOriginal = builder.GetOriginalMessage(EntryHeader);
			AssertType<IPTDEC>(iptOriginal);
			Assert("ExpectedIPTDEC is not expected", XmlUtil.CompareXmlElements(ExpectedIPTDEC, iptOriginal.MessageText));
			Declaration.JE_MessageType = "XXX";
			AssertExceptionThrown<NotSupportedException>("Unrecognised EntryType", "Unspecified is not supported yet.", () => builder.GetOriginalMessage(EntryHeader));
		}

		[TestDate(2020, 01, 03, 12, 30, 50)]
		public void TestGetAmendmentMessage()
		{
			IMessageFactory builder = GetBuilder();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var inpAmendment = builder.GetAmendmentMessage(EntryHeader);
			AssertType<INPUPD>(inpAmendment);
			Assert("ExpectedINPUPD is not expected", XmlUtil.CompareXmlElements(ExpectedINPUPD, inpAmendment.MessageText));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var iptAmendment = builder.GetAmendmentMessage(EntryHeader);
			AssertType<IPTUPD>(iptAmendment);
			Assert("ExpectedIPTUPD is not expected", XmlUtil.CompareXmlElements(ExpectedIPTUPD, iptAmendment.MessageText));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			var outAmendment = builder.GetAmendmentMessage(EntryHeader);
			AssertType<OUTUPD>(outAmendment);
			Assert("ExpectedOUTUPD is not expected", XmlUtil.CompareXmlElements(ExpectedOUTUPD, outAmendment.MessageText));
			Declaration.JE_MessageType = "XXX";
			AssertExceptionThrown<NotSupportedException>("Unrecognised EntryType", "Unspecified is not supported yet.", () => builder.GetAmendmentMessage(EntryHeader));
		}

		[TestDate(2020, 01, 03, 12, 30, 50)]
		public void TestGetRefundMessage()
		{
			IMessageFactory builder = GetBuilder();
			var line1 = Declaration.InvoiceLines.First() as JobComInvoiceLine;
			line1.SG_RefundForItemCustomsDutyAmount = 22m;
			line1.SG_RefundForItemExciseAmount = 33m;
			line1.SG_RefundForItemGSTAmount = 44m;
			var item1 = EntryHeader.MergedLines.AddNew();
			item1.InvoiceLines.Add(line1);
			var additionalMessageInformation = new RefundAdditionalMessageInformation(Declaration, Factory);
			additionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var inpRefund = builder.GetRefundMessage(EntryHeader);
			AssertNull(inpRefund);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var iptRefund = builder.GetRefundMessage(EntryHeader);
			AssertType<IPTUPDRefund>(iptRefund);
			Assert("ExpectedIPTUPDRefund is not expected", XmlUtil.CompareXmlElements(ExpectedIPTUPDRefund, iptRefund.MessageText));
			Declaration.JE_MessageType = "XXX";
			AssertExceptionThrown<NotSupportedException>("Unrecognised EntryType", "Unspecified is not supported yet.", () => builder.GetRefundMessage(EntryHeader));
		}

		[TestDate(2020, 01, 03, 12, 30, 50)]
		public void TestGetCancellationMessage()
		{
			IMessageFactory builder = GetBuilder();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var inpCancellation = builder.GetCancellationMessage(EntryHeader);
			AssertType<INPUPDCancel>(inpCancellation);
			Assert("ExpectedINPUPDCancel is not expected", XmlUtil.CompareXmlElements(ExpectedINPUPDCancel, inpCancellation.MessageText));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var iptCancellation = builder.GetCancellationMessage(EntryHeader);
			AssertType<IPTUPDCancel>(iptCancellation);
			Assert("ExpectedIPTUPDCancel is not expected", XmlUtil.CompareXmlElements(ExpectedIPTUPDCancel, iptCancellation.MessageText));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			var outCancellation = builder.GetCancellationMessage(EntryHeader);
			AssertType<OUTUPDCancel>(outCancellation);
			Assert("ExpectedOUTUPDCancel is not expected", XmlUtil.CompareXmlElements(ExpectedOUTUPDCancel, outCancellation.MessageText));
			Declaration.JE_MessageType = "XXX";
			AssertExceptionThrown<NotSupportedException>("Unrecognised EntryType", "Unspecified is not supported yet.", () => builder.GetCancellationMessage(EntryHeader));
		}

		const string ExpectedINPDEC = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <inp:InNonPayment>
      <inp:Header>
        <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
        <cac:UniqueReferenceNumber>
          <cbc:ID># MSGNO PLACEHOLDER #</cbc:ID>
          <cbc:Date># MESSAGE DATE TIME CREATE PLACE HOLDER #</cbc:Date>
          <cbc:SequenceNumeric># UNIQUE BATCH NUMBER PLACE HOLDER #</cbc:SequenceNumeric>
        </cac:UniqueReferenceNumber>
        <cbc:DeclarantID># SENDERS REFERENCE PLACE HOLDER #</cbc:DeclarantID>
        <cbc:CommonAccessReference>INPDEC</cbc:CommonAccessReference>
        <cbc:DeclarationType>SFZ</cbc:DeclarationType>
        <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
      </inp:Header>
      <inp:Cargo>
        <cbc:CargoPackingType>5</cbc:CargoPackingType>
        <cac:ReleaseLocation>
          <cbc:LocationCode />
        </cac:ReleaseLocation>
        <cac:ReceiptLocation>
          <cbc:LocationCode />
        </cac:ReceiptLocation>
      </inp:Cargo>
      <inp:Party>
        <cac:DeclarantParty>
          <cac:PersonInformation>
            <cbc:CodeValue />
            <cbc:Name>TEST SG4 BROKER</cbc:Name>
          </cac:PersonInformation>
          <cbc:Telephone />
        </cac:DeclarantParty>
        <cac:DeclaringAgentParty>
          <cac:PartyIdentification>
            <cbc:ID>AAA374M</cbc:ID>
          </cac:PartyIdentification>
          <cac:PartyName>
            <cbc:Name>EAGLE DATAMATION INTERNATIONAL</cbc:Name>
          </cac:PartyName>
        </cac:DeclaringAgentParty>
        <cac:FreightForwarderParty>
          <cac:PartyIdentification>
            <cbc:ID />
          </cac:PartyIdentification>
          <cac:PartyName>
            <cbc:Name>EDI CUSTOMS BROKERS</cbc:Name>
          </cac:PartyName>
        </cac:FreightForwarderParty>
      </inp:Party>
      <inp:Summary>
        <cbc:NumberOfItems>0</cbc:NumberOfItems>
        <cbc:TotalOuterPack unitCode=""PKG"">0</cbc:TotalOuterPack>
        <cbc:TotalGrossWeight unitCode=""KGM"">50.000</cbc:TotalGrossWeight>
      </inp:Summary>
    </inp:InNonPayment>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedIPTDEC = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <ipt:InPayment>
      <ipt:Header>
        <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
        <cac:UniqueReferenceNumber>
          <cbc:ID># MSGNO PLACEHOLDER #</cbc:ID>
          <cbc:Date># MESSAGE DATE TIME CREATE PLACE HOLDER #</cbc:Date>
          <cbc:SequenceNumeric># UNIQUE BATCH NUMBER PLACE HOLDER #</cbc:SequenceNumeric>
        </cac:UniqueReferenceNumber>
        <cbc:DeclarantID># SENDERS REFERENCE PLACE HOLDER #</cbc:DeclarantID>
        <cbc:CommonAccessReference>IPTDEC</cbc:CommonAccessReference>
        <cbc:DeclarationType>GST</cbc:DeclarationType>
        <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
      </ipt:Header>
      <ipt:Cargo>
        <cbc:CargoPackingType>5</cbc:CargoPackingType>
        <cac:ReleaseLocation>
          <cbc:LocationCode />
        </cac:ReleaseLocation>
        <cac:ReceiptLocation>
          <cbc:LocationCode />
        </cac:ReceiptLocation>
      </ipt:Cargo>
      <ipt:Party>
        <cac:DeclarantParty>
          <cac:PersonInformation>
            <cbc:CodeValue />
            <cbc:Name>TEST SG4 BROKER</cbc:Name>
          </cac:PersonInformation>
          <cbc:Telephone />
        </cac:DeclarantParty>
        <cac:DeclaringAgentParty>
          <cac:PartyIdentification>
            <cbc:ID>AAA374M</cbc:ID>
          </cac:PartyIdentification>
          <cac:PartyName>
            <cbc:Name>EAGLE DATAMATION INTERNATIONAL</cbc:Name>
          </cac:PartyName>
        </cac:DeclaringAgentParty>
        <cac:FreightForwarderParty>
          <cac:PartyIdentification>
            <cbc:ID />
          </cac:PartyIdentification>
          <cac:PartyName>
            <cbc:Name>EDI CUSTOMS BROKERS</cbc:Name>
          </cac:PartyName>
        </cac:FreightForwarderParty>
      </ipt:Party>
      <ipt:Summary>
        <cbc:NumberOfItems>0</cbc:NumberOfItems>
        <cbc:TotalOuterPack unitCode=""PKG"">0</cbc:TotalOuterPack>
        <cbc:TotalGrossWeight unitCode=""KGM"">50.000</cbc:TotalGrossWeight>
      </ipt:Summary>
    </ipt:InPayment>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedINPUPD = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <inp:InNonPaymentUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>AME</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <inp:Declaration>
        <inp:Header>
          <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID># MSGNO PLACEHOLDER #</cbc:ID>
            <cbc:Date># MESSAGE DATE TIME CREATE PLACE HOLDER #</cbc:Date>
            <cbc:SequenceNumeric># UNIQUE BATCH NUMBER PLACE HOLDER #</cbc:SequenceNumeric>
          </cac:UniqueReferenceNumber>
          <cbc:DeclarantID># SENDERS REFERENCE PLACE HOLDER #</cbc:DeclarantID>
          <cbc:CommonAccessReference>INPUPD</cbc:CommonAccessReference>
          <cbc:DeclarationType>SFZ</cbc:DeclarationType>
          <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
        </inp:Header>
        <inp:Cargo>
          <cbc:CargoPackingType>5</cbc:CargoPackingType>
          <cac:ReleaseLocation>
            <cbc:LocationCode />
          </cac:ReleaseLocation>
          <cac:ReceiptLocation>
            <cbc:LocationCode />
          </cac:ReceiptLocation>
        </inp:Cargo>
        <inp:Party>
          <cac:DeclarantParty>
            <cac:PersonInformation>
              <cbc:CodeValue />
              <cbc:Name>TEST SG4 BROKER</cbc:Name>
            </cac:PersonInformation>
            <cbc:Telephone />
          </cac:DeclarantParty>
          <cac:DeclaringAgentParty>
            <cac:PartyIdentification>
              <cbc:ID>AAA374M</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
              <cbc:Name>EAGLE DATAMATION INTERNATIONAL</cbc:Name>
            </cac:PartyName>
          </cac:DeclaringAgentParty>
          <cac:FreightForwarderParty>
            <cac:PartyIdentification>
              <cbc:ID />
            </cac:PartyIdentification>
            <cac:PartyName>
              <cbc:Name>EDI CUSTOMS BROKERS</cbc:Name>
            </cac:PartyName>
          </cac:FreightForwarderParty>
        </inp:Party>
        <inp:Summary>
          <cbc:NumberOfItems>0</cbc:NumberOfItems>
          <cbc:TotalOuterPack unitCode=""PKG"">0</cbc:TotalOuterPack>
          <cbc:TotalGrossWeight unitCode=""KGM"">50.000</cbc:TotalGrossWeight>
        </inp:Summary>
      </inp:Declaration>
    </inp:InNonPaymentUpdate>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedIPTUPD = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <ipt:InPaymentUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>AME</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <ipt:Declaration>
        <ipt:Header>
          <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID># MSGNO PLACEHOLDER #</cbc:ID>
            <cbc:Date># MESSAGE DATE TIME CREATE PLACE HOLDER #</cbc:Date>
            <cbc:SequenceNumeric># UNIQUE BATCH NUMBER PLACE HOLDER #</cbc:SequenceNumeric>
          </cac:UniqueReferenceNumber>
          <cbc:DeclarantID># SENDERS REFERENCE PLACE HOLDER #</cbc:DeclarantID>
          <cbc:CommonAccessReference>IPTUPD</cbc:CommonAccessReference>
          <cbc:DeclarationType>GST</cbc:DeclarationType>
          <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
        </ipt:Header>
        <ipt:Cargo>
          <cbc:CargoPackingType>5</cbc:CargoPackingType>
          <cac:ReleaseLocation>
            <cbc:LocationCode />
          </cac:ReleaseLocation>
          <cac:ReceiptLocation>
            <cbc:LocationCode />
          </cac:ReceiptLocation>
        </ipt:Cargo>
        <ipt:Party>
          <cac:DeclarantParty>
            <cac:PersonInformation>
              <cbc:CodeValue />
              <cbc:Name>TEST SG4 BROKER</cbc:Name>
            </cac:PersonInformation>
            <cbc:Telephone />
          </cac:DeclarantParty>
          <cac:DeclaringAgentParty>
            <cac:PartyIdentification>
              <cbc:ID>AAA374M</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
              <cbc:Name>EAGLE DATAMATION INTERNATIONAL</cbc:Name>
            </cac:PartyName>
          </cac:DeclaringAgentParty>
          <cac:FreightForwarderParty>
            <cac:PartyIdentification>
              <cbc:ID />
            </cac:PartyIdentification>
            <cac:PartyName>
              <cbc:Name>EDI CUSTOMS BROKERS</cbc:Name>
            </cac:PartyName>
          </cac:FreightForwarderParty>
        </ipt:Party>
        <ipt:Summary>
          <cbc:NumberOfItems>0</cbc:NumberOfItems>
          <cbc:TotalOuterPack unitCode=""PKG"">0</cbc:TotalOuterPack>
          <cbc:TotalGrossWeight unitCode=""KGM"">50.000</cbc:TotalGrossWeight>
        </ipt:Summary>
      </ipt:Declaration>
    </ipt:InPaymentUpdate>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedINPUPDCancel = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <inp:InNonPaymentUpdate>
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
          <cbc:CommonAccessReference>INPUPD</cbc:CommonAccessReference>
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
    </inp:InNonPaymentUpdate>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedIPTUPDRefund = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <ipt:InPaymentUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>PRS</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <cac:RefundOnly>
        <cac:RefundHeader>
          <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID># MSGNO PLACEHOLDER #</cbc:ID>
            <cbc:Date># MESSAGE DATE TIME CREATE PLACE HOLDER #</cbc:Date>
            <cbc:SequenceNumeric># UNIQUE BATCH NUMBER PLACE HOLDER #</cbc:SequenceNumeric>
          </cac:UniqueReferenceNumber>
          <cbc:DeclarantID># SENDERS REFERENCE PLACE HOLDER #</cbc:DeclarantID>
          <cbc:CommonAccessReference>IPTUPD</cbc:CommonAccessReference>
          <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
        </cac:RefundHeader>
        <cac:DeclarantParty>
          <cac:PersonInformation>
            <cbc:CodeValue />
            <cbc:Name>TEST SG4 BROKER</cbc:Name>
          </cac:PersonInformation>
          <cbc:Telephone />
        </cac:DeclarantParty>
        <cac:RefundItem>
          <cbc:ItemSequenceNumeric>1</cbc:ItemSequenceNumeric>
          <cbc:ItemHarmonizedSystemCode />
          <cac:TariffRefund>
            <cbc:GoodsAndServicesTaxRefundAmount>44.00</cbc:GoodsAndServicesTaxRefundAmount>
            <cbc:ExciseDutyRefundAmount>33.00</cbc:ExciseDutyRefundAmount>
            <cbc:CustomsDutyRefundAmount>22.00</cbc:CustomsDutyRefundAmount>
          </cac:TariffRefund>
        </cac:RefundItem>
        <cac:RefundSummary>
          <cac:TotalTariffRefund>
            <cbc:TotalGoodsAndServicesTaxRefundAmount>44.00</cbc:TotalGoodsAndServicesTaxRefundAmount>
            <cbc:TotalExciseDutyRefundAmount>33.00</cbc:TotalExciseDutyRefundAmount>
            <cbc:TotalCustomsDutyRefundAmount>22.00</cbc:TotalCustomsDutyRefundAmount>
          </cac:TotalTariffRefund>
        </cac:RefundSummary>
      </cac:RefundOnly>
    </ipt:InPaymentUpdate>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedIPTUPDCancel = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <ipt:InPaymentUpdate>
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
          <cbc:CommonAccessReference>IPTUPD</cbc:CommonAccessReference>
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
    </ipt:InPaymentUpdate>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedOUTUPD = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202001031230"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <out:OutwardUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>AME</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <out:Declaration>
        <out:Header>
          <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID># MSGNO PLACEHOLDER #</cbc:ID>
            <cbc:Date># MESSAGE DATE TIME CREATE PLACE HOLDER #</cbc:Date>
            <cbc:SequenceNumeric># UNIQUE BATCH NUMBER PLACE HOLDER #</cbc:SequenceNumeric>
          </cac:UniqueReferenceNumber>
          <cbc:DeclarantID># SENDERS REFERENCE PLACE HOLDER #</cbc:DeclarantID>
          <cbc:CommonAccessReference>OUTUPD</cbc:CommonAccessReference>
          <cbc:DeclarationType>DRT</cbc:DeclarationType>
          <cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
        </out:Header>
        <out:Cargo>
          <cbc:CargoPackingType>5</cbc:CargoPackingType>
          <cac:ReleaseLocation>
            <cbc:LocationCode />
          </cac:ReleaseLocation>
          <cac:ReceiptLocation>
            <cbc:LocationCode />
          </cac:ReceiptLocation>
        </out:Cargo>
        <out:Party>
          <cac:DeclarantParty>
            <cac:PersonInformation>
              <cbc:CodeValue />
              <cbc:Name>TEST SG4 BROKER</cbc:Name>
            </cac:PersonInformation>
            <cbc:Telephone />
          </cac:DeclarantParty>
          <cac:DeclaringAgentParty>
            <cac:PartyIdentification>
              <cbc:ID>AAA374M</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
              <cbc:Name>EAGLE DATAMATION INTERNATIONAL</cbc:Name>
            </cac:PartyName>
          </cac:DeclaringAgentParty>
          <cac:FreightForwarderParty>
            <cac:PartyIdentification>
              <cbc:ID />
            </cac:PartyIdentification>
            <cac:PartyName>
              <cbc:Name>EDI CUSTOMS BROKERS</cbc:Name>
            </cac:PartyName>
          </cac:FreightForwarderParty>
        </out:Party>
        <out:Summary>
          <cbc:NumberOfItems>0</cbc:NumberOfItems>
          <cbc:TotalOuterPack unitCode=""PKG"">0</cbc:TotalOuterPack>
          <cbc:TotalGrossWeight unitCode=""KGM"">50.000</cbc:TotalGrossWeight>
        </out:Summary>
      </out:Declaration>
    </out:OutwardUpdate>
  </InboundMessage>
</TradenetDeclaration>";
		const string ExpectedOUTUPDCancel = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
		CusEntryHeader EntryHeader
		{
			get
			{
				if (entryHeader == null)
				{
					entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
					entryHeader.Factory.RefreshEnabled = false;
					entryHeader.CH_EntryStatus = "XXX";
					entryHeader.Factory.Save();
				}

				return entryHeader;
			}
		}

		CusEntryHeader entryHeader;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					var testSGBroker = Factory.New<GlbStaff>();
					testSGBroker.GS_Code = "TST";
					testSGBroker.GS_FullName = "Test SG4 Broker";
					var wrapper = SGGlbStaffWrapper.Get(testSGBroker);
					wrapper.Tradenetv4Password.GP_UserID = "ASDF23L";
					GlbStaff.CurrentUser.GS_Code = testSGBroker.GS_Code;
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.JE_GS_NKCusAgent = testSGBroker.GS_Code;
					declaration.JE_TotalWeight = 50m;
					declaration.JE_TotalWeightUnit = "KG";
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
					AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
					declaration.AdditionalMessageInformation = additionalMessageInformation;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		[TestDate(2020, 05, 15, 09, 30, 50)]
		public void TestGetTNPMessage()
		{
			var builder = GetBuilder();
			var declaration = builder.CreateTradeNetMessageParent();
			var inboundMessage = declaration.InboundMessage = new TradenetDeclarationInboundMessage();
			inboundMessage.TranshipmentMovement = new TranshipmentMovement()
			{ Header = new Header { DeclarantID = "v13t001" } };
			var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" dateTime=""202005150930"" instanceIdentifier=""# INTERCHANGE NUMBER PLACE HOLDER #"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID># SENDERS REFERENCE PLACE HOLDER #</cbc:SenderID>
  <cbc:RecipientID># RECIPIENT REFERENCE PLACE HOLDER #</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <tnp:TranshipmentMovement>
      <tnp:Header>
        <cbc:DeclarantID>V13T001</cbc:DeclarantID>
      </tnp:Header>
    </tnp:TranshipmentMovement>
  </InboundMessage>
</TradenetDeclaration>";
			var actualMessage = builder.GetMessageContent(declaration);
			Assert("actualMessage is not expected", XmlUtil.CompareXmlElements(expectedMessage, actualMessage));
		}

		protected override TradeNetMessageFactory<TradenetDeclaration> GetBuilder() => new TradenetDeclarationMessageFactory();
	}
}
