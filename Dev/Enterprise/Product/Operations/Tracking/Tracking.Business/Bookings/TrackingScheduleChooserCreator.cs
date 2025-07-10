using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingScheduleChooserCreator : IScheduleChooserCreator
	{
		public TrackingScheduleChooserCreator(TrackingSiteUser siteUser)
		{
			this.siteUser = siteUser;
		}

		public ScheduleChooser Create(ISailingChooserParent parent) => new TrackingScheduleChooser(parent, siteUser);

		readonly TrackingSiteUser siteUser;
	}
}
