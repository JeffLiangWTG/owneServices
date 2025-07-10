namespace Enterprise.Freight.QuotedBookings.Business
{
	public interface IScheduleChooserCreator
	{
		ScheduleChooser Create(ISailingChooserParent parent);
	}
}
