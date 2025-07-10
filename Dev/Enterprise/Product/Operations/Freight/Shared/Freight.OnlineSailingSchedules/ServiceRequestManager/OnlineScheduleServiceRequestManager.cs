using CargoWise.ComponentModel;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public abstract class OnlineScheduleServiceRequestManager : AdjustableServiceRequestManager
	{
		protected OnlineScheduleServiceRequestManager(INotifications notifications)
			: base(notifications)
		{
		}

		protected override string ExceptionErrorKey { get; } = "SFS_LoadRoutesError";
		protected internal override string ExceptionErrorMessage => ResString.GetMultilingualString("9d130b66-6afa-4d4f-9518-49fe0511b7a6", "Error occurred while loading routes from Global Schedules service.");
	}
}
