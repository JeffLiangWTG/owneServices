using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders
{
	[TestedType(typeof(SupplierBookingShipmentCreator))]
	class SupplierBookingShipmentCreatorTest : LogSubscriberTest<SupplierBookingShipmentCreator>
	{
		public void TestCYSupplierBookingShipmentCreation_BasicMappings_ActionNew()
		{
			AssertCYSupplierBookingShipmentCreation_BasicMappings();
		}

		public void TestCYSupplierBookingShipmentCreation_BasicMappings_ActionNew_ParcialPacked()
		{
			AssertCYSupplierBookingShipmentCreation_BasicMappings(false);
		}

		public void TestCYSupplierBookingShipmentCreation_ShipmentPlanningValidation()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var helper = new PlannedShipmentValidationTestHelper(builder);

			var unlinkedSupplierBookingKey = "JSB001";
			helper.BuildCYContainerLoadListWithPlannedShipment(
				unlinkedSupplierBookingKey,
				containerLoadListKey: "CLH001",
				containerLoadListLineKey: "CLL001",
				plannedShipmentKey: "JS001",
				plannedConsolKey: null,
				allocatedConsolKey: "JK001",
				allocatedContainerKey: "JC001");

			var correctlyLinkedSupplierBookingKey = "JSB002";
			var correctlyLinkedContainerLoadListKey = "CLH002";
			helper.BuildCYContainerLoadListWithPlannedShipment(
				correctlyLinkedSupplierBookingKey,
				containerLoadListKey: "CLH002",
				containerLoadListLineKey: "CLL002",
				plannedShipmentKey: "JS002",
				plannedConsolKey: "JK002",
				allocatedConsolKey: "JK002",
				allocatedContainerKey: "JC002");

			var unlinkedSupplierBooking = builder.MatchExistedEntity<JobSupplierBooking>(unlinkedSupplierBookingKey);
			unlinkedSupplierBooking.JSB_Status = SupplierBookingStatus.Shipped;
			unlinkedSupplierBooking.Logs.AddNew(AutoEvents.StatusUpdated, $"|ACT=NEW|NEW=SHP");

			var correctlyLinkedSupplierBooking = builder.MatchExistedEntity<JobSupplierBooking>(correctlyLinkedSupplierBookingKey);
			var correctlyLinkedContainerLoadList = builder.MatchExistedEntity<CYContainerLoadList>(correctlyLinkedContainerLoadListKey);
			correctlyLinkedSupplierBooking.JSB_Status = SupplierBookingStatus.Shipped;
			correctlyLinkedContainerLoadList.CLH_Status = CommonContainerLoadListStatusList.Codes.PLC;
			correctlyLinkedSupplierBooking.Logs.AddNew(AutoEvents.StatusUpdated, $"|ACT=NEW|NEW=SHP");

			Factory.Save();

			RunLogWalkerCycleForTest();

			unlinkedSupplierBooking.Reload();
			correctlyLinkedSupplierBooking.Reload();

			AssertEquals(1, unlinkedSupplierBooking.Logs.GetAllLogs().OfType<StmALog>().Count(
				   x => x.SL_Reference == $"|RES=The shipments were detached from the container's consol before the pack lines could be created"
				   && x.SL_SE_NKEvent == Events.ExceptionRaisedCode));
			AssertEquals("Status should be reverted on failed validation", SupplierBookingStatusList.Codes.PLN, unlinkedSupplierBooking.JSB_Status);

			AssertEquals("Should proceed with conversion on successful validation", SupplierBookingStatusList.Codes.CNV, correctlyLinkedSupplierBooking.JSB_Status);
		}

		void AssertCYSupplierBookingShipmentCreation_BasicMappings(bool fullyPacked = true)
		{
			SetupBasicTestData(bookedQuantity: 12, TransportModes.Sea, ContainerLoadListHeaderStatus.Placed, fullyPacked);
			booking.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");

			Factory.Save();

			consol.Shipments.Reload(false);
			AssertEquals("Precondition: no shipment exists on the consol.", 0, consol.Shipments.Count);

			RunLogWalkerCycleForTest();

			consol.Shipments.Reload(false);
			AssertEquals("Shipment has been created", 1, consol.Shipments.Count);
			Assert("HouseBill should be generated", !consol.Shipments[0].JS_HouseBill.IsEmpty);

			var shipment = consol.Shipments[0];
			AssertShipment(shipment);
			AssertStatus(loadListHeader, ContainerLoadListHeaderStatus.Converted);
			AssertStatus(booking, SupplierBookingStatus.Converted);
			AssertStatus(booking.SupplierBookingLines[0].OrderLine, OrderStatus.Delivered);
			AssertStatus(booking.SupplierBookingLines[0].OrderLine.Order, OrderStatus.Delivered);

			var log = loadListHeader.Logs.GetAllLogs().OfType<StmALog>().Where(x => x.SL_SE_NKEvent == "STU");
			AssertEquals(1, log.Count(x => x.SL_Reference == $"|TYP=CLL|OLD=PLC|NEW=CNV"));
		}

		public void TestCYSupplierBookingShipmentCreation_NoAPPStatusContainerLoadListHeaderAttached_TransportModeAir_New()
		{
			AssertCYSupplierBookingShipmentCreation_NoAPPStatusAttached_BasicMappings(TransportModes.Air);
		}

		public void TestCYSupplierBookingShipmentCreation_NoAPPStatusContainerLoadListHeaderAttached_TransportModeSea_New()
		{
			AssertCYSupplierBookingShipmentCreation_NoAPPStatusAttached_BasicMappings(TransportModes.Sea);
		}

		void AssertCYSupplierBookingShipmentCreation_NoAPPStatusAttached_BasicMappings(string transportMode)
		{
			SetupBasicTestData(bookedQuantity: 12, transportMode, ContainerLoadListHeaderStatus.Incomplete);
			booking.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");

			Factory.Save();

			consol.Shipments.Reload(false);
			AssertEquals("Precondition: no shipment exists on the consol.", 0, consol.Shipments.Count);

			RunLogWalkerCycleForTest();

			consol.Shipments.Reload(false);
			AssertEquals("Precondition: no shipment exists on the consol.", 0, consol.Shipments.Count);
		}

		public void TestLSESupplierBookingShipmentCreation_BasicMappings_FullDispatched()
		{
			PrepareLSESupplierBookingTestData();

			var bookingLine1 = AddBookingLine(booking, "JSL001", orderLine, 1, 2, 3, 4);
			var bookingLine2 = AddBookingLine(booking, "JSL002", orderLine, 3, 2, 3, 2);
			booking.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");

			Factory.Save();
			var shipments = Factory.Load<ForwardingShipment>(new ZQuery());
			AssertEquals("Shipment has not been created", 0, shipments.Length);

			RunLogWalkerCycleForTest();

			shipments = Factory.Load<ForwardingShipment>(new ZQuery());
			AssertEquals("Shipment has been created", 1, shipments.Length);
			var shipment = shipments[0];
			booking.Reload();

			AssertShipment(shipment);
			AssertStatus(booking, SupplierBookingStatus.Converted);

			AssertEquals("Packline has been created", 2, shipment.OuterPackLines.Count);
			bookingLine1.Reload();
			bookingLine2.Reload();
			var packLine1 = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, bookingLine1.PK));
			var packLine2 = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, bookingLine2.PK));
			AssertPackingLine(bookingLine1, packLine1, 1, 2, 3, 4);
			AssertPackingLine(bookingLine2, packLine2, 3, 2, 3, 2);
		}

		public void TestLSESupplierBookingShipmentCreation_BasicMappings_ParcialDispatched()
		{
			PrepareLSESupplierBookingTestData();

			var bookingLine1 = AddBookingLine(booking, "JSL001", orderLine, 1, 2, 3, 4);
			var bookingLine2 = AddBookingLine(booking, "JSL002", orderLine, 0, 0, 0, 0);
			booking.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");

			Factory.Save();
			var shipments = Factory.Load<ForwardingShipment>(new ZQuery());
			AssertEquals("Shipment has not been created", 0, shipments.Length);

			RunLogWalkerCycleForTest();

			shipments = Factory.Load<ForwardingShipment>(new ZQuery());
			AssertEquals("Shipment has been created", 1, shipments.Length);
			var shipment = shipments[0];
			booking.Reload();

			AssertShipment(shipment);
			AssertStatus(booking, SupplierBookingStatus.Converted);

			AssertEquals("Packline has been created", 1, shipment.OuterPackLines.Count);
			bookingLine1.Reload();
			var packLine1 = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, bookingLine1.PK));
			AssertPackingLine(bookingLine1, packLine1, 1, 2, 3, 4);
		}

		#region New SPT

		public void TestShouldGenerateShipmentForPlanningShipment()
		{
			var (order, supplierBooking, planningShipment) = Converters.OrderShipmentPlanningConverterTest.PrepareSetup(Factory);
			Factory.Save();

			supplierBooking.Logs.AddNew(AutoEvents.StatusUpdated, reference: "|TYP=Shipment Planning Complete|");
			Factory.Save();

			RunLogWalkerCycleForTest();

			var shipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;

			AssertNotNull("generate shipment should work", shipment);
		}

		#endregion

		#region Implementation

		void PrepareTestDataForPackLine(BusinessObjectFactory factory, OrderLine orderLine, ContainerLoadListLine loadListLine, ZDecimal packedQuantity, bool fullyPacked)
		{
			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "TestProductNum";
			orderLine.JO_Partno = product.OP_PartNum;
			orderLine.JO_Description = "Order Line Description Test";
			orderLine.JO_AdditionalInformation = "Order Line Additional Information Test";

			loadListLine.CLL_Volume = 2.1;
			loadListLine.CLL_VolumeUnit = Volume.CubicMetres;
			loadListLine.CLL_Weight = 4.3;
			loadListLine.CLL_WeightUnit = Weight.Kilograms;
			loadListLine.CLL_LoadSequence = 5;
			loadListLine.CLL_Packages = 3;
			loadListLine.CLL_F3_NKPackagesUnit = PkgUnit.Package;
			loadListLine.CLL_RH_NKCommodityCode = "GEN";
			loadListLine.CLL_HarmonizedCode = "HAR";
			loadListLine.CLL_ReferenceNumber = "RN0001";
			loadListLine.CLL_PackedQuantity = fullyPacked ? packedQuantity : 3;
		}

		static ContainerLoadListLine AddLoadListLine(
			CommonContainerLoadList containerLoadList,
			JobSupplierBookingLine bookingLine,
			ForwardingContainer container,
			decimal volume = 0.0m,
			string volumeUnit = Volume.CubicMetres,
			decimal weight = 0.0m,
			string weightUnit = Weight.Kilograms,
			int packages = 0,
			string packagesUnit = PkgUnit.Piece,
			int quantity = 0,
			int sequence = 0)
		{
			var loadListLine = containerLoadList.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JC_Container = container.PK;
			loadListLine.CLL_Volume = volume;
			loadListLine.CLL_VolumeUnit = volumeUnit;
			loadListLine.CLL_Weight = weight;
			loadListLine.CLL_WeightUnit = weightUnit;
			loadListLine.CLL_LoadSequence = sequence;
			loadListLine.CLL_Packages = packages;
			loadListLine.CLL_F3_NKPackagesUnit = packagesUnit;
			loadListLine.CLL_PackedQuantity = quantity;

			return loadListLine;
		}

		static CommonContainerLoadList CreateContainerLoadList(BusinessObjectFactory factory, string loadListID, JobSupplierBooking booking, OrgHeader bookingParty, string status, string loadMode = SupplierBookingLoadMode.ContainerYard)
		{
			var containerLoadList = factory.NewWithValidTestData<CommonContainerLoadList>();
			containerLoadList.CLH_JSB_Booking = booking.PK;
			containerLoadList.CLH_LoadListId = loadListID;
			containerLoadList.CLH_OH_LoadListParty = bookingParty.PK;
			containerLoadList.CLH_Status = status;
			containerLoadList.CLH_LoadMode = loadMode;
			containerLoadList.ControllingCustomerAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;
			containerLoadList.CLH_OA_CFSAddress = factory.NewWithValidTestData<OrgAddress>().PK;

			return containerLoadList;
		}

		static ForwardingConsol CreateConsolWithTransport(BusinessObjectFactory factory, string transportMode, ZDateTime eta, ZDateTime etd)
		{
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var mainVoyage = factory.NewWithValidTestData<JobVoyage>();
			mainVoyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ANRO ASIA", factory).First().RV_FK;
			mainVoyage.JV_VoyageFlight = "11111";
			var origin1 = mainVoyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_ARV = eta;
			origin1.JA_E_DEP = etd;
			var destination1 = mainVoyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";

			var mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = true;
			mainTransport.JW_JX = mainVoyage.Sailings[0].PK;

			factory.Save();

			return consol;
		}

		void SetupBasicTestData(ZDecimal bookedQuantity, string transportMode = TransportModes.Sea, string status = ContainerLoadListHeaderStatus.Placed, bool fullyPacked = true)
		{
			var supplier = OrderManagerTestHelper.CreateOrgWithAddressesAndContacts(Factory, "SUPPLSB");
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();

			order = OrderManagerTestHelper.CreateGenericOrder(Factory);
			OrderManagerTestHelper.SetupAddressForOrder(Factory, order);

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_ItemPrice = 2;

			booking = OrderManagerTestHelper.CreateSupplierBooking(Factory, "JSB00001", bookingParty, supplier);
			booking.JSB_Status = SupplierBookingStatus.Shipped;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_BookedQuantity = bookedQuantity;
			bookingLine.JSL_MarksAndNumbers = "123";

			consol = CreateConsolWithTransport(Factory, transportMode, eta, etd);

			var transport = consol.Transports[0];
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;

			var container1 = consol.Containers.AddNew();
			container1.FillWithValidTestData();
			container1.JC_JSB_SupplierBooking = booking.PK;

			loadListHeader = CreateContainerLoadList(Factory, "CLL00001", booking, bookingParty, status);
			var loadListLine1 = AddLoadListLine(loadListHeader, bookingLine, container1);

			PrepareTestDataForPackLine(Factory, orderLine, loadListLine1, bookedQuantity, fullyPacked);
		}

		void AssertStatus(BusinessObject businessObject, string status)
		{
			businessObject.Reload();

			if (businessObject is JobSupplierBooking supplierBooking)
			{
				AssertEquals($"The status of supplier booking should be {status}", status, supplierBooking.JSB_Status);
			}
			else if (businessObject is CommonContainerLoadList containerLoadList)
			{
				AssertEquals($"The status of container load list should be {status}", status, containerLoadList.CLH_Status);
			}
		}

		void PrepareLSESupplierBookingTestData()
		{
			var supplier = OrderManagerTestHelper.CreateOrgWithAddressesAndContacts(Factory, "SUPPLSB");
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();

			order = OrderManagerTestHelper.CreateGenericOrder(Factory);
			OrderManagerTestHelper.SetupAddressForOrder(Factory, order);

			orderLine = order.OrderLines.AddNew();
			orderLine.JO_ItemPrice = 2;
			orderLine.JO_F3_NKPackType = PkgUnit.Package;

			booking = OrderManagerTestHelper.CreateSupplierBooking(Factory, "JSB00001", bookingParty, supplier, SupplierBookingLoadModeList.Codes.LSE);
			booking.JSB_Status = SupplierBookingStatus.Shipped;

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "TestProductNum";
			orderLine.JO_Partno = product.OP_PartNum;
			orderLine.JO_Description = "Order Line Description Test";
			orderLine.JO_AdditionalInformation = "Order Line Additional Information Test";
		}

		static JobSupplierBookingLine AddBookingLine(JobSupplierBooking booking, string bookingLineID, OrderLine orderLine, decimal dispatchedVolume, decimal dispatchedWeight, int dispatchedPackages, decimal dispatchedQuantity)
		{
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_BookingLineId = bookingLineID;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_DispatchedVolume = dispatchedVolume;
			bookingLine.JSL_VolumeUnit = Volume.CubicMetres;
			bookingLine.JSL_DispatchedWeight = dispatchedWeight;
			bookingLine.JSL_GrossWeightUnit = Weight.Kilograms;
			bookingLine.JSL_DispatchedPackages = dispatchedPackages;
			bookingLine.JSL_F3_NKBookedPackagesUnit = PkgUnit.Package;
			bookingLine.JSL_DispatchedQuantity = dispatchedQuantity;
			bookingLine.JSL_RH_NKCommodityCode = "GEN";
			bookingLine.JSL_MarksAndNumbers = "123";
			bookingLine.JSL_Description = "sbk line desc";
			bookingLine.JSL_HarmonisedCode = "HC001";
			return bookingLine;
		}

		void AssertShipment(ForwardingShipment shipment)
		{
			AssertEquals(true, booking.ConsigneeDocumentaryAddress.IsEmpty);
			AssertNotEquals(booking.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
			AssertNotEquals(booking.ConsigneeDocumentaryAddress.ContactPK, shipment.ConsigneeDocumentaryAddress.ContactPK);
			AssertEquals(order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
			AssertEquals(order.JD_OC_BuyerContact, shipment.ConsigneeDocumentaryAddress.ContactPK);
			AssertEquals(Guid.Empty, shipment.BuyerDocAddress.E2_OA_Address);
			AssertEquals(Guid.Empty, shipment.BuyerDocAddress.ContactPK);
		}

		void AssertPackingLine(JobSupplierBookingLine bookingLine, ForwardingPackLine packLine, decimal expectedVolume, decimal expectedWeight, int expectedPackages, decimal expectedQuantity)
		{
			bookingLine.Reload();
			AssertEquals(packLine.JL_JSL_BookingLine, bookingLine.PK);
			AssertEquals(expectedVolume, packLine.JL_ActualVolume);
			AssertEquals(Volume.CubicMetres, packLine.JL_ActualVolumeUQ);
			AssertEquals(expectedWeight, packLine.JL_ActualWeight);
			AssertEquals(Weight.Kilograms, packLine.JL_ActualWeightUQ);
			AssertEquals(expectedPackages, packLine.JL_PackageCount);
			AssertEquals(PkgUnit.Package, packLine.JL_F3_NKPackType);
			AssertEquals("GEN", packLine.JL_RH_NKCommodityCode);
			AssertEquals(bookingLine.JSL_MarksAndNumbers, packLine.JL_MarksAndNumbers);
			AssertEquals("HC001", packLine.JL_HarmonisedCode);
			AssertEquals(bookingLine.JSL_Description, packLine.JL_Description);
			AssertEquals("Order Line Additional Information Test", packLine.JL_DetailedDescription);
			AssertEquals(expectedQuantity * 2, packLine.JL_LinePrice);

			AssertEquals(1, packLine.Products.Count);
			var product = packLine.Products[0];
			AssertEquals(expectedQuantity, product.D2_ProductQuantity);
			AssertEquals(PkgUnit.Package, product.D2_ProductUnitOfQty);
		}

		Order order;
		JobSupplierBooking booking;
		ForwardingConsol consol;
		CommonContainerLoadList loadListHeader;
		OrderLine orderLine;
		readonly ZDateTime etd = ZDateTime.Today;
		readonly ZDateTime eta = ZDateTime.Today.AddDays(2);

		#endregion
	}
}
