using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document.Testing
{
	class CIN750InNotificationWriterTest : CIN750NotificationWriterTest<CIN750InNotification, CIN750InNotificationWriter>
	{
		protected override CIN750InNotificationWriter GetWriter()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			return new CIN750InNotificationWriter(manager);
		}

		protected override CIN750InNotification GetSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			var ctoAddress = Factory.NewWithValidTestData<OrgAddress>();
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			Factory.Save();

			return new CIN750InNotificationBuilder(rcn).Build();
		}

		protected override ZString ExpectedXML() =>
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>RC0000001</Key>
        <Type>TransitReceive</Type>
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
    <MessageStatus Description=""Community"">C</MessageStatus>
    <MessageType Description=""CIN 750 In Notification"">IN</MessageType>
    <AddInfoCollection>
      <AddInfo>
        <Key>JID</Key>
        <Value>RC0000001</Value>
      </AddInfo>
      <AddInfo>
        <Key>MID</Key>
        <Value>00000000-0000-0000-0000-000000000000</Value>
      </AddInfo>
      <AddInfo>
        <Key>STP</Key>
        <Value>TST</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Reference Number"">REF</Type>
        <ReferenceNumber>EDIDATRC0000001</ReferenceNumber>
        <ContextInformation>EDIDAT</ContextInformation>
        <IssueDate>2023-12-31T00:00:00</IssueDate>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>LocalCartageCFS</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>WH1Address</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>Header</CompanyName>
        <Contact></Contact>
        <Country></Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port></Port>
        <Postcode></Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C001</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ArrivalCTOAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>CTOAddress</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>Header</CompanyName>
        <Contact></Contact>
        <Country></Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port></Port>
        <Postcode></Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C002</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <GoodsDescription>Test Description</GoodsDescription>
        <PackQty>5</PackQty>
        <Weight>8.8</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value>TST1</Value>
          </AddInfo>
          <AddInfo>
            <Key>T1</Key>
            <Value>CEN1</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";
	}
}
