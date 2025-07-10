using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class IM1MessageBuilderTest : TSWMessageBuilderTest
	{
		public void TestShouldNotThrownExcpetionIfNoImporter()
		{
			CreateImportSeaJob();
			JobDeclaration.JE_MasterBill = "OB293042-24902Y2992203928";
			JobDeclaration.JE_HouseBill = "123456789B123456789C123456789D12345";
			JobDeclaration.JE_OH_Importer = ZGuid.Empty;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			AssertNoExceptionThrown(() => im1Builder.GetXMLMessage());
		}

		public void TestPopulateCurrencies_XmlMessage_Contains_NoDuplicateCurrencyEntries()
		{
			CreateExportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice2.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_JZ = invoice2.PK;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			foreach (var invoiceHeader in entryHeader.InvoiceHeaders)
			{
				invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			}

			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var ex1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var xmlMessage = ex1Builder.GetXMLMessage();
			var xmlDoc = XDocument.Parse(xmlMessage);
			var exchangeRates = xmlDoc.Descendants().Where(xe => xe.Name.LocalName == "CurrencyExchange");
			Assert("All invoice headers are for NZD currency", entryHeader.InvoiceHeaders.All(x => x.JZ_RX_NKInvoice_Currency == Core.Constants.CurrencyCodes.NewZealand));
			AssertEquals("CusEntry header contains 2 invoice headers", 2, entryHeader.InvoiceHeaders.Length);
			AssertEquals("Xml message contains only 1 currency change rate", 1, exchangeRates.Count());
		}

		[TestDate(2013, 03, 14)]
		public void TestIM1MessageBuilder()
		{
			SetUpNZTaxOrFee(Factory);
			var expectedResult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BIS00002309</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">15000</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20130314</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalDocument>
    <ID>POD,123456</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>Test Certificate</ID>
    <TypeCode>CER</TypeCode>
  </AdditionalDocument>
  <Agent>
    <ID>0092178J</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>HYOGO MARU</Name>
    <ID />
    <TypeCode>1</TypeCode>
    <JourneyID>227W</JourneyID>
  </BorderTransportMeans>
  <Carrier>
    <Name>ANL SHIPPING</Name>
  </Carrier>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">2075.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>SG</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <AdditionalInformation>
        <StatementDescription>YYNNNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <LoadingLocation>
        <ID>USLAX</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>OB293042-24902</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>40</CharacteristicCode>
        <FullnessCode />
        <ID>FMKU00329847</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode />
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">0</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSP</StatementCode>
        <StatementDescription>Y</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSR</StatementCode>
        <StatementDescription>SUPPLIERGSTNO</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>7176F</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>PERFUMED BATH SALTS ETC</Description>
        <LotNumberID>LN000100999</LotNumberID>
        <ProductBestBeforeDateTime formatCode=""102"">20140630</ProductBestBeforeDateTime>
        <ValueAmount currencyID=""NZD"">0</ValueAmount>
        <IntendedUseCode>SP</IntendedUseCode>
        <Classification>
          <ID>3307300000E</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Itinerary>
          <SequenceNumeric>1</SequenceNumeric>
          <RoutingCountryCode>AU</RoutingCountryCode>
        </Itinerary>
        <Manufacturer>
          <Name>PURESTEEL FABRICATORS</Name>
          <Address>
            <CityName>SYDNEY</CityName>
            <CountryCode />
            <CountrySubDivisionName>NSW</CountrySubDivisionName>
            <Line>UNIT 18, 100 MAIN ST. WOLLOOMOOLOO</Line>
            <PostcodeID>2009</PostcodeID>
          </Address>
        </Manufacturer>
        <ProductName>
          <Name>BONDS</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>
        <Source>
          <CountryCode>NZ</CountryCode>
        </Source>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>FMKU00329847</ID>
        </TransportEquipment>
      </Commodity>
      <ExaminationPlace>
        <Name>ABC Fumigators Pty Ltd.</Name>
        <ID />
      </ExaminationPlace>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">15000</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""KGM"">1500</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>NZ</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <PackingMaterialDescription>STRAW</PackingMaterialDescription>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">200</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">55</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID />
      <ConditionCode />
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>CNT</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			CreateImportSeaJob();
			JobDeclaration.JE_RL_NKPortOfLoading = "USLAX";
			var leg1 = JobDeclaration.Transports[0];
			leg1.JW_VoyageFlight = "227W";
			leg1.JW_RL_NKLoadPort = "USLAX";
			leg1.JW_RL_NKDiscPort = "AUSYD";
			leg1.JW_ETD = new ZDateTime(2018, 01, 02);
			var leg2 = JobDeclaration.Transports.AddNew();
			leg2.JW_VoyageFlight = "132E";
			leg2.JW_RL_NKLoadPort = "AUSYD";
			leg2.JW_RL_NKDiscPort = "NZAKL";
			leg2.JW_ETD = new ZDateTime(2018, 01, 15);
			AssertEquals("Pre-condition: Declaration has two transport legs", 2, JobDeclaration.Transports.Count);
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertMultilineASCIIEquals("Import Sea - Original Message", expectedResult, im1MessageString);
		}

		[TestDate(2013, 03, 14)]
		public void TestTotalGrossWeightIsRounded()
		{
			var totalGrossMassMeasureTagText = @"<TotalGrossMassMeasure unitCode=""KGM"">{0}</TotalGrossMassMeasure>";
			CreateImportAirJob();
			AssertEquals("JE_TotalWeight has decimals", 150.752m, JobDeclaration.JE_TotalWeight);
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			var expectedResult = string.Format(totalGrossMassMeasureTagText, "151");
			AssertContains("IM1 Message - gross weight element should be rounded", expectedResult, im1MessageString);
			JobDeclaration.JE_TotalWeight = 0.0m;
			im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			im1MessageString = im1Builder.GetXMLMessage();
			expectedResult = string.Format(totalGrossMassMeasureTagText, "0");
			AssertContains("IM1 Message - gross weight element should be rounded", expectedResult, im1MessageString);
			JobDeclaration.JE_TotalWeight = 0.33m;
			im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			im1MessageString = im1Builder.GetXMLMessage();
			expectedResult = string.Format(totalGrossMassMeasureTagText, "1");
			AssertContains("IM1 Message - gross weight element should be rounded", expectedResult, im1MessageString);
			JobDeclaration.JE_TotalWeight = 2.45m;
			im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			im1MessageString = im1Builder.GetXMLMessage();
			expectedResult = string.Format(totalGrossMassMeasureTagText, "2");
			AssertContains("IM1 Message - gross weight element should be rounded", expectedResult, im1MessageString);
		}

		public void TestGrossWeightOnLineIsRounded()
		{
			CreateImportAirJob();
			var invoiceLine = JobDeclaration.InvoiceLines[0];
			invoiceLine.JI_Weight = 3m;
			invoiceLine.JI_WeightUQ = Enterprise.Core.Constants.Weight.Pounds;
			AssertEquals("precondition. Weight in Kilos has more than 3 decimal places", 1.360777m, invoiceLine.GrossWeightInKG);
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var importDeclaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(importDeclaration, TSWTransactionTypes.Original);
			AssertContains("<GrossMassMeasure unitCode=\"KGM\">1.361</GrossMassMeasure>", im1Builder.GetXMLMessage());
		}

		public void TestRoutingItineraryIsIncluded()
		{
			var expectedResult = @"<Itinerary>
          <SequenceNumeric>1</SequenceNumeric>
          <RoutingCountryCode>AU</RoutingCountryCode>
        </Itinerary>";
			CreateImportAirJob();
			JobDeclaration.JE_RL_NKPortOfLoading = "USLAX";
			var leg1 = JobDeclaration.Transports[0];
			leg1.JW_VoyageFlight = "QF2";
			leg1.JW_RL_NKLoadPort = "USLAX";
			leg1.JW_RL_NKDiscPort = "AUSYD";
			leg1.JW_ETD = new ZDateTime(2018, 01, 12);
			var leg2 = JobDeclaration.Transports.AddNew();
			leg2.JW_VoyageFlight = "QF117";
			leg2.JW_RL_NKLoadPort = "AUSYD";
			leg2.JW_RL_NKDiscPort = "NZAKL";
			leg2.JW_ETD = new ZDateTime(2018, 01, 14);
			AssertEquals("Pre-condition: Declaration has two transport legs", 2, JobDeclaration.Transports.Count);
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - routing itinerary has been included in TSW message", true, im1MessageString.Contains(expectedResult));
		}

		public void TestGoodsDescriptionMaxLength()
		{
			var expectedResult = @"<Description>2007 FORD FPV PURSUIT   VIN  6FPAAAJGCM7K62816  I, THE UNDERSIGNED, BEING THE IMPORTER OF THE VEHICLE DECLARED IN THIS IMPORT ENTRY, UNDERTAKE THAT SHOULD I SELL OR OTHERWISE DISPOSE OF THE VEHICLE WITHIN 2 YEARS FROM THE DATE OF IMPORTATION I WILL I</Description>";
			CreateImportAirJob();
			var invoiceLine = JobDeclaration.InvoiceLines[0];
			invoiceLine.JI_Description = "2007 FORD FPV PURSUIT   VIN  6FPAAAJGCM7K62816  I, THE UNDERSIGNED, BEING THE IMPORTER OF THE VEHICLE DECLARED IN THIS IMPORT ENTRY, UNDERTAKE THAT SHOULD I SELL OR OTHERWISE DISPOSE OF THE VEHICLE WITHIN 2 YEARS FROM THE DATE OF IMPORTATION I WILL IMMEDIATELY PAY NZ CUSTOMS THE SUM OF $1743.15, OR ANY LESSER AMOUNT THAT MAY BE REQUIRED.   .................................................... NAME AND SIGNATURE OF IMPORTER";
			AssertEquals("JI_Description is longer than 250 chars", true, invoiceLine.JI_Description.Length > 250);
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - goods description element should be truncated to the message specification maximum length (250)", true, im1MessageString.Contains(expectedResult));
		}

		public void TestIM1MessageType()
		{
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			AssertEquals("IM1 MessageType", "I10", im1Builder.MessageType);
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I51;
			declaration = new MockCusEntryHeaderWrapper(entryHeader);
			im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			AssertEquals("IM1 MessageType", "I51", im1Builder.MessageType);
		}

		public void TestIM1CancellationMessage()
		{
			var expectedResult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <ID>734929209</ID>
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BIS00002309</FunctionalReferenceID>
  <FunctionCode>1</FunctionCode>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalDocument>
    <ID>POD,123456</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>Test Certificate</ID>
    <TypeCode>CER</TypeCode>
  </AdditionalDocument>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>
</Declaration>
</DocumentMetadata>";
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "734929209";
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Cancel);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertMultilineASCIIEquals("Import/IM1 - Cancellation Message", expectedResult, im1MessageString);
		}

		public void TestIPIEntryDoesNotSendDeclarantElement()
		{
			var elementNotExpected = @"<Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>";
			CreateImportSeaJob(MessageTypeList.Codes.IPI);
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IPI Entry - Declarant element is NOT to be included", false, im1MessageString.Contains(elementNotExpected));
		}

		public void TestBillNumbersAreNotTruncated()
		{
			var expectedMBElement = "<TransportContractDocument>\r\n        <ID>OB293042-24902Y2992203928</ID>\r\n        <TypeCode>MB</TypeCode>";
			var expectedHBElement = "<TransportContractDocument>\r\n        <ID>123456789B123456789C123456789D12345</ID>\r\n        <TypeCode>BM</TypeCode>";
			CreateImportSeaJob();
			JobDeclaration.JE_MasterBill = "OB293042-24902Y2992203928";
			JobDeclaration.JE_HouseBill = "123456789B123456789C123456789D12345";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - MasterBill number element should not be truncated", true, im1MessageString.Contains(expectedMBElement));
			AssertEquals("IM1 Message - HouseBill number element should not be truncated", true, im1MessageString.Contains(expectedHBElement));
		}

		public void TestMasterBillNotRequired()
		{
			var noMBElement = "<TypeCode>MB</TypeCode>";
			var expectedHBElement = "<TransportContractDocument>\r\n        <ID>HB-ONLY</ID>\r\n        <TypeCode>BM</TypeCode>";
			CreateImportSeaJob();
			JobDeclaration.JE_MasterBill = "";
			JobDeclaration.JE_HouseBill = "HB-ONLY";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - MasterBill number element should not be generated", false, im1MessageString.Contains(noMBElement));
			AssertEquals("IM1 Message - HouseBill number element should be generated", true, im1MessageString.Contains(expectedHBElement));
		}

		public void TestTransportContractElementForMail()
		{
			var expectedMailTransportContractElement = @"<TransportContractDocument>
        <ID>P588299528</ID>
        <TypeCode>ABU</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";
			CreateImportAirJob();
			JobDeclaration.JE_MessageSubType = "NOR";
			JobDeclaration.JE_TransportMode = "PST";
			JobDeclaration.JE_MasterBill = "";
			JobDeclaration.JE_HouseBill = "P588299528";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Post declaration TransportContractDocument element generated", true, im1MessageString.Contains(expectedMailTransportContractElement));
		}

		public void TestIM1FromAirJob()
		{
			SetUpNZTaxOrFee(Factory);
			var expectedResult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BIS00002309</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">151</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20130719</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalDocument>
    <ID>POD,123456</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>Test Certificate</ID>
    <TypeCode>CER</TypeCode>
  </AdditionalDocument>
  <Agent>
    <ID>0092178J</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <ApprovedEstablishmentPlace>
    <ID>25001</ID>
  </ApprovedEstablishmentPlace>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS AIRFREIGHT</Name>
  </Carrier>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">2075.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>AU</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <GoodsLocation>
        <ID />
      </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>0810049584</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode />
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">0</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSP</StatementCode>
        <StatementDescription>Y</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSR</StatementCode>
        <StatementDescription>SUPPLIERGSTNO</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>7176F</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>PERFUMED BATH SALTS ETC</Description>
        <LotNumberID>LN000100999</LotNumberID>
        <ProductBestBeforeDateTime formatCode=""102"">20140630</ProductBestBeforeDateTime>
        <ValueAmount currencyID=""NZD"">0</ValueAmount>
        <IntendedUseCode>SP</IntendedUseCode>
        <Classification>
          <ID>3307300000E</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Manufacturer>
          <Name>PURESTEEL FABRICATORS</Name>
          <Address>
            <CityName>SYDNEY</CityName>
            <CountryCode />
            <CountrySubDivisionName>NSW</CountrySubDivisionName>
            <Line>UNIT 18, 100 MAIN ST. WOLLOOMOOLOO</Line>
            <PostcodeID>2009</PostcodeID>
          </Address>
        </Manufacturer>
        <ProductName>
          <Name>BONDS</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>
        <Source>
          <CountryCode>NZ</CountryCode>
        </Source>
      </Commodity>
      <ExaminationPlace>
        <Name>ABC Fumigators Pty Ltd.</Name>
        <ID />
      </ExaminationPlace>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">150.752</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""KGM"">1500</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>NZ</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <PackingMaterialDescription>STRAW</PackingMaterialDescription>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">200</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">55</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID />
      <ConditionCode />
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>PCS</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			CreateImportAirJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertMultilineASCIIEquals("Import Air - Original Message", expectedResult, im1MessageString);
		}

		public void TestCompletionJob()
		{
			SetUpNZTaxOrFee(Factory);
			var expectedResult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>COM</TypeCode>
  <FunctionalReferenceID>BIS00002309</FunctionalReferenceID>
  <FunctionCode>22</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">150</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20130719</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalDocument>
    <ID>POD,123456</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>Test Certificate</ID>
    <TypeCode>CER</TypeCode>
  </AdditionalDocument>
  <Agent>
    <ID>0092178J</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <ApprovedEstablishmentPlace>
    <ID>25001</ID>
  </ApprovedEstablishmentPlace>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS AIRFREIGHT</Name>
  </Carrier>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">2075.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>AU</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <GoodsLocation>
        <ID />
      </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>0810049584</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode />
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">0</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSP</StatementCode>
        <StatementDescription>Y</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSR</StatementCode>
        <StatementDescription>SUPPLIERGSTNO</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>7176F</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>PERFUMED BATH SALTS ETC</Description>
        <LotNumberID>LN000100999</LotNumberID>
        <ProductBestBeforeDateTime formatCode=""102"">20140630</ProductBestBeforeDateTime>
        <ValueAmount currencyID=""NZD"">0</ValueAmount>
        <IntendedUseCode>SP</IntendedUseCode>
        <Classification>
          <ID>3307300000E</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Manufacturer>
          <Name>PURESTEEL FABRICATORS</Name>
          <Address>
            <CityName>SYDNEY</CityName>
            <CountryCode />
            <CountrySubDivisionName>NSW</CountrySubDivisionName>
            <Line>UNIT 18, 100 MAIN ST. WOLLOOMOOLOO</Line>
            <PostcodeID>2009</PostcodeID>
          </Address>
        </Manufacturer>
        <ProductName>
          <Name>BONDS</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>
        <Source>
          <CountryCode>NZ</CountryCode>
        </Source>
      </Commodity>
      <ExaminationPlace>
        <Name>ABC Fumigators Pty Ltd.</Name>
        <ID />
      </ExaminationPlace>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">150</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""KGM"">1500</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>NZ</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <PackingMaterialDescription>STRAW</PackingMaterialDescription>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">200</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">55</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID />
      <ConditionCode />
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>PCS</TypeCode>
  </Packaging>
  <PreviousDocument>
    <ID>76423920</ID>
    <TypeCode>I51</TypeCode>
  </PreviousDocument>
</Declaration>
</DocumentMetadata>";
			//Type code (JE_MessageSubType) dummied here to 'COM' purely for testing purposes - in reality it would be the appropriate TSW code
			CreateCompletionJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "";
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Completion);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertMultilineASCIIEquals("Import Air - Completion Message", expectedResult, im1MessageString);
		}

		public void TestIM1ProhibitedCodes()
		{
			var expectedResult = @"<AdditionalInformation>
        <StatementCode>HWA</StatementCode>
        <StatementTypeCode>PRO</StatementTypeCode>
      </AdditionalInformation>";
			CreateImportAirJob();
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I11;
			var invoiceLine2 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0203.11.00.02C";
			invoiceLine2.JI_LinePrice = 3000m;
			var invoiceLine3 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "4201.00.00.01B";
			invoiceLine3.JI_LinePrice = 4500m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("Generated xml message string has line AdditionalInformation with Prohibited code", true, im1MessageString.Contains(expectedResult));
		}

		public void TestPostCodesAreTruncated()
		{
			var expectedImporterPostcode = @"<PostcodeID>H70009SW1</PostcodeID>";
			var expectedDeliverToPostcode = @"<PostcodeID>170945645</PostcodeID>";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "QANTAS Shipping";
			importer.MainAddress.Postcode = "H70009 SW1";
			var deliveryParty = Factory.NewWithValidTestData<OrgHeader>();
			deliveryParty.OH_FullName = "Google Aust.";
			deliveryParty.MainAddress.Postcode = "1709456456";
			CreateImportSeaJob();
			JobDeclaration.JE_OH_Importer = importer.PK;
			var requirement = new JobDocAddressRequirement(DocAddressType.ImporterDocumentaryAddress, ContactType.Consignee);
			var deliverTo = JobDeclaration.DocAddresses.FindOrCreateWithRequirement(requirement);
			deliverTo.E2_OA_Address = deliveryParty.MainAddress.PK;
			requirement = new JobDocAddressRequirement(DocAddressType.ImporterPickupDeliveryAddress, ContactType.Consignee);
			var deliverToAlt = JobDeclaration.DocAddresses.FindOrCreateWithRequirement(requirement);
			deliverToAlt.E2_OA_Address = deliveryParty.MainAddress.PK;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Importer postcode element should be truncated to 9 characters", true, im1MessageString.Contains(expectedImporterPostcode));
			AssertEquals("IM1 Message - Deliver To Party postcode element should be truncated to 9 characters", true, im1MessageString.Contains(expectedDeliverToPostcode));
		}

		public void TestVoyageIsTruncated()
		{
			var expectedResult = @"<JourneyID>PO227WES</JourneyID>";
			CreateImportSeaJob();
			JobDeclaration.JE_VoyageFlightNo = "PO227WEST";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - voyage number element should be truncated to 8 characters", true, im1MessageString.Contains(expectedResult));
		}

		public void TestFlightNoIsCapitalized()
		{
			var expectedResult = @"<BorderTransportMeans>
    <Name>QF128</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>";
			CreateImportAirJob();
			JobDeclaration.JE_VoyageFlightNo = "qf128";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Flight Number should be in Uppercase", true, im1MessageString.Contains(expectedResult));
		}

		public void TestCraftAndVoyageNumberIsCapitalized()
		{
			var expectedResult = @"<BorderTransportMeans>
    <Name>MURSK VIKING</Name>
    <ID>7582961</ID>
    <TypeCode>1</TypeCode>
    <JourneyID>157S</JourneyID>
  </BorderTransportMeans>";
			CreateImportSeaJob();
			JobDeclaration.JE_VoyageFlightNo = "157s";
			JobDeclaration.JE_VesselName = "Mursk Viking";
			JobDeclaration.JE_LloydsIMO = "7582961";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - voyage number element should be Uppercase", true, im1MessageString.Contains(expectedResult));
		}

		[TestDate(2013, 08, 02)]
		public void TestIM1WithMultipleValues()
		{
			SetUpNZTaxOrFee(Factory);
			var expectedResult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BIS2309MULTI</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">15000</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20130802</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalDocument>
    <ID>POD,123456</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>Test Certificate</ID>
    <TypeCode>CER</TypeCode>
  </AdditionalDocument>
  <Agent>
    <ID>0092178J</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>HYOGO MARU</Name>
    <ID />
    <TypeCode>1</TypeCode>
    <JourneyID>227W</JourneyID>
  </BorderTransportMeans>
  <Carrier>
    <Name>ANL SHIPPING</Name>
  </Carrier>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">535.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">1685.25</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">2220.25</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>SG</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <AdditionalInformation>
        <StatementDescription>YYNNNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <LoadingLocation>
        <ID>SGSIN</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>OB293042-24902</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>40</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>FMKU00329847</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>16B</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>40</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>NKFU00734293</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>16B</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode />
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">0</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSP</StatementCode>
        <StatementDescription>Y</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSR</StatementCode>
        <StatementDescription>SUPPLIERGSTNO</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>7176F</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>PERFUMED BATH SALTS ETC</Description>
        <LotNumberID>LN000100999</LotNumberID>
        <ProductBestBeforeDateTime formatCode=""102"">20140630</ProductBestBeforeDateTime>
        <ValueAmount currencyID=""NZD"">0</ValueAmount>
        <IntendedUseCode>SP</IntendedUseCode>
        <Classification>
          <ID>3307300000E</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">360.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">1134.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Manufacturer>
          <Name>PURESTEEL FABRICATORS</Name>
          <Address>
            <CityName>SYDNEY</CityName>
            <CountryCode />
            <CountrySubDivisionName>NSW</CountrySubDivisionName>
            <Line>UNIT 18, 100 MAIN ST. WOLLOOMOOLOO</Line>
            <PostcodeID>2009</PostcodeID>
          </Address>
        </Manufacturer>
        <ProductName>
          <Name>BONDS</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>
        <Source>
          <CountryCode>NZ</CountryCode>
        </Source>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>FMKU00329847</ID>
        </TransportEquipment>
      </Commodity>
      <ExaminationPlace>
        <Name>ABC Fumigators Pty Ltd.</Name>
        <ID />
      </ExaminationPlace>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""KGM"">1500</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>NZ</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <PackingMaterialDescription>STRAW</PackingMaterialDescription>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">200</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">55</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>2</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">0</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSP</StatementCode>
        <StatementDescription>Y</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSR</StatementCode>
        <StatementDescription>SUPPLIERGSTNO</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>7176F</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>SADDLERY AND HARNESS FOR ANY ANIMAL, ETC OF ANY MATERIAL, NEW</Description>
        <LotNumberID>LN000100999</LotNumberID>
        <ProductBestBeforeDateTime formatCode=""102"">20140630</ProductBestBeforeDateTime>
        <ValueAmount currencyID=""NZD"">0</ValueAmount>
        <IntendedUseCode>SP</IntendedUseCode>
        <Classification>
          <ID>4201000001B</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">175.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">551.25</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Manufacturer>
          <Name>PURESTEEL FABRICATORS</Name>
          <Address>
            <CityName>SYDNEY</CityName>
            <CountryCode />
            <CountrySubDivisionName>NSW</CountrySubDivisionName>
            <Line>UNIT 18, 100 MAIN ST. WOLLOOMOOLOO</Line>
            <PostcodeID>2009</PostcodeID>
          </Address>
        </Manufacturer>
        <ProductName>
          <Name>BONDS</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>
        <Source>
          <CountryCode>NZ</CountryCode>
        </Source>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>NKFU00734293</ID>
        </TransportEquipment>
      </Commodity>
      <ExaminationPlace>
        <Name>ABC Fumigators Pty Ltd.</Name>
        <ID />
      </ExaminationPlace>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""KGM"">1500</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>NZ</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <PackingMaterialDescription>STRAW</PackingMaterialDescription>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">200</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">55</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID />
      <ConditionCode>FOB</ConditionCode>
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
    <Invoice>
      <ID />
      <ConditionCode>FOB</ConditionCode>
      <SequenceNumeric>2</SequenceNumeric>
    </Invoice>
    <Seller>
      <Name>SMITH &amp; SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </Seller>
    <Seller>
      <Name>BOUNTY INDUSTRIES</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </Seller>
    <StuffingEstablishment>
      <Name />
      <Address>
        <CityName />
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName />
        <Line />
        <PostcodeID />
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name />
      <Address>
        <CityName />
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName />
        <Line />
        <PostcodeID />
      </Address>
    </StuffingEstablishment>
    <Supplier>
      <Name>SMITH &amp; SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </Supplier>
    <Supplier>
      <Name>BOUNTY INDUSTRIES</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </Supplier>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>20</QuantityQuantity>
    <TypeCode>CT</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>BX</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			CreateJobWithMultipleValues();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertMultilineASCIIEquals("Import Sea - Multiple Containers/Invoices/Suppliers/Lines Message", expectedResult, im1MessageString);
		}

		public void TestPointers()
		{
			SetUpNZTaxOrFee(Factory);
			var expectedResult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BIS00002309</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">151</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20130719</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalDocument>
    <ID>POD,123456</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>Test Certificate</ID>
    <TypeCode>CER</TypeCode>
  </AdditionalDocument>
  <Agent>
    <ID>0092178J</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <ApprovedEstablishmentPlace>
    <ID>25001</ID>
  </ApprovedEstablishmentPlace>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS AIRFREIGHT</Name>
  </Carrier>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">2075.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>AU</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <GoodsLocation>
        <ID />
      </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>0810049584</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode />
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">0</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSP</StatementCode>
        <StatementDescription>Y</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSR</StatementCode>
        <StatementDescription>SUPPLIERGSTNO</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>7176F</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>PERFUMED BATH SALTS ETC</Description>
        <LotNumberID>LN000100999</LotNumberID>
        <ProductBestBeforeDateTime formatCode=""102"">20140630</ProductBestBeforeDateTime>
        <ValueAmount currencyID=""NZD"">0</ValueAmount>
        <IntendedUseCode>SP</IntendedUseCode>
        <Classification>
          <ID>3307300000E</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Manufacturer>
          <Name>PURESTEEL FABRICATORS</Name>
          <Address>
            <CityName>SYDNEY</CityName>
            <CountryCode />
            <CountrySubDivisionName>NSW</CountrySubDivisionName>
            <Line>UNIT 18, 100 MAIN ST. WOLLOOMOOLOO</Line>
            <PostcodeID>2009</PostcodeID>
          </Address>
        </Manufacturer>
        <ProductName>
          <Name>BONDS</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>
        <Source>
          <CountryCode>NZ</CountryCode>
        </Source>
      </Commodity>
      <ExaminationPlace>
        <Name>ABC Fumigators Pty Ltd.</Name>
        <ID />
      </ExaminationPlace>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">150.752</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""KGM"">1500</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>NZ</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <PackingMaterialDescription>STRAW</PackingMaterialDescription>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">200</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">55</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID />
      <ConditionCode />
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>PCS</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			CreateImportAirJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertMultilineASCIIEquals("Import Air - Original Message", expectedResult, im1MessageString);
		}

		public void TestPointers2HBs1Container()
		{
			//<!--This example represents the association between 1 Masterbill, 2 Housebills, 1 Container and 2 Package Types-->
			var expectedBillDetailsSeq1 = "<TransportContractDocument>\r\n        <ID>H458239-1</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>B942042-2</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedBillDetailsSeq2 = "<TransportContractDocument>\r\n        <ID>H458239-1</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>B942042-2</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedContainerElementsSeq1 = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedContainerElementsSeq2 = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedPackagingElementsSeq1 = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>2</SequenceNumeric>\r\n    <QuantityQuantity>8</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>";
			var expectedPackagingElementsSeq2 = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>8</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>2</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>";
			Create2HB1ContainerJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("expectedBillDetails:" + "\r\n\r\n" + expectedBillDetailsSeq1 + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedBillDetailsSeq1) || im1MessageString.Contains(expectedBillDetailsSeq2));
			AssertEquals("expectedContainerElements:" + "\r\n\r\n" + expectedContainerElementsSeq1 + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedContainerElementsSeq1) || im1MessageString.Contains(expectedContainerElementsSeq2));
			AssertEquals("expectedPackagingElements:" + "\r\n\r\n" + expectedPackagingElementsSeq1 + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedPackagingElementsSeq1) || im1MessageString.Contains(expectedPackagingElementsSeq2));
		}

		public void TestBillPointersWhenNoMasterBill()
		{
			var expectedBillDetails = "<TransportContractDocument>\r\n        <ID>HB92027</ID>\r\n        <TypeCode>HWB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			CreateImportAirJob();
			JobDeclaration.JE_MasterBill = ZString.Empty;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("Expected bill pointer details when no MasterBill:" + "\r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedBillDetails));
		}

		public void TestIM1MessageWithContactDetails()
		{
			CreateImportSeaJob();
			var importer = JobDeclaration.Importer;
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "Bill Brown";
			importerContact.OC_Email = "bill.brown@importer.com.au";
			var allocatedContact = importerContact.Allocations.AddNew();
			allocatedContact.PC_Type = "NZC";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SMITH AND SONS";
			supplier.MainAddress.OA_Email = "info@smithandson.org.au";
			var supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_ContactName = "Wendy Smith";
			var supplierAllocatedContact = supplierContact.Allocations.AddNew();
			supplierAllocatedContact.PC_Type = "NZC";
			JobDeclaration.JE_OH_Supplier = supplier.PK;
			var invoice = JobDeclaration.Invoices[0];
			invoice.JZ_OH_Supplier = supplier.PK;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			var importerDetailsExpected = @"<Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
    <Contact>
      <Name>Bill Brown</Name>
      <Communication>
        <ID>bill.brown@importer.com.au</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </Contact>
  </Importer>";
			AssertEquals("Importer contact", true, im1MessageString.Contains(importerDetailsExpected));
			var supplierDetailsExpected = @"<Supplier>
      <Name>SMITH AND SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
      <Contact>
        <Name>Wendy Smith</Name>
        <Communication>
          <ID>info@smithandson.org.au</ID>
          <TypeID>EM</TypeID>
        </Communication>
      </Contact>
    </Supplier>";
			AssertEquals("Supplier contact", true, im1MessageString.Contains(supplierDetailsExpected));
		}

		public void TestIM1SupplierWithNoContactDetails()
		{
			CreateImportSeaJob();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SMITH AND SONS";
			supplier.MainAddress.OA_Email = "info@smithandson.org.au";
			JobDeclaration.JE_OH_Supplier = supplier.PK;
			var invoice = JobDeclaration.Invoices[0];
			invoice.JZ_OH_Supplier = supplier.PK;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			var supplierDetailsExpected = @"<Supplier>
      <Name>SMITH AND SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </Supplier>";
			AssertEquals("Supplier details when no Contact information is available", true, im1MessageString.Contains(supplierDetailsExpected));
		}

		public void TestContactDetailsIncludesFAXIfPresent()
		{
			CreateImportSeaJob();
			var importer = JobDeclaration.Importer;
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "Bill Brown";
			importerContact.OC_Email = "bill.brown@importer.com.au";
			importerContact.OC_Fax = "+61 2 7082 4901";
			var allocatedContact = importerContact.Allocations.AddNew();
			allocatedContact.PC_Type = "NZC";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SMITH AND SONS";
			supplier.MainAddress.OA_Email = "info@smithandson.org.au";
			supplier.MainAddress.OA_Fax = "+61 2 8760 9278";
			var supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_ContactName = "Wendy Smith";
			var supplierAllocatedContact = supplierContact.Allocations.AddNew();
			supplierAllocatedContact.PC_Type = "NZC";
			JobDeclaration.JE_OH_Supplier = supplier.PK;
			var invoice = JobDeclaration.Invoices[0];
			invoice.JZ_OH_Supplier = supplier.PK;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			var importerDetailsExpected = @"<Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
    <Contact>
      <Name>Bill Brown</Name>
      <Communication>
        <ID>bill.brown@importer.com.au</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>61270824901</ID>
        <TypeID>FX</TypeID>
      </Communication>
    </Contact>
  </Importer>";
			AssertEquals("Importer contact includes Fax number", true, im1MessageString.Contains(importerDetailsExpected));
			var supplierDetailsExpected = @"<Supplier>
      <Name>SMITH AND SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
      <Contact>
        <Name>Wendy Smith</Name>
        <Communication>
          <ID>info@smithandson.org.au</ID>
          <TypeID>EM</TypeID>
        </Communication>
        <Communication>
          <ID>61287609278</ID>
          <TypeID>FX</TypeID>
        </Communication>
      </Contact>
    </Supplier>";
			AssertEquals("Supplier contact includes FAX number", true, im1MessageString.Contains(supplierDetailsExpected));
		}

		public void TestGSTIsSentEvenWhenZero()
		{
			CreateImportSeaJobWithNoCharges();
			var importer = JobDeclaration.Importer;
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "Bill Brown";
			var allocatedContact = importerContact.Allocations.AddNew();
			allocatedContact.PC_Type = "NZC";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			var chargeDetailsExpected = @"<DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertEquals("Duty / GST / Total values should be present even if 0", true, im1MessageString.Contains(chargeDetailsExpected));
		}

		public void TestPointers_Example_a()
		{
			//<!--This example represents the association between 1 Masterbill, 1 Housebill, 1 Container and 1 Package Type-->
			var expectedBillDetails = "<TransportContractDocument>\r\n        <ID>123456</ID>\r\n        <TypeCode>MB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>COS12345678</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>";
			var expectedContainerElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>";
			CreateExampleAJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("expectedBillDetails:" + "\r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedBillDetails));
			AssertEquals("expectedContainerElements:" + "\r\n\r\n" + expectedContainerElements + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedContainerElements));
			AssertEquals("expectedPackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedPackagingElements));
		}

		public void TestPointers_Example_d()
		{
			//<!--This example represents the association between 1 Masterbill, 2 Housebills each with 2 Containers and 1 Package Type-->
			var expectedBillDetails = "<TransportContractDocument>\r\n        <ID>123456</ID>\r\n        <TypeCode>MB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>COS12345678</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>XYZ99887766</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>4</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedContainerElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>UUXU99203930</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>3</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>ZZUU9283929</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>4</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>ZZXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>4</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>2</SequenceNumeric>\r\n    <QuantityQuantity>8</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>3</SequenceNumeric>\r\n    <QuantityQuantity>8</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>4</SequenceNumeric>\r\n    <QuantityQuantity>5</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>";
			CreateExampleDJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("expectedBillDetails:" + "\r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedBillDetails));
			AssertEquals("expectedContainerElements:" + "\r\n\r\n" + expectedContainerElements + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedContainerElements));
			AssertEquals("expectedPackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + im1MessageString, true, im1MessageString.Contains(expectedPackagingElements));
		}

		public void TestIntendeUseIsConcatenated()
		{
			var expectedResult = @"<IntendedUse>MOVE LOGISTICS 30 HIGHBROOK DRIVE EAST TAMAKI AUCKLAND 2013  CONTACT NAME: KARAN SIAN CONTACT :  090265-4225</IntendedUse>";
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader, true);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - intended use description should be concatenated", true, im1MessageString.Contains(expectedResult));
		}

		public void TestPackageTypeIsDefaulted()
		{
			var expectedLinePackaging = @"<Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <PackingMaterialDescription>STRAW</PackingMaterialDescription>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>";
			var expectedDeclarationPackaging = @"<Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>PK</TypeCode>
  </Packaging>";
			CreateImportSeaJob();
			JobDeclaration.JE_TotalNoOfPacksPackType = "";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader, true);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - item package type defaults to 'PK' if empty", true, im1MessageString.Contains(expectedLinePackaging));
			AssertEquals("IM1 Message - dec package type defaults to 'PK' if empty", true, im1MessageString.Contains(expectedDeclarationPackaging));
		}

		public void TestBondedWarehouseCode()
		{
			var expectedResult = @"<Warehouse>
      <ID>7198J</ID>
    </Warehouse>";
			CreateBondedWarehouseJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Bonded Warehouse included in xml", true, im1MessageString.Contains(expectedResult));
		}

		public void TestNoBondedWarehouse()
		{
			var tagNotWanted = @"<Warehouse>";
			CreateImportAirJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Bonded Warehouse element SHOULD NOT BE included in this message", false, im1MessageString.Contains(tagNotWanted));
		}

		public void TestGoodsLocation()
		{
			var expectedElement = @"<GoodsLocation>
        <ID>NZWSZ</ID>
      </GoodsLocation>";
			CreateImportSeaJob();
			JobDeclaration.JE_RL_NKPortOfArrival = "NZWSZ";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("This IM1 Message should contain the Port of Arrival as the GoodsLocation", true, im1MessageString.Contains(expectedElement));
		}

		public void TestTariffQtySentWhenUQExists()
		{
			var expectedTariffElements = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">100</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">90</NetNetWeightMeasure>
        <TariffQuantity unitCode=""NMB"">0</TariffQuantity>
      </GoodsMeasure>";
			CreateImportAirJob();
			var invoiceLine = JobDeclaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = "9019.10.09.00B";
			invoiceLine.JI_LinePrice = 30000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_NetWeight = 90m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 0;
			invoiceLine.JI_CustomsUnitQty = "NMB";
			Factory.Save();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Tariff Quantity element should be present here - even if quantity is zero", true, im1MessageString.Contains(expectedTariffElements));
		}

		public void TestConvertCorrectEnumValues()
		{
			CreateImportSeaJob();
			var wrapper = new MockCusEntryHeaderWrapper(JobDeclaration.CustomsEntryHeaders[0], false);
			var builder = new IM1MessageBuilderForTest(wrapper, TSWTransactionTypes.None);
			CombineAssertions(() =>
			{
				AssertEquals("Valid Value", Iso3AlphaCurrencyCodeContentType.Fjd, builder.CurrencyIDExtend<Iso3AlphaCurrencyCodeContentType>("FJD"));
				AssertEquals("Invalid Value", Iso3AlphaCurrencyCodeContentType.Aud, builder.CurrencyIDExtend<Iso3AlphaCurrencyCodeContentType>("1234"));
				AssertEquals("Empty Value", Iso3AlphaCurrencyCodeContentType.Aud, builder.CurrencyIDExtend<Iso3AlphaCurrencyCodeContentType>(""));
				AssertEquals("Valid Value", MimeMediaTypeContentType.ImageJpeg, builder.PopulateMimeCodeExtend("testimage.jpeg", MimeMediaTypeContentType.ApplicationPdf));
				AssertEquals("Invalid Value", MimeMediaTypeContentType.ImageTiff, builder.PopulateMimeCodeExtend("1234", MimeMediaTypeContentType.ImageTiff));
				AssertEquals("Empty Value", MimeMediaTypeContentType.TextCsv, builder.PopulateMimeCodeExtend("", MimeMediaTypeContentType.TextCsv));
				AssertEquals("Valid Value", MeasurementUnitCommonCodeContentType.Ltr, builder.MeasurementTypeExtend<MeasurementUnitCommonCodeContentType>("LTR"));
				AssertEquals("Invalid Value", MeasurementUnitCommonCodeContentType.Bdu, builder.MeasurementTypeExtend<MeasurementUnitCommonCodeContentType>("1234"));
				AssertEquals("Empty Value", MeasurementUnitCommonCodeContentType.Bdu, builder.MeasurementTypeExtend<MeasurementUnitCommonCodeContentType>(""));
			});
		}

		class IM1MessageBuilderForTest : IM1MessageBuilder
		{
			public IM1MessageBuilderForTest(IImportDeclaration declarationHeader, TSWTransactionTypes transactionType) : base(declarationHeader, transactionType)
			{
			}

			public X CurrencyIDExtend<X>(ZString currencyCode)
				where X : struct, IConvertible
			{
				return base.CurrencyID<X>(currencyCode);
			}

			public X PopulateMimeCodeExtend<X>(ZString fileName, X defaultValue)
				where X : struct, IConvertible
			{
				return base.PopulateMimeCode<X>(fileName, defaultValue);
			}

			public X MeasurementTypeExtend<X>(ZString measurementCode)
				where X : struct, IConvertible
			{
				return base.MeasurementType<X>(measurementCode);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			var broker = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			broker.GS_FullName = "JOHNATHON TESTER";
			broker.GS_MobilePhone = "0419687522";
			broker.GS_EmailAddress = "johnathon.tester@testcompany.com.nz";
			var wrapper = broker.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			Factory.Save();
		}

		static void SetUpNZTaxOrFee(BusinessObjectFactory factory)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var assembly = Assembly.LoadFile(Path.Combine(binPath, "Enterprise.Customs.NZ.Business.Test.dll"));
			var taxOrFeeTestHelperType = assembly.GetType("Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing.TaxOrFeeTestHelper");
			var setUpMethod = taxOrFeeTestHelperType.GetMethod("SetUp", BindingFlags.Public | BindingFlags.Static);
			setUpMethod?.Invoke(null, new object[] { factory });
		}

		protected void Create2HB1ContainerJob()
		{
			JobDeclaration.JE_MessageType = "IMP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BA00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "OB93428378";
			JobDeclaration.JE_HouseBill = "H458239-1";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var houseBill2 = JobDeclaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "B942042-2";
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "YKKU9388747";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_PackQty = 8;
			packLine2.CW_PackType = "BX";
			packLine2.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateExampleAJob()
		{
			JobDeclaration.JE_MessageType = "IMP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BA00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "123456";
			JobDeclaration.JE_HouseBill = "COS12345678";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_VesselName = "Hyogo Maru";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CAXU2968920";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateBondedWarehouseJob()
		{
			var nz = Factory.Load<RefCountry>(Constants.CountryGuids.NewZealand);
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "QANTAS AIRFREIGHT";
			var controlledWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			controlledWarehouse.OH_FullName = "Auckland Bond";
			controlledWarehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "7198J", nz);
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_TotalWeight = 150.752m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "QF108";
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2016, 11, 9);
			JobDeclaration.JE_DeclarationReference = "BIS00002309";
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_GoodsDescription = "NEWS PRINT";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "0810049584";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = carrier.PK;
			JobDeclaration.WarehouseDocAddress.OrganisationPK = controlledWarehouse.PK;
			JobDeclaration.WarehouseDocAddress.E2_OA_Address = controlledWarehouse.MainAddress.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PCS";
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2016, 10, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		#endregion
	}
}
