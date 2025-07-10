using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class TSWMessageFormatterTest : TestCaseWithFactory
	{
		public void TestMessageInterpretationForTSW()
		{
			var messageText = ocrMessage;
			var expectedresult = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>CRE</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
    <TypeCode>OCR</TypeCode>
    <FunctionalReferenceID>C00001052</FunctionalReferenceID>
    <FunctionCode>9</FunctionCode>
    <Submitter>
      <ID>51352379D</ID>
    </Submitter>
    <AdditionalInformation>
      <Content>OCR from new Test Company</Content>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>CON</StatementTypeCode>
    </AdditionalInformation>
    <BorderTransportMeans>
      <Name>QF108</Name>
      <TypeCode>4</TypeCode>
      <DepartureDateTime formatCode=""102"">20130717</DepartureDateTime>
      <Itinerary>
        <SequenceNumeric>1</SequenceNumeric>
        <RoutingCountryCode>AU</RoutingCountryCode>
      </Itinerary>
    </BorderTransportMeans>
    <Carrier>
      <Name>QANTAS AIRWAYS LIMITED</Name>
    </Carrier>
    <Consignment>
      <SequenceNumeric>1</SequenceNumeric>
      <AdditionalDocument>
        <ID>4249295</ID>
        <TypeCode>EDO</TypeCode>
      </AdditionalDocument>
      <AssociatedTransportDocument>
        <ID>S00001078</ID>
        <TypeCode>HWB</TypeCode>
      </AssociatedTransportDocument>
      <TransportContractDocument>
        <ID>08152923824</ID>
        <TypeCode>MB</TypeCode>
        <Consolidator>
          <Name>NZ Demo Company 2</Name>
        </Consolidator>
      </TransportContractDocument>
    </Consignment>
    <ExitOffice>
      <ID>NZAKL</ID>
    </ExitOffice>
  </Declaration>
</DocumentMetadata>";
			AssertMultilineASCIIEquals("Text from TSW Messages should be formatted like an xml message", expectedresult.Trim(), TSWMessageFormatter.FormatWithXMLRepresentation(messageText));
		}

		public void TestMessageInterpretationIM1Message()
		{
			var messageText = im1Message;
			var expectedresult = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>IM</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
    <TypeCode>I10</TypeCode>
    <FunctionalReferenceID>B00001220</FunctionalReferenceID>
    <FunctionCode>9</FunctionCode>
    <TotalGrossMassMeasure unitCode=""KGM"">1500</TotalGrossMassMeasure>
    <JurisdictionDateTime formatCode=""102"">20130719</JurisdictionDateTime>
    <Submitter>
      <ID>00009908C</ID>
    </Submitter>
    <AdditionalInformation>
      <Content>IM1 Testing</Content>
    </AdditionalInformation>
    <Agent>
      <ID>00009908C</ID>
      <RoleCode>CB</RoleCode>
    </Agent>
    <BorderTransportMeans>
      <Name>ADMIRALENGRACHT</Name>
      <ID>8811924</ID>
      <TypeCode>1</TypeCode>
      <JourneyID>242E</JourneyID>
    </BorderTransportMeans>
    <Carrier>
      <Name>ANL LINE</Name>
    </Carrier>
    <CurrencyExchange>
      <RateNumeric>0.83</RateNumeric>
      <CurrencyTypeCode>AUD</CurrencyTypeCode>
    </CurrencyExchange>
    <Declarant>
      <ID>40006206E</ID>
      <Communication>
        <ID>gary.odea@cargowise.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>61280012200</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Declarant>
    <DutyTaxFee>
      <Payment>
        <MethodCode>B</MethodCode>
      </Payment>
    </DutyTaxFee>
    <DutyTaxFee>
      <TypeCode>GST</TypeCode>
      <Payment>
        <TaxAssessedAmount currencyID=""NZD"">1610.70</TaxAssessedAmount>
      </Payment>
    </DutyTaxFee>
    <DutyTaxFee>
      <TypeCode>CUD</TypeCode>
      <Payment>
        <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
      </Payment>
    </DutyTaxFee>
    <DutyTaxFee>
      <TypeCode>TOT</TypeCode>
      <Payment>
        <TaxAssessedAmount currencyID=""NZD"">1610.70</TaxAssessedAmount>
      </Payment>
    </DutyTaxFee>
    <GoodsShipment>
      <ExportationCountryCode>AU</ExportationCountryCode>
      <TransactionNatureCode>10</TransactionNatureCode>
      <Consignment>
        <LoadingLocation>
          <ID>AUSYD</ID>
        </LoadingLocation>
        <TransportContractDocument>
          <ID>OB84290</ID>
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
          <ID>H49024J</ID>
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
        </TransportContractDocument>
        <UnloadingLocation>
          <ID>NZCHC</ID>
        </UnloadingLocation>
      </Consignment>
      <CustomsValuation>
        <FreightChargeAmount currencyID=""NZD"">1000</FreightChargeAmount>
        <FreightChargeApportionmentCode>160</FreightChargeApportionmentCode>
      </CustomsValuation>
      <DeliveryDestination>
        <Name>TSW WELLINGTON IMPORTER</Name>
        <Address>
          <CityName>WELLINGTON</CityName>
          <CountryCode>NZ</CountryCode>
          <CountrySubDivisionName>WGN</CountrySubDivisionName>
          <Line>123 TEST STREET</Line>
          <PostcodeID>6102</PostcodeID>
        </Address>
      </DeliveryDestination>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <CustomsValueAmount currencyID=""NZD"">4819</CustomsValueAmount>
        <AdditionalInformation>
          <StatementCode>137</StatementCode>
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
        <ApprovedEstablishmentPlace>
          <ID>5115</ID>
        </ApprovedEstablishmentPlace>
        <Commodity>
          <Description>OTHER BOARDS PANELS ETC OVER 1000 VOLTS</Description>
          <ValueAmount currencyID=""AUD"">4000.00</ValueAmount>
          <Classification>
            <ID>8537200009G</ID>
            <IdentificationTypeCode>HS</IdentificationTypeCode>
          </Classification>
          <DutyTaxFee>
            <DutyRegimeCode>AU</DutyRegimeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>GST</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""NZD"">805.35</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <ProductName>
            <Name />
            <NameQualifierCode>223</NameQualifierCode>
          </ProductName>
          <ProductName>
            <Name />
            <NameQualifierCode>226</NameQualifierCode>
          </ProductName>
          <ProductName>
            <Name />
            <NameQualifierCode>55</NameQualifierCode>
          </ProductName>
          <ProductName>
            <Name />
            <NameQualifierCode>57</NameQualifierCode>
          </ProductName>
          <Source>
            <CountryCode>AU</CountryCode>
          </Source>
        </Commodity>
        <GoodsMeasure>
          <GrossMassMeasure unitCode=""KGM"">13895</GrossMassMeasure>
          <NetNetWeightMeasure unitCode=""KGM"">14000</NetNetWeightMeasure>
        </GoodsMeasure>
        <Origin>
          <CountryCode>AU</CountryCode>
        </Origin>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <MarksNumbersID>ADDRESSED</MarksNumbersID>
          <QuantityQuantity>5</QuantityQuantity>
          <TypeCode>PK</TypeCode>
          <VolumeMeasure unitCode=""MTQ"">1.8</VolumeMeasure>
        </Packaging>
        <ValuationAdjustment>
          <AdditionCode>151</AdditionCode>
          <AmountAmount currencyID=""NZD"">500</AmountAmount>
        </ValuationAdjustment>
        <ValuationAdjustment>
          <AdditionCode>150</AdditionCode>
          <AmountAmount currencyID=""NZD"">50</AmountAmount>
        </ValuationAdjustment>
      </GovernmentAgencyGoodsItem>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>2</SequenceNumeric>
        <CustomsValueAmount currencyID=""NZD"">4819</CustomsValueAmount>
        <AdditionalInformation>
          <StatementCode>137</StatementCode>
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
        <ApprovedEstablishmentPlace>
          <ID>5115</ID>
        </ApprovedEstablishmentPlace>
        <Commodity>
          <Description>OTHER BOARDS PANELS ETC OVER 1000 VOLTS</Description>
          <ValueAmount currencyID=""AUD"">4000.00</ValueAmount>
          <Classification>
            <ID>8537200009G</ID>
            <IdentificationTypeCode>HS</IdentificationTypeCode>
          </Classification>
          <DutyTaxFee>
            <DutyRegimeCode>AU</DutyRegimeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>GST</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""NZD"">805.35</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <ProductName>
            <Name />
            <NameQualifierCode>223</NameQualifierCode>
          </ProductName>
          <ProductName>
            <Name />
            <NameQualifierCode>226</NameQualifierCode>
          </ProductName>
          <ProductName>
            <Name />
            <NameQualifierCode>55</NameQualifierCode>
          </ProductName>
          <ProductName>
            <Name />
            <NameQualifierCode>57</NameQualifierCode>
          </ProductName>
          <Source>
            <CountryCode>AU</CountryCode>
          </Source>
        </Commodity>
        <GoodsMeasure>
          <GrossMassMeasure unitCode=""KGM"">13895</GrossMassMeasure>
          <NetNetWeightMeasure unitCode=""KGM"">14000</NetNetWeightMeasure>
        </GoodsMeasure>
        <Origin>
          <CountryCode>AU</CountryCode>
        </Origin>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <MarksNumbersID />
          <QuantityQuantity>5</QuantityQuantity>
          <TypeCode>PK</TypeCode>
          <VolumeMeasure unitCode=""MTQ"">1.3</VolumeMeasure>
        </Packaging>
        <ValuationAdjustment>
          <AdditionCode>151</AdditionCode>
          <AmountAmount currencyID=""NZD"">500</AmountAmount>
        </ValuationAdjustment>
        <ValuationAdjustment>
          <AdditionCode>150</AdditionCode>
          <AmountAmount currencyID=""NZD"">50</AmountAmount>
        </ValuationAdjustment>
      </GovernmentAgencyGoodsItem>
      <Invoice>
        <ID>INV9428</ID>
        <ConditionCode>CIF</ConditionCode>
        <SequenceNumeric>1</SequenceNumeric>
      </Invoice>
      <Seller>
        <Name>TSW TEST SUPPLIER</Name>
        <Address>
          <CityName>SYDNEY</CityName>
          <CountryCode>AU</CountryCode>
          <CountrySubDivisionName>NSW</CountrySubDivisionName>
          <Line>SYDNEY</Line>
          <PostcodeID>2000</PostcodeID>
        </Address>
      </Seller>
      <StuffingEstablishment>
        <Name />
        <Address>
          <CityName />
          <CountryCode />
          <CountrySubDivisionName />
          <Line />
          <PostcodeID />
        </Address>
      </StuffingEstablishment>
      <Supplier>
        <ID>00896286Q</ID>
      </Supplier>
    </GoodsShipment>
    <Importer>
      <ID>51352368J</ID>
      <Contact>
        <Name>Unknown</Name>
        <Communication>
          <ID>admin@wellingtonimporter.nz</ID>
          <TypeID>EM</TypeID>
        </Communication>
        <Communication>
          <ID>6445556501</ID>
          <TypeID>TE</TypeID>
        </Communication>
        <Communication>
          <ID>6445556543</ID>
          <TypeID>FX</TypeID>
        </Communication>
      </Contact>
    </Importer>
    <Packaging>
      <SequenceNumeric>1</SequenceNumeric>
      <QuantityQuantity>10</QuantityQuantity>
      <TypeCode>PK</TypeCode>
    </Packaging>
  </Declaration>
</DocumentMetadata>";
			AssertMultilineASCIIEquals("Message Text displayed for TSW Messages should be formatted like an xml message", expectedresult.Trim(), TSWMessageFormatter.FormatWithXMLRepresentation(messageText));
		}

		public void TestFormatFileName()
		{
			AssertEquals("FormatFileName replaces embedded periods - except for file extension", "sharp@company_co_nz_20160203_133507.pdf", TSWMessageFormatter.FormatAcceptableFileNameForNZC("sharp@company.co.nz_20160203_133507.pdf"));
			AssertEquals("FormatFileName replaces embedded periods - except for file extension", "abc_175320_testdoc_20160203.docx", TSWMessageFormatter.FormatAcceptableFileNameForNZC("abc.175320.testdoc.20160203.docx"));
			AssertEquals("FormatFileName replaces forbidden characters", "Y73/94.docx", TSWMessageFormatter.FormatAcceptableFileNameForNZC("Y73/#9~4£≠β≥.docx"));
			AssertEquals("The control character is removed", "KC1900417 Commercial Invoice.pdf", TSWMessageFormatter.FormatAcceptableFileNameForNZC("KC1900417 Commercial Invoice.pdf"));
			AssertEquals("The control character is removed", "KC1900417 Commercial Invoice.pdf", TSWMessageFormatter.FormatAcceptableFileNameForNZC("KC1900417 Commercial Invoice\u007f.pdf"));
		}

		public void TestNonASCIICharactersAreRemovedFromFileName()
		{
			AssertEquals("FormatFileName replaces non-ascii characters", "sharp@company_co_nz_20160203_133507.pdf", TSWMessageFormatter.FormatAcceptableFileNameForNZC("sharp@company.co.nz_20160203–133507.pdf"));
			AssertEquals("FormatFileName replaces non-ascii characters", "Non_ASCIIY73/94.docx", TSWMessageFormatter.FormatAcceptableFileNameForNZC("Non–ASCIIY73/#9~4£≠β≥.docx"));
		}

		public void TestFormatFileName_Colons()
		{
			AssertEquals("FormatFileName replaces colons", "sharp@company_co_nz_20160203__133507.pdf", TSWMessageFormatter.FormatAcceptableFileNameForNZC("sharp@company.co.nz_20160203::133507.pdf"));
			AssertEquals("FormatFileName replaces colons", "Non_ASCIIY73_94.docx", TSWMessageFormatter.FormatAcceptableFileNameForNZC("Non:ASCIIY73:#9~4£≠β≥.docx"));
			AssertEquals("FormatFileName replaces colons", "1_2_3_4_5_6_7_8_.docx", TSWMessageFormatter.FormatAcceptableFileNameForNZC("1[2]3{4}5(6)7:8?.docx"));
		}

		public void TestFormatFileName_QuestionMark()
		{
			AssertEquals("FormatFileName replaces question marks", "abc_175_testdoc_20200406.doc", TSWMessageFormatter.FormatAcceptableFileNameForNZC("abc_175_testdoc?20200406.doc"));
		}

		public void TestFormatFileName_Percentage()
		{
			AssertEquals("FormatFileName replaces percentage sign", "abc_175_testdoc_20200406.doc", TSWMessageFormatter.FormatAcceptableFileNameForNZC("abc_175_testdoc%20200406.doc"));
			AssertEquals("FormatFileName replaces percentage sign", "abc_123_.doc", TSWMessageFormatter.FormatAcceptableFileNameForNZC("abc?123?.doc"));
		}

		const string ocrMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\">\n<WCODataModelVersion>3.2</WCODataModelVersion>\n<WCODocumentName>CRE</WCODocumentName>\n<CountryCode>NZ</CountryCode>\n<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>\n<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>\n<Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\"><TypeCode>OCR</TypeCode><FunctionalReferenceID>C00001052</FunctionalReferenceID><FunctionCode>9</FunctionCode><Submitter><ID>51352379D</ID></Submitter><AdditionalInformation><Content>OCR from new Test Company</Content><StatementCode>Y</StatementCode><StatementTypeCode>CON</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=\"102\">20130717</DepartureDateTime><Itinerary><SequenceNumeric>1</SequenceNumeric><RoutingCountryCode>AU</RoutingCountryCode></Itinerary></BorderTransportMeans><Carrier><Name>QANTAS AIRWAYS LIMITED</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalDocument><ID>4249295</ID><TypeCode>EDO</TypeCode></AdditionalDocument><AssociatedTransportDocument><ID>S00001078</ID><TypeCode>HWB</TypeCode></AssociatedTransportDocument><TransportContractDocument><ID>08152923824</ID><TypeCode>MB</TypeCode><Consolidator><Name>NZ Demo Company 2</Name></Consolidator></TransportContractDocument></Consignment><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>\n</DocumentMetadata>";
		const string im1Message = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\"><WCODataModelVersion>3.2</WCODataModelVersion><WCODocumentName>IM</WCODocumentName><CountryCode>NZ</CountryCode><AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName><AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion><Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\"><TypeCode>I10</TypeCode><FunctionalReferenceID>B00001220</FunctionalReferenceID><FunctionCode>9</FunctionCode><TotalGrossMassMeasure unitCode=\"KGM\">1500</TotalGrossMassMeasure><JurisdictionDateTime formatCode=\"102\">20130719</JurisdictionDateTime><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>IM1 Testing</Content></AdditionalInformation><Agent><ID>00009908C</ID><RoleCode>CB</RoleCode></Agent><BorderTransportMeans><Name>ADMIRALENGRACHT</Name><ID>8811924</ID><TypeCode>1</TypeCode><JourneyID>242E</JourneyID></BorderTransportMeans><Carrier><Name>ANL LINE</Name></Carrier><CurrencyExchange><RateNumeric>0.83</RateNumeric><CurrencyTypeCode>AUD</CurrencyTypeCode></CurrencyExchange><Declarant><ID>40006206E</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>61280012200</ID><TypeID>TE</TypeID></Communication></Declarant><DutyTaxFee><Payment><MethodCode>B</MethodCode></Payment></DutyTaxFee><DutyTaxFee><TypeCode>GST</TypeCode><Payment><TaxAssessedAmount currencyID=\"NZD\">1610.70</TaxAssessedAmount></Payment></DutyTaxFee><DutyTaxFee><TypeCode>CUD</TypeCode><Payment><TaxAssessedAmount currencyID=\"NZD\">0</TaxAssessedAmount></Payment></DutyTaxFee><DutyTaxFee><TypeCode>TOT</TypeCode><Payment><TaxAssessedAmount currencyID=\"NZD\">1610.70</TaxAssessedAmount></Payment></DutyTaxFee><GoodsShipment><ExportationCountryCode>AU</ExportationCountryCode><TransactionNatureCode>10</TransactionNatureCode><Consignment><LoadingLocation><ID>AUSYD</ID></LoadingLocation><TransportContractDocument><ID>OB84290</ID><TypeCode>MB</TypeCode><Pointer><DocumentSectionCode>42A</DocumentSectionCode></Pointer><Pointer><DocumentSectionCode>67A</DocumentSectionCode></Pointer><Pointer><DocumentSectionCode>28A</DocumentSectionCode></Pointer><Pointer><SequenceNumeric>2</SequenceNumeric><DocumentSectionCode>30B</DocumentSectionCode></Pointer></TransportContractDocument><TransportContractDocument><ID>H49024J</ID><TypeCode>BM</TypeCode><Pointer><DocumentSectionCode>42A</DocumentSectionCode></Pointer><Pointer><DocumentSectionCode>67A</DocumentSectionCode></Pointer><Pointer><DocumentSectionCode>28A</DocumentSectionCode></Pointer></TransportContractDocument><UnloadingLocation><ID>NZCHC</ID></UnloadingLocation></Consignment><CustomsValuation><FreightChargeAmount currencyID=\"NZD\">1000</FreightChargeAmount><FreightChargeApportionmentCode>160</FreightChargeApportionmentCode></CustomsValuation><DeliveryDestination><Name>TSW WELLINGTON IMPORTER</Name><Address><CityName>WELLINGTON</CityName><CountryCode>NZ</CountryCode><CountrySubDivisionName>WGN</CountrySubDivisionName><Line>123 TEST STREET</Line><PostcodeID>6102</PostcodeID></Address></DeliveryDestination><GovernmentAgencyGoodsItem><SequenceNumeric>1</SequenceNumeric><CustomsValueAmount currencyID=\"NZD\">4819</CustomsValueAmount><AdditionalInformation><StatementCode>137</StatementCode><StatementTypeCode>REL</StatementTypeCode><Pointer><DocumentSectionCode>42A</DocumentSectionCode></Pointer><Pointer><DocumentSectionCode>67A</DocumentSectionCode></Pointer><Pointer><SequenceNumeric>1</SequenceNumeric><DocumentSectionCode>18B</DocumentSectionCode></Pointer></AdditionalInformation><ApprovedEstablishmentPlace><ID>5115</ID></ApprovedEstablishmentPlace><Commodity><Description>OTHER BOARDS PANELS ETC OVER 1000 VOLTS</Description><ValueAmount currencyID=\"AUD\">4000.00</ValueAmount><Classification><ID>8537200009G</ID><IdentificationTypeCode>HS</IdentificationTypeCode></Classification><DutyTaxFee><DutyRegimeCode>AU</DutyRegimeCode></DutyTaxFee><DutyTaxFee><TypeCode>GST</TypeCode><Payment><TaxAssessedAmount currencyID=\"NZD\">805.35</TaxAssessedAmount></Payment></DutyTaxFee><DutyTaxFee><TypeCode>CUD</TypeCode><Payment><TaxAssessedAmount currencyID=\"NZD\">0</TaxAssessedAmount></Payment></DutyTaxFee><ProductName><Name /><NameQualifierCode>223</NameQualifierCode></ProductName><ProductName><Name /><NameQualifierCode>226</NameQualifierCode></ProductName><ProductName><Name /><NameQualifierCode>55</NameQualifierCode></ProductName><ProductName><Name /><NameQualifierCode>57</NameQualifierCode></ProductName><Source><CountryCode>AU</CountryCode></Source></Commodity><GoodsMeasure><GrossMassMeasure unitCode=\"KGM\">13895</GrossMassMeasure><NetNetWeightMeasure unitCode=\"KGM\">14000</NetNetWeightMeasure></GoodsMeasure><Origin><CountryCode>AU</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><MarksNumbersID>ADDRESSED</MarksNumbersID><QuantityQuantity>5</QuantityQuantity><TypeCode>PK</TypeCode><VolumeMeasure unitCode=\"MTQ\">1.8</VolumeMeasure></Packaging><ValuationAdjustment><AdditionCode>151</AdditionCode><AmountAmount currencyID=\"NZD\">500</AmountAmount></ValuationAdjustment><ValuationAdjustment><AdditionCode>150</AdditionCode><AmountAmount currencyID=\"NZD\">50</AmountAmount></ValuationAdjustment></GovernmentAgencyGoodsItem><GovernmentAgencyGoodsItem><SequenceNumeric>2</SequenceNumeric><CustomsValueAmount currencyID=\"NZD\">4819</CustomsValueAmount><AdditionalInformation><StatementCode>137</StatementCode><StatementTypeCode>REL</StatementTypeCode><Pointer><DocumentSectionCode>42A</DocumentSectionCode></Pointer><Pointer><DocumentSectionCode>67A</DocumentSectionCode></Pointer><Pointer><SequenceNumeric>1</SequenceNumeric><DocumentSectionCode>18B</DocumentSectionCode></Pointer></AdditionalInformation><ApprovedEstablishmentPlace><ID>5115</ID></ApprovedEstablishmentPlace><Commodity><Description>OTHER BOARDS PANELS ETC OVER 1000 VOLTS</Description><ValueAmount currencyID=\"AUD\">4000.00</ValueAmount><Classification><ID>8537200009G</ID><IdentificationTypeCode>HS</IdentificationTypeCode></Classification><DutyTaxFee><DutyRegimeCode>AU</DutyRegimeCode></DutyTaxFee><DutyTaxFee><TypeCode>GST</TypeCode><Payment><TaxAssessedAmount currencyID=\"NZD\">805.35</TaxAssessedAmount></Payment></DutyTaxFee><DutyTaxFee><TypeCode>CUD</TypeCode><Payment><TaxAssessedAmount currencyID=\"NZD\">0</TaxAssessedAmount></Payment></DutyTaxFee><ProductName><Name /><NameQualifierCode>223</NameQualifierCode></ProductName><ProductName><Name /><NameQualifierCode>226</NameQualifierCode></ProductName><ProductName><Name /><NameQualifierCode>55</NameQualifierCode></ProductName><ProductName><Name /><NameQualifierCode>57</NameQualifierCode></ProductName><Source><CountryCode>AU</CountryCode></Source></Commodity><GoodsMeasure><GrossMassMeasure unitCode=\"KGM\">13895</GrossMassMeasure><NetNetWeightMeasure unitCode=\"KGM\">14000</NetNetWeightMeasure></GoodsMeasure><Origin><CountryCode>AU</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><MarksNumbersID /><QuantityQuantity>5</QuantityQuantity><TypeCode>PK</TypeCode><VolumeMeasure unitCode=\"MTQ\">1.3</VolumeMeasure></Packaging><ValuationAdjustment><AdditionCode>151</AdditionCode><AmountAmount currencyID=\"NZD\">500</AmountAmount></ValuationAdjustment><ValuationAdjustment><AdditionCode>150</AdditionCode><AmountAmount currencyID=\"NZD\">50</AmountAmount></ValuationAdjustment></GovernmentAgencyGoodsItem><Invoice><ID>INV9428</ID><ConditionCode>CIF</ConditionCode><SequenceNumeric>1</SequenceNumeric></Invoice><Seller><Name>TSW TEST SUPPLIER</Name><Address><CityName>SYDNEY</CityName><CountryCode>AU</CountryCode><CountrySubDivisionName>NSW</CountrySubDivisionName><Line>SYDNEY</Line><PostcodeID>2000</PostcodeID></Address></Seller><StuffingEstablishment><Name /><Address><CityName /><CountryCode /><CountrySubDivisionName /><Line /><PostcodeID /></Address></StuffingEstablishment><Supplier><ID>00896286Q</ID></Supplier></GoodsShipment><Importer><ID>51352368J</ID><Contact><Name>Unknown</Name><Communication><ID>admin@wellingtonimporter.nz</ID><TypeID>EM</TypeID></Communication><Communication><ID>6445556501</ID><TypeID>TE</TypeID></Communication><Communication><ID>6445556543</ID><TypeID>FX</TypeID></Communication></Contact></Importer><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>10</QuantityQuantity><TypeCode>PK</TypeCode></Packaging></Declaration></DocumentMetadata>";
	}
}
