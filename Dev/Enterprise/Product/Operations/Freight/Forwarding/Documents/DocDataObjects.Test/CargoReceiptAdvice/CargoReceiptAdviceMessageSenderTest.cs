using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.Testing
{
	sealed class CargoReceiptAdviceWithoutUIMessageSenderTest : DocDataObjectWithoutUIMessageSenderTest
	{
		protected override IDocDataObjectWithoutUIMessageSender MessageSender => messageSender ?? (messageSender = new CargoReceiptAdviceWithoutUIMessageSender());
		IDocDataObjectWithoutUIMessageSender messageSender;

		protected override BusinessObject CreateBusinessObject() => CreateShipment();

		protected override ZString DataStoreName => ShipmentDocumentDataStoreNames.CargoReceiptAdvice;

		public override bool DocumentHasMenuItem => false;

		protected override ZString MSNReference => "|DEP=Booking Party|MST=Cargo Receipt Advice";

		protected override string SenderObjectFactoryRegistrationKey => ShipmentDocumentDataStoreNames.CargoReceiptAdvice;

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CargoReceiptAdvice/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>SH0001000</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Cargo Receipt Advice</DocumentName>
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

    <InterimReceiptNumber>interimReceipt123</InterimReceiptNumber>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>SHP001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2022-03-08T10:58:51</Value>
      </Date>
    </DateCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>Marks &amp; Numbers</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>marks &amp; numbers</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 200</Address1>
        <Address2>55 Why Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Paris</City>
        <CompanyName>YUMMY</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Paris"">FRPAR</Port>
        <Postcode>2000</Postcode>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 15</Address1>
        <Address2>5 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Marseille</City>
        <CompanyName>CONSPA</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Marmande"">FRMAR</Port>
        <Postcode>2000</Postcode>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <Commodity Description=""General"">GEN</Commodity>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>DetailedDescription</DetailedDescription>
        <ExportReferenceNumber>REF002</ExportReferenceNumber>
        <GoodsDescription>DetailedDescription</GoodsDescription>
        <HarmonisedCode>WHISKY</HarmonisedCode>
        <Height>3</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>10</Length>
        <LengthUnit Description=""Meters"">M</LengthUnit>
        <MarksAndNos>MarksAndNumbers</MarksAndNos>
        <OutturnComment></OutturnComment>
        <PackingLineID>EDIDAT00000001</PackingLineID>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber>REF001</ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>300</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>100</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>10</Width>

        <ClassificationCollection>
        </ClassificationCollection>

        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
      <PackingLine>
        <Commodity Description=""General"">GEN</Commodity>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>DetailedDescription</DetailedDescription>
        <ExportReferenceNumber>REF002</ExportReferenceNumber>
        <GoodsDescription>DetailedDescription</GoodsDescription>
        <HarmonisedCode>WHISKY</HarmonisedCode>
        <Height>3</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>10</Length>
        <LengthUnit Description=""Meters"">M</LengthUnit>
        <MarksAndNos>MarksAndNumbers</MarksAndNos>
        <OutturnComment></OutturnComment>
        <PackingLineID>EDIDAT00000002</PackingLineID>
        <PackQty>2</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber>REF001</ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>600</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>200</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>10</Width>

        <ClassificationCollection>
        </ClassificationCollection>

        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";

		public void TestSendNoPackline()
		{
			var bizObj = CreateShipment(false);
			var notifications = new NotificationsHandler();

			var res = MessageSender.SendMessage(bizObj, notifications);

			Assert("message has not been sent", !res);
			AssertEquals(1, notifications.Notifications.Count);
			AssertEquals("notifications",
				"The Shipment must have at least one valid packing line to send Cargo Receipt Advice to Booking Party.",
				string.Join(System.Environment.NewLine, notifications.Notifications.Select(n => n.Message)));
		}

		public void TestSendWithMasterShipment()
		{
			using (Factory.AddDisposableService())
			{
				var bizObj = CreateShipment();
				var masterShipment = CreateMasterShipment(bizObj);
				var notifications = new NotificationsHandler();

				var res = MessageSender.SendMessage(bizObj, notifications);

				Assert("message has been sent", res);
				AssertEquals(0, notifications.Notifications.Count);

				masterShipment.Logs.AddNew(Events.StatusUpdated, $"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}", new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.ElectronicShippingInstruction));
				Factory.Save();

				res = MessageSender.SendMessage(bizObj, notifications);

				Assert("message has not been sent", !res);
				AssertEquals(1, notifications.Notifications.Count);
				AssertEquals("notifications",
					"Shipping Instruction is received against this Shipment; Cargo Receipt Advice is not allowed to be sent to Booking Party once Shipping Instruction is received.",
					string.Join(System.Environment.NewLine, notifications.Notifications.Select(n => n.Message)));
			}
		}

		ForwardingShipment CreateMasterShipment(ForwardingShipment shipment)
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_UniqueConsignRef = "SH0001010";
			masterShipment.JS_TransportMode = Constants.TransportModes.Sea;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "HOUSEBILL002";
			masterShipment.JS_RL_NKOrigin = "MXTIJ";
			masterShipment.JS_RL_NKDestination = "MXCJS";
			masterShipment.JS_RL_NKLoadPort = "MXTCT";
			masterShipment.JS_RL_NKDischargePort = "MXELP";
			masterShipment.JS_E_DEP = ZDate.Today.AddDays(1);
			masterShipment.JS_E_ARV = ZDate.Today.AddDays(2);
			masterShipment.JS_MarksAndNumbers = "marks & numbers";
			masterShipment.JS_BookingReference = "BKG000002";
			masterShipment.JS_InterimReceipt = "interimReceipt123";
			masterShipment.JS_A_RCV = new ZDateTime(2022, 3, 8, 10, 58, 51);

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();

			return masterShipment;
		}

		ForwardingShipment CreateShipment(bool hasPackLine = true)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_RL_NKOrigin = "MXTIJ";
			shipment.JS_RL_NKDestination = "MXCJS";
			shipment.JS_RL_NKLoadPort = "MXTCT";
			shipment.JS_RL_NKDischargePort = "MXELP";
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_InterimReceipt = "interimReceipt123";
			shipment.JS_A_RCV = new ZDateTime(2022, 3, 8, 10, 58, 51);

			var messageReference = Factory.New<CusEntryNumber>();
			messageReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			messageReference.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			messageReference.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			messageReference.CE_EntryNum = "SHP001";
			shipment.Numbers.Add(messageReference);

			PopulateShipmentAddresses(shipment);

			shipment.OuterPackLines.RemoveAndDeleteAll();

			if (hasPackLine)
			{
				PopulatePackingLine(shipment, 1, 100, 300);
				PopulatePackingLine(shipment, 2, 200, 300);
			}

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRMAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_FullName = "YUMMY";
			bookingParty.OH_RL_NKClosestPort = "FRPAR";
			bookingParty.MainAddress.Address1 = "Unit 200";
			bookingParty.MainAddress.Address2 = "55 Why Lane";
			bookingParty.MainAddress.City = "Paris";
			bookingParty.MainAddress.Postcode = "2000";
			bookingParty.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
		}

		void PopulatePackingLine(ForwardingShipment shipment, ZInt count, ZDecimal weight, ZDecimal volume)
		{
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = count;
			packline.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packline.JL_ActualWeight = weight;
			packline.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packline.JL_ActualVolume = volume;
			packline.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packline.JL_Length = 10;
			packline.JL_Width = 10;
			packline.JL_Height = 3;
			packline.JL_UnitOfDimension = "M";
			packline.JL_HarmonisedCode = "WHISKY";
			packline.JL_RefNumber = "REF001";
			packline.JL_ExportRefNumber = "REF002";
			packline.JL_DetailedDescription = "DetailedDescription";
			packline.JL_MarksAndNumbers = "MarksAndNumbers";
			packline.JL_Description = "Description";
		}
	}
}
