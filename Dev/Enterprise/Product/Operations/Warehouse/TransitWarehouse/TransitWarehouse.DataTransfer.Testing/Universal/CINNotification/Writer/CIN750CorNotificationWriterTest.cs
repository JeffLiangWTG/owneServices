using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document.Testing
{
	class CIN750CorNotificationWriterTest : CIN750NotificationWriterTest<CIN750CorNotification, CIN750CorNotificationWriter>
	{
		protected override CIN750CorNotificationWriter GetWriter()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			return new CIN750CorNotificationWriter(manager);
		}

		protected override CIN750CorNotification GetSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P2", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 800, weightUQ: "G", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;
			Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8");
			Factory.Save();

			return new CIN750CorNotificationBuilder(rcn).Build();
		}

		protected override ZString ExpectedXML() =>
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>RC0000001</Key>
        <Type>WhsItemReceiveConsignment</Type>
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

    <MessageType Description=""CIN 750 Cor Notification"">COR</MessageType>
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
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <GoodsDescription></GoodsDescription>
        <PackQty>-2</PackQty>
        <Weight>-2.8</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
	}
}
