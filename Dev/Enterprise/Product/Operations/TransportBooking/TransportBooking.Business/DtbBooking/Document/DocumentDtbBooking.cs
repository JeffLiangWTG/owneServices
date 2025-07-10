using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.TransportBookings.Business
{
	public class DocumentDtbBooking : NonPersistentBusinessObject
	{
		public DocumentDtbBooking(DtbBooking booking)
			: base()
		{
			this.booking = booking;
		}

		readonly DtbBooking booking;

		public static class Schema
		{
			public const string IncludeInDelivery = "IncludeInDelivery";
		}

		public DtbBooking Booking
		{
			get { return booking; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			includeInDelivery = true;
		}

		[ResourceStringData("DocumentDtbBooking|IncludeInDelivery", Caption = "Deliver?")]
		public ZBool IncludeInDelivery
		{
			get { return includeInDelivery; }
			set { SetNonPersistentPropertyValue(IncludeInDeliveryInfo, ref includeInDelivery, value); }
		}

		ZBool includeInDelivery;

		public ZPropertyInfo IncludeInDeliveryInfo
		{
			get { return GetZPropertyInfo(Schema.IncludeInDelivery); }
		}
	}
}
