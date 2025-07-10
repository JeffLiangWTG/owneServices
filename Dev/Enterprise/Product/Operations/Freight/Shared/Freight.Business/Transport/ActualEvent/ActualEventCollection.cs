using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class ActualEventCollection : NonPersistentBusinessObjectCollection<ActualEvent>
	{
		public ActualEventCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ActualEventCollection(CommonConsol consol)
			: base(consol.Factory)
		{
			this.consol = consol;

			if (consol is IAirlineTrackingEventProvider provider)
			{
				using (consol.SuspendSettingHasChanges())
				{
					AddRange(provider.LoadActualEvents());
				}
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in ActualEventCollection")]
		readonly CommonConsol consol;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ActualEvent();
		}

		protected override bool AllowNewCore => false;
	}
}
