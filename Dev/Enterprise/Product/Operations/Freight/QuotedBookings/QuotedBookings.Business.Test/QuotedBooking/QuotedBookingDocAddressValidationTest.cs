using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAttachOrderToShipmentWithNonMatchingControllingCustomer()
		{
			var origin = Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed;
			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed = false;

			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var order = Factory.New<Order>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";

			order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
			order.JD_JS = quotedBooking.Booking.PK;

			quotedBooking.Booking.ControllingCustomerAddress.RunPreSaveValidation();
			AssertHasError(quotedBooking.Booking.ControllingCustomerAddress.E2_OA_AddressInfo, "You do not have security rights to allow order and its booking have non-matching Controlling Customers.");

			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed = origin;
		}

		public void TestAttachOrderToShipmentHandlesWhsOrders_WithoutAddressOverride()
		{
			var origin = Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed;
			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = false;

			var order = (BusinessObject)Factory.New<IWhsOrder>();
			order.FillWithValidTestData();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";

			var controllingCustomerJobDocAddress = (order as IDocAddresses).DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			controllingCustomerJobDocAddress.E2_OA_Address = controllingCustomerAddress.PK;

			quotedBooking.Booking.GenericOrders.Add(order);

			AssertNoExceptionThrown(() =>
			{
				quotedBooking.Booking.ControllingCustomerAddress.RunPreSaveValidation();
			});

			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = origin;
		}

		public void TestAttachOrderToShipmentHandlesWhsOrders_WithAddressOverride()
		{
			var origin = Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed;
			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = false;

			var order = (BusinessObject)Factory.New<IWhsOrder>();
			order.FillWithValidTestData();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";

			var controllingCustomerJobDocAddress = (order as IDocAddresses).DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			controllingCustomerJobDocAddress.E2_AddressOverride = true;
			controllingCustomerJobDocAddress.E2_Address1 = "Some overridden address";

			quotedBooking.Booking.GenericOrders.Add(order);

			AssertNoExceptionThrown(() =>
			{
				quotedBooking.Booking.ControllingCustomerAddress.RunPreSaveValidation();
			});

			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = origin;
		}
	}
}
