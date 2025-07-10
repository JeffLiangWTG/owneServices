using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.Testing
{
	sealed class ACASHouseChecklistMessageSenderTest : DocDataObjectMessageSenderTest
	{
		protected override IDocDataObjectMessageSender MessageSender => messageSender ?? (messageSender = new ACASHouseChecklistMessageSender());
		IDocDataObjectMessageSender messageSender;

		protected override BusinessObject SetBusinessObject()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "12345678916");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");
			AddLog(shipment, Events.InterchangeSent, ZDateTime.UtcToday.AddHours(-11), $"MST={DocumentNames.AdvancedCargoReport}|LOC=US");

			return consol;
		}

		protected override ZGuid MenuItemPK => ConsolSystemFormMenuItems.DocumentMenuACASHouseChecklistUSPK;

		protected override ZString DocumentName => ConsolDocumentDataStoreNames.AdvancedManifestUS;

		protected override ZString MSNReference => "|DEP=Customs|LOC=US|MST=Advanced Manifest";

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/HouseCheckList/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00000001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Advanced Manifest</DocumentName>
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
    <PortOfDestination Name=""Los Angeles"">USLAX</PortOfDestination>
    <PortOfFirstArrival Name=""John F. Kennedy Apt/New York"">USJFK</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <TotalNoOfPacks>25</TotalNoOfPacks>
    <TotalWeight>12</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <WayBillNumber>123-45678916</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>USJFK</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00000001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 000</Address1>
        <Address2>Hypocrea astronidii</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Mel</City>
        <CompanyName>Carrier</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Melbourne"">AUMEL</Port>
        <Postcode>2019</Postcode>
        <State>VIC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 200</Address1>
        <Address2>Xin</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Nanjing</City>
        <CompanyName>SendingForwarder</CompanyName>
        <Contact></Contact>
        <Country Name=""China"">CN</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Nanjing Pt"">CNNJG</Port>
        <Postcode>10000</Postcode>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000001</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>
        <GoodsDescription>goods1</GoodsDescription>
        <OuterPacks>25</OuterPacks>
        <PortOfDestination Name=""Los Angeles"">USLAX</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
        <TotalWeight>12</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>081001</WayBillNumber>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		void AddLog(IStmALogParent parent, Event eventType, ZDateTime eventDate, string parameters)
		{
			var paramList = new List<KeyValuePair<string, string>>();
			foreach (var paramPair in parameters.Split('|'))
			{
				var pair = paramPair.Split('=');
				paramList.Add(new KeyValuePair<string, string>(pair[0], pair[1]));
			}

			parent.Logs.AddNew(eventType, eventDate.ToOffset(), paramList.ToArray());

			Thread.Sleep(1);
			Factory.Save();
		}

		void PopulateConsol(ForwardingConsol consol, ZString mawb)
		{
			consol.JK_UniqueConsignRef = "C00000001";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = mawb;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SendingForwarder";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJG";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "Xin";
			sendingForwarder.MainAddress.City = "Nanjing";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "USJFK";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "USJFK";
			transportLeg2.JW_RL_NKDiscPort = "USLAX";
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_UniqueConsignRef = "S00000001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "USLAX";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "US";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}
	}
}
