using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGEDIInterchange))]
	public class EDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2020, 5, 20)]
		public void TestCreateNewInterchangeFromXmlOrEdifactString()
		{
			string interchangeString = "UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T001:ZZ+20120210:0808+91461284++APERAK'UNH+1+APERAK:D:09B:UN:041+ERRORM'BGM+963+199702247W       201202096951'ERC+E17001'FTX+AAO+++DUPLICATE DECLARATION NUMBER:0:BGM:0#0'UNT+5+1'UNZ+1+91461284'";
			interchange = SGEDIInterchange.CreateNewInterchangeFromXmlOrEdifactString(Factory, interchangeString, ZString.Empty);
			AssertEquals("EDIFACT interchange generated", "UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T001:ZZ+20120210:0808+91461284++APERAK'", interchange.EI_HeaderText);
			AssertEquals("EDIFACT interchange generated", "UNH+1+APERAK:D:09B:UN:041+ERRORM'BGM+963+199702247W       201202096951'ERC+E17001'FTX+AAO+++DUPLICATE DECLARATION NUMBER:0:BGM:0#0'UNT+5+1'", interchange.EI_BodyText);
			AssertEquals("EDIFACT interchange generated", "UNZ+1+91461284'", interchange.EI_FooterText);
			interchangeString = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
			interchange = SGEDIInterchange.CreateNewInterchangeFromXmlOrEdifactString(Factory, interchangeString, EDIInterchange.ApplicationCodes.SingaporeTradenet4);
			AssertEquals("xml interchange generated - Header needs to be empty", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("xml interchange generated", interchangeString, interchange.EI_BodyText);
			AssertEquals("xml interchange generated - Footer needs to be empty", ZString.Empty, interchange.EI_FooterText);
			interchangeString = xmlErrorResponse;
			interchange = SGEDIInterchange.CreateNewInterchangeFromXmlOrEdifactString(Factory, interchangeString, EDIInterchange.ApplicationCodes.SingaporeTradenet4);
			Factory.Save();
			AssertEquals("xml interchange generated - Header needs to be empty", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("xml interchange generated", interchangeString, interchange.EI_BodyText);
			AssertEquals("xml interchange generated - Footer needs to be empty", ZString.Empty, interchange.EI_FooterText);
			AssertEquals("TradeNet xml messages", "SGX", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "SGX", interchange.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", "RCV", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_Priority", "HGH", interchange.EI_Priority);
			AssertEquals("EI_Status", "RCV", interchange.EI_Status);
			AssertEquals("EI_InterchangeNum", "202005209233", interchange.EI_InterchangeNum);
			AssertEquals("EI_From", "DCST.DCST401", interchange.EI_From);
			AssertEquals("EI_To", "V13T.V13T001", interchange.EI_To);
			var message = (SGXmlEDIMessage)interchange.ContainedMessages.Single();
			AssertEquals("EM_MessageText", interchangeString, message.EM_MessageText);
			AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.SGCustomsTradenetXML, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "XMS", message.EM_MessageType);
			AssertEquals("EM_Status", EDIInterchange.Status.Queued, message.EM_Status);
		}

		const string xmlErrorResponse = @"<TradenetResponse xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:app=""urn:crimsonlogic:tn:schema:xsd:ApprovalMessage"" dateTime=""202005200732"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:rej=""urn:crimsonlogic:tn:schema:xsd:RejectionMessage"" xmlns:fee=""urn:crimsonlogic:tn:schema:xsd:FeeMessage"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse"" xmlns:err=""urn:crimsonlogic:tn:schema:xsd:ErrorMessage"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:com=""urn:crimsonlogic:tn:schema:xsd:CustomsCommonCode"" xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" xmlns:cux=""urn:crimsonlogic:tn:schema:xsd:CustomsExchangeRate"" instanceIdentifier=""91237649""><cbc:MessageVersion>041</cbc:MessageVersion><cbc:SenderID>DCST.DCST401</cbc:SenderID><cbc:RecipientID>V13T.V13T001</cbc:RecipientID><cbc:TotalNumberOfResponse>1</cbc:TotalNumberOfResponse><OutboundMessage><err:ErrorMessage><cbc:MessageReference>1</cbc:MessageReference><cac:UniqueReferenceNumber><cbc:ID>199702247W       </cbc:ID><cbc:Date>20200520</cbc:Date><cbc:SequenceNumeric>9233</cbc:SequenceNumeric></cac:UniqueReferenceNumber><cbc:CommonAccessReference>ERRORM</cbc:CommonAccessReference><cac:ErrorDetail><cbc:ErrorCode>E10123</cbc:ErrorCode><cbc:ErrorDescription>INVALID DATA</cbc:ErrorDescription><cbc:ErrorTrace>#</cbc:ErrorTrace></cac:ErrorDetail></err:ErrorMessage></OutboundMessage></TradenetResponse>";
		protected EDIInterchange interchange;
	}
}
