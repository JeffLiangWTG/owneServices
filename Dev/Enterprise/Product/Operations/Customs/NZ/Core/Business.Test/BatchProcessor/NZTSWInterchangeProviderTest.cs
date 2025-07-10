using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Customs.NZ.Business.BatchProcessor.Testing
{
	using System.Data;
	using CargoWise.EntityFramework;
	using Enterprise.Environment;
	using Enterprise.Messaging.InterchangeProviders;
	using Enterprise.Messaging.InterchangeProviders.Testing;
	using Moq.Protected;

	public class NZTSWInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var testBroker = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			testBroker.GS_Code = "TST";
			var wrapper = testBroker.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(testBroker.PK.ToGuid()).Encrypt("NN12WW");
			AssertEquals("Broker Password should be Encyrpted value", "Ix1yvDKoBSDh5nWjOgrt5g==", wrapper.NZBPassword.GP_CurrentPassword);
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = messages.AddNew();
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>CRE</TypeCode><FunctionalReferenceID /><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>TEST CRE Message</Content></AdditionalInformation><AdditionalInformation><RequestOverrideCode>Y</RequestOverrideCode><StatementDescription>Include manual processing request</StatementDescription><StatementTypeCode>ALP</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130301</DepartureDateTime></BorderTransportMeans><Carrier><Name>QANTAS AIRWAYS LIMITED</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalInformation><StatementCode>Y</StatementCode><StatementTypeCode>WOF</StatementTypeCode></AdditionalInformation><Consignee><Name>B &amp; A INTERNATIONAL FASHION</Name><Address><CityName>WEST PENNANT HILLS NSW</CityName><CountryCode>AU</CountryCode><CountrySubDivisionName>NSW</CountrySubDivisionName><Line>14 LYNTON GREEN</Line><PostcodeID>2152</PostcodeID></Address></Consignee><ConsignmentItem><SequenceNumeric>1</SequenceNumeric><Commodity><CargoDescription>MENS GARMENTS</CargoDescription><CommercialCategorizationID /><ValueAmount currencyID=""NZD"">320.0000</ValueAmount><IdentityQualifierCode /></Commodity><GoodsMeasure><GrossMassMeasure unitCode=""KGM"">2</GrossMassMeasure></GoodsMeasure><Origin><CountryCode>NZAKL</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>1</QuantityQuantity><TypeCode>BG</TypeCode></Packaging></ConsignmentItem><Consignor><Name>BRACKS APPAREL (NZ) PTY LTD</Name><Address><CityName>AUCKLAND</CityName><CountryCode>NZ</CountryCode><CountrySubDivisionName>AUK</CountrySubDivisionName><Line>9 DOUGLAS ALEXANDER PDE</Line><PostcodeID /></Address></Consignor><Freight><PaymentMethodCode /></Freight><GoodsLocation><ID /></GoodsLocation><LoadingLocation><ID>NZAKL</ID></LoadingLocation><TransportContractDocument><ID>92840289</ID><TypeCode>HWB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name><ID /></Consolidator></TransportContractDocument><UnloadingLocation><ID>AUSYD</ID></UnloadingLocation></Consignment><Declarant><ID>65432198B</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>61280012206</ID><TypeID>TE</TypeID></Communication></Declarant><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_IsTestMessage = true;
			message.EM_MessageType = Enterprise.Customs.NZ.TradeSingleWindow.MessageTypeList.Codes.CRE;
			message.EM_SystemCreateUser = testBroker.GS_Code;
			message.EM_MessageOwner = "Ix1yvDKoBSDh5nWjOgrt";

			var message2 = messages.AddNew();
			message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>OCR</TypeCode><FunctionalReferenceID>C00025886</FunctionalReferenceID><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><StatementCode>Y</StatementCode><StatementTypeCode>CON</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>CX063</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130124</DepartureDateTime><Itinerary><SequenceNumeric>1</SequenceNumeric><RoutingCountryCode>HK</RoutingCountryCode></Itinerary></BorderTransportMeans><Carrier><Name>CATHAY PACIFIC</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalDocument><ID /><TypeCode>EDO</TypeCode></AdditionalDocument><AssociatedTransportDocument><ID>MATTESTHAWB006</ID><TypeCode>HWB</TypeCode></AssociatedTransportDocument><TransportContractDocument><ID>16066665550</ID><TypeCode>MB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name></Consolidator></TransportContractDocument></Consignment><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message2.EM_IsTestMessage = true;
			message2.EM_MessageType = Enterprise.Customs.NZ.TradeSingleWindow.MessageTypeList.Codes.CRE;
			message2.EM_SystemCreateUser = testBroker.GS_Code;
			message2.EM_MessageOwner = "Ix1yvDKoBSDh5nWjOgrt";

			var message3 = messages.AddNew();
			message3.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>OCR</TypeCode><FunctionalReferenceID>C00001044</FunctionalReferenceID><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>OCR from Consol</Content><StatementCode>Y</StatementCode><StatementTypeCode>CON</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>QF115</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130513</DepartureDateTime><Itinerary><SequenceNumeric>1</SequenceNumeric><RoutingCountryCode>AU</RoutingCountryCode></Itinerary></BorderTransportMeans><Carrier><Name>QANTAS AIRWAYS LIMITED</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalDocument><ID>57848290</ID><TypeCode>EDO</TypeCode></AdditionalDocument><AssociatedTransportDocument><ID>S00001068</ID><TypeCode>HWB</TypeCode></AssociatedTransportDocument><TransportContractDocument><ID>08100293484</ID><TypeCode>MB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name></Consolidator></TransportContractDocument></Consignment><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message3.EM_IsTestMessage = true;
			message3.EM_MessageType = Enterprise.Customs.NZ.TradeSingleWindow.MessageTypeList.Codes.OCR;
			message3.EM_SystemCreateUser = testBroker.GS_Code;
			message3.EM_MessageOwner = "";

			var provider = new NZTSWInterchangeProvider(new LoggingInformation(), messages);

			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("NumberOfInterchanges", 3, interchanges.Count);
			var interchange1 = interchanges[0];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange1.EI_HeaderText);
			AssertEquals("Footer for xml interchange should contain the MAC generated when necessary", "Da+aFY5D4mR5kayeroa2EkKphFNQVihl33QEP38iT5E=", interchange1.EI_FooterText);
			AssertEquals("Body should contain the message1 xml message text", message.EM_MessageText, interchange1.EI_BodyText);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange1.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange1.EI_TransportType);

			var interchange2 = interchanges[1];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange2.EI_HeaderText);
			AssertEquals("Footer for xml interchange should contain the MAC when necessary", "mtB9yDQ+MJMSHpb8ydLwxXrgqpXl247987aNn1Il+wo=", interchange2.EI_FooterText);
			AssertEquals("Body should contain the message2 xml message text", message2.EM_MessageText, interchange2.EI_BodyText);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange2.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange2.EI_TransportType);

			var interchange3 = interchanges[2];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange3.EI_HeaderText);
			AssertEquals("Footer for xml interchange should be empty in this case as Authentication is not required for this message", "", interchange3.EI_FooterText);
			AssertEquals("Body should contain the message3 xml message text", message3.EM_MessageText, interchange3.EI_BodyText);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange3.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange3.EI_TransportType);
		}

		public void TestPopulateInterchangesWithDifferentBrokerIDs()
		{
			var testBroker1 = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			testBroker1.GS_Code = "TST";
			var wrapper1 = testBroker1.GetNZWrapper();
			wrapper1.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(testBroker1.PK.ToGuid()).Encrypt("NN12WW");
			AssertEquals("Broker Password should be Encyrpted value", "Ix1yvDKoBSDh5nWjOgrt5g==", wrapper1.NZBPassword.GP_CurrentPassword);

			var testBroker2 = Factory.Load<GlbStaff>(new ZGuid("70EFA270-3F0F-479C-9AED-0009455622E2"));
			testBroker2.GS_Code = "T2";
			var wrapper2 = testBroker2.GetNZWrapper();
			wrapper2.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(testBroker2.PK.ToGuid()).Encrypt("1234TT");
			AssertEquals("Broker Password should be Encyrpted value", "gmFLFAYbgnlD9NPahiVI1g==", wrapper2.NZBPassword.GP_CurrentPassword);

			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = messages.AddNew();
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>CRE</TypeCode><FunctionalReferenceID /><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>TEST CRE Message</Content></AdditionalInformation><AdditionalInformation><RequestOverrideCode>Y</RequestOverrideCode><StatementDescription>Include manual processing request</StatementDescription><StatementTypeCode>ALP</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130301</DepartureDateTime></BorderTransportMeans><Carrier><Name>QANTAS AIRWAYS LIMITED</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalInformation><StatementCode>Y</StatementCode><StatementTypeCode>WOF</StatementTypeCode></AdditionalInformation><Consignee><Name>B &amp; A INTERNATIONAL FASHION</Name><Address><CityName>WEST PENNANT HILLS NSW</CityName><CountryCode>AU</CountryCode><CountrySubDivisionName>NSW</CountrySubDivisionName><Line>14 LYNTON GREEN</Line><PostcodeID>2152</PostcodeID></Address></Consignee><ConsignmentItem><SequenceNumeric>1</SequenceNumeric><Commodity><CargoDescription>MENS GARMENTS</CargoDescription><CommercialCategorizationID /><ValueAmount currencyID=""NZD"">320.0000</ValueAmount><IdentityQualifierCode /></Commodity><GoodsMeasure><GrossMassMeasure unitCode=""KGM"">2</GrossMassMeasure></GoodsMeasure><Origin><CountryCode>NZAKL</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>1</QuantityQuantity><TypeCode>BG</TypeCode></Packaging></ConsignmentItem><Consignor><Name>BRACKS APPAREL (NZ) PTY LTD</Name><Address><CityName>AUCKLAND</CityName><CountryCode>NZ</CountryCode><CountrySubDivisionName>AUK</CountrySubDivisionName><Line>9 DOUGLAS ALEXANDER PDE</Line><PostcodeID /></Address></Consignor><Freight><PaymentMethodCode /></Freight><GoodsLocation><ID /></GoodsLocation><LoadingLocation><ID>NZAKL</ID></LoadingLocation><TransportContractDocument><ID>92840289</ID><TypeCode>HWB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name><ID /></Consolidator></TransportContractDocument><UnloadingLocation><ID>AUSYD</ID></UnloadingLocation></Consignment><Declarant><ID>65432198B</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>61280012206</ID><TypeID>TE</TypeID></Communication></Declarant><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_IsTestMessage = true;
			message.EM_MessageType = Enterprise.Customs.NZ.TradeSingleWindow.MessageTypeList.Codes.CRE;
			message.EM_SystemCreateUser = testBroker1.GS_Code;
			message.EM_MessageOwner = "Ix1yvDKoBSDh5nWjOgrt";

			var message2 = messages.AddNew();
			message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>OCR</TypeCode><FunctionalReferenceID>C00025886</FunctionalReferenceID><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><StatementCode>Y</StatementCode><StatementTypeCode>CON</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>CX063</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130124</DepartureDateTime><Itinerary><SequenceNumeric>1</SequenceNumeric><RoutingCountryCode>HK</RoutingCountryCode></Itinerary></BorderTransportMeans><Carrier><Name>CATHAY PACIFIC</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalDocument><ID /><TypeCode>EDO</TypeCode></AdditionalDocument><AssociatedTransportDocument><ID>MATTESTHAWB006</ID><TypeCode>HWB</TypeCode></AssociatedTransportDocument><TransportContractDocument><ID>16066665550</ID><TypeCode>MB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name></Consolidator></TransportContractDocument></Consignment><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message2.EM_IsTestMessage = true;
			message2.EM_MessageType = Enterprise.Customs.NZ.TradeSingleWindow.MessageTypeList.Codes.CRE;
			message2.EM_SystemCreateUser = testBroker2.GS_Code;
			message2.EM_MessageOwner = "gmFLFAYbgnlD9NPahiVI";

			var provider = new NZTSWInterchangeProvider(new LoggingInformation(), messages);

			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("NumberOfInterchanges", 2, interchanges.Count);
			var interchange1 = interchanges[0];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange1.EI_HeaderText);
			AssertEquals("Footer for xml interchange should contain the MAC generated when necessary", "Da+aFY5D4mR5kayeroa2EkKphFNQVihl33QEP38iT5E=", interchange1.EI_FooterText);
			AssertEquals("Body should contain the message1 xml message text", message.EM_MessageText, interchange1.EI_BodyText);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange1.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange1.EI_TransportType);

			var interchange2 = interchanges[1];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange2.EI_HeaderText);
			AssertEquals("Footer for xml interchange should contain the MAC when necessary", "GYoU8X1sFAf9w3gJ7GSis+B4XakzW+1wv56pgEalzwU=", interchange2.EI_FooterText);
			AssertEquals("Body should contain the message2 xml message text", message2.EM_MessageText, interchange2.EI_BodyText);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange2.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange2.EI_TransportType);
		}

		public void TestEmptyInterchangeFailsMessageAlso()
		{
			var mock = Factory.NewMoq<EDIMessageDummyForTest>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_HeldUntilDate = ZDateTime.Now.AddDays(-1);
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			AssertEquals("PreCondition", EDIMessage.Status.Queued, message.EM_Status);

			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);
			var logger = new LoggingInformation();
			var provider = new NZTSWInterchangeProvider(logger, messageCollection);

			ErrorReporter.Clear();
			try
			{
				AssertEquals(0, provider.Interchanges.Length);
				Factory.Save();
				message.Reload();

				Assert(logger.UserLogStrings.Count == 1);
				AssertEquals("Message should be failed as it has no text & is not linked to an interchange.", EDIMessage.Status.Failed, message.EM_Status);

				message.EM_MessageText = "TEST";
				message.EM_Status = EDIMessage.Status.Queued;
				provider = new NZTSWInterchangeProvider(logger, messageCollection);

				AssertEquals(1, provider.Interchanges.Length);
				Factory.Save();
				message.Reload();
				AssertEquals("Message should be processed OK.", EDIMessage.Status.Sent, message.EM_Status);
			}
			finally
			{
				ErrorReporter.Clear();
			}
			mock.VerifyAll();
		}

		#region Implementation

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new NZTSWInterchangeProvider(new LoggingInformation(), collection);
		}

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}

		#endregion
	}

	public class EDIMessageDummyForTest : EDIMessage
	{
		public EDIMessageDummyForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "TEST";
		}
	}
}
