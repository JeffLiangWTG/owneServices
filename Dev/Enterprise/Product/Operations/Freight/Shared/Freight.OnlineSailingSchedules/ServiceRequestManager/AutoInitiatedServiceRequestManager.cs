using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class AutoInitiatedServiceRequestManager : OnlineScheduleServiceRequestManager
	{
		public AutoInitiatedServiceRequestManager(INotifications notifications)
			: base(notifications)
		{
		}

		protected override int TimeoutInSeconds
		{
			get { return FreightDataRegistry.Instance.OnlineSailingSchedulesAutoInitiatedTimeoutInSeconds.Value; }
		}

		protected internal override int SuppressionLevel
		{
			get { return TimeoutSettings.Instance.OnlineSailingSchedulesAutoInitiatedSuppressionLevel; }
			set { TimeoutSettings.Instance.OnlineSailingSchedulesAutoInitiatedSuppressionLevel = value; }
		}

		protected internal override ZDateTime SuppressUntilUtc
		{
			get { return TimeoutSettings.Instance.OnlineSailingSchedulesAutoInitiatedSuppressUntilUtc; }
			set { TimeoutSettings.Instance.OnlineSailingSchedulesAutoInitiatedSuppressUntilUtc = value; }
		}

		protected override bool IsAutoInitiatedServiceRequest => true;
	}
}
