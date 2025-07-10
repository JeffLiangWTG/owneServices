using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class BookingConfirmationCollection : NonPersistentBusinessObjectCollection<BookingConfirmation>
	{
		public BookingConfirmationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BookingConfirmationCollection(CommonConsol consol)
			: base(consol.Factory)
		{
			this.consol = consol;

			if (consol is IAirlineTrackingEventProvider provider)
			{
				using (consol.SuspendSettingHasChanges())
				{
					AddRange(provider.LoadBookingConfirmations());
				}
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in BookingConfirmationCollection")]
		readonly CommonConsol consol;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BookingConfirmation();
		}

		protected override bool AllowNewCore => false;
	}
}
