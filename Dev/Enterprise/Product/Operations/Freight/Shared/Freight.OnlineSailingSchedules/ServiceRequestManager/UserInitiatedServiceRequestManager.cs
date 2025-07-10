using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class UserInitiatedServiceRequestManager : OnlineScheduleServiceRequestManager
	{
		public UserInitiatedServiceRequestManager(INotifications notifications)
			: base(notifications)
		{
		}

		protected override int TimeoutInSeconds
		{
			get { return FreightDataRegistry.Instance.OnlineSailingSchedulesUserInitiatedTimeoutInSeconds.Value; }
		}

		protected internal override int SuppressionLevel
		{
			get { return TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressionLevel; }
			set { TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressionLevel = value; }
		}

		protected internal override ZDateTime SuppressUntilUtc
		{
			get { return TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressUntilUtc; }
			set { TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressUntilUtc = value; }
		}
	}
}
