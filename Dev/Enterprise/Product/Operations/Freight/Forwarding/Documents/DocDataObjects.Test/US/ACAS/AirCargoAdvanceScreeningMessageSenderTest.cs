using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.Testing
{
	sealed class AirCargoAdvanceScreeningMessageSenderTest : DocDataObjectMessageSenderTest
	{
		protected override IDocDataObjectMessageSender MessageSender => messageSender ?? (messageSender = new AirCargoAdvanceScreeningMessageSender());
		IDocDataObjectMessageSender messageSender;

		protected override BusinessObject SetBusinessObject()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "123", Core.Constants.CountryCodes.UnitedStates);

			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipmentWithConsol(shipment);
			return shipment;
		}

		protected override ZGuid MenuItemPK => ShipmentSystemFormMenuItems.DocumentMenuACASShipmentReport;

		protected override ZString DocumentName => ShipmentDocumentDataStoreNames.AdvancedCargoReportUS;

		protected override ZString MSNReference => "|DEP=Customs|LOC=US|MST=Advanced Cargo Report";

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/AirCargoAdvanceScreening/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key></Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Advanced Cargo Report</DocumentName>
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
    <GoodsDescription>Pizza Boxes</GoodsDescription>
    <OuterPacks>1</OuterPacks>
    <PortOfFirstArrival Name=""Los Angeles"">USLAX</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalWeight>123</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <VoyageFlightNo>AW1234</VoyageFlightNo>
    <WayBillNumber>HAWB</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>123-45678916</Value>
      </AddInfo>
      <AddInfo>
        <Key>NotifyPartyType_Code</Key>
        <Value>CTC</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortOfOriginIata</Key>
        <Value>SYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortOfFirstArrivalIata</Key>
        <Value>LAX</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsolType_Code</Key>
        <Value>AGT</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>USLAX</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
    </DateCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 159</Address1>
        <Address2>Dorcus alcides</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>Carrier</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2015</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit52</Address1>
        <Address2>Dorcus yamadai</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>Consignor</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2017</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>801</Address1>
        <Address2>Prismognathus delislei</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Somewhere</City>
        <CompanyName>Consignee</CompanyName>
        <Contact></Contact>
        <Country Name=""China"">CN</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Ningbo"">CNNBO</Port>
        <Postcode>10043</Postcode>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>115</Address1>
        <Address2>Coelosis sylvanus</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Auckland</City>
        <CompanyName>Notify Party</CompanyName>
        <Contact></Contact>
        <Country Name=""New Zealand"">NZ</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Auckland"">NZAKL</Port>
        <Postcode>1050</Postcode>
        <State>AUK</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>10 HUTCHESON STREET</Address1>
        <Address2>ALBION  QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode>4010</Postcode>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""ACAS (Air Cargo Advance Screening) "">ACA</Type>
            <CountryOfIssue Name=""United States"">US</CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>

      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		void PopulateShipmentWithConsol(ForwardingShipment shipment)
		{
			PopulateShipment(shipment);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "12345678916";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var transportLeg = consol.Transports.AddNew();
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg.JW_RL_NKLoadPort = "AUSYD";
			transportLeg.JW_RL_NKDiscPort = "USLAX";
			transportLeg.JW_VoyageFlight = "AW1234";
			transportLeg.JW_ETA = ZDateTime.Today;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.MainAddress.Address1 = "Unit 159";
			carrier.MainAddress.Address2 = "Dorcus alcides";
			carrier.MainAddress.City = "Sydney";
			carrier.MainAddress.Postcode = "2015";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			transportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;
		}

		void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HAWB";
			shipment.JS_ActualWeight = 123;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = "Pizza Boxes";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = 1;

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
			consignee.OH_RL_NKClosestPort = "CNNBO";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Party";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "115";
			notifyParty.MainAddress.Address2 = "Coelosis sylvanus";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "1050";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
		}
	}
}
