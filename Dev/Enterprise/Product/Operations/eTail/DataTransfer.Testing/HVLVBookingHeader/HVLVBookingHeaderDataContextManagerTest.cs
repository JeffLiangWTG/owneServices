using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVBookingHeaderDataContextManager))]
	public class HVLVBookingHeaderDataContextManagerTest : ShipmentDataContextManagerTestCase<HVLVBookingHeaderDataContextManager, HVLVBookingHeader>
	{
		public void TestMatchingByDataContextKey()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00001015";
			bookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.HVLVBookingHeader, "M00001015");
			shipment.ServiceLevel = new ServiceLevel { Code = "D2D" };

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated HVLV Booking Header M00001015 from UniversalShipment.
Successfully saved HVLV Booking Header M00001015.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Message Log", @"
Successfully loaded matching HVLVBookingHeader.
Populating HVLVBookingHeader...
Updated HVLV Booking Header M00001015 from UniversalShipment.
Successfully saved HVLV Booking Header M00001015.
".Trim(), logNoteText);

				var bookingHeaders = new BusinessObjectFactory().Load<HVLVBookingHeader>(new ZQuery());
				AssertEquals(1, bookingHeaders.Length);
				AssertEquals("D2D", bookingHeaders[0].HVH_RS_NKBookingServiceLevel);
			});
		}

		public void TestMatchesEventByBookingReference()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00001015";

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.HVLVBookingHeader, "M00001013");
			universalEvent.EventType = AutoEvents.BookingConfirmedCode;
			universalEvent.EventTime = new ZDateTimeOffset(2017, 6, 1);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			universalEvent.DataContext.DataTargetCollection.Single().Key = "M00001015";
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to HVLV Booking Header M00001015.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Message Log", @"
Linked Event to HVLV Booking Header M00001015.
".Trim(), logNoteText);

				var headerInNewFactory = new BusinessObjectFactory().Load<HVLVBookingHeader>(bookingHeader.PK);
				var importedEvent = headerInNewFactory.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingConfirmedCode).Single();
				AssertEquals(new ZDateTime(2017, 6, 1), importedEvent.SL_EventTime);
			});
		}

		#region Implementation

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			HVLVTestHelper.SetGS1FountainOnOrgProxy(new BusinessObjectFactory(), "1234567");

			var billToParty = Factory.New<OrgHeader>();
			billToParty.OH_Code = "XVBQP68SIYXQ";
			Factory.SaveForTesting();
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
	<DataContext>
	  <DataSource>
		<Key>M00001015</Key>
		<Type>HVLVBookingHeader</Type>
	  </DataSource>
	</DataContext>

	<IsLastMileDeliverySelfBooked>true</IsLastMileDeliverySelfBooked>
	<ServiceLevel Description=""Standard"">STD</ServiceLevel>
	<TotalNoOfPieces>3</TotalNoOfPieces>
	<TotalVolume>2</TotalVolume>
	<TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
	<TotalWeight>30</TotalWeight>
	<TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<Address1>45 Bill To Street</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>45 Bill To Street</AddressShortCode>
		<City></City>
		<CompanyName></CompanyName>
		<Contact>Bill</Contact>
		<Email></Email>
		<Fax></Fax>
		<Mobile></Mobile>
		<OrganizationCode>XVBQP68SIYXQ</OrganizationCode>
		<Phone></Phone>
		<Port></Port>
		<Postcode></Postcode>
		<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<Address1>56 Dispatch Avenue</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>56 Dispatch Avenue</AddressShortCode>
		<City></City>
		<CompanyName></CompanyName>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>H5ZX52PAMCOI</OrganizationCode>
		<Phone></Phone>
		<Port></Port>
		<Postcode></Postcode>
		<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ExportBroker</AddressType>
		<Address1>67 Agent Lane</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>67 Agent Lane</AddressShortCode>
		<City></City>
		<CompanyName></CompanyName>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ZGP5LX5SQPEB</OrganizationCode>
		<Phone></Phone>
		<Port></Port>
		<Postcode></Postcode>
		<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>DepartureCFSAddress</AddressType>
		<Address1>89 Depot Road</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>89 Depot Road</AddressShortCode>
		<City></City>
		<CompanyName></CompanyName>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>013LIP1SZFVU</OrganizationCode>
		<Phone></Phone>
		<Port></Port>
		<Postcode></Postcode>
		<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<Address1>78 Booking Street</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>78 Booking Street</AddressShortCode>
		<City></City>
		<CompanyName></CompanyName>
		<Contact>Brooke</Contact>
		<Email></Email>
		<Fax></Fax>
		<Mobile></Mobile>
		<OrganizationCode>IRED1TLAV242</OrganizationCode>
		<Phone></Phone>
		<Port></Port>
		<Postcode></Postcode>
		<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>

	<SubShipmentCollection>
	  <SubShipment>

		<GoodsDescription>Biscuits</GoodsDescription>
		<GoodsValue>50</GoodsValue>
		<GoodsValueCurrency Description=""United States Dollar"">USD</GoodsValueCurrency>
		<IsHazardous>false</IsHazardous>
		<ManifestedVolume>0.5</ManifestedVolume>
		<ManifestedWeight>10</ManifestedWeight>
		<TotalNoOfPieces>1</TotalNoOfPieces>
		<TotalVolume>1</TotalVolume>
		<TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
		<TotalWeight>10</TotalWeight>
		<TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
		<WayBillNumber>XYZ456</WayBillNumber>
		<WayBillType Description=""House Waybill"">HWB</WayBillType>

		<NoteCollection Content=""Partial"">
		  <Note>
			<Description>Goods Handling Instructions</Description>
			<IsCustomDescription>false</IsCustomDescription>
			<NoteText>Leave at back</NoteText>
		  </Note>
		</NoteCollection>

		<OrganizationAddressCollection>
		  <OrganizationAddress>
			<AddressType>ArrivalCFSAddress</AddressType>
			<Address1>13 Destination St</Address1>
			<Address2></Address2>
			<AddressOverride>false</AddressOverride>
			<AddressShortCode>13 Destination St</AddressShortCode>
			<City></City>
			<CompanyName></CompanyName>
			<Email></Email>
			<Fax></Fax>
			<OrganizationCode>2MIZFGXS85CF</OrganizationCode>
			<Phone></Phone>
			<Port></Port>
			<Postcode></Postcode>
			<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
			<State></State>
		  </OrganizationAddress>
		  <OrganizationAddress>
			<AddressType>ConsigneeDocumentaryAddress</AddressType>
			<Address1>12 Something St</Address1>
			<Address2></Address2>
			<City>Sydney</City>
			<CompanyName>Bobs Company</CompanyName>
			<Contact>Bob</Contact>
			<Country Name=""Australia"">AU</Country>
			<Email>bob@bob.com</Email>
			<Fax></Fax>
			<Mobile></Mobile>
			<Phone></Phone>
			<Postcode>2000</Postcode>
			<State>NSW</State>
		  </OrganizationAddress>
		  <OrganizationAddress>
			<AddressType>ConsignorDocumentaryAddress</AddressType>
			<Address1>23 Another St</Address1>
			<Address2></Address2>
			<City>Melbourne</City>
			<CompanyName>Another Company</CompanyName>
			<Contact>Vic</Contact>
			<Country Name=""Australia"">AU</Country>
			<Email>victor@fakedomain.com</Email>
			<Fax></Fax>
			<Mobile></Mobile>
			<Phone></Phone>
			<Postcode>3000</Postcode>
			<State>VIC</State>
		  </OrganizationAddress>
		</OrganizationAddressCollection>

		<PackingLineCollection>
		  <PackingLine>
			<IsPerishable>true</IsPerishable>
			<IsPersonalEffects>true</IsPersonalEffects>
			<IsSignatureRequired>false</IsSignatureRequired>
			<IsTimber>true</IsTimber>
			<ManifestedVolume>0.5</ManifestedVolume>
			<ManifestedWeight>10</ManifestedWeight>
			<PackQty>1</PackQty>
			<PackType Description=""Package"">PKG</PackType>
			<ReferenceNumber>X0012931292</ReferenceNumber>
			<RequiresFumigationCertificate>true</RequiresFumigationCertificate>
			<Volume>1</Volume>
			<VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
			<Weight>10</Weight>
			<WeightUnit Description=""Kilograms"">KG</WeightUnit>
		  </PackingLine>
		</PackingLineCollection>
	  </SubShipment>
	  <SubShipment>

		<GoodsDescription>Explosives</GoodsDescription>
		<GoodsValue>200</GoodsValue>
		<GoodsValueCurrency Description=""Australian Dollar"">AUD</GoodsValueCurrency>
		<IsHazardous>true</IsHazardous>
		<ManifestedVolume>1.0</ManifestedVolume>
		<ManifestedWeight>15</ManifestedWeight>
		<TotalNoOfPieces>2</TotalNoOfPieces>
		<TotalVolume>1.0</TotalVolume>
		<TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
		<TotalWeight>20</TotalWeight>
		<TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
		<WayBillNumber>ABC123</WayBillNumber>
		<WayBillType Description=""House Waybill"">HWB</WayBillType>

		<NoteCollection Content=""Partial"">
		  <Note>
			<Description>Goods Handling Instructions</Description>
			<IsCustomDescription>false</IsCustomDescription>
			<NoteText>Leave at front</NoteText>
		  </Note>
		</NoteCollection>

		<OrganizationAddressCollection>
		  <OrganizationAddress>
			<AddressType>ArrivalCFSAddress</AddressType>
			<Address1>12 Destination St</Address1>
			<Address2></Address2>
			<AddressOverride>false</AddressOverride>
			<AddressShortCode>12 Destination St</AddressShortCode>
			<City></City>
			<CompanyName></CompanyName>
			<Email></Email>
			<Fax></Fax>
			<OrganizationCode>KCSSYKIA3SMN</OrganizationCode>
			<Phone></Phone>
			<Port></Port>
			<Postcode></Postcode>
			<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
			<State></State>
		  </OrganizationAddress>
		  <OrganizationAddress>
			<AddressType>ConsigneeDocumentaryAddress</AddressType>
			<Address1>99 Consignee Road</Address1>
			<Address2>Downtown</Address2>
			<City>New York</City>
			<CompanyName>Murray</CompanyName>
			<Contact>Moo ray</Contact>
			<Country Name=""United States"">US</Country>
			<Email>murray.hewitt@usconsulate.gov.nz</Email>
			<Fax>+0987654321</Fax>
			<Mobile>+1234567890</Mobile>
			<Phone>7</Phone>
			<Postcode>12345</Postcode>
			<State>NY</State>
		  </OrganizationAddress>
		  <OrganizationAddress>
			<AddressType>ConsignorDocumentaryAddress</AddressType>
			<Address1>22 Shipper Street</Address1>
			<Address2></Address2>
			<City>Wellington</City>
			<CompanyName>Some Company</CompanyName>
			<Contact>Randy</Contact>
			<Country Name=""New Zealand"">NZ</Country>
			<Email>randy@randysdomain.com</Email>
			<Fax>8</Fax>
			<Mobile>+555 5555</Mobile>
			<Phone>01189998819991197253</Phone>
			<Postcode>54321</Postcode>
			<State>WLG</State>
		  </OrganizationAddress>
		</OrganizationAddressCollection>

		<PackingLineCollection>
		  <PackingLine>
			<IsPerishable>false</IsPerishable>
			<IsPersonalEffects>false</IsPersonalEffects>
			<IsSignatureRequired>true</IsSignatureRequired>
			<IsTimber>false</IsTimber>
			<ManifestedVolume>0.5</ManifestedVolume>
			<ManifestedWeight>5</ManifestedWeight>
			<PackQty>1</PackQty>
			<PackType Description=""Box"">BOX</PackType>
			<ReferenceNumber>D9901239028</ReferenceNumber>
			<RequiresFumigationCertificate>false</RequiresFumigationCertificate>
			<Volume>0.5</Volume>
			<VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
			<Weight>10</Weight>
			<WeightUnit Description=""Kilograms"">KG</WeightUnit>

			<UNDGCollection>
			  <UNDG>
				<IMOClass>1.1D</IMOClass>
			  </UNDG>
			</UNDGCollection>
		  </PackingLine>
		  <PackingLine>
			<IsPerishable>false</IsPerishable>
			<IsPersonalEffects>false</IsPersonalEffects>
			<IsSignatureRequired>true</IsSignatureRequired>
			<IsTimber>false</IsTimber>
			<ManifestedVolume>0.5</ManifestedVolume>
			<ManifestedWeight>10</ManifestedWeight>
			<PackQty>1</PackQty>
			<PackType Description=""Box"">BOX</PackType>
			<ReferenceNumber>D5465421481</ReferenceNumber>
			<RequiresFumigationCertificate>false</RequiresFumigationCertificate>
			<Volume>0.5</Volume>
			<VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
			<Weight>10</Weight>
			<WeightUnit Description=""Kilograms"">KG</WeightUnit>

			<UNDGCollection>
			  <UNDG>
				<IMOClass>1.1D</IMOClass>
			  </UNDG>
			</UNDGCollection>
		  </PackingLine>
		</PackingLineCollection>
	  </SubShipment>
	</SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
			}
		}

		#endregion
	}
}
