using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGXmlEDIMessage))]
	sealed class SGXMLEDIMessageTest : XmlEDIMessageTest
	{
		public void TestTradenetDeclaration()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			AssertNull("Should default to null.", message.TradenetDeclaration);
			message.EM_MessageText = @"<TradenetDeclaration
xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2""
xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID>DCST.DCST401</cbc:SenderID>
  <cbc:RecipientID>E01T.E01T001</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
</TradenetDeclaration>";
			AssertNotNull("Should generate a valid TradenetDeclaration.", message.TradenetDeclaration);
		}

		public void TestTradenetResponse()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			AssertNull("Should default to null.", message.TradenetResponse);
			message.EM_MessageText = @"<TradenetResponse
xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse""
xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2""
dateTime=""201102140028""
instanceIdentifier=""92225976"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID>DCST.DCST401</cbc:SenderID>
  <cbc:RecipientID>E01T.E01T001</cbc:RecipientID>
  <cbc:TotalNumberOfResponse>1</cbc:TotalNumberOfResponse>
</TradenetResponse>";
			AssertNotNull("Should generate a valid TradenetResponse.", message.TradenetResponse);
		}

		public new void TestSetDefaultValues()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			AssertEquals(ApplicationCodeList.Codes.SGCustomsTradenetXML, message.EM_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestEDIMessageWithXmlContentIsNotFormattedWithTheEDIMessageFormatter()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			string messageText = @"<UniversalShipment>
  <Shipment>
	<SomeStuff>l:'sk?'dq'+++</SomeStuff>
  </Shipment>
</UniversalShipment>";
			message.EM_MessageText = messageText;
			AssertEquals("message.EM_MessageTextDetail", messageText, message.EM_MessageTextDetail);
		}

		[TestDate(2020, 1, 1)]
		public void TestPopulateMessageNumber()
		{
			try
			{
				Db.Connection.BeginTransaction(); // required for number generation in unit test
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAbbCCddE";
				SGCustomsDataRegistry.Instance.MessageNumberOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 900);
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 9999);
				Env.NumberFountains.SGMessageNumberSequence.GetNextFormatted(Db.Connection);
				var message = Factory.New<TestSGXmlEDIMessage>();
				message.EM_MessageText = $@"<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:fee=""urn:crimsonlogic:tn:schema:xsd:FeeMessage"" xmlns:rej=""urn:crimsonlogic:tn:schema:xsd:RejectionMessage"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:cux=""urn:crimsonlogic:tn:schema:xsd:CustomsExchangeRate"" xmlns:err=""urn:crimsonlogic:tn:schema:xsd:ErrorMessage"" xmlns:com=""urn:crimsonlogic:tn:schema:xsd:CustomsCommonCode-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:app=""urn:crimsonlogic:tn:schema:xsd:ApprovalMessage"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID>&amp;lt;&amp;lt;SENDERS REFERENCE PLACE HOLDER&amp;gt;&amp;gt;</cbc:SenderID>
  <cbc:RecipientID>&amp;lt;&amp;lt;RECIPIENT REFERENCE PLACE HOLDER&amp;gt;&amp;gt;</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <ipt:InPayment>
      <ipt:Header>
        <cbc:DeclarantID>D00002020</cbc:DeclarantID>
        <UniqueReferenceNumber>
          <ID>{SGXmlEDIMessage.MessageNumberPlaceHolderXml}</ID>
          <Date>{SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml}</Date>
          <SequenceNumeric>{SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml}</SequenceNumeric>
      </ipt:Header>
    </ipt:InPayment>
  </InboundMessage>
</TradenetDeclaration>";
				message.PopulateMessageNumber();
				CombineAssertions(() =>
				{
					AssertEquals("EM_MessageNum", "X2020010145444910901", message.EM_MessageNum);
					AssertEquals("EM_ApplicationReference", "AABBCCDDE202001010901", message.EM_ApplicationReference);
					AssertContains("EM_MessageText - MessageNumberPlaceHolderXml", "<ID>AABBCCDDE</ID>", message.EM_MessageText);
					AssertContains("EM_MessageText - MessageDateTimeCreatePlaceHolderXml", "<Date>20200101</Date>", message.EM_MessageText);
					AssertContains("EM_MessageText - UniqueBatchNumberPlaceHolderXml", "<SequenceNumeric>0901</SequenceNumeric>", message.EM_MessageText);
				}

				);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestPopulateMessageNumber_SMNFountain()
		{
			try
			{
				Db.Connection.BeginTransaction();
				var sg1 = Factory.New<GlbCompany>();
				sg1.GC_Code = "SC1";
				sg1.GC_RN_NKCountryCode = "SG";
				sg1.GC_CustomsRegistrationNo = "SA0001";
				var sg1Branch = sg1.Branches.AddNew();
				sg1Branch.GB_Code = "SB1";
				var customsNumber1 = sg1.CustomsNumberProvider.CustomsNumbers.AddNew();
				customsNumber1.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
				customsNumber1.SN_MinimumValue = 1000;
				customsNumber1.SN_MaximumValue = 2000;
				customsNumber1.SN_Value = 1001;
				customsNumber1.SN_FountainName = "SA0001";
				SGCustomsDataRegistry.Instance.MessageNumberOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 900);
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 9999);
				Env.NumberFountains.SGMessageNumberSequence.GetNextFormatted(Db.Connection);
				Factory.Save();
				using (DisposableEnvironment.ForBranch(sg1Branch.PK.ToGuid()))
				{
					var message = Factory.New<TestSGXmlEDIMessage>();
					message.EM_MessageText = $@"<TradenetDeclaration xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:fee=""urn:crimsonlogic:tn:schema:xsd:FeeMessage"" xmlns:rej=""urn:crimsonlogic:tn:schema:xsd:RejectionMessage"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:cux=""urn:crimsonlogic:tn:schema:xsd:CustomsExchangeRate"" xmlns:err=""urn:crimsonlogic:tn:schema:xsd:ErrorMessage"" xmlns:com=""urn:crimsonlogic:tn:schema:xsd:CustomsCommonCode-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:app=""urn:crimsonlogic:tn:schema:xsd:ApprovalMessage"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetDeclaration"">
  <cbc:MessageVersion>041</cbc:MessageVersion>
  <cbc:SenderID>&amp;lt;&amp;lt;SENDERS REFERENCE PLACE HOLDER&amp;gt;&amp;gt;</cbc:SenderID>
  <cbc:RecipientID>&amp;lt;&amp;lt;RECIPIENT REFERENCE PLACE HOLDER&amp;gt;&amp;gt;</cbc:RecipientID>
  <cbc:TotalNumberOfDeclaration>1</cbc:TotalNumberOfDeclaration>
  <InboundMessage>
    <ipt:InPayment>
      <ipt:Header>
        <cbc:DeclarantID>D00002020</cbc:DeclarantID>
        <UniqueReferenceNumber>
          <ID>{SGXmlEDIMessage.MessageNumberPlaceHolderXml}</ID>
          <Date>{SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml}</Date>
          <SequenceNumeric>{SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml}</SequenceNumeric>
      </ipt:Header>
    </ipt:InPayment>
  </InboundMessage>
</TradenetDeclaration>";
					message.PopulateMessageNumber();
					CombineAssertions(() =>
					{
						AssertEquals("EM_MessageNum", "X2020010153433101001", message.EM_MessageNum);
						AssertEquals("EM_ApplicationReference", "SA0001202001011001", message.EM_ApplicationReference);
						AssertContains("EM_MessageText MessageNumberPlaceHolderXml", "<ID>SA0001</ID>", message.EM_MessageText);
						AssertContains("EM_MessageText MessageDateTimeCreatePlaceHolderXml", "<Date>20200101</Date>", message.EM_MessageText);
						AssertContains("EM_MessageText UniqueBatchNumberPlaceHolderXml", "<SequenceNumeric>1001</SequenceNumeric>", message.EM_MessageText);
					}

					);
					Factory.Save();
					message.PopulateMessageNumber();
					AssertEquals("EM_MessageNum is unchanged", "X2020010153433101001", message.EM_MessageNum);
					message.EM_MessageNum = ZString.Empty;
					message.PopulateMessageNumber();
					AssertEquals("EM_MessageNum gets next number", "X2020010153433101002", message.EM_MessageNum);
				}
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<SGXmlEDIMessage>();
		}

		class TestSGXmlEDIMessage : SGXmlEDIMessage
		{
			public TestSGXmlEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new void PopulateMessageNumber()
			{
				base.PopulateMessageNumber();
			}
		}
	}
}
