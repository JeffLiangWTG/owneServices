using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingScheduleChooser : ScheduleChooser
	{
		public TrackingScheduleChooser(ISailingChooserParent parent, TrackingSiteUser siteUser) : base(parent)
		{
			this.siteUser = siteUser;
		}

		protected override void SetNextSailing(ZGuid nextSailingPK)
		{
			if (CanBookSailings)
			{
				this.nextSailingPK = nextSailingPK;
			}
		}

		public bool HasNextSailing => nextSailingPK != null && !nextSailingPK.IsEmpty && nextSailingPK.IsValid;

		bool CanBookSailings => siteUser?.CanBookSailings ?? false;

		public void SetNextSailing()
		{
			if (HasNextSailing)
			{
				var nextSailingPK = this.nextSailingPK;
				ClearNextSailing();

				base.SetNextSailing(nextSailingPK);
			}
		}

		public void ClearNextSailing() => nextSailingPK = ZGuid.Empty;

		ZGuid nextSailingPK;
		readonly TrackingSiteUser siteUser;

#if DEBUG
		#region Test

		public ZGuid NextSailingPKForTesting
		{
			get => nextSailingPK;
			set => nextSailingPK = value;
		}

		public void CallSetNextSailingForTesting(ZGuid nextSailingPK) => SetNextSailing(nextSailingPK);

		#endregion
#endif
	}
}
