using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingFlattened : AutoDtbBookingFlattened
	{
		public DtbBookingFlattened()
		{
		}

		[EmailAddress]
		public override ZString DeliveryAddress_E2_Email
		{
			get
			{
				return base.DeliveryAddress_E2_Email;
			}

			set
			{
				base.DeliveryAddress_E2_Email = value;
			}
		}

		[EmailAddress]
		public override ZString PickupAddress_E2_Email
		{
			get
			{
				return base.PickupAddress_E2_Email;
			}

			set
			{
				base.PickupAddress_E2_Email = value;
			}
		}
	}
}
