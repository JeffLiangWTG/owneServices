using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.CertificateOfOrigin.Base;
using Enterprise.UniversalDataBuss.Integration;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.CertificateOfOrigin
{
	sealed class RCEPDataObjectWriterTest : CertificateOfOriginDataObjectWriterBaseTest<RCEP, RCEPLineItem, OriginCriterionListRCEP>
	{
		protected override string ShipmentDocumentName => ShipmentDocumentNames.RCEPCertificateOfOrigin;

		protected override Documents.DataTransfer.CertificateOfOrigin.Base.CertificateOfOriginDataObjectWriter<RCEP, RCEPLineItem> CreateWriter(IDataWritingManager writeManager, IDocument document) => new RCEPDataObjectWriter(writeManager, document);
		protected override RCEP CreateCertificate(ZString sourceType, ZString sourceId) => new RCEP(sourceType, sourceId);

		protected override RCEPLineItem CreateLineItem(object packlineId) => new (packlineId)
		{
			Origin = nameof(Constants.CountryCodes.Australia),
			OriginCode = Constants.CountryCodes.Australia,
			FOB = new Money(Context)
		};

		protected override IUnloco DefaultPortOfLoading => new DummyUnloco { Code = "AUSYD", Name = "Sydney", IATACode = "SYD" };
		protected override IUnloco DefaultPortOfDischarge => new DummyUnloco { Code = "CNCAN", Name = "Guangzhou", IATACode = "CAN" };
		protected override IUnloco DefaultPortOfOrigin => new DummyUnloco { Code = "AUSYD", Name = "Sydney", IATACode = "SYD" };
		protected override IUnloco DefaultPortOfDestination => new DummyUnloco { Code = "HKHKG", Name = "Hong Kong", IATACode = "HKG" };

		protected override string FirstLineItemOriginCode => Constants.CountryCodes.Australia;
		protected override string FirstLineItemOrigin => nameof(Constants.CountryCodes.Australia);
		protected override string SecondLineItemOriginCode => Constants.CountryCodes.China;
		protected override string SecondLineItemOrigin => nameof(Constants.CountryCodes.China);

		protected override void PopulateCertificate(RCEP certificate)
		{
			var lineItems = certificate.LineItems.ToArray();

			lineItems[0].OriginCriterion.Code = OriginCriterionListRCEP.Codes.WO;

			lineItems[1].OriginCriterion.Code = OriginCriterionListRCEP.Codes.RVC;
			lineItems[1].FOB.Currency.Code = Constants.CurrencyCodes.UnitedKingdom;
			lineItems[1].FOB.Amount = 42.42;
		}

		#region Expected Xml

		protected override string ExpectedCommercialInvoiceLineXml => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001000</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>
    </DataContext>

    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <InvoiceNumber>INV0001</InvoiceNumber>
          <InvoiceCurrency>AUD</InvoiceCurrency>
          <InvoiceDate>2023-07-11T00:00:00</InvoiceDate>
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>100</LineNo>
              <CountryOfOrigin Name=""{FirstLineItemOrigin}"">{FirstLineItemOriginCode}</CountryOfOrigin>
              <Description>goods description (1)</Description>
              <HarmonisedCode>1234</HarmonisedCode>
              <InvoiceQuantity>20</InvoiceQuantity>
              <LinePrice>10.00</LinePrice>
              <Weight>1</Weight>
              <WeightUnit>KG</WeightUnit>
              <AddInfoCollection>
                <AddInfo>
                  <Key>OriginCriterion</Key>
                  <Value>WO</Value>
                </AddInfo>
                <AddInfo>
                  <Key>MarksAndNumbers</Key>
                  <Value>marks &amp; nums (1)</Value>
                </AddInfo>
              </AddInfoCollection>
            </CommercialInvoiceLine>
            <CommercialInvoiceLine>
              <LineNo>200</LineNo>
              <CountryOfOrigin Name=""{SecondLineItemOrigin}"">{SecondLineItemOriginCode}</CountryOfOrigin>
              <Description>goods description (2)</Description>
              <HarmonisedCode>2345</HarmonisedCode>
              <InvoiceQuantity>10</InvoiceQuantity>
              <InvoiceQuantityUnit Description=""Package"">PKG</InvoiceQuantityUnit>
              <LinePrice>10.00</LinePrice>
              <Weight>2</Weight>
              <WeightUnit>KG</WeightUnit>
              <AddInfoCollection>
                <AddInfo>
                  <Key>OriginCriterion</Key>
                  <Value>RVC</Value>
                </AddInfo>
                <AddInfo>
                  <Key>MarksAndNumbers</Key>
                  <Value>marks &amp; nums (1)</Value>
                </AddInfo>
              </AddInfoCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
        <CommercialInvoice>
          <InvoiceNumber>INV0002</InvoiceNumber>
          <InvoiceCurrency>AUD</InvoiceCurrency>
          <InvoiceDate>2023-07-11T00:00:00</InvoiceDate>
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>100</LineNo>
              <CountryOfOrigin Name=""{FirstLineItemOrigin}"">{FirstLineItemOriginCode}</CountryOfOrigin>
              <Description>goods description (1)</Description>
              <HarmonisedCode>4567</HarmonisedCode>
              <InvoiceQuantity>20</InvoiceQuantity>
              <LinePrice>13.37</LinePrice>
              <Weight>1</Weight>
              <WeightUnit>KG</WeightUnit>
              <AddInfoCollection>
                <AddInfo>
                  <Key>OriginCriterion</Key>
                  <Value>DMI</Value>
                </AddInfo>
                <AddInfo>
                  <Key>MarksAndNumbers</Key>
                  <Value>marks &amp; nums (2)</Value>
                </AddInfo>
              </AddInfoCollection>
            </CommercialInvoiceLine>
            <CommercialInvoiceLine>
              <LineNo>200</LineNo>
              <CountryOfOrigin Name=""{SecondLineItemOrigin}"">{SecondLineItemOriginCode}</CountryOfOrigin>
              <Description>goods description (2)</Description>
              <HarmonisedCode>5678</HarmonisedCode>
              <InvoiceQuantity>10</InvoiceQuantity>
              <InvoiceQuantityUnit Description=""Package"">PKG</InvoiceQuantityUnit>
              <LinePrice>13.37</LinePrice>
              <Weight>2</Weight>
              <WeightUnit>KG</WeightUnit>
              <AddInfoCollection>
                <AddInfo>
                  <Key>OriginCriterion</Key>
                  <Value>DMI</Value>
                </AddInfo>
                <AddInfo>
                  <Key>MarksAndNumbers</Key>
                  <Value>marks &amp; nums (2)</Value>
                </AddInfo>
              </AddInfoCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <PortOfDestination Name=""Hong Kong"">HKHKG</PortOfDestination>
    <PortOfDischarge Name=""Guangzhou"">CNCAN</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <TransportMode>SEA</TransportMode>
    <VesselName>VesselName</VesselName>
    <VoyageFlightNo>TT9876</VoyageFlightNo>
    <AddInfoCollection>
      <AddInfo>
        <Key>CertificateOfOriginId</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>DateOfIssue</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuingBody</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>SignatureUsed</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>DateSigned</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>SelfDeclaration</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>BackToBackCertificateOfOrigin</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>SubjectToThirdPartyInvoice</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuedRetroactively</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>DeMinimis</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>Accumulation</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>VerboseLogging</Key>
        <Value>true</Value>
      </AddInfo>
      <AddInfo>
        <Key>SubmitToCustomsAuthority</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>Originals</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>Copies</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>Legalized</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>DocumentType</Key>
        <Value>RCEP</Value>
      </AddInfo>
      <AddInfo>
        <Key>IndemnityTermsAccepted</Key>
        <Value>true</Value>
      </AddInfo>
      <AddInfo>
        <Key>IndemnityTermsVersion</Key>
        <Value>2</Value>
      </AddInfo>
    </AddInfoCollection>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>INV0001.pdf</FileName>
        <ImageData>AQI=</ImageData>
        <Type Description=""Commercial Invoice"">INV</Type>
        <IsPublished>true</IsPublished>
        <ContextCollection>
          <Context>
            <Type>Certify</Type>
            <Value>true</Value>
          </Context>
          <Context>
            <Type>Originals</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>Copies</Type>
            <Value>0</Value>
          </Context>
          <Context>
            <Type>Legalized</Type>
            <Value>false</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>
      <AttachedDocument>
        <FileName>INV0003.pdf</FileName>
        <ImageData>AQI=</ImageData>
        <Type Description=""Commercial Invoice"">INV</Type>
        <IsPublished>true</IsPublished>
        <ContextCollection>
          <Context>
            <Type>Certify</Type>
            <Value>false</Value>
          </Context>
          <Context>
            <Type>Originals</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>Copies</Type>
            <Value>0</Value>
          </Context>
          <Context>
            <Type>Legalized</Type>
            <Value>false</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>
    </AttachedDocumentCollection>
    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-08-22T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-09-10T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection>
      <Note>
        <Description>Certificate of Origin Notes</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>remarks for the certification of origin</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ApplicantCompanyAddress</AddressType>
        <AdditionalAddressInformation>ApplicantCompanyAddress additional info</AdditionalAddressInformation>
        <Address1>ApplicantCompanyAddress address line 1</Address1>
        <Address2>ApplicantCompanyAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ApplicantCompanyAddress city</City>
        <CompanyName>ApplicantCompanyAddress</CompanyName>
        <Contact>ApplicantCompanyAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ApplicantCompanyAddress email</Email>
        <Fax>091593217</Fax>
        <GovRegNum>ApplicantCompanyAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>091593218</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>H91Y6CT</Postcode>
        <State>Auckland</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>ExporterAddress additional info</AdditionalAddressInformation>
        <Address1>ExporterAddress address line 1</Address1>
        <Address2>ExporterAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ExporterAddress city</City>
        <CompanyName>ExporterAddress</CompanyName>
        <Contact>ExporterAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ExporterAddress email</Email>
        <Fax>ExporterAddress fax</Fax>
        <GovRegNum>ExporterAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ExporterAddress phon</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ExporterAd</Postcode>
        <State>ExporterAddress state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>ImporterAddress additional info</AdditionalAddressInformation>
        <Address1>ImporterAddress address line 1</Address1>
        <Address2>ImporterAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ImporterAddress city</City>
        <CompanyName>ImporterAddress</CompanyName>
        <Contact>ImporterAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ImporterAddress email</Email>
        <Fax>ImporterAddress fax</Fax>
        <GovRegNum>ImporterAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ImporterAddress phon</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ImporterAd</Postcode>
        <State>ImporterAddress state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Manufacturer</AddressType>
        <AdditionalAddressInformation>ProducerAddress additional info</AdditionalAddressInformation>
        <Address1>ProducerAddress address line 1</Address1>
        <Address2>ProducerAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ProducerAddress city</City>
        <CompanyName>ProducerAddress</CompanyName>
        <Contact>ProducerAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ProducerAddress email</Email>
        <Fax>ProducerAddress fax</Fax>
        <GovRegNum>ProducerAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ProducerAddress phon</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ProducerAd</Postcode>
        <State>ProducerAddress state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CurrentUser address line 1</Address1>
        <Address2>CurrentUser address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CurrentUser city</City>
        <CompanyName>CurrentUser</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CurrentUse</Postcode>
        <State>CurrentUser state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";

		protected override string ExpectedPackingLineXml => @$"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001000</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>
    </DataContext>

    <PortOfDestination Name=""Hong Kong"">HKHKG</PortOfDestination>
    <PortOfDischarge Name=""Guangzhou"">CNCAN</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <TransportMode>SEA</TransportMode>
    <VesselName>VesselName</VesselName>
    <VoyageFlightNo>TT9876</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>CertificateOfOriginId</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>DateOfIssue</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuingBody</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>SignatureUsed</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>DateSigned</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>SelfDeclaration</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>BackToBackCertificateOfOrigin</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>SubjectToThirdPartyInvoice</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuedRetroactively</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>DeMinimis</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>Accumulation</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>VerboseLogging</Key>
        <Value>true</Value>
      </AddInfo>
      <AddInfo>
        <Key>SubmitToCustomsAuthority</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>Originals</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>Copies</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>Legalized</Key>
        <Value>false</Value>
      </AddInfo>
      <AddInfo>
        <Key>DocumentType</Key>
        <Value>RCEP</Value>
      </AddInfo>
      <AddInfo>
        <Key>IndemnityTermsAccepted</Key>
        <Value>true</Value>
      </AddInfo>
      <AddInfo>
        <Key>IndemnityTermsVersion</Key>
        <Value>2</Value>
      </AddInfo>
    </AddInfoCollection>

    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>INV0001.pdf</FileName>
        <ImageData>AQI=</ImageData>
        <Type Description=""Commercial Invoice"">INV</Type>
        <IsPublished>true</IsPublished>
        <ContextCollection>
          <Context>
            <Type>Certify</Type>
            <Value>true</Value>
          </Context>
          <Context>
            <Type>Originals</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>Copies</Type>
            <Value>0</Value>
          </Context>
          <Context>
            <Type>Legalized</Type>
            <Value>false</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>
      <AttachedDocument>
        <FileName>INV0003.pdf</FileName>
        <ImageData>AQI=</ImageData>
        <Type Description=""Commercial Invoice"">INV</Type>
        <IsPublished>true</IsPublished>
        <ContextCollection>
          <Context>
            <Type>Certify</Type>
            <Value>false</Value>
          </Context>
          <Context>
            <Type>Originals</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>Copies</Type>
            <Value>0</Value>
          </Context>
          <Context>
            <Type>Legalized</Type>
            <Value>false</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>
    </AttachedDocumentCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-08-22T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-09-10T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Certificate of Origin Notes</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>remarks for the certification of origin</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ApplicantCompanyAddress</AddressType>
        <AdditionalAddressInformation>ApplicantCompanyAddress additional info</AdditionalAddressInformation>
        <Address1>ApplicantCompanyAddress address line 1</Address1>
        <Address2>ApplicantCompanyAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ApplicantCompanyAddress city</City>
        <CompanyName>ApplicantCompanyAddress</CompanyName>
        <Contact>ApplicantCompanyAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ApplicantCompanyAddress email</Email>
        <Fax>091593217</Fax>
        <GovRegNum>ApplicantCompanyAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>091593218</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>H91Y6CT</Postcode>
        <State>Auckland</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>ExporterAddress additional info</AdditionalAddressInformation>
        <Address1>ExporterAddress address line 1</Address1>
        <Address2>ExporterAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ExporterAddress city</City>
        <CompanyName>ExporterAddress</CompanyName>
        <Contact>ExporterAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ExporterAddress email</Email>
        <Fax>ExporterAddress fax</Fax>
        <GovRegNum>ExporterAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ExporterAddress phon</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ExporterAd</Postcode>
        <State>ExporterAddress state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>ImporterAddress additional info</AdditionalAddressInformation>
        <Address1>ImporterAddress address line 1</Address1>
        <Address2>ImporterAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ImporterAddress city</City>
        <CompanyName>ImporterAddress</CompanyName>
        <Contact>ImporterAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ImporterAddress email</Email>
        <Fax>ImporterAddress fax</Fax>
        <GovRegNum>ImporterAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ImporterAddress phon</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ImporterAd</Postcode>
        <State>ImporterAddress state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Manufacturer</AddressType>
        <AdditionalAddressInformation>ProducerAddress additional info</AdditionalAddressInformation>
        <Address1>ProducerAddress address line 1</Address1>
        <Address2>ProducerAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ProducerAddress city</City>
        <CompanyName>ProducerAddress</CompanyName>
        <Contact>ProducerAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ProducerAddress email</Email>
        <Fax>ProducerAddress fax</Fax>
        <GovRegNum>ProducerAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ProducerAddress phon</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ProducerAd</Postcode>
        <State>ProducerAddress state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CurrentUser address line 1</Address1>
        <Address2>CurrentUser address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CurrentUser city</City>
        <CompanyName>CurrentUser</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CurrentUse</Postcode>
        <State>CurrentUser state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <CountryOfOrigin Name=""{FirstLineItemOrigin}"">{FirstLineItemOriginCode}</CountryOfOrigin>
        <DetailedDescription>goods description (1)</DetailedDescription>
        <HarmonisedCode>2345</HarmonisedCode>
        <ItemNo>100</ItemNo>
        <MarksAndNos>marks &amp; nums (1)</MarksAndNos>
        <PackQty>20</PackQty>
        <Weight>1</Weight>
        <WeightUnit>KG</WeightUnit>

        <AddInfoCollection>
          <AddInfo>
            <Key>OriginCriterion</Key>
            <Value>WO</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceNumber</Key>
            <Value>INV0001</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceDate</Key>
            <Value>2023-07-11T00:00:00</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceAmount</Key>
            <Value>10.00</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceCurrency</Key>
            <Value>AUD</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
      <PackingLine>
        <CountryOfOrigin Name=""{SecondLineItemOrigin}"">{SecondLineItemOriginCode}</CountryOfOrigin>
        <DetailedDescription>goods description (2)</DetailedDescription>
        <ItemNo>200</ItemNo>
        <MarksAndNos>marks &amp; nums (2)</MarksAndNos>
        <PackQty>10</PackQty>
        <PackType Description=""Package"">PKG</PackType>
        <Weight>2</Weight>
        <WeightUnit>KG</WeightUnit>

        <AddInfoCollection>
          <AddInfo>
            <Key>OriginCriterion</Key>
            <Value>RVC</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceNumber</Key>
            <Value>INV0002</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceDate</Key>
            <Value>2023-07-11T00:00:00</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceAmount</Key>
            <Value>13.37</Value>
          </AddInfo>
          <AddInfo>
            <Key>InvoiceCurrency</Key>
            <Value>AUD</Value>
          </AddInfo>
          <AddInfo>
            <Key>FOBCurrency</Key>
            <Value>GBP</Value>
          </AddInfo>
          <AddInfo>
            <Key>FOBAmount</Key>
            <Value>42.42</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";

		#endregion
	}
}
