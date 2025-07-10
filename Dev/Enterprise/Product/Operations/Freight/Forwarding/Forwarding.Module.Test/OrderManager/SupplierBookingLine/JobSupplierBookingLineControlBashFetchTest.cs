using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	class JobSupplierBookingLineControlBashFetchTest : FilterControlBashFetchHintTest<JobSupplierBookingLine>
	{
		protected override SchemaPKColumn PkColumn => JobSupplierBookingLineSchema.PK;

		public void TestBashFetchForView_SupplierBooking_JSB_BookingId()
		{
			BashFetchForView("SupplierBooking.JSB_BookingId", 12);
		}

		public void TestBashFetchForView_OrderLine_Order_JD_OrderNumber()
		{
			BashFetchForView("OrderLine.Order.JD_OrderNumber", 24);
		}

		public void TestBashFetchForView_OrderLine_JO_LineNo()
		{
			BashFetchForView("OrderLine.JO_LineNo", 12);
		}

		public void TestBashFetchForView_OrderLine_JO_Partno()
		{
			BashFetchForView("OrderLine.JO_Partno", 12);
		}

		public void TestBashFetchForView_OrderLine_JO_LineReference()
		{
			BashFetchForView("OrderLine.JO_LineReference", 12);
		}

		public void TestBashFetchForView_SupplierBooking_JSB_GoodsDescription()
		{
			BashFetchForView("SupplierBooking.JSB_GoodsDescription", 12);
		}

		public void TestBashFetchForView_OrderLine_JO_LineSplitNumber()
		{
			BashFetchForView("OrderLine.JO_LineSplitNumber", 12);
		}

		public void TestBashFetchForView_OrderLine_JO_SubLineNo()
		{
			BashFetchForView("OrderLine.JO_SubLineNo", 12);
		}

		public void TestBashFetchForView_JSL_SystemCreateUser()
		{
			BashFetchForView("JSL_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_JSL_SystemCreateBranch()
		{
			BashFetchForView("JSL_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_JSL_SystemCreateDepartment()
		{
			BashFetchForView("JSL_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_JSL_SystemCreateTimeUtc()
		{
			BashFetchForView("JSL_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_JSL_SystemLastEditUser()
		{
			BashFetchForView("JSL_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_JSL_SystemLastEditTimeUtc()
		{
			BashFetchForView("JSL_SystemLastEditTimeUtc", 0);
		}

		protected override ZGuid[] CreateKeysForTest()
		{
			var factory = new BusinessObjectFactory();

			var result = Enumerable.Range(0, 12).Select(i => CreateTestBookingLine(factory, "JSL" + i)).Select(bookingLine => bookingLine.PK).ToArray();
			factory.Save();

			return result;
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new JobSupplierBookingLineCollection(Factory);
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new JobSupplierBookingLineCollection(Factory);
			var filterBusinessObject = new JobSupplierBookingLineFilterBusinessObject();
			return new JobSupplierBookingLineFilterControl(collection, filterBusinessObject);
		}

		JobSupplierBookingLine CreateTestBookingLine(BusinessObjectFactory factory, string bookingLineId)
		{
			var booking = factory.NewWithValidTestData<JobSupplierBooking>();
			var order = factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_BookingLineId = bookingLineId;

			return bookingLine;
		}
	}
}
