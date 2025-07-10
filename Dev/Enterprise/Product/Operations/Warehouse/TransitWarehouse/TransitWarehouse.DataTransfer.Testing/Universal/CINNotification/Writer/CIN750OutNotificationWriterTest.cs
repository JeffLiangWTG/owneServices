using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document.Testing
{
	class CIN750OutNotificationWriterTest : CIN750NotificationWriterTest<CIN750OutNotification, CIN750OutNotificationWriter>
	{
		protected override CIN750OutNotificationWriter GetWriter()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			return new CIN750OutNotificationWriter(manager);
		}

		protected override CIN750OutNotification GetSource()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var inLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			warehouse.WW_DefaultInboundDockDoor = inLocation.PK;
			var outLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-2");
			warehouse.WW_DefaultOutboundDockDoor = outLocation.PK;

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			var crn1 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			crn1.PopulateAddOnValue("SourceType", "STR", "T1");
			var crn2 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN2");
			crn2.PopulateAddOnValue("SourceType", "STR", "T2");
			var crn3 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN3");

			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(dcn, DocAddressTypes.Codes.DepartureCTOAddress, ctoAddress);
			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");
			dcn.WDC_HouseBillNumber = "HB1";

			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Arrived, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll);
			packageState.Package.KP_GoodsDescription = "ppp";

			var additionalData = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = 1,
				PackageWeightReadyToOut = 1
			};
			return new CIN750OutNotificationBuilder(dcn, additionalData).Build();
		}

		protected override ZString ExpectedXML() =>
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>DC0000001</Key>
        <Type>WhsItemDispatchConsignment</Type>
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
    <MessageStatus></MessageStatus>
    <MessageType Description=""CIN 750 Out Notification"">OUT</MessageType>
    <AddInfoCollection>
      <AddInfo>
        <Key>JID</Key>
        <Value>DC0000001</Value>
      </AddInfo>
      <AddInfo>
        <Key>MID</Key>
        <Value>00000000-0000-0000-0000-000000000000</Value>
      </AddInfo>
      <AddInfo>
        <Key>STP</Key>
        <Value>TST</Value>
      </AddInfo>
      <AddInfo>
        <Key>T1, T2</Key>
        <Value>CRN1, CRN2</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""House Air Waybill"">HWB</Type>
        <ReferenceNumber>HB1</ReferenceNumber>
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
        <CompanyName>WH1111</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Tyabb"">AU2CO</Port>
        <Postcode></Postcode>
        <State>VIC</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C001</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>CTOAddress</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>WHTEST</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
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
        <GoodsDescription>ppp</GoodsDescription>
        <PackQty>1</PackQty>
        <Weight>1</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value>TSD1</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";
	}
}
