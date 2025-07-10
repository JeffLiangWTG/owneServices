using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class InvoiceMessageActionTest : ImportMessageActionTest
	{
		public void TestExecuteAction_ImportInvoice()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			int noOfInvoices = Factory.GetDatabaseCount(typeof(BaseJobComInvoiceHeader));

			var buffer = new NotificationBuffer();
			var message = CreateMessage(Factory);
			message.Interchange.EI_InterchangeType = ApplicationCodeList.Codes.XMS;
			message.Interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			message.Interchange.EI_From = "blah";
			message.Interchange.EI_To = "blah blah";
			message.Interchange.EI_BodyText = interchangeMsg;

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Invoices;
			message.EM_MessageText = MessageBody;
			var action = new InvoiceMessageAction(factoryProvider);

			Assert(action.ExecuteAction(message, buffer, out var _));
			Factory.Save();
			AssertEquals(noOfInvoices + 1, Factory.GetDatabaseCount(typeof(BaseJobComInvoiceHeader)));
		}

		readonly string interchangeMsg = string.Format("<XmlInterchange><InterchangeInfo></InterchangeInfo><Payload><Invoices>{0}</Invoices></Payload></XmlInterchange>", MessageBody);

		const string MessageBody = @"<InvoiceHeader><InvoiceNumber>~Test001</InvoiceNumber><InvoiceAmount CurrencyCode=""USD"">0</InvoiceAmount><ExchangeRate>0</ExchangeRate><InvoiceDate>2012-02-01</InvoiceDate><Consignor EDICode=""HAIFEN"" OwnerCode=""HAIFEN""><OrganisationDetails><Name>HAI FENG (WILD LIFE) ARTS LTD</Name><Location Country=""Hong Kong"" City=""Hong Kong"">HKHKG</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1305 WELL TECH CENTRE</AddressLine1><AddressLine2>9 PAT TAT STREET SAN PO KO</AddressLine2><AddressCode>Pickup and Delivery Addre</AddressCode><CityOrSuburb>KONG</CityOrSuburb><Location>HKHKG</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>1305 WELL TECH CENTRE</AddressLine1><AddressLine2>9 PAT TAT STREET, SAN PO KONG, KOWLOON HONG KONG</AddressLine2><AddressCode>PST: 1305 WELL TECH CENTR</AddressCode><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PST"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Consignor><Consignee EDICode=""AASDRA"" OwnerCode=""AASDRA""><OrganisationDetails><Name>A&amp;S FURNISHING CO LTD</Name><Location Country=""Hong Kong"" City=""Hong Kong"">HKHKG</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</AddressLine1><AddressLine2>213 WAI YIP STREET, KWUN TONG, KOWLOON</AddressLine2><AddressCode>PST: UNIT A1, 4TH FLOOR,</AddressCode><CityOrSuburb>HONG KONG</CityOrSuburb><Language>EN</Language><Location>HKHKG</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>UNIT 1, 1ST FLOOR, BLOCK B</AddressLine1><AddressLine2>HOI BUN IND BLDG 6 WIN YIP</AddressLine2><AddressCode>Pick Up Address</AddressCode><CityOrSuburb>KONG</CityOrSuburb><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PIC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Consignee><IsGroupInvoice>false</IsGroupInvoice><Incoterm>FOB</Incoterm><Volume>0</Volume><Weight DimensionType=""KG"">0</Weight><AddCustomsDetails><AddCustomsDetail><CustomsDetailType>BOMLineExpanded</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>EffectDutyDate</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>ExcisableGoods</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>HeaderREL</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>ImporterToOrder</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>IncADJ</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>IsManualTILV</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>IsNonAQISAEPLine</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>IsPAYRECAck</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>IsSubjectToRedLine</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>ORG</CustomsDetailType><CustomsDetailValue>HK</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>PrescribedGoods</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>SendZeroDutyOverride</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>VALB</CustomsDetailType><CustomsDetailValue>TV</CustomsDetailValue></AddCustomsDetail><AddCustomsDetail><CustomsDetailType>VIS</CustomsDetailType><CustomsDetailValue>N</CustomsDetailValue></AddCustomsDetail></AddCustomsDetails><StandAloneInvoiceDirection>EXP</StandAloneInvoiceDirection></InvoiceHeader>";
	}
}
