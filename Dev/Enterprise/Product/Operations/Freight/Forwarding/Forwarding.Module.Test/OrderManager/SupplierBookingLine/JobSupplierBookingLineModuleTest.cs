using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobSupplierBookingLineModule))]
	class JobSupplierBookingLineModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.SupplierBookingLine;

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			JobSupplierBookingLine CreateBookingLine(string bookingLineId, string bookingStatus)
			{
				var order = collection.Factory.NewWithValidTestData<Order>();
				var orderLine = order.OrderLines.AddNew();

				var booking = collection.Factory.NewWithValidTestData<JobSupplierBooking>();
				booking.JSB_Status = bookingStatus;

				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.JSL_BookingLineId = bookingLineId;
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				return bookingLine;
			}

			CreateBookingLine("JSL001", SupplierBookingStatus.Approved);
			CreateBookingLine("JSL002", SupplierBookingStatus.Cancelled);
			CreateBookingLine("JSL003", SupplierBookingStatus.Converted);
			CreateBookingLine("JSL004", SupplierBookingStatus.Incomplete);
			CreateBookingLine("JSL005", SupplierBookingStatus.Placed);
			CreateBookingLine("JSL006", SupplierBookingStatus.Planned);
			CreateBookingLine("JSL007", SupplierBookingStatus.Received);
			CreateBookingLine("JSL008", SupplierBookingStatus.Rejected);
			CreateBookingLine("JSL009", SupplierBookingStatus.Shipped);

			collection.Factory.Save();

			base.AddTestObjects(collection);
		}
	}
}
