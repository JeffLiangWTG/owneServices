using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleMovementDataContextManager))]
	public class GteVehicleMovementDataContextManagerTest : ShipmentDataContextManagerTestCase<GteVehicleMovementDataContextManager, GteVehicleMovement>
	{
		GteLane gateLane;
		WhsLocation gateDock;
		WhsLocation gateWarehouse;

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("GteVehicleMovement does not implement IJobNumber", true);
		}

		public void TestGivenExistingGVM_WhenIncomingXUE_ThenMatchByGateActionNumber()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_CancelledReason = ZString.Empty;
			booking.GateMovementBookings.Add(gateMovementBooking);

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_CancelledReason = ZString.Empty;
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_CancelledReason = ZString.Empty;
			vehicleMovement.GateMovements.Add(gateMovement);

			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_GateActionNumber = "GVE001";
			gateIn.GVE_IsIncoming = true;
			gateIn.GVE_CancelledReason = ZString.Empty;

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.GateVehicleMovement, "GVEXXX");
			universalEvent.EventType = AutoEvents.GateIn.Code;
			universalEvent.EventTime = new ZDateTimeOffset(2024, 2, 2);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("Expected DCD message status if there is no matching GVM", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			universalEvent.DataContext.DataTargetCollection.Single().Key = "GVE001";
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals("Service Task Log", "Linked Event to Vehicle Movement.", serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertEquals("Message Log", "Linked Event to Vehicle Movement.", logNoteText);

				var newVehicleMovement = new BusinessObjectFactory().Load<GteVehicleMovement>(vehicleMovement.PK);
				var importedEvent = newVehicleMovement.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GateIn.Code).Single();
				AssertEquals(new ZDateTime(2024, 2, 2), importedEvent.SL_EventTime);
			});
		}

		public void TestEventContextValues()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking.GBM_SourceReferenceNumber = "SRN002";
			gateMovementBooking.GBM_CancelledReason = ZString.Empty;
			booking.GateMovementBookings.Add(gateMovementBooking);

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_CancelledReason = ZString.Empty;
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_CancelledReason = ZString.Empty;
			vehicleMovement.GateMovements.Add(gateMovement);

			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_GateActionNumber = "GVE001";
			gateIn.GVE_IsIncoming = true;
			gateIn.GVE_CancelledReason = ZString.Empty;

			var contextValues = ((IEventDataContextManager)vehicleMovement.GetUniversalDataContextManager()).EventContextValues;

			AssertEquals("Expected no event context values", 0, contextValues.Count());
		}

		protected override GteVehicleMovement GetNewBusinessObjectForTesting()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();

			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_IsIncoming = true;
			gateIn.GVE_GateActionNumber = "GVE001";
			gateIn.GVE_GVM_VehicleMovement = gateMovement.VehicleMovement.PK;

			gateMovement.GateMovementBooking.GBM_MovementBookingNumber = "GBM-001";

			Factory.SaveForTesting();

			return gateMovement.VehicleMovement;
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_IsIncoming = true;
			gateIn.GVE_GateActionNumber = "GVE001";
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			gateDock = Factory.NewWithValidTestData<WhsLocation>();
			gateDock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			gateDock.Warehouse.WarehouseAddress.Address1 = "Address 1";
			gateDock.Warehouse.WarehouseAddress.Address2 = "Address 2";
			gateDock.Warehouse.WarehouseAddress.City = "SYD";
			gateDock.Warehouse.WarehouseAddress.Postcode = "0000";
			gateDock.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			gateDock.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			gateWarehouse = Factory.NewWithValidTestData<WhsLocation>();
			gateWarehouse.WLV_WA_PutawayArea = gateDock.WLV_WA_PutawayArea;
			gateWarehouse.Warehouse.WarehouseAddress.Address1 = "Address 01";
			gateWarehouse.Warehouse.WarehouseAddress.Address2 = "Address 02";
			gateWarehouse.Warehouse.WarehouseAddress.City = "SYD";
			gateWarehouse.Warehouse.WarehouseAddress.Postcode = "0000";
			gateWarehouse.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD 2";
			gateWarehouse.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			gateLane = Factory.NewWithValidTestData<GteLane>();
			gateLane.GLN_Code = "LNE";
			gateLane.Gate.GTE_Code = "GTE";
			gateLane.Gate.GTE_WW_Facility = gateWarehouse.Warehouse.PK;

			Factory.SaveForTesting();
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => @$"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
	<DataContext>
		<DataTargetCollection>
		<DataTarget>
			<Type>GateVehicleMovement</Type>
			<Key>GVE001</Key>
		</DataTarget>
		</DataTargetCollection>

		<RecipientRoleCollection>
		<RecipientRole>
			<Code>ATW</Code>
			<Description>Arrival Transit Warehouse</Description>
			<ServiceCode>GTB</ServiceCode>
		</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>

	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>TransportCompanyDocumentaryAddress</AddressType>
			<OrganizationCode>UNIULU</OrganizationCode>
		</OrganizationAddress>
		<OrganizationAddress>
			<AddressType>LocalCartageYard</AddressType>
			<AddressShortCode>{gateWarehouse.Warehouse.WarehouseAddress.OA_Code}</AddressShortCode>
			<Address1>{gateWarehouse.Warehouse.WarehouseAddress.Address1}</Address1>
			<Address2>{gateWarehouse.Warehouse.WarehouseAddress.Address2}</Address2>
			<City>{gateWarehouse.Warehouse.WarehouseAddress.City}</City>
			<Postcode>{gateWarehouse.Warehouse.WarehouseAddress.Postcode}</Postcode>
			<OrganizationCode>{gateWarehouse.Warehouse.WarehouseAddress.Header.OH_Code}</OrganizationCode>
		</OrganizationAddress>
	</OrganizationAddressCollection>

	<WarehouseLocation>{gateWarehouse.ToLocationString()}</WarehouseLocation>

	<VehicleRun>
		<Vehicle>
			<Registration>
				<Number>REG-123</Number>
			</Registration>
			<VehicleType>
				<Code>RTRK</Code>
			</VehicleType>
		</Vehicle>
	</VehicleRun>

	<DateCollection>
		<Date>
			<Type>Start</Type>
			<Value>2024-04-18T12:00:00</Value>
		</Date>
		<Date>
			<Type>End</Type>
			<Value>2024-04-19T12:00:00</Value>
		</Date>
	</DateCollection>

	<RelatedShipmentCollection>
		<RelatedShipment>
			<TotalWeight>20.00</TotalWeight>
			<TotalWeightUnit>
				<Code>KG</Code>
			</TotalWeightUnit>
			<WarehouseLocation>{gateLane.Gate.GTE_Code}|{gateLane.GLN_Code}</WarehouseLocation>

			<AddInfoCollection>
				<AddInfo>
					<Key>IsIncoming</Key>
					<Value>true</Value>
				</AddInfo>
			</AddInfoCollection>

			<VehicleRun>
				<CrewCollection>
					<Crew>
						<FullName>Jimothy</FullName>
						<LicenseNumber>1234 5678</LicenseNumber>
					</Crew>
				</CrewCollection>
			</VehicleRun>
		</RelatedShipment>

		<RelatedShipment>
			<TotalWeight>25.00</TotalWeight>
			<TotalWeightUnit>
				<Code>KG</Code>
			</TotalWeightUnit>
			<WarehouseLocation>{gateLane.Gate.GTE_Code}|{gateLane.GLN_Code}</WarehouseLocation>

			<AddInfoCollection>
				<AddInfo>
					<Key>IsIncoming</Key>
					<Value>false</Value>
				</AddInfo>
			</AddInfoCollection>

			<VehicleRun>
				<CrewCollection>
					<Crew>
						<FullName>Jimothy</FullName>
						<LicenseNumber>1234 5678</LicenseNumber>
					</Crew>
				</CrewCollection>
			</VehicleRun>
		</RelatedShipment>
	</RelatedShipmentCollection>

	<SubShipmentCollection>
		<SubShipment>
			<BookingConfirmationReference>BRN001</BookingConfirmationReference>
			<AdditionalReferenceCollection>
				<AdditionalReference>
					<Type>
						<Code>BPR</Code>
						<Description>BookingPartyReference</Description>
					</Type>
					<ReferenceNumber>VBS0001</ReferenceNumber>
				</AdditionalReference>
				<AdditionalReference>
					<Type>
						<Code>TRF</Code>
						<Description>TransportReference</Description>
					</Type>
					<ReferenceNumber>TRF0001</ReferenceNumber>
				</AdditionalReference>
				<AdditionalReference>
					<Type>
						<Code>MBN</Code>
						<Description>MovementBookingNumber</Description>
					</Type>
					<ReferenceNumber>GBM-001</ReferenceNumber>
				</AdditionalReference>
			</AdditionalReferenceCollection>

			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>TransportCompanyDocumentaryAddress</AddressType>
					<OrganizationCode>UNIULU</OrganizationCode>
				</OrganizationAddress>
				<OrganizationAddress>
					<AddressType>LocalCartageYard</AddressType>
					<AddressShortCode>{gateDock.Warehouse.WarehouseAddress.OA_Code}</AddressShortCode>
					<Address1>{gateDock.Warehouse.WarehouseAddress.Address1}</Address1>
					<Address2>{gateDock.Warehouse.WarehouseAddress.Address2}</Address2>
					<City>{gateDock.Warehouse.WarehouseAddress.City}</City>
					<Postcode>{gateDock.Warehouse.WarehouseAddress.Postcode}</Postcode>
					<OrganizationCode>{gateDock.Warehouse.WarehouseAddress.Header.OH_Code}</OrganizationCode>
				</OrganizationAddress>
			</OrganizationAddressCollection>

			<TransportBookingDirection>
				<Code>PIC</Code>
			</TransportBookingDirection>
			<WarehouseLocation>{gateDock.ToLocationString()}</WarehouseLocation>

			<DateCollection>
				<Date>
					<Type>Start</Type>
					<Value>2024-04-18T12:00:00</Value>
				</Date>
				<Date>
					<Type>End</Type>
					<Value>2024-04-19T12:00:00</Value>
				</Date>
			</DateCollection>

			<PackingLineCollection>
				<PackingLine>
					<Commodity>
						<Code>AABT</Code>
					</Commodity>
					<PackType>
						<Code>BAG</Code>
					</PackType>
				</PackingLine>
			</PackingLineCollection>

			<ContainerCollection>
				<Container>
					<ContainerNumber>UNT-001</ContainerNumber>
					<ContainerType>
						<Code>20FR</Code>
					</ContainerType>
				</Container>
			</ContainerCollection>
		</SubShipment>
	</SubShipmentCollection>
	</Shipment>
</UniversalShipment>";
	}
}
