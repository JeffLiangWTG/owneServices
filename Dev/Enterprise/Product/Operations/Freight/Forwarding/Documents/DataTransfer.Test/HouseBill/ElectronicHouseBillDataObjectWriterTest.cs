using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class ElectronicHouseBillDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);

			var houseBill = new HouseBill("ForwardingShipment", "S00001142");

			houseBill.ElectronicBillOfLadingShipper = CreateAddress("ConsignorDocumentaryAddress");
			houseBill.ElectronicBillOfLadingConsignee = CreateAddress("ConsigneeDocumentaryAddress");
			houseBill.CurrentUser = CreateAddress("CurrentUser");
			houseBill.Holder = CreateAddress("Holder");
			houseBill.SurrenderParty = CreateAddress("SurrenderParty");

			houseBill.ElectronicBillOfLadingVersion = (ZShort)2;
			houseBill.HouseBillNumber = "S00001142";
			houseBill.BillTerms = new DummyCodeDescription { Code = "NTR", Description = "Non-Transferable" };
			houseBill.BillType = new DummyCodeDescription { Code = "STR", Description = "Straight" };

			houseBill.AmendmentRequestID = "A00001";

			var writer = new ElectronicHouseBillDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(houseBill);

			AssertNotNull("DataObject has been produced", dataObject);
			AssertUXml(dataObject, @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>S00001142</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <BillTerms Description=""Non-Transferable"">NTR</BillTerms>
    <BillType Description=""Straight"">STR</BillType>
    <ElectronicBillOfLadingVersion>2</ElectronicBillOfLadingVersion>
    <WayBillNumber>S00001142</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>AmendmentRequestID</Key>
        <Value>A00001</Value>
      </AddInfo>
    </AddInfoCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>ConsignorDocumentaryAddress additional info</AdditionalAddressInformation>
        <Address1>ConsignorDocumentaryAddress address line 1</Address1>
        <Address2>ConsignorDocumentaryAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ConsignorDocumentaryAddress city</City>
        <CompanyName>ConsignorDocumentaryAddress</CompanyName>
        <Contact>ConsignorDocumentaryAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ConsignorDocumentaryAddress email</Email>
        <Fax>ConsignorDocumentary</Fax>
        <GovRegNum>ConsignorDocumentaryAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ConsignorDocumentary</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ConsignorD</Postcode>
        <State>ConsignorDocumentaryAddre</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OHP</Type>
            <Value>de645d17-5ea0-4ec2-8a53-0cf241f2ab27</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OAP</Type>
            <Value>939ac365-5fbb-41e0-8d89-3cbca40264a0</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>ConsigneeDocumentaryAddress additional info</AdditionalAddressInformation>
        <Address1>ConsigneeDocumentaryAddress address line 1</Address1>
        <Address2>ConsigneeDocumentaryAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ConsigneeDocumentaryAddress city</City>
        <CompanyName>ConsigneeDocumentaryAddress</CompanyName>
        <Contact>ConsigneeDocumentaryAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ConsigneeDocumentaryAddress email</Email>
        <Fax>ConsigneeDocumentary</Fax>
        <GovRegNum>ConsigneeDocumentaryAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ConsigneeDocumentary</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ConsigneeD</Postcode>
        <State>ConsigneeDocumentaryAddre</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OHP</Type>
            <Value>de645d17-5ea0-4ec2-8a53-0cf241f2ab27</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OAP</Type>
            <Value>939ac365-5fbb-41e0-8d89-3cbca40264a0</Value>
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
          <RegistrationNumber>
            <Type>OHP</Type>
            <Value>de645d17-5ea0-4ec2-8a53-0cf241f2ab27</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OAP</Type>
            <Value>939ac365-5fbb-41e0-8d89-3cbca40264a0</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Holder</AddressType>
        <AdditionalAddressInformation>Holder additional info</AdditionalAddressInformation>
        <Address1>Holder address line 1</Address1>
        <Address2>Holder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Holder city</City>
        <CompanyName>Holder</CompanyName>
        <Contact>Holder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Holder email</Email>
        <Fax>Holder fax</Fax>
        <GovRegNum>Holder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Holder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Holder pos</Postcode>
        <State>Holder state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OHP</Type>
            <Value>de645d17-5ea0-4ec2-8a53-0cf241f2ab27</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OAP</Type>
            <Value>939ac365-5fbb-41e0-8d89-3cbca40264a0</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SurrenderParty</AddressType>
        <AdditionalAddressInformation>SurrenderParty additional info</AdditionalAddressInformation>
        <Address1>SurrenderParty address line 1</Address1>
        <Address2>SurrenderParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SurrenderParty city</City>
        <CompanyName>SurrenderParty</CompanyName>
        <Contact>SurrenderParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SurrenderParty email</Email>
        <Fax>SurrenderParty fax</Fax>
        <GovRegNum>SurrenderParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SurrenderParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SurrenderP</Postcode>
        <State>SurrenderParty state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OHP</Type>
            <Value>de645d17-5ea0-4ec2-8a53-0cf241f2ab27</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>OAP</Type>
            <Value>939ac365-5fbb-41e0-8d89-3cbca40264a0</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
");
		}

		public void TestPopulateDataObject_BillTypeIsToOrder()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);

			var houseBill = new HouseBill("ForwardingShipment", "S00001142");

			houseBill.ElectronicBillOfLadingShipper = CreateAddress("ConsignorDocumentaryAddress");
			houseBill.ElectronicBillOfLadingConsignee = CreateAddress("ConsigneeDocumentaryAddress");
			houseBill.CurrentUser = CreateAddress("CurrentUser");
			houseBill.Holder = CreateAddress("Holder");
			houseBill.SurrenderParty = CreateAddress("SurrenderParty");
			houseBill.ElectronicBillOfLadingToOrder = CreateAddress("ToOrder");

			houseBill.ElectronicBillOfLadingVersion = (ZShort)2;
			houseBill.HouseBillNumber = "S00001142";
			houseBill.BillTerms = new DummyCodeDescription { Code = "NTR", Description = "Non-Transferable" };
			houseBill.BillType = new DummyCodeDescription { Code = "TOR", Description = "To Order" };

			houseBill.AmendmentRequestID = "A00001";

			var writer = new ElectronicHouseBillDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(houseBill);

			Assert(!dataObject.OrganizationAddressCollection.Any(o => o.CompanyName.HasValue && o.CompanyName.Value == "ConsigneeDocumentaryAddress"));
			Assert(dataObject.OrganizationAddressCollection.Any(o => o.CompanyName.HasValue && o.CompanyName.Value == "ToOrder"));
		}
	}
}
