using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750DataObjectMessageSenderTestHelper : WhsTransitTestHelper
	{
		public CIN750DataObjectMessageSenderTestHelper(BusinessObjectFactory factory) : base(factory) { }

		#region ExpectedMSNReferences

		public IEnumerable<ZString> InNotificationMSNReferences
		{
			get
			{
				yield return "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800";
			}
		}

		public IEnumerable<ZString> CorNotificationMSNReferences
		{
			get
			{
				yield return "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-5|PTP=REF|RFN=EDIDATRC0000001|WGT=-8.800";
			}
		}

		public IEnumerable<ZString> DeconsNotificationMSNReferences
		{
			get
			{
				yield return "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750DeconsNotification_From|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=13.000";
				yield return "|HBL=HSB1|JOB=RC0000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDC0000001|WGT=13.000";
			}
		}

		public IEnumerable<ZString> ConsNotificationMSNReferences
		{
			get
			{
				yield return "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=2|PTP=REF|RFN=EDIDATRC0000001|WGT=13.000";
				yield return "|HBL=HSB1|JOB=DC0000001|MBL=-|MST=CIN750ConsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDC0000001|WGT=13.000";
			}
		}

		public IEnumerable<ZString> OutNotificationMSNReferences
		{
			get
			{
				yield return "|HBL=HSB1|JOB=DC0000001|MBL=-|MST=CIN750OutNotification|OTY=2|PTP=HWB|RFN=EDIDATDC0000001|WGT=13";
			}
		}

		#endregion

		#region CreateOrUpdateBusinessObject

		public WhsItemReceiveConsignment CreateBusinessObjectForInNotification(bool createTSD = true, bool createMasterBill = false)
		{
			var warehouse = CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = CreateReceiveConsignment("RC0000001", warehouse.PK);
			if (createMasterBill)
			{
				CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			}
			var rtu = CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var ctoAddress = CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");

			var cen = CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");
			if (createTSD)
			{
				CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			}

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			return rcn;
		}

		public void UpdateBusinessObjectForInNotification(WhsItemReceiveConsignment rcn)
		{
			var rtu = rcn.PackageStates.FirstOrDefault()?.ReceiveTransportationUnit;

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P11", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P22", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P33", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P44", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P55", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";
		}

		public WhsItemReceiveConsignment CreateBusinessObjectForCorNotification(bool createTSD = true, bool createMasterBill = false)
		{
			var warehouse = CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rcnAWB = TransitDocDataConstants.DEFAULTREFERENCE;
			if (createMasterBill)
			{
				rcnAWB = "MAB-1";
				CreateAdditionalReference(rcn, rcnAWB, AdditionalReferenceTypes.Codes.MasterBill);
			}
			var rtu = CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var cen = CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			if (createTSD)
			{
				CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			}

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 800, weightUQ: "G", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;
			CreateStmALog(rcn, "MSN", $"|HBL=-|JOB=RC0000001|MBL={rcnAWB}|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8");
			return rcn;
		}

		public void UpdateBusinessObjectForCorNotification(WhsItemReceiveConsignment rcn)
		{
			var rtu = rcn.PackageStates.FirstOrDefault()?.ReceiveTransportationUnit;

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P11", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P22", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 800, weightUQ: "G", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P33", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P44", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P55", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			packageState1.Package.KP_GoodsDescription = "Test Description";

			CreateStmALog(rcn, "MSN", $"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8");
		}

		public WhsItemDispatchConsignment CreateBusinessObjectForDeconsNotification(string inMessageLog = "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=13")
		{
			var warehouse = CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var dcn = CreateDispatchConsignment("DC0000001", warehouse.PK);
			var location = CreateLocation(warehouse);
			var rtu = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = CreateReceiveConsignment("RC0000001", warehouse.PK);
			var dll = CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			dcn.WDC_HouseBillNumber = "HSB1";
			CreateStmALog(rcn, EventCodes.MessageSent, inMessageLog);

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll, weight: 10);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 3);

			return dcn;
		}

		public void UpdateBusinessObjectForDeconsNotification(WhsItemDispatchConsignment dcn)
		{
			var firstPackageStateInDCN = dcn.PackageStates.First();
			var rcn = firstPackageStateInDCN.ReceiveConsignment;
			var rtu = firstPackageStateInDCN.ReceiveTransportationUnit;
			var dtu = firstPackageStateInDCN.DispatchTransportationUnit;
			var dll = firstPackageStateInDCN.DispatchLoadList;

			CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=13");

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P11", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll, weight: 10);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P22", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 3);
		}

		public WhsItemDispatchConsignment CreateBusinessObjectForConsNotification()
		{
			var warehouse = CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var dcn = CreateDispatchConsignment("DC0000001", warehouse.PK);
			var location = CreateLocation(warehouse);
			var rtu = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = CreateReceiveConsignment("RC0000001", warehouse.PK);
			var dll = CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			dcn.WDC_HouseBillNumber = "HSB1";
			CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRC0000001|WGT=13");

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 10);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 3);

			return dcn;
		}

		public void UpdateBusinessObjectForConsNotification(WhsItemDispatchConsignment dcn)
		{
			var firstPackageStateInDCN = dcn.PackageStates.First();
			var rcn = firstPackageStateInDCN.ReceiveConsignment;
			var rtu = firstPackageStateInDCN.ReceiveTransportationUnit;
			var dtu = firstPackageStateInDCN.DispatchTransportationUnit;
			var dll = firstPackageStateInDCN.DispatchLoadList;

			CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRC0000001|WGT=13");

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P11", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 10);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P22", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 3);
		}

		public WhsItemDispatchConsignment CreateBusinessObjectForOutNotification()
		{
			var warehouse = CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var dcn = CreateDispatchConsignment("DC0000001", warehouse.PK);
			var location = CreateLocation(warehouse);
			var rtu = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = CreateReceiveConsignment("RC0000001", warehouse.PK);
			var dll = CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			var crn = CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			crn.PopulateAddOnValue("SourceType", "STR", "T1");
			dcn.WDC_HouseBillNumber = "HSB1";
			CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=13");

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 10);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 3);
			var packageId = dcn.PackageStates.FirstOrDefault().PK;

			var documentOverride =
@$"<Entity DataMajorVersion=""1"" DataMinorVersion=""0"">
  <Id>424042a5-654d-4b74-bd08-ab30ac9cf681</Id>
  <Property Name=""Goods"">
    <EntityCollection>
      <Items>
        <Entity>
          <Id>{packageId}</Id>
          <Property Name=""AmountWeight"">
            <Value>13</Value>
          </Property>
          <Property Name=""AmountQuantity"">
            <Value>2</Value>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>";
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = dcn.TablePrefix;
			documentData.JDD_Name = TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750OutFromDCN;
			documentData.JDD_OverriddenData = documentOverride;
			documentData.JDD_ParentID = dcn.PK;

			Factory.Save();

			return dcn;
		}

		public void UpdateBusinessObjectForOutNotification(WhsItemDispatchConsignment dcn)
		{
			var firstPackageStateInDCN = dcn.PackageStates.First();
			var rcn = firstPackageStateInDCN.ReceiveConsignment;
			var rtu = firstPackageStateInDCN.ReceiveTransportationUnit;
			var dtu = firstPackageStateInDCN.DispatchTransportationUnit;
			var dll = firstPackageStateInDCN.DispatchLoadList;

			CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=13");

			var packageState1 = CreatePackageState(rcn, 1, PackType.Freight.PKG, "P11", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 10);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			CreatePackageState(rcn, 1, PackType.Freight.PKG, "P22", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, weight: 3);
		}

		#endregion

		#region AssertMSNReferenceCore

		public void AssertMSNReferenceCore(IEnumerable<StmALog> logs, IEnumerable<ZString> expectedReferences, int count = 1)
		{
			var validLogs = logs.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode && !l.SL_IsCancelled);
			foreach (var msnReference in expectedReferences)
			{
				AssertEquals("MSN reference", count, validLogs.Count(log => log.SL_Reference.Contains(msnReference)));
			}
		}

		#endregion

		#region ExpectedMessage

		public ZString InNotificationExpectedMessage =>
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CIN750/In"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>RC0000001</Key>
        <Type>TransitReceive</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>CIN750InNotification</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
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
        <Value>{3}</Value>
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
        <IssueDate>2021-01-01T00:00:00</IssueDate>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
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

		public ZString CorNotificationExpectedMessage =>
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CIN750/Cor"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>RC0000001</Key>
        <Type>WhsItemReceiveConsignment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>CIN750CorNotification</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
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
        <Value>{3}</Value>
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
        <IssueDate>2021-01-01T00:00:00</IssueDate>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
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
        <GoodsDescription>Test Description</GoodsDescription>
        <PackQty>-5</PackQty>
        <Weight>-8.8</Weight>
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

		public ZString DeconsNotificationExpectedMessage =>
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CIN750/Decons"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>DC0000001</Key>
        <Type>TransitDispatch</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>CIN750DeconsNotification</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
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
        <IssueDate>2021-01-01T00:00:00</IssueDate>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
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
        <Weight>13</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value>TST1</Value>
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
            <Value>{3}</Value>
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
            <Weight>13</Weight>
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
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		public ZString ConsNotificationExpectedMessage =>
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CIN750/Cons"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>DC0000001</Key>
        <Type>WhsItemDispatchConsignment</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>CIN750ConsNotification</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
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
        <Value>{3}</Value>
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
        <IssueDate>2021-01-01T00:00:00</IssueDate>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
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
        <Weight>13</Weight>
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
            <ReferenceNumber>EDIDATRC0000001</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>DESC 1</GoodsDescription>
            <PackQty>2</PackQty>
            <Weight>13</Weight>
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

		public ZString OutNotificationExpectedMessage =>
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CIN750/Out"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>DC0000001</Key>
        <Type>WhsItemDispatchConsignment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>CIN750OutNotification</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
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
        <Value>{3}</Value>
      </AddInfo>
      <AddInfo>
        <Key>STP</Key>
        <Value>TST</Value>
      </AddInfo>
      <AddInfo>
        <Key>T1</Key>
        <Value>CRN1</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""House Air Waybill"">HWB</Type>
        <ReferenceNumber>HSB1</ReferenceNumber>
        <ContextInformation>EDIDAT</ContextInformation>
        <IssueDate>2021-01-01T00:00:00</IssueDate>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
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
        <Weight>13</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>TSD</Key>
            <Value>TST1</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";

		#endregion
	}
}
