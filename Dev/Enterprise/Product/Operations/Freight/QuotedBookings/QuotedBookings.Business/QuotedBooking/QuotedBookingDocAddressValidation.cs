using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingDocAddressValidation : ShipmentDocAddressValidation
	{
		readonly ForwardingShipment booking;
		public QuotedBookingDocAddressValidation(JobDocAddress addressToValidate, ForwardingShipment booking) : base(addressToValidate, booking)
		{
			this.booking = booking;
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			if (Parent.E2_AddressType == DocAddressTypes.Codes.ControllingCustomer && !Parent.E2_AddressOverride && booking != null)
			{
				foreach (var order in booking.GenericOrders.OfType<Order>())
				{
					if (!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(booking, out var errorMessage))
					{
						Parent.E2_OA_AddressInfo.AddError(errorMessage);
					}

					break;
				}
			}
		}

		protected override void CheckE2_AddressOverride()
		{
			base.CheckE2_AddressOverride();

			if (Parent.E2_AddressType == DocAddressTypes.Codes.ControllingCustomer && Parent.E2_AddressOverride && booking != null)
			{
				foreach (var order in booking.GenericOrders.OfType<Order>())
				{
					if (!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(booking, out var errorMessage))
					{
						Parent.E2_AddressOverrideInfo.AddError(errorMessage);
					}

					break;
				}
			}
		}
	}
}
