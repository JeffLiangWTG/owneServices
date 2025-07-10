using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	public class CYBookingCreationIntegrationTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestContainerYardDropOffBookingCreationWithSingleVehicle()
		{
			var booking = createTestBooking();
			var gbv = createTestVehicleBooking(booking);
			var gbm1 = createTestMovementBooking(booking, containerType1, isPickup: false, id: 1);
			var gbm2 = createTestMovementBooking(booking, containerType2, isPickup: false, id: 2);
			gbm1.GBM_BookingReferenceNumber = string.Empty;
			gbm2.GBM_BookingReferenceNumber = string.Empty;
			gbm2.GBM_SlotStartTime = gbm2.GBM_SlotStartTime.AddDays(1);

			Factory.SaveForTesting();

			var existingTpus = businessObjectFactory.Load<ICYDTransportationUnit>(new ZQuery());
			AssertEquals("Precondition: There should be no existing TPU's", 0, existingTpus.Length);

			var dropOffShipment = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			dropOffShipment.DataContext.RecipientRoleCollection = new List<IRecipientRoleDataObject>()
			{
				new RecipientRole()
				{
					Code = RecipientRoleType.CYD,
					Description = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetDescriptionFromCode(MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse),
				}
			};

			manager.Process(GetQueuedUniversalShipmentMessage(dropOffShipment));

			var tpu = businessObjectFactory.Load<ICYDTransportationUnit>(new ZQuery()).First();
			AssertNotNull("A matching TPU entity should have been created", tpu);

			AssertEquals("YTU_TransportationReference should match with (first) GBV_VehicleRegistration",
				gbv.GBV_VehicleRegistration,
				tpu.YTU_TransportationReference);

			AssertEquals("YTU_Estimated GateInTime should match with earliest GBM_SlotStartTime",
				gbm1.GBM_SlotStartTime.Date,
				tpu.YTU_EstimatedGateInTime.Date);

			CombineAssertions("GBM Pickups and Drop-offs should be mapped into Pickups and Deliveries", () =>
			{
				AssertEquals("tpu should have 2 deliveries", 2, tpu.GetDeliveries.Count);
				AssertEquals("tpu should have 0 pickups", 0, tpu.GetPickups.Count);
			});

			AssertContainsExactElementsInAnyOrder("GBM_TransportReference should be mapped to CYDDelivery.YDL_TransportReference",
				new[] { gbm1, gbm2 }.Select(gbm => gbm.GBM_TransportReference),
				tpu.GetDeliveries.Select(d => d.YDL_TransportReference));

			AssertContainsExactElementsInAnyOrder("GBM_UnitNumber should be mapped to CYDDelivery.CYDYardUnitState.YUS_UnitID",
				new[] { gbm1, gbm2 }.Select(gbm => gbm.GBM_UnitNumber),
				tpu.GetDeliveries.Select(d => d.GetLinkedYardUnit.YUS_UnitID));

			AssertContainsExactElementsInAnyOrder("GBM_RC_UnitType.RC_Code should match with CYDDelivery.CYDUnitLineItem.YLI_RC_ContainerType.Code",
				new[] { containerType1.RC_Code, containerType2.RC_Code },
				tpu.GetDeliveries.Select(d => Factory.Load<RefContainer>(d.GetUnitLineItem.YLI_RC_ContainerType).RC_Code));
		}

		public void TestContainerYardPickUpBookingCreationWithSingleVehicle()
		{
			var booking = createTestBooking();
			var gbv = createTestVehicleBooking(booking);
			var gbm1 = createTestMovementBooking(booking, containerType1, isPickup: true, id: 1);
			var gbm2 = createTestMovementBooking(booking, containerType2, isPickup: true, id: 2);
			gbm2.GBM_SlotStartTime = gbm2.GBM_SlotStartTime.AddDays(1);

			Factory.SaveForTesting();

			var existingTpus = businessObjectFactory.Load<ICYDTransportationUnit>(new ZQuery());
			AssertEquals("Precondition: There should be no existing TPU's", 0, existingTpus.Length);

			var pickUpShipment = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			pickUpShipment.DataContext.RecipientRoleCollection = new List<IRecipientRoleDataObject>()
			{
				new RecipientRole()
				{
					Code = RecipientRoleType.CYD,
					Description = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetDescriptionFromCode(MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse),
				}
			};

			manager.Process(GetQueuedUniversalShipmentMessage(pickUpShipment));

			var tpu = businessObjectFactory.Load<ICYDTransportationUnit>(new ZQuery()).FirstOrDefault();
			AssertNotNull("A matching TPU entity should have been created", tpu);

			AssertEquals("TPU should be created matching YTU_TransportationReference with (first) GBV_VehicleRegistration",
				gbv.GBV_VehicleRegistration,
				tpu.YTU_TransportationReference);

			AssertEquals("YTU_Estimated GateInTime should match with earliest GBM_SlotStartTime",
				gbm1.GBM_SlotStartTime.Date,
				tpu.YTU_EstimatedGateInTime.Date);

			CombineAssertions("GBM Pickups and Drop-offs should be mapped into Pickups and Deliveries", () =>
			{
				AssertEquals("tpu should have 0 deliveries", 0, tpu.GetDeliveries.Count);
				AssertEquals("tpu should have 2 pickups", 2, tpu.GetPickups.Count);
			});

			AssertContainsExactElementsInAnyOrder("GBM_TransportReference should be mapped to CYDPickup.YPL_TransportReference",
				new[] { gbm1, gbm2 }.Select(gbm => gbm.GBM_TransportReference),
				tpu.GetPickups.Select(d => d.YPL_TransportReference));

			AssertContainsExactElementsInAnyOrder("GBM_UnitNumber should be mapped to CYDPickup.CYDYardUnitState.YUS_UnitID",
				new[] { gbm1, gbm2 }.Select(gbm => gbm.GBM_UnitNumber),
				tpu.GetPickups.Select(d => d.GetLinkedYardUnit.YUS_UnitID));

			AssertContainsExactElementsInAnyOrder("GBM_RC_UnitType.RC_Code should match with CYDPickup.CYDUnitLineItem.YLI_RC_ContainerType.Code",
				new[] { containerType1.RC_Code, containerType2.RC_Code },
				tpu.GetPickups.Select(d => Factory.Load<RefContainer>(d.GetUnitLineItem.YLI_RC_ContainerType).RC_Code));
		}

		#region Helpers

		GteBooking createTestBooking(int id = 1)
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "BookingReference" + id;
			booking.GBK_OH_TransportCompany = transportCompany.PK;
			booking.GBK_WW_Facility = containerYard.PK;

			return booking;
		}

		GteVehicleMovementBooking createTestVehicleBooking(GteBooking booking, int id = 1)
		{
			var gbv = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			gbv.GBV_GBK_Booking = booking.PK;
			gbv.GBV_VehicleRegistration = "Vehicle" + id;

			return gbv;
		}

		GteGateMovementBooking createTestMovementBooking(GteBooking booking, RefContainer containerType, bool isPickup, int id)
		{
			var gbm = Factory.New<GteGateMovementBooking>();
			gbm.GBM_GBK_Booking = booking.PK;
			gbm.GBM_BookingReferenceNumber = "gbm" + id;
			gbm.GBM_TransportReference = (isPickup ? "pickup" : "dropoff") + id;
			gbm.GBM_UnitNumber = "container" + id;
			gbm.GBM_SlotStartTime = new ZDateTimeOffset(2025, 1, 1, 10, 10, 10);
			gbm.GBM_IsPickup = isPickup;
			gbm.GBM_RC_UnitType = containerType.PK;
			gbm.GBM_Source = "VBS";
			gbm.GBM_SourceReferenceNumber = "Gate-VBS-GBM" + id;

			if (isPickup)
			{
				markContainerForPickupInCY(gbm, containerType, id);
			}

			return gbm;
		}

		void markContainerForPickupInCY(GteGateMovementBooking gbm, RefContainer containerType, int id)
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			cydTestHelper.SetupDataForPickup(gbm.GBM_BookingReferenceNumber, gbm.GBM_BookingReferenceNumber, today, tomorrow, containerType.RC_Code, new string[] { gbm.GBM_UnitNumber }, id);
		}

		#endregion

		#region Setup Test Data

		protected override void SetUp()
		{
			base.SetUp();

			containerType1 = Factory.NewWithValidTestData<RefContainer>();
			containerType1.RC_Code = "20GP";
			containerType1.RC_Description = "Twenty foot general purpose";

			containerType2 = Factory.NewWithValidTestData<RefContainer>();
			containerType2.RC_Code = "20FR";
			containerType2.RC_Description = "Twenty foot flatrack";

			transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			containerYard = Factory.NewWithValidTestData<WhsWarehouse>();
			containerYard.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			cydTestHelper = ObjectFactory.New<ICYDUniversalTestData>(Factory, new TestErrorLogger());
			manager = new UniversalMessageProcessingManager(Factory, new ServiceTaskLogForTesting());
			businessObjectFactory = new BusinessObjectFactory();
		}

		BusinessObjectFactory businessObjectFactory;
		RefContainer containerType1, containerType2;
		OrgHeader transportCompany;
		WhsWarehouse containerYard;
		UniversalMessageProcessingManager manager;
		ICYDUniversalTestData cydTestHelper;

		#endregion
	}
}

