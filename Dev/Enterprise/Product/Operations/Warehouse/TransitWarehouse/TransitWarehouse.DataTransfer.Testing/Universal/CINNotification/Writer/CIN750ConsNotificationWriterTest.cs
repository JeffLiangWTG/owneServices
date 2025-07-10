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
	class CIN750ConsNotificationWriterTest : CIN750NotificationWriterTest<CIN750ConsNotification, CIN750ConsNotificationWriter>
	{
		protected override CIN750ConsNotificationWriter GetWriter()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			return new CIN750ConsNotificationWriter(manager);
		}

		protected override CIN750ConsNotification GetSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_ConsignmentID = "DCN1";
			dcn.WDC_HouseBillNumber = "HSB1";

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.DefaultLocation);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn1.WRC_ConsignmentID = "RCN1";
			Helper.CreateCustomsAdditionalReference(rcn1, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var rcn2 = Helper.CreateReceiveConsignment("RC0000002", warehouse.PK);
			rcn2.WRC_ConsignmentID = "RCN2";
			Helper.CreateCustomsAdditionalReference(rcn2, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST2");

			var packageState1 = Helper.CreatePackageState(rcn1, 1, Constants.PkgUnit.Package, "P01", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: (decimal)2.2, weightUQ: "KG");
			packageState1.WPS_UnloadedNotYetProcessedTime = new ZDateTimeOffset(2023, 1, 17, 7, 0, 0);
			packageState1.WPS_UnloadedTime = new ZDateTimeOffset(2023, 1, 17, 8, 0, 0);
			var packageState2 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Package, "P02", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: (decimal)0.3, weightUQ: "KG");
			packageState2.WPS_UnloadedNotYetProcessedTime = new ZDateTimeOffset(2023, 1, 17, 8, 0, 0);
			packageState2.WPS_UnloadedTime = new ZDateTimeOffset(2023, 1, 17, 9, 0, 0);

			packageState1.Package.KP_GoodsDescription = "DESC 1";
			packageState2.Package.KP_GoodsDescription = "DESC 2";

			Helper.CreateStmALog(rcn1, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRCN1|WGT=2.2");
			Helper.CreateStmALog(rcn2, EventCodes.MessageSent, "|HBL=-|JOB=RC0000002|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRCN2|WGT=0.3");

			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			return new CIN750ConsNotificationBuilder(dcn, historyManager.AdditionalDataForCons).Build();
		}

		protected override ZString ExpectedXML() =>
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>DCN1</Key>
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
    <MessageType Description=""CIN 750 Consolidation Notification"">CON</MessageType>
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
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""House Air Waybill"">HWB</Type>
        <ReferenceNumber>HSB1</ReferenceNumber>
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
        <GoodsDescription>DESC 1</GoodsDescription>
        <PackQty>2</PackQty>
        <Weight>2.5</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>RC0000001</Key>
            <Type>TransitReceive</Type>
          </DataSource>
        </DataContext>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Reference Number"">REF</Type>
            <ReferenceNumber>EDIDATRCN1</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>DESC 1</GoodsDescription>
            <PackQty>1</PackQty>
            <Weight>2.2</Weight>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>TSD</Key>
                <Value></Value>
              </AddInfo>
            </AddInfoCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>RC0000002</Key>
            <Type>TransitReceive</Type>
          </DataSource>
        </DataContext>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Reference Number"">REF</Type>
            <ReferenceNumber>EDIDATRCN2</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>DESC 2</GoodsDescription>
            <PackQty>1</PackQty>
            <Weight>0.3</Weight>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>TSD</Key>
                <Value></Value>
              </AddInfo>
            </AddInfoCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>

";
	}
}
