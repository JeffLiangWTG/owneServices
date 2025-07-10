using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders
{
	[TestedType(typeof(ContainerLoadListShipmentCreator))]
	class ContainerLoadListShipmentCreatorTest : LogSubscriberTest<ContainerLoadListShipmentCreator>
	{
		public void TestContainerLoadListShipmentCreation_BasicMappings_CY()
		{
			AssertContainerLoadListShipmentCreation_BasicMappings(SupplierBookingLoadMode.ContainerYard, TransportModes.Sea);
		}

		public void TestContainerLoadListShipmentCreation_BasicMappings_CFS()
		{
			AssertContainerLoadListShipmentCreation_BasicMappings(SupplierBookingLoadMode.ContainerFreightStation, TransportModes.Sea);
		}

		void AssertContainerLoadListShipmentCreation_BasicMappings(string loadMode, string transportMode)
		{
			var isCY = loadMode == SupplierBookingLoadMode.ContainerYard;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
				var orderLine = CreateOrderLineWithOrder();
				var (booking, bookingLine) = CreateSupplierBookingWithOneLine(loadMode, orderLine, bookingParty, transportMode: transportMode, bookingId: GetNextId(), hasConsigneeDocumentaryAddress: false);

				var loadListHeader = isCY
					? CreateContainerLoadList(GetNextId(), booking, bookingParty)
					: CreateContainerLoadPlan(GetNextId(), booking.ControllingCustomerAddress, booking.CFSAddress).With(plan => plan.CLH_PlannedTransportMode = transportMode);

				var consol1 = CreateConsol(transportMode);
				var container1 = isCY ? CreateAndAttachContainer(booking, consol1) : CreateAndAttachContainer(loadListHeader, consol1);
				var loadListLine = AddLoadListLine(loadListHeader, bookingLine, container1);

				loadListHeader.Logs.AddNew(AutoEvents.StatusUpdated, $"|NEW=SHP");
				PrepareTestDataForPackLine(orderLine, loadListLine, packedQuantity: 12);

				Factory.Save();

				consol1.Shipments.Reload(false);
				AssertEquals("Precondition: no shipment exists on the consol.", 0, consol1.Shipments.Count);

				new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListHeader.PK, Factory);
				Factory.Save();

				consol1.Shipments.Reload(false);
				AssertEquals("Shipment has been created", 1, consol1.Shipments.Count);
				Assert("HouseBill should be generated", !consol1.Shipments[0].JS_HouseBill.IsEmpty);

				var shipment = consol1.Shipments[0];

				if (isCY)
				{
					OrderManagerTestHelper.AssertPopulatedShipmentToExpected(
						shipment,
						orderLine.Order,
						booking,
						etd,
						eta,
						hasConsigneeDocumentaryAddress: false,
						controllingCustomerAddress: null,
						transportMode == TransportModes.Sea ? ContainerModes.FCL : ContainerModes.ULD,
						loadListHeader);
					AssertStatus(booking, SupplierBookingStatus.Placed);
				}
				else
				{
					AssertPopulatedShipmentFromCFSToExpected(shipment, booking, orderLine.Order, loadListHeader, consol1);
					AssertStatus(booking, SupplierBookingStatus.Placed);
				}

				AssertPackedPackLine(shipment.OuterPackLines, 0, loadListLine, orderLine, container1);
				AssertStatus(loadListHeader, ContainerLoadListHeaderStatus.Converted);
			}
		}

		public void TestCYContainerLoadListShipmentCreation_MultipleBookings_AlloctatedToSameConsol()
		{
			RunContainerLoadListShipmentCreation_MultipleBookings(SupplierBookingLoadMode.ContainerYard, true);
		}

		public void TestCYContainerLoadListShipmentCreation_MultipleBookings_AllocatedToMultipleConsol()
		{
			RunContainerLoadListShipmentCreation_MultipleBookings(SupplierBookingLoadMode.ContainerYard, false);
		}

		public void TestCFSContainerLoadPlanShipmentCreation_MultipleBookings_AllocatedToSameConsol()
		{
			RunContainerLoadListShipmentCreation_MultipleBookings(SupplierBookingLoadMode.ContainerFreightStation, true);
		}

		public void TestCFSContainerLoadPlanShipmentCreation_MultipleBookings_AllocatedToMultipleConsol()
		{
			RunContainerLoadListShipmentCreation_MultipleBookings(SupplierBookingLoadMode.ContainerFreightStation, false);
		}

		void RunContainerLoadListShipmentCreation_MultipleBookings(string loadMode, bool shareConsol)
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			var consol1 = builder.BuildConsol("JK001");
			var consol2 = shareConsol ? consol1 : builder.BuildConsol("JK002");
			var booking1 = builder.BuildSupplierBooking("JSB001", "PLC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_LoadMode)] = loadMode
			});
			var bookingLine1 = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");

			var booking2 = builder.BuildSupplierBooking("JSB002", "PLC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_LoadMode)] = loadMode
			});
			var bookingLine2 = builder.BuildSupplierBookingLine("JSL002", "JSB002", null, "JO001");

			if (loadMode == Core.Constants.SupplierBookingLoadMode.ContainerYard)
			{
				builder.BuildContainer("JC001", consol1.JK_UniqueConsignRef, "JSB001", null);
				builder.BuildContainer("JC002", consol2.JK_UniqueConsignRef, "JSB002", null);

				var loadList1 = builder.BuildContainerLoadList("CLH001", "JSB001", status: "SHP");
				var loadListLine1 = builder.BuildContainerLoadListLine("CLL001", "CLH001", "JSL001", "JC001");
				loadList1.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
				Factory.Save();

				new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadList1.PK, Factory);
				Factory.Save();

				loadList1.Reload();
				AssertStatus(loadList1, ContainerLoadListHeaderStatus.Converted);
				AssertEquals(true, consol1.Shipments.OfType<ForwardingShipment>().Any(shipment => shipment.OuterPackLines.All(line => line.PK == loadListLine1.CLL_JL_PackLine)));

				var loadList2 = builder.BuildContainerLoadList("CLH002", "JSB002", status: "SHP");
				var loadListLine2 = builder.BuildContainerLoadListLine("CLL002", "CLH002", "JSL002", "JC002");
				loadList2.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
				Factory.Save();

				new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadList2.PK, Factory);
				Factory.Save();

				loadList2.Reload();
				AssertStatus(loadList2, ContainerLoadListHeaderStatus.Converted);
				AssertEquals(true, consol2.Shipments.OfType<ForwardingShipment>().Any(shipment => shipment.OuterPackLines.All(line => line.PK == loadListLine2.CLL_JL_PackLine)));
			}
			else
			{
				var loadPlan1 = builder.BuildContainerLoadPlan("CLH001", "JSB001", status: "SHP");
				builder.BuildContainer("JC001", consol1.JK_UniqueConsignRef, null, "CLH001");
				var loadPlanLine1 = builder.BuildContainerLoadListLine("CLL001", "CLH001", "JSL001", "JC001");
				loadPlan1.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
				Factory.Save();

				new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadPlan1.PK, Factory);
				Factory.Save();

				loadPlan1.Reload();
				AssertEquals(true, consol1.Shipments.OfType<ForwardingShipment>().Any(shipment => shipment.OuterPackLines.All(line => line.PK == loadPlanLine1.CLL_JL_PackLine)));

				var loadPlan2 = builder.BuildContainerLoadPlan("CLH002", "JSB001", status: "SHP");
				builder.BuildContainer("JC002", consol2.JK_UniqueConsignRef, null, "CLH002");
				var loadPlanLine2 = builder.BuildContainerLoadListLine("CLL002", "CLH002", "JSL002", null);
				var loadPlanLine3 = builder.BuildContainerLoadListLine("CLL003", "CLH002", "JSL002", "JC002");
				loadPlan2.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
				Factory.Save();

				new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadPlan2.PK, Factory);
				Factory.Save();

				loadPlan2.Reload();
				AssertStatus(loadPlan2, ContainerLoadListHeaderStatus.Converted);
				AssertEquals(true, consol2.Shipments.OfType<ForwardingShipment>().Any(shipment => shipment.OuterPackLines.All(line => line.PK == loadPlanLine3.CLL_JL_PackLine)));
			}

			if (shareConsol)
			{
				consol1.Shipments.Reload(false);
				AssertEquals("Precondition: two shipment exists on the consol1.", 2, consol1.Shipments.Count);
				AssertEquals("Precondition: one packline exists on the shipment1 of consol1.", 1, consol1.Shipments[0].OuterPackLines.Count);
				AssertEquals("Precondition: one packline exists on the shipment2 of consol1.", 1, consol1.Shipments[0].OuterPackLines.Count);
			}
			else
			{
				consol1.Shipments.Reload(false);
				AssertEquals("Precondition: one shipment exists on the consol1.", 1, consol1.Shipments.Count);
				AssertEquals("Precondition: one packline exists on the shipment1 of consol1.", 1, consol1.Shipments[0].OuterPackLines.Count);
				consol2.Shipments.Reload(false);
				AssertEquals("Precondition: one shipment exists on the consol2.", 1, consol2.Shipments.Count);
				AssertEquals("Precondition: one packline exists on the shipment1 of consol2.", 1, consol2.Shipments[0].OuterPackLines.Count);
			}
		}

		public void TestCFSContainerLoadListShipmentCreation_ShouldExcludeLinesWithoutContainer()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var orderLine = CreateOrderLineWithOrder();
				var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
				var (booking, bookingLine) = CreateSupplierBookingWithOneLine(SupplierBookingLoadMode.ContainerFreightStation, orderLine, bookingParty);
				booking.JSB_OA_CFSAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

				var loadListHeader = CreateContainerLoadPlan("CLL00001", booking.ControllingCustomerAddress, booking.CFSAddress);
				var consol1 = CreateConsol();
				var container1 = CreateAndAttachContainer(loadListHeader, consol1);
				var loadListLine = AddLoadListLine(loadListHeader, bookingLine, container1);
				loadListLine.CLL_JC_Container = Guid.Empty;
				loadListHeader.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");

				Factory.Save();

				consol1.Shipments.Reload(false);
				AssertEquals("Precondition: no shipment exists on the consol.", 0, consol1.Shipments.Count);

				RunLogWalkerCycleForTest();

				consol1.Shipments.Reload(false);
				AssertEquals("Should not create a shipment when there's no container attached", 0, consol1.Shipments.Count);
			}
		}

		public void TestCYContainerLoadPlanShipmentCreation_BasicMappings_MultipleConsol()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			var consol1 = builder.BuildConsol("JK001");
			var consol2 = builder.BuildConsol("JK002");

			var booking1 = builder.BuildSupplierBooking("JSB001", "PLC");
			var bookingLine1 = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");
			var bookingLine2 = builder.BuildSupplierBookingLine("JSL002", "JSB001", null, "JO001");

			builder.BuildContainer("JC001", consol1.JK_UniqueConsignRef, "JSB001", null);
			builder.BuildContainer("JC002", consol2.JK_UniqueConsignRef, "JSB001", null);

			var loadList = builder.BuildContainerLoadList("CLH001", "JSB001", status: "SHP");
			var loadListLine1 = builder.BuildContainerLoadListLine("CLL001", "CLH001", "JSL001", "JC001");
			var loadListLine2 = builder.BuildContainerLoadListLine("CLL002", "CLH001", "JSL002", "JC002");
			var loadListLine3 = builder.BuildContainerLoadListLine("CLL003", "CLH001", "JSL002", "JC001");
			loadList.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadList.PK, Factory);
			Factory.Save();

			consol1.Shipments.Reload(false);
			consol2.Shipments.Reload(false);
			loadListLine1.Reload();
			loadListLine2.Reload();
			loadListLine3.Reload();

			AssertStatus(loadList, ContainerLoadListHeaderStatus.Converted);

			AssertEquals(1, consol1.Shipments.Count);
			AssertEquals(1, consol2.Shipments.Count);
			AssertEquals(2, consol1.Shipments[0].OuterPackLines.Count);
			AssertEquals(true, consol1.Shipments[0].OuterPackLines.All(line => line.PK == loadListLine1.CLL_JL_PackLine || line.PK == loadListLine3.CLL_JL_PackLine));
			AssertEquals(1, consol2.Shipments[0].OuterPackLines.Count);
			AssertEquals(true, consol2.Shipments[0].OuterPackLines.All(line => line.PK == loadListLine2.CLL_JL_PackLine));
		}

		public void TestCFSContainerLoadPlanShipmentCreation_BasicMappings_MultipleConsol()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			var consol1 = builder.BuildConsol("JK001");
			var consol2 = builder.BuildConsol("JK002");

			var booking1 = builder.BuildSupplierBooking("JSB001", "PLC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_LoadMode)] = Core.Constants.SupplierBookingLoadMode.ContainerFreightStation
			});

			var bookingLine1 = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");
			var bookingLine2 = builder.BuildSupplierBookingLine("JSL002", "JSB001", null, "JO001");

			builder.BuildContainer("JC001", consol1.JK_UniqueConsignRef, "JSB001", null);
			builder.BuildContainer("JC002", consol2.JK_UniqueConsignRef, "JSB001", null);

			var loadPlan = builder.BuildContainerLoadPlan("CLH001", "JSB001", status: "SHP");
			builder.BuildContainerLoadListLine("CLL001", "CLH001", "JSL001", "");
			builder.BuildContainerLoadListLine("CLL002", "CLH001", "JSL002", "");
			var loadPlanLine3 = builder.BuildContainerLoadListLine("CLL003", "CLH001", "JSL001", "JC001");
			var loadPlanLine4 = builder.BuildContainerLoadListLine("CLL004", "CLH001", "JSL002", "JC002");
			var loadPlanLine5 = builder.BuildContainerLoadListLine("CLL005", "CLH001", "JSL002", "JC001");
			loadPlan.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadPlan.PK, Factory);
			Factory.Save();

			consol1.Shipments.Reload(false);
			consol2.Shipments.Reload(false);
			loadPlanLine3.Reload();
			loadPlanLine4.Reload();
			loadPlanLine5.Reload();

			AssertStatus(loadPlan, ContainerLoadListHeaderStatus.Converted);

			AssertEquals(1, consol1.Shipments.Count);
			AssertEquals(1, consol2.Shipments.Count);
			AssertEquals(2, consol1.Shipments[0].OuterPackLines.Count);
			AssertEquals(true, consol1.Shipments[0].OuterPackLines.All(line => line.PK == loadPlanLine3.CLL_JL_PackLine || line.PK == loadPlanLine5.CLL_JL_PackLine));
			AssertEquals(1, consol2.Shipments[0].OuterPackLines.Count);
			AssertEquals(true, consol2.Shipments[0].OuterPackLines.All(line => line.PK == loadPlanLine4.CLL_JL_PackLine));
		}

		public void TestCFSContainerLoadPlanShipmentCreation_BasicMappings_MultipleBooking()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			var consol1 = builder.BuildConsol("JK001");
			var consol2 = builder.BuildConsol("JK002");

			var booking1 = builder.BuildSupplierBooking("JSB001", "PLC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_LoadMode)] = Core.Constants.SupplierBookingLoadMode.ContainerFreightStation
			});

			var booking2 = builder.BuildSupplierBooking("JSB002", "PLC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_LoadMode)] = Core.Constants.SupplierBookingLoadMode.ContainerFreightStation
			});

			var bookingLine1 = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");
			var bookingLine2 = builder.BuildSupplierBookingLine("JSL002", "JSB002", null, "JO001");

			builder.BuildContainer("JC001", consol1.JK_UniqueConsignRef, "JSB001", null);
			builder.BuildContainer("JC002", consol2.JK_UniqueConsignRef, "JSB002", null);

			var loadPlan = builder.BuildContainerLoadPlan("CLH001", "JSB001", status: "SHP");
			builder.BuildContainerLoadListLine("CLL001", "CLH001", "JSL001", "");
			builder.BuildContainerLoadListLine("CLL002", "CLH001", "JSL002", "");
			var loadPlanLine3 = builder.BuildContainerLoadListLine("CLL003", "CLH001", "JSL001", "JC001");
			var loadPlanLine4 = builder.BuildContainerLoadListLine("CLL004", "CLH001", "JSL002", "JC002");
			var loadPlanLine5 = builder.BuildContainerLoadListLine("CLL005", "CLH001", "JSL002", "JC001");
			loadPlan.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");

			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadPlan.PK, Factory);
			Factory.Save();

			consol1.Shipments.Reload(false);
			consol2.Shipments.Reload(false);

			AssertStatus(loadPlan, ContainerLoadListHeaderStatus.Converted);

			AssertEquals(2, consol1.Shipments.Count);
			AssertEquals(1, consol2.Shipments.Count);
			var shipment3 = Factory.Load<ForwardingPackLine>(loadPlanLine3.CLL_JL_PackLine).Shipment;
			var shipment4 = Factory.Load<ForwardingPackLine>(loadPlanLine4.CLL_JL_PackLine).Shipment;
			var shipment5 = Factory.Load<ForwardingPackLine>(loadPlanLine5.CLL_JL_PackLine).Shipment;
			AssertEquals(true, consol1.Shipments.All(shipment => shipment.PK == shipment3.PK || shipment.PK == shipment5.PK));
			AssertEquals(shipment4.PK, consol2.Shipments.Single().PK);
			AssertEquals(1, shipment3.OuterPackLines.Count);
			AssertEquals(1, shipment5.OuterPackLines.Count);
			AssertEquals(1, shipment4.OuterPackLines.Count);
		}

		void AssertPopulatedShipmentFromCFSToExpected(ForwardingShipment shipment, JobSupplierBooking booking, Order order, CommonContainerLoadList loadListHeader, ForwardingConsol relatedConsol)
		{
			var controllingCustomer = loadListHeader.ControllingCustomerAddress;
			var orderLine = order.OrderLines?.FirstOrDefault();

			CombineAssertions("Packed Shipment should match expected values", () =>
			{
				if (relatedConsol != null)
				{
					AssertEquals("Transport Mode", relatedConsol.JK_TransportMode, shipment.JS_TransportMode);
					AssertEquals("Incoterm", booking.JSB_IncoTerm, shipment.JS_INCO);
					AssertEquals("Packing Mode", loadListHeader.CLH_PlannedTransportMode == TransportModes.Air ? ContainerModes.ULD : ContainerModes.FCL, shipment.JS_PackingMode);
					AssertEquals("ETA", relatedConsol.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETA, shipment.JS_E_ARV);
					AssertEquals("ETD", relatedConsol.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETD, shipment.JS_E_DEP);
					AssertEquals("Load Port", booking.JSB_RL_NKOrigin, shipment.JS_RL_NKOrigin);
					AssertEquals("Discharge Port", booking.JSB_RL_NKDestination, shipment.JS_RL_NKDestination);
					AssertEquals("Receiving Depot", loadListHeader.CFSAddress?.PK ?? Guid.Empty, shipment.JS_OA_ExportReceivingDepot);
					if (order.BuyerAddress != null && order.BuyerAddress.PK != (shipment.ConsigneeDocumentaryAddress?.E2_OA_Address ?? ZGuid.Empty))
					{
						AssertEquals("Buyer Organisation PK", order.BuyerAddress.OA_OH, shipment.BuyerDocAddress.OrganisationPK);
						AssertEquals("Buyer Contact PK", order.JD_OC_BuyerContact, shipment.BuyerDocAddress.ContactPK);
						AssertEquals("Buyer Address PK", order.JD_OA_BuyerAddress, shipment.BuyerDocAddress.E2_OA_Address);
					}

					if (!booking.ConsigneeDocumentaryAddress.E2_AddressOverride && booking.ConsigneeDocumentaryAddress.Address != null)
					{
						AssertEquals("Consignee Organisation PK", booking.ConsigneeDocumentaryAddress.OrganisationPK, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
						AssertEquals("Consignee Contact PK", booking.ConsigneeDocumentaryAddress.ContactPK, shipment.ConsigneeDocumentaryAddress.ContactPK);
						AssertEquals("Consignee Address PK", booking.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

						AssertEquals("Consignee Delivery Organisation PK", booking.ConsigneeDocumentaryAddress.OrganisationPK, shipment.ConsigneeDeliveryAddress.OrganisationPK);
						AssertEquals("Consignee Delivery Contact PK", booking.ConsigneeDocumentaryAddress.ContactPK, shipment.ConsigneeDeliveryAddress.ContactPK);
						AssertEquals("Consignee Delivery Address PK", booking.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
					}
					else if (order.BuyerAddress != null)
					{
						AssertEquals("Consignee Organisation PK", order.Buyer.PK, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
						AssertEquals("Consignee Contact PK", order.JD_OC_BuyerContact, shipment.ConsigneeDocumentaryAddress.ContactPK);
						AssertEquals("Consignee Address PK", order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

						AssertEquals("Consignee Delivery Organisation PK", order.BuyerAddress.OA_OH, shipment.ConsigneeDeliveryAddress.OrganisationPK);
						AssertEquals("Consignee Delivery Contact PK", order.JD_OC_BuyerContact, shipment.ConsigneeDeliveryAddress.ContactPK);
						AssertEquals("Consignee Delivery Address PK", order.JD_OA_BuyerAddress, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
					}
				}
				else
				{
					AssertEquals("Transport Mode", loadListHeader.CLH_PlannedTransportMode, shipment.JS_TransportMode);
					AssertEquals("Incoterm", ZString.Empty, shipment.JS_INCO);
					AssertEquals("Packing Mode", loadListHeader.CLH_PlannedTransportMode == TransportModes.Air ? ContainerModes.ULD : ContainerModes.FCL, shipment.JS_PackingMode);
					AssertEquals("ETA", ZDateTime.Empty, shipment.JS_E_ARV);
					AssertEquals("ETD", ZDateTime.Empty, shipment.JS_E_DEP);
					AssertEquals("Load Port", loadListHeader.CLH_RL_NKPlannedLoadPort, shipment.JS_RL_NKOrigin);
					AssertEquals("Discharge Port", loadListHeader.CLH_RL_NKPlannedDischargePort, shipment.JS_RL_NKDestination);

					AssertEquals("Consignee Organisation PK", loadListHeader.ControllingCustomerAddress.OrganisationPK, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
					AssertEquals("Consignee Contact PK", loadListHeader.ControllingCustomerAddress.ContactPK, shipment.ConsigneeDocumentaryAddress.ContactPK);
					AssertEquals("Consignee Address PK", loadListHeader.ControllingCustomerAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

					AssertEquals("Consignee Delivery Organisation PK", loadListHeader.ControllingCustomerAddress.OrganisationPK, shipment.ConsigneeDeliveryAddress.OrganisationPK);
					AssertEquals("Consignee Delivery Contact PK", loadListHeader.ControllingCustomerAddress.ContactPK, shipment.ConsigneeDeliveryAddress.ContactPK);
					AssertEquals("Consignee Delivery Address PK", loadListHeader.ControllingCustomerAddress.E2_OA_Address, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
				}

				AssertEquals("Supplier Organisation PK", booking?.SupplierAddress?.OrganisationPK, shipment.ConsignorDocumentaryAddress.OrganisationPK);
				AssertEquals("Supplier Contact PK", booking?.SupplierAddress?.ContactPK, shipment.ConsignorDocumentaryAddress.ContactPK);
				AssertEquals("Supplier Address PK", booking?.SupplierAddress?.E2_OA_Address, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
			});

			OrderManagerTestHelper.AssertAddressEquals(controllingCustomer, shipment.ControllingCustomerAddress);
		}

		public void TestCYContainerLoadListShipmentCreation_ContinuousLoadSequence_ShouldCreateContinousPacklines()
		{
			TestContainerLoadListShipmentCreation_ContinuousLoadSequence(Core.Constants.ContainerLoadListHeaderLoadMode.ContainerYard, 1, 2, 3, 4);
		}

		public void TestCYContainerLoadListShipmentCreation_DiscontinuousLoadSequence_ShouldCreateContinousPacklines()
		{
			TestContainerLoadListShipmentCreation_ContinuousLoadSequence(Core.Constants.ContainerLoadListHeaderLoadMode.ContainerYard, 5, 7, 9, 13);
		}

		public void TestCYContainerLoadListShipmentCreation_IrregularLoadSequence_ShouldCreateContinousPacklines()
		{
			TestContainerLoadListShipmentCreation_ContinuousLoadSequence(Core.Constants.ContainerLoadListHeaderLoadMode.ContainerYard, 5, 7, 4, 3);
		}

		public void TestCFSContainerLoadListShipmentCreation_ContinuousLoadSequence_ShouldCreateContinousPacklines()
		{
			TestContainerLoadListShipmentCreation_ContinuousLoadSequence(Core.Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation, 1, 2, 3, 4);
		}

		public void TestCFSContainerLoadListShipmentCreation_DiscontinuousLoadSequence_ShouldCreateContinousPacklines()
		{
			TestContainerLoadListShipmentCreation_ContinuousLoadSequence(Core.Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation, 5, 7, 9, 13);
		}

		public void TestCFSContainerLoadListShipmentCreation_IrregularLoadSequence_ShouldCreateContinousPacklines()
		{
			TestContainerLoadListShipmentCreation_ContinuousLoadSequence(Core.Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation, 5, 7, 4, 3);
		}

		void TestContainerLoadListShipmentCreation_ContinuousLoadSequence(string loadMode, params int[] sequenceArray)
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			var booking = builder.BuildSupplierBooking("JSB001", "PLC");
			var bookingLine = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001", new Dictionary<string, string>
			{
				[nameof(JobSupplierBookingLine.JSL_BookedQuantity)] = "12",
				[nameof(JobSupplierBookingLine.JSL_BookedPackages)] = "12",
			});
			var consol = builder.BuildConsol("JK001");
			builder.BuildContainer("JC001", "JK001", "JSB001", null);
			builder.BuildContainer("JC002", "JK001", "JSB001", null);

			if (loadMode == Core.Constants.ContainerLoadListHeaderLoadMode.ContainerYard)
			{
				var loadListHeader = builder.BuildContainerLoadList("CLH001", "JSB001", status: "SHP");
				var random = new Random();
				for (var index = 0; index < sequenceArray.Length; index++)
				{
					builder.BuildContainerLoadListLine("CLL00" + index, "CLH001", "JSL001", (index % 2 == 0) ? "JC001" : "JC002", new Dictionary<string, string>
					{
						[nameof(ContainerLoadListLine.CLL_PackedQuantity)] = random.Next(1000).ToString(),
						[nameof(ContainerLoadListLine.CLL_LoadSequence)] = sequenceArray[index].ToString(),
					});
				}
				loadListHeader.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
				Factory.Save();

				AssertContinuousContainerPackingOrder(consol, new[] { loadListHeader });
			}
			else
			{
				var loadListHeader = builder.BuildContainerLoadPlan("CLH001", "JSB001", status: "SHP");
				var random = new Random();
				for (var index = 0; index < sequenceArray.Length; index++)
				{
					var quantity = random.Next(1000);
					builder.BuildContainerLoadListLine("CLL00" + index, "CLH001", "JSL001", null, new Dictionary<string, string>
					{
						[nameof(ContainerLoadListLine.CLL_PackedQuantity)] = quantity.ToString(),
						[nameof(ContainerLoadListLine.CLL_LoadSequence)] = sequenceArray[index].ToString(),
					});
					builder.BuildContainerLoadListLine("CLL01" + index, "CLH001", "JSL001", (index % 2 == 0) ? "JC001" : "JC002", new Dictionary<string, string>
					{
						[nameof(ContainerLoadListLine.CLL_PackedQuantity)] = quantity.ToString(),
						[nameof(ContainerLoadListLine.CLL_LoadSequence)] = sequenceArray[index].ToString(),
					});
				}
				loadListHeader.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
				Factory.Save();

				AssertContinuousContainerPackingOrder(consol, new[] { loadListHeader });
			}
		}

		public void AssertContinuousContainerPackingOrder(ForwardingConsol consol1, CommonContainerLoadList[] loadLists, int shipmentCount = 1)
		{
			Factory.Save();

			consol1.Shipments.Reload(false);
			AssertEquals("Precondition: no shipment exists on the consol.", 0, consol1.Shipments.Count);

			RunLogWalkerCycleForTest();

			consol1.Shipments.Reload(false);
			AssertEquals("Shipment has been created", shipmentCount, consol1.Shipments.Count);

			consol1.Shipments.Reload(false);
			foreach (var shipment in consol1.Shipments)
			{
				shipment.Reload();
			}

			var packLines = consol1.Shipments.OfType<ForwardingShipment>().SelectMany(shipment => shipment.OuterPackLines.OfType<PackLine>());

			foreach (var container in consol1.Containers.OfType<ForwardingContainer>())
			{
				var expectedPackLines = loadLists
					.SelectMany(x => x.LoadListLines.Where(cll => !cll.CLL_JC_Container.IsEmpty && cll.Container.PK == container.PK))
					.Count();

				var containerPackLines = packLines.Where(pl => pl.JL_Calc_ContainerNumber == container.JC_ContainerNum).ToList();

				AssertEquals("Expected number of packlines", expectedPackLines, containerPackLines.Count);

				for (int i = 0; i < containerPackLines.Count; i++)
				{
					AssertEquals("Container Packing Order", i + 1, containerPackLines[i].JL_ContainerPackingOrder);
				}
			}
		}

		public void TestContainerLoadListShipmentCreation_ForceGeneratesHouseBill()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			var booking = builder.BuildSupplierBooking("JSB001", "PLC");
			var bookingLine = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");
			var consol = builder.BuildConsol("JK001");
			builder.BuildContainer("JC001", "JK001", "JSB001", null);
			var loadListHeader = builder.BuildContainerLoadList("CLH001", "JSB001", status: "SHP");
			builder.BuildContainerLoadListLine("CLL001", "CLH001", "JSL001", "JC001");
			loadListHeader.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListHeader.PK, Factory);
			Factory.Save();

			consol.Shipments.Reload(true);
			AssertNotNull(consol.Shipments.Single());
			AssertNotNullOrEmpty(consol.Shipments[0].JS_HouseBill);
		}

		#region Test Planned Shipment Validation

		public void TestContainerLoadListShipmentCreation_ShipmentPlanningValidation()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var helper = new PlannedShipmentValidationTestHelper(builder);

			CommonContainerLoadList BuildLoadListWithPlannedShipment(string key, string loadMode, bool linkedCorrectly)
			{
				helper.BuildCYContainerLoadListWithPlannedShipment(
					supplierBookingKey: $"JSB_{key}",
					key,
					containerLoadListLineKey: $"CLL_{key}",
					plannedShipmentKey: $"JS_{key}",
					plannedConsolKey: linkedCorrectly ? $"JK_{key}" : null,
					allocatedConsolKey: $"JK_{key}",
					allocatedContainerKey: $"JC_{key}");
				var loadList = builder.MatchExistedEntity<CYContainerLoadList>(key);
				loadList.CLH_LoadMode = loadMode;
				return loadList;
			}

			var unlinkedContainerLoadList =
				BuildLoadListWithPlannedShipment("CLH_WRONG", ContainerLoadListHeaderLoadMode.ContainerYard, linkedCorrectly: false);

			var unlinkedContainerLoadPlan =
				BuildLoadListWithPlannedShipment("CLP_WRONG", ContainerLoadListHeaderLoadMode.ContainerFreightStation, linkedCorrectly: false);

			var correctlyLinkedContainerLoadList =
				BuildLoadListWithPlannedShipment("CLH_LINKED", ContainerLoadListHeaderLoadMode.ContainerYard, linkedCorrectly: true);

			var correctlyLinkedContainerLoadPlan =
				BuildLoadListWithPlannedShipment("CLP_LINKED", ContainerLoadListHeaderLoadMode.ContainerFreightStation, linkedCorrectly: true);

			unlinkedContainerLoadList.Logs.AddNew(AutoEvents.StatusUpdated, $"|ACT=NEW|NEW=SHP");
			unlinkedContainerLoadPlan.Logs.AddNew(AutoEvents.StatusUpdated, $"|ACT=NEW|NEW=SHP");
			correctlyLinkedContainerLoadList.Logs.AddNew(AutoEvents.StatusUpdated, $"|ACT=NEW|NEW=SHP");
			correctlyLinkedContainerLoadPlan.Logs.AddNew(AutoEvents.StatusUpdated, $"|ACT=NEW|NEW=SHP");

			Factory.Save();

			RunLogWalkerCycleForTest();

			unlinkedContainerLoadList.Reload();
			correctlyLinkedContainerLoadList.Reload();
			unlinkedContainerLoadPlan.Reload();
			correctlyLinkedContainerLoadPlan.Reload();

			AssertEquals(1, unlinkedContainerLoadList.Logs.GetAllLogs().OfType<StmALog>().Count(
				   x => x.SL_Reference == "|RES=The shipments were detached from the container's consol before the pack lines could be created"
				   && x.SL_SE_NKEvent == Events.ExceptionRaisedCode));
			AssertEquals("Status should be reverted on failed validation", ContainerLoadListHeaderStatus.Placed, unlinkedContainerLoadList.CLH_Status);

			AssertEquals(1, unlinkedContainerLoadPlan.Logs.GetAllLogs().OfType<StmALog>().Count(
				   x => x.SL_Reference == "|RES=The shipments were detached from the container's consol before the pack lines could be created"
				   && x.SL_SE_NKEvent == Events.ExceptionRaisedCode));
			AssertEquals("Status should be reverted on failed validation", ContainerLoadListHeaderStatus.Planned, unlinkedContainerLoadPlan.CLH_Status);

			AssertEquals("Should proceed with conversion on successful validation", ContainerLoadListHeaderStatus.Converted, correctlyLinkedContainerLoadList.CLH_Status);
			AssertEquals("Should proceed with conversion on successful validation", ContainerLoadListHeaderStatus.Converted, correctlyLinkedContainerLoadPlan.CLH_Status);
		}

		#endregion

		public void TestUpdateShipment()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			var booking = builder.BuildSupplierBooking("JSB001", "PLC");
			var bookingLine = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");
			var consol = builder.BuildConsol("JK001");
			builder.BuildContainer("JC001", "JK001", "JSB001", null);
			var loadListHeader = builder.BuildContainerLoadList("CLH001", "JSB001", status: "SHP");
			builder.BuildContainerLoadListLine("CLL001", "CLH001", "JSL001", "JC001", new Dictionary<string, string>
			{
				[nameof(ContainerLoadListLine.CLL_PackedQuantity)] = "2.1",
				[nameof(ContainerLoadListLine.CLL_Weight)] = "3.1",
				[nameof(ContainerLoadListLine.CLL_Volume)] = "4.1",
				[nameof(ContainerLoadListLine.CLL_Packages)] = "5",
				[nameof(ContainerLoadListLine.CLL_VolumeUnit)] = Core.Constants.Volume.CubicFeet,
				[nameof(ContainerLoadListLine.CLL_WeightUnit)] = Core.Constants.Weight.LongTons,
				[nameof(ContainerLoadListLine.CLL_F3_NKPackagesUnit)] = Core.Constants.PkgUnit.Case,
			});

			builder.BuildContainerLoadListLine("CLL002", "CLH001", "JSL001", "JC001", new Dictionary<string, string>
			{
				[nameof(ContainerLoadListLine.CLL_PackedQuantity)] = "12.1",
				[nameof(ContainerLoadListLine.CLL_Weight)] = "13.1",
				[nameof(ContainerLoadListLine.CLL_Volume)] = "14.1",
				[nameof(ContainerLoadListLine.CLL_Packages)] = "15",
				[nameof(ContainerLoadListLine.CLL_VolumeUnit)] = Core.Constants.Volume.CubicYards,
				[nameof(ContainerLoadListLine.CLL_WeightUnit)] = Core.Constants.Weight.Tonnes,
				[nameof(ContainerLoadListLine.CLL_F3_NKPackagesUnit)] = Core.Constants.PkgUnit.Coil,
			});

			loadListHeader.Logs.AddNew(AutoEvents.StatusUpdated, "|NEW=SHP");
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListHeader.PK, Factory);
			Factory.Save();

			consol.Shipments.Reload(true);
			CombineAssertions(() =>
			{
				var shipment = consol.Shipments.Single() as ForwardingShipment;

				AssertEquals(Core.Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
				AssertEquals(Core.Constants.Weight.Kilograms, shipment.JS_UnitOfWeight);
				AssertEquals(Core.Constants.PkgUnit.Package, shipment.JS_F3_NKPackType);

				AssertEquals(20, shipment.JS_OuterPacks);
				AssertEquals(16249.745m, shipment.JS_ActualWeight);
				AssertEquals(10.896m, shipment.JS_ActualVolume);
			});
		}

		#region Implementation

		void PrepareTestDataForPackLine(OrderLine orderLine, ContainerLoadListLine loadListLine, ZDecimal packedQuantity)
		{
			var random = new Random();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "TestProductNum";
			orderLine.JO_Partno = product.OP_PartNum;
			orderLine.JO_Description = "Order Line Description Test";
			orderLine.JO_AdditionalInformation = "Order Line Additional Information Test";

			loadListLine.CLL_Volume = 2.1 + random.Next(100);
			loadListLine.CLL_VolumeUnit = Volume.CubicMetres;
			loadListLine.CLL_Weight = 4.3 + random.Next(100);
			loadListLine.CLL_WeightUnit = Weight.Kilograms;
			loadListLine.CLL_LoadSequence = 5 + random.Next(10);
			loadListLine.CLL_Packages = 3 + random.Next(10);
			loadListLine.CLL_F3_NKPackagesUnit = PkgUnit.Package;
			loadListLine.CLL_RH_NKCommodityCode = "GEN";
			loadListLine.CLL_HarmonizedCode = "HAR";
			loadListLine.CLL_ReferenceNumber = "RN0001";
			loadListLine.CLL_PackedQuantity = packedQuantity;
			loadListLine.CLL_Description = "CLL line description";
			loadListLine.CLL_MarksAndNumbers = "CLL line marks and numbers";
		}

		static void AssertPackedPackLine(ForwardingPackLineCollection packLines, int packIdx, ContainerLoadListLine loadListLine, OrderLine orderLine, ForwardingContainer container)
		{
			var factory = new BusinessObjectFactory();
			var packLine = factory.Load<ForwardingPackLine>(packLines[packIdx].PK);
			packLine.Validation.ValidateJL_ContainerPackingOrder();

			AssertEquals("Packed Pack Line should pull values from container", container.PK, packLine.JL_JC);

			CombineAssertions("Packed Pack Line should pull values from load list line", () =>
			{
				AssertEquals("Actual Volume", loadListLine.CLL_Volume, packLine.JL_ActualVolume);
				AssertEquals("Actual Weight", loadListLine.CLL_Weight, packLine.JL_ActualWeight);
				AssertEquals("Volume Unit", loadListLine.CLL_VolumeUnit, packLine.JL_ActualVolumeUQ);
				AssertEquals("Weight Unit", loadListLine.CLL_WeightUnit, packLine.JL_ActualWeightUQ);
				AssertNoErrors("Container Packing Order", packLine.JL_ContainerPackingOrderInfo);
				AssertEquals("Package Count", loadListLine.CLL_Packages, packLine.JL_PackageCount);
				AssertEquals("Pack Type", loadListLine.CLL_F3_NKPackagesUnit, packLine.JL_F3_NKPackType);
				AssertEquals("Commodity Code", loadListLine.CLL_RH_NKCommodityCode, packLine.JL_RH_NKCommodityCode);
				AssertEquals("Harmonised Code", loadListLine.CLL_HarmonizedCode, packLine.JL_HarmonisedCode);
				AssertEquals("Reference Number", loadListLine.CLL_ReferenceNumber, packLine.JL_RefNumber);
				AssertEquals("Marks and Numbers", loadListLine.CLL_MarksAndNumbers, packLine.JL_MarksAndNumbers);
				AssertEquals("Packed Quantity", loadListLine.CLL_PackedQuantity, packLine.Products[0].D2_ProductQuantity);
				AssertEquals("Description", loadListLine.CLL_Description, packLine.JL_Description);
			});

			CombineAssertions("Packed Pack Line should pull values from order line", () =>
			{
				AssertEquals("Product Order Line", orderLine.PK, packLine.Products[0].D2_JO);
				AssertEquals("Unit Of Packed Quantity", orderLine.JO_F3_NKPackType, packLine.Products[0].D2_ProductUnitOfQty);
				AssertEquals("Line Price", orderLine.JO_ItemPrice * loadListLine.CLL_PackedQuantity, packLine.JL_LinePrice);
				AssertEquals("Detailed Description", orderLine.JO_AdditionalInformation, packLine.JL_DetailedDescription);
			});
		}

		CommonContainerLoadList CreateContainerLoadList(string loadListID, JobSupplierBooking booking, OrgHeader bookingParty, string status = ContainerLoadListHeaderStatus.Shipped, string loadMode = SupplierBookingLoadMode.ContainerYard)
		{
			CommonContainerLoadList containerLoadList =
				loadMode == SupplierBookingLoadMode.ContainerYard
					? Factory.NewWithValidTestData<CYContainerLoadList>().With(cy => cy.CLH_JSB_Booking = booking.PK)
					: Factory.NewWithValidTestData<CFSContainerLoadList>();

			containerLoadList.CLH_LoadListId = loadListID;
			containerLoadList.CLH_OH_LoadListParty = bookingParty.PK;
			containerLoadList.CLH_Status = status;
			containerLoadList.ControllingCustomerAddress.OrganisationPK = booking.ControllingCustomerAddress.OrganisationPK;
			containerLoadList.CLH_OA_CFSAddress = booking.JSB_OA_CFSAddress;
			containerLoadList.CLH_MarksAndNumbers = "header marks & numbers";
			containerLoadList.CLH_GoodsDescription = "header goods description";
			containerLoadList.CLH_DetailedGoodsDescription = "header detailed goods description";

			return containerLoadList;
		}

		CFSContainerLoadList CreateContainerLoadPlan(string loadListID, JobDocAddress controllingCustomer, OrgAddress cfsAddress, string status = ContainerLoadListHeaderStatus.Shipped)
		{
			var containerLoadList = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadList.CLH_LoadListId = loadListID;
			containerLoadList.CLH_Status = status;
			containerLoadList.ControllingCustomerAddress.OrganisationPK = controllingCustomer.OrganisationPK;
			containerLoadList.ControllingCustomerAddress.ContactPK = controllingCustomer.ContactPK;
			containerLoadList.CLH_OA_CFSAddress = cfsAddress.PK;

			return containerLoadList;
		}

		ContainerLoadListLine AddLoadListLine(
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
			loadListLine.CLL_JC_Container = container?.PK ?? ZGuid.Empty;
			loadListLine.CLL_Volume = volume;
			loadListLine.CLL_VolumeUnit = volumeUnit;
			loadListLine.CLL_Weight = weight;
			loadListLine.CLL_WeightUnit = weightUnit;
			loadListLine.CLL_LoadSequence = sequence;
			loadListLine.CLL_Packages = packages;
			loadListLine.CLL_F3_NKPackagesUnit = packagesUnit;
			loadListLine.CLL_PackedQuantity = quantity;
			loadListLine.CLL_LoadMode = containerLoadList.CLH_LoadMode;

			return loadListLine;
		}

		void AssertStatus(BusinessObject businessObject, string status, string expectedBookingStatusBeforeConvert = Core.Constants.SupplierBookingStatus.Placed)
		{
			businessObject.Reload();

			if (businessObject is JobSupplierBooking supplierBooking)
			{
				AssertEquals($"The status of supplier booking should be {status}", status, supplierBooking.JSB_Status);
				if (status == SupplierBookingStatus.Converted)
				{
					var log = ((JobSupplierBooking)businessObject).Logs.GetAllLogs().OfType<StmALog>().Where(x => x.SL_SE_NKEvent == "STU");
					var logCountOfNewAction = log.Count(x => x.SL_Reference == $"|TYP=SBK|OLD={expectedBookingStatusBeforeConvert}|NEW=CNV|ACT={OrdersConstants.ConvertToShipmentPackLinesUserAction.New}");
					var logCountOfExistingAction = log.Count(x => x.SL_Reference == $"|TYP=SBK|OLD={expectedBookingStatusBeforeConvert}|NEW=CNV|ACT={OrdersConstants.ConvertToShipmentPackLinesUserAction.Existing}");
					AssertEquals("Booking should have STU event in the log", true, logCountOfNewAction == 1 || logCountOfExistingAction == 1);
				}
			}
			else if (businessObject is CommonContainerLoadList containerLoadList)
			{
				AssertEquals($"The status of container load list should be {status}", status, containerLoadList.CLH_Status);
			}
		}

		(JobSupplierBooking, JobSupplierBookingLine) CreateSupplierBookingWithOneLine(string loadMode, OrderLine orderLine, OrgHeader bookingParty, decimal bookedQuantity = 12, string transportMode = TransportModes.Sea, string bookingId = "JSB00001", bool hasConsigneeDocumentaryAddress = false)
		{
			var booking = OrderManagerTestHelper.CreateSupplierBooking(Factory, bookingId, bookingParty, orderLine.Order.Supplier);
			booking.JSB_TransportMode = transportMode;
			booking.JSB_LoadMode = loadMode;
			booking.ControllingCustomerAddress.OrganisationPK = orderLine.Order.ControllingCustomerDocAddress.OrganisationPK;
			booking.ControllingCustomerAddress.ContactPK = orderLine.Order.ControllingCustomerDocAddress.ContactPK;

			if (hasConsigneeDocumentaryAddress)
			{
				booking.ConsigneeDocumentaryAddress.OrganisationPK = orderLine.Order.ConsigneeDocumentaryAddress.OrganisationPK;
				booking.ConsigneeDocumentaryAddress.ContactPK = orderLine.Order.ConsigneeDocumentaryAddress.ContactPK;
			}

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_BookedQuantity = bookedQuantity;
			bookingLine.JSL_MarksAndNumbers = "123";
			bookingLine.JSL_BookingLineId = bookingId + "-1";

			if (loadMode == SupplierBookingLoadMode.ContainerFreightStation)
			{
				booking.JSB_OA_CFSAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			}

			return (booking, bookingLine);
		}

		OrderLine CreateOrderLineWithOrder()
		{
			var order = CreateOrder();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_ItemPrice = 2;
			return orderLine;
		}

		Order CreateOrder(string transportMode = TransportModes.Sea, OrgHeader supplier = null, OrgHeader buyer = null) => OrderHelpers.CreateOrder(Factory, transportMode, supplier, buyer);

		static ForwardingContainer CreateAndAttachContainer(JobSupplierBooking booking, ForwardingConsol consol)
		{
			var container1 = consol.Containers.AddNew();
			container1.FillWithValidTestData();
			container1.JC_JSB_SupplierBooking = booking.PK;
			return container1;
		}

		static ForwardingContainer CreateAndAttachContainer(CommonContainerLoadList containerLoadPlan, ForwardingConsol consol)
		{
			var container1 = CreateContainer(consol);
			container1.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			return container1;
		}

		static ForwardingContainer CreateContainer(ForwardingConsol consol)
		{
			var container1 = consol.Containers.AddNew();
			container1.FillWithValidTestData();
			return container1;
		}

		ForwardingConsol CreateConsol(string transportMode = TransportModes.Sea, string consolId = "")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports[0];
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			if (!consolId.IsNullOrEmpty())
			{
				consol.JK_UniqueConsignRef = consolId;
			}

			Factory.Save();

			return consol;
		}

		readonly ZDateTime etd = ZDateTime.Today;
		readonly ZDateTime eta = ZDateTime.Today.AddDays(2);
		static string GetNextId() => OrderHelpers.GetNextId();

		#endregion
	}
}
