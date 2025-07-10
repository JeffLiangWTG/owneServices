using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.CertificateOfOrigin.Base;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.CertificateOfOrigin
{
	sealed class COONZDataObjectWriterTests : CertificateOfOriginDataObjectWriterBaseTest<COONZ, COONZLineItem, OriginCriterionListCOONZ>
	{
		protected override string ShipmentDocumentName => ShipmentDocumentNames.NZCertificateOfOrigin;

		protected override COONZ CreateCertificate(ZString sourceType, ZString sourceId)
			=> new COONZ(sourceType, sourceId);

		protected override COONZLineItem CreateLineItem(object lineItemId) => new COONZLineItem(lineItemId);

		protected override Documents.DataTransfer.CertificateOfOrigin.Base.CertificateOfOriginDataObjectWriter<COONZ, COONZLineItem> CreateWriter(IDataWritingManager writeManager, IDocument document)
			=> new COONZDataObjectWriter(writeManager, document);

		protected override void PopulateCertificate(COONZ certificate)
		{
			certificate.LineItems.ForEach(i => i.Invoice = null);
		}

		protected override string FirstLineItemOriginCode => Core.Constants.CountryCodes.NewZealand;
		protected override string FirstLineItemOrigin => nameof(Core.Constants.CountryCodes.NewZealand);
		protected override string SecondLineItemOriginCode => Core.Constants.CountryCodes.NewZealand;
		protected override string SecondLineItemOrigin => nameof(Core.Constants.CountryCodes.NewZealand);

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
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>100</LineNo>
              <CountryOfOrigin Name=""{FirstLineItemOrigin}"">{FirstLineItemOriginCode}</CountryOfOrigin>
              <Description>goods description (1)</Description>
              <HarmonisedCode>1234</HarmonisedCode>
              <InvoiceQuantity>20</InvoiceQuantity>
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
              <Weight>2</Weight>
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
              <LineNo>100</LineNo>
              <CountryOfOrigin Name=""{FirstLineItemOrigin}"">{FirstLineItemOriginCode}</CountryOfOrigin>
              <Description>goods description (1)</Description>
              <HarmonisedCode>4567</HarmonisedCode>
              <InvoiceQuantity>20</InvoiceQuantity>
              <Weight>1</Weight>
              <WeightUnit>KG</WeightUnit>
              <AddInfoCollection>
                <AddInfo>
                  <Key>OriginCriterion</Key>
                  <Value>PSR</Value>
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
              <Weight>2</Weight>
              <WeightUnit>KG</WeightUnit>
              <AddInfoCollection>
                <AddInfo>
                  <Key>OriginCriterion</Key>
                  <Value>PSR</Value>
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
    <PortOfLoading Name=""Auckland"">NZAKL</PortOfLoading>
    <PortOfOrigin Name=""Auckland"">NZAKL</PortOfOrigin>
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
        <Value>COONZ</Value>
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
    <PortOfLoading Name=""Auckland"">NZAKL</PortOfLoading>
    <PortOfOrigin Name=""Auckland"">NZAKL</PortOfOrigin>
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
        <Value>COONZ</Value>
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
            <Value>PSR</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion
	}
}
