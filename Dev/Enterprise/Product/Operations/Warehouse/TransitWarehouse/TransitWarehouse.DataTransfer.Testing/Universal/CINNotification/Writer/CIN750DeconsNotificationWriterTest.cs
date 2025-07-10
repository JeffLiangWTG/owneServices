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
	class CIN750DeconsNotificationWriterTest : CIN750NotificationWriterTest<CIN750DeconsNotification, CIN750DeconsNotificationWriter>
	{
		protected override CIN750DeconsNotificationWriter GetWriter()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			return new CIN750DeconsNotificationWriter(manager);
		}

		protected override CIN750DeconsNotification GetSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HSB1";

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.DefaultLocation);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateAdditionalReference(rcn1, "MAB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateCustomsAdditionalReference(rcn1, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			Helper.CreateStmALog(rcn1, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=4.2");

			var rcn2 = Helper.CreateReceiveConsignment("RC0000002", warehouse.PK);
			Helper.CreateAdditionalReference(rcn2, "MAB2", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateCustomsAdditionalReference(rcn2, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST2");
			Helper.CreateStmALog(rcn2, EventCodes.MessageSent, "|HBL=-|JOB=RC0000002|MBL=MAB2|MST=CIN750InNotification|OTY=3|PTP=AWB|RFN=EDIDATRC0000002|WGT=7.3");

			var packageState1 = Helper.CreatePackageState(rcn1, 1, Constants.PkgUnit.Package, "P01", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 2200, weightUQ: "G");
			Helper.CreatePackageState(rcn1, 1, Constants.PkgUnit.Package, "P02", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 2, weightUQ: "KG");
			Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Package, "P03", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 3, weightUQ: "KG");
			Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Package, "P04", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 4, weightUQ: "KG");
			var packageState5 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Package, "P05", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 300, weightUQ: "G");

			packageState1.Package.KP_GoodsDescription = "DESC 1";
			packageState5.Package.KP_GoodsDescription = "DESC 2";

			Factory.Save();

			var result = new CIN750DeconsNotificationBuilder(dcn).Build();

			var n = 1;

			foreach (var goodsPair in result.GoodsPairs)
			{
				goodsPair.Item1.DeconsMessageID = $"00000000-0000-0000-0000-00000000000{n}";
				n++;
			}

			return result;
		}

		protected override ZString ExpectedXML() =>
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>DC0000001</Key>
        <Type>TransitDispatch</Type>
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
    <MessageType Description=""CIN 750 Deconsolidation Notification"">DEC</MessageType>
    <AddInfoCollection>
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
        <Link>1</Link>
        <PackQty>2</PackQty>
        <Weight>4.2</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value>TST1</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
      <PackingLine>
        <GoodsDescription>DESC 2</GoodsDescription>
        <Link>2</Link>
        <PackQty>3</PackQty>
        <Weight>7.3</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value>TST2</Value>
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
        <AddInfoCollection>
          <AddInfo>
            <Key>JID</Key>
            <Value>DC0000001</Value>
          </AddInfo>
          <AddInfo>
            <Key>MID</Key>
            <Value>00000000-0000-0000-0000-000000000001</Value>
          </AddInfo>
        </AddInfoCollection>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Master Air Waybill"">AWB</Type>
            <ReferenceNumber>MAB1</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>DESC 1</GoodsDescription>
            <Link>1</Link>
            <PackQty>2</PackQty>
            <Weight>4.2</Weight>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>TSD</Key>
                <Value>TST1</Value>
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
        <AddInfoCollection>
          <AddInfo>
            <Key>JID</Key>
            <Value>DC0000001</Value>
          </AddInfo>
          <AddInfo>
            <Key>MID</Key>
            <Value>00000000-0000-0000-0000-000000000002</Value>
          </AddInfo>
        </AddInfoCollection>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Master Air Waybill"">AWB</Type>
            <ReferenceNumber>MAB2</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>DESC 2</GoodsDescription>
            <Link>2</Link>
            <PackQty>3</PackQty>
            <Weight>7.3</Weight>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>TSD</Key>
                <Value>TST2</Value>
              </AddInfo>
            </AddInfoCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
	}
}
