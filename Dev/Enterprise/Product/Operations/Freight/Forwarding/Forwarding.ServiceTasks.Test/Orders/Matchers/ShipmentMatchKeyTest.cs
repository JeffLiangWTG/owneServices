using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Matchers
{
	class ShipmentMatchKeyTest : TestCaseWithFactory
	{
		public void TestBuildKey()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Road;
			order.JD_RL_NKPortOfLoading = "THKK";
			order.JD_RL_NKPortOfDischarge = "CNSZX";
			order.JD_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			order.JD_OA_BuyerAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			order.JD_OA_SupplierAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			order.ControllingCustomerDocAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var orderLine = order.OrderLines.AddNew();

			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_OH_BookingParty = bookingParty.PK;
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			supplierBooking.SupplierAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var supplierBookingLine = supplierBooking.SupplierBookingLines.AddNew();
			supplierBookingLine.JSL_JO_OrderLine = orderLine.PK;

			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList.CLH_RL_NKPlannedLoadPort = "AUSYD";
			containerLoadList.CLH_RL_NKPlannedDischargePort = "CNNPJ";
			containerLoadList.CLH_JSB_Booking = supplierBooking.PK;
			var containerLoadListLine = containerLoadList.LoadListLines.AddNew();
			containerLoadListLine.CLL_JSL_BookingLine = supplierBookingLine.PK;
			containerLoadListLine.CLL_JC_Container = container.PK;

			Factory.Save();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JSL_BookingLine = supplierBookingLine.PK;

			Factory.Save();

			var matcherKey = ShipmentMatchKey.BuildKey(containerLoadListLine);

			AssertEquals(Core.Constants.SupplierBookingLoadMode.ContainerYard, matcherKey.LoadMode);
			AssertEquals(consol.PK, matcherKey.PackedConsol.PK);
			AssertEquals(order.BuyerAddress.PK, matcherKey.ConsigneeAddress.PK);
			AssertEquals(supplierBooking.PK, matcherKey.SupplierBooking.PK);
			AssertEquals(shipment.PK, matcherKey.PlannedShipment.PK);

			supplierBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			matcherKey = ShipmentMatchKey.BuildKey(containerLoadListLine);
			AssertEquals("booking consignee documentary address take precedence on order consignee documentary address", supplierBooking.ConsigneeDocumentaryAddress.E2_OA_Address, matcherKey.ConsigneeAddress.PK);
		}

		public void TestBuildKey_PlannedShipment()
		{
			var loadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine.CLL_JSL_BookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>().PK;
			loadListLine.CLL_JC_Container = Factory.NewWithValidTestData<ForwardingContainer>().PK;

			Factory.Save();

			var matcherKey = ShipmentMatchKey.BuildKey(loadListLine);
			AssertNull("should match none while booking line has not been allocated", matcherKey.PlannedShipment);

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1_1 = shipment1.OuterPackLines.AddNew();
			packLine1_1.JL_JSL_BookingLine = loadListLine.CLL_JSL_BookingLine;
			packLine1_1.JL_SystemCreateTimeUtc = new ZDateTime(2024, 1, 1);
			Factory.Save();

			matcherKey = ShipmentMatchKey.BuildKey(loadListLine);
			AssertEquals("should match a planned shipment", packLine1_1.Shipment.PK, matcherKey.PlannedShipment.PK);

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine2_1 = shipment2.OuterPackLines.AddNew();
			loadListLine.CLL_JL_PackLine = packLine1_1.PK;
			packLine2_1.JL_JSL_BookingLine = loadListLine.CLL_JSL_BookingLine;
			packLine2_1.JL_SystemCreateTimeUtc = new ZDateTime(2023, 12, 31);
			Factory.Save();

			matcherKey = ShipmentMatchKey.BuildKey(loadListLine);
			AssertEquals("should match a planned shipment with earliest created planned packline", packLine2_1.Shipment.PK, matcherKey.PlannedShipment.PK);
		}
	}
}
