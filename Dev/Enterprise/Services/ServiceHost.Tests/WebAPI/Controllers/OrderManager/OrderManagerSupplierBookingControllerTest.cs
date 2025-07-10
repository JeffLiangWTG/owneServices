using System;
using System.Linq;
using System.Web.Http.Results;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Services.ServiceHost.Tests
{
	class OrderManagerSupplierBookingControllerTest : TestCaseWithFactory
	{
		public void TestCancelSupplierBooking_EmptySupplierBookingPK()
		{
			var args = GetCancelSupplierBookingArgs(Guid.Empty);
			var result = controller.CancelSupplierBooking(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Please provide valid Supplier Booking details.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestCancelSupplierBooking_EmptyPackLines()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var supplierBookingLine1 = CreateSupplierBookingLine(supplierBooking.PK);

			var args = GetCancelSupplierBookingArgs(supplierBooking.PK.ToGuid());
			var result = controller.CancelSupplierBooking(args);

			AssertType<OkResult>(result);
		}

		public void TestCancelSupplierBooking_DeletePackLines()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var supplierBookingLine1 = CreateSupplierBookingLine(supplierBooking.PK);
			var supplierBookingLine2 = CreateSupplierBookingLine(supplierBooking.PK);
			var shipment = CreateShipment("S00000001");
			var packLine1 = CreatePackLine(supplierBookingLine1.PK, shipment.PK);
			var packLine2 = CreatePackLine(supplierBookingLine2.PK, shipment.PK);

			var args = GetCancelSupplierBookingArgs(supplierBooking.PK.ToGuid());
			var result = controller.CancelSupplierBooking(args);

			AssertType<OkResult>(result);
			var currentPackLine1 = Factory.LoadTop1<PackLine>(new ZQuery(JobPackLinesSchema.PK, packLine1.PK));
			AssertNull(currentPackLine1);
			var currentPackLine2 = Factory.LoadTop1<PackLine>(new ZQuery(JobPackLinesSchema.PK, packLine2.PK));
			AssertNull(currentPackLine2);
		}

		public void TestCancelSupplierBooking_DeleteOrderShipmentPlanning()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var supplierBookingLine1 = CreateSupplierBookingLine(supplierBooking.PK);
			var supplierBookingLine2 = CreateSupplierBookingLine(supplierBooking.PK);
			var shipmentPlanning1 = CreateOrderShipmentPlanning(supplierBooking.PK);
			var shipmentPlanningLine1 = CreateOrderPlanningShipment(shipmentPlanning1.PK, supplierBookingLine1.PK);
			var shipmentPlanning2 = CreateOrderShipmentPlanning(supplierBooking.PK);
			var shipmentPlanningLine2 = CreateOrderPlanningShipment(shipmentPlanning2.PK, supplierBookingLine2.PK);

			var args = GetCancelSupplierBookingArgs(supplierBooking.PK.ToGuid());
			var result = controller.CancelSupplierBooking(args);

			AssertType<OkResult>(result);
			var currentShipmentPlanning1 = Factory.LoadTop1<OrderShipmentPlanning>(new ZQuery(OrderShipmentPlanningSchema.PK, shipmentPlanning1.PK));
			AssertNull(currentShipmentPlanning1);
			var currentShipmentPlanningLine1 = Factory.LoadTop1<OrderShipmentPlanningLine>(new ZQuery(OrderShipmentPlanningLineSchema.PK, shipmentPlanningLine1.PK));
			AssertNull(currentShipmentPlanningLine1);
			var currentShipmentPlanning2 = Factory.LoadTop1<OrderShipmentPlanning>(new ZQuery(OrderShipmentPlanningSchema.PK, shipmentPlanning2.PK));
			AssertNull(currentShipmentPlanning2);
			var currentShipmentPlanningLine2 = Factory.LoadTop1<OrderShipmentPlanningLine>(new ZQuery(OrderShipmentPlanningLineSchema.PK, shipmentPlanningLine2.PK));
			AssertNull(currentShipmentPlanningLine2);
		}

		public void TestCancelSupplierBooking_ConvertedLinesShouldNotBeDeleted()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var supplierBookingLine1 = CreateSupplierBookingLine(supplierBooking.PK);
			var supplierBookingLine2 = CreateSupplierBookingLine(supplierBooking.PK);
			var shipment = CreateShipment("S00000001");
			var packLine1 = CreatePackLine(supplierBookingLine1.PK, shipment.PK);
			var packLine2 = CreatePackLine(supplierBookingLine2.PK, shipment.PK);
			var containerLoadList = CreateContainerLoadList("CLL01", supplierBooking.PK, SupplierBookingLoadMode.ContainerYard);
			var containerLoadListLine = CreateContainerLoadListLine(containerLoadList.PK, supplierBookingLine2.PK, packLine2.PK);

			var args = GetCancelSupplierBookingArgs(supplierBooking.PK.ToGuid());
			var result = controller.CancelSupplierBooking(args);

			AssertType<OkResult>(result);
			var currentPackLine1 = Factory.LoadTop1<PackLine>(new ZQuery(JobPackLinesSchema.PK, packLine1.PK));
			AssertNull(currentPackLine1);
			var currentPackLine2 = Factory.LoadTop1<PackLine>(new ZQuery(JobPackLinesSchema.PK, packLine2.PK));
			AssertNotNull(currentPackLine2);
		}

		public void TestCancelSupplierBooking_ShouldAddLogToShipment()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var supplierBookingLine1 = CreateSupplierBookingLine(supplierBooking.PK);
			var supplierBookingLine2 = CreateSupplierBookingLine(supplierBooking.PK);
			var shipment = CreateShipment("S00000001");
			var packLine1 = CreatePackLine(supplierBookingLine1.PK, shipment.PK);
			var packLine2 = CreatePackLine(supplierBookingLine2.PK, shipment.PK);
			var reference1 = $"Deleted a record in the system pack line {packLine1.JL_PackLineId} with supplier booking line {supplierBookingLine1.JSL_BookingLineId} because supplier booking {supplierBooking.JSB_BookingId} was cancelled.|REF=Pack line {packLine1.JL_PackLineId} with supplier booking line {supplierBookingLine1.JSL_BookingLineId}|RES=Supplier booking {supplierBooking.JSB_BookingId} was cancelled";
			var reference2 = $"Deleted a record in the system pack line {packLine2.JL_PackLineId} with supplier booking line {supplierBookingLine2.JSL_BookingLineId} because supplier booking {supplierBooking.JSB_BookingId} was cancelled.|REF=Pack line {packLine2.JL_PackLineId} with supplier booking line {supplierBookingLine2.JSL_BookingLineId}|RES=Supplier booking {supplierBooking.JSB_BookingId} was cancelled";

			var args = GetCancelSupplierBookingArgs(supplierBooking.PK.ToGuid());
			var result = controller.CancelSupplierBooking(args);
			AssertType<OkResult>(result);

			ZQuery logsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeletedARecordInTheSystemCode);
			StmALog[] delLogs = shipment.Logs.Find(logsQuery);
			AssertEquals(true, delLogs.Any(log => log.SL_Reference == reference1));
			AssertEquals(true, delLogs.Any(log => log.SL_Reference == reference2));
		}

		#region Implementation

		CancelSupplierBookingArgs GetCancelSupplierBookingArgs(
			Guid supplierBookingPK)
		{
			return new CancelSupplierBookingArgs()
			{
				SupplierBookingPK = supplierBookingPK,
			};
		}

		JobSupplierBooking CreateJobSupplierBooking(string supplierBookingID)
		{
			var jobSupplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			jobSupplierBooking.JSB_BookingId = supplierBookingID;

			Factory.Save();

			return jobSupplierBooking;
		}

		JobSupplierBookingLine CreateSupplierBookingLine(ZGuid supplierBookingPK)
		{
			var jobSupplierBookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			jobSupplierBookingLine.JSL_JSB_Booking = supplierBookingPK;

			Factory.Save();

			return jobSupplierBookingLine;
		}

		PackLine CreatePackLine(ZGuid supplierBookingLinePK, ZGuid shipmentPK)
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			packLine.JL_JSL_BookingLine = supplierBookingLinePK;
			packLine.JL_JS = shipmentPK;

			Factory.Save();

			return packLine;
		}

		OrderShipmentPlanning CreateOrderShipmentPlanning(ZGuid supplierBookingPK)
		{
			var orderShipmentPlanning = Factory.NewWithValidTestData<OrderShipmentPlanning>();
			orderShipmentPlanning.OPS_JSB_Booking = supplierBookingPK;

			Factory.Save();

			return orderShipmentPlanning;
		}

		OrderShipmentPlanningLine CreateOrderPlanningShipment(ZGuid orderShipmentPlanningPK, ZGuid supplierBookingLinePK)
		{
			var orderShipmentPlanningLine = Factory.NewWithValidTestData<OrderShipmentPlanningLine>();
			orderShipmentPlanningLine.OPL_OPS_Planning = orderShipmentPlanningPK;
			orderShipmentPlanningLine.OPL_JSL_BookingLine = supplierBookingLinePK;

			Factory.Save();

			return orderShipmentPlanningLine;
		}

		CommonContainerLoadList CreateContainerLoadList(string containerLoadListID, ZGuid supplierBookingPK, string loadMode)
		{
			var containerLoadListHeader = Factory.NewWithValidTestData<CommonContainerLoadList>();
			containerLoadListHeader.CLH_LoadListId = containerLoadListID;
			containerLoadListHeader.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Incomplete;
			containerLoadListHeader.CLH_LoadMode = loadMode;
			containerLoadListHeader.CLH_JSB_Booking = supplierBookingPK;

			Factory.Save();

			return containerLoadListHeader;
		}

		ContainerLoadListLine CreateContainerLoadListLine(ZGuid containerLoadListPK, ZGuid bookingLinePK, ZGuid packLinePK)
		{
			var containerLoadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			containerLoadListLine.CLL_CLH_LoadListHeader = containerLoadListPK;
			containerLoadListLine.CLL_JSL_BookingLine = bookingLinePK;
			containerLoadListLine.CLL_JL_PackLine = packLinePK;

			Factory.Save();

			return containerLoadListLine;
		}

		CommonShipment CreateShipment(string shipmentRef)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = shipmentRef;

			Factory.Save();
			return shipment;
		}

		protected override void SetUp()
		{
			controller = new OrderManagerSupplierBookingController();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
		}

		OrderManagerSupplierBookingController controller;

		#endregion
	}
}
