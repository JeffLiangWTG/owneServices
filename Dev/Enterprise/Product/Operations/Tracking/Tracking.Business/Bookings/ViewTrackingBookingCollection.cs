using System;
using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class ViewTrackingBookingCollection : BusinessObjectCollection<ViewTrackingBooking>
	{
		public ViewTrackingBookingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Can't AddNew to this ViewTrackingBookingCollection");
		}
	}
}
