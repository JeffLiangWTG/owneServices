using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCallServiceRequestManager : AdjustableServiceRequestManager
	{
		public PortCallServiceRequestManager(INotifications notifications)
			: base(notifications)
		{
		}

		protected override string ExceptionErrorKey { get; } = "SFS_LoadPortCallError";
		protected internal override string ExceptionErrorMessage => ResString.GetMultilingualString("9217c795-0424-4eb2-8050-85be37bc8011", "Error occurred while loading Port Call data from Global Schedules service.");
		protected override int TimeoutInSeconds => FreightDataRegistry.Instance.OnlineSailingSchedulesUserInitiatedTimeoutInSeconds.Value;

		protected internal override int SuppressionLevel
		{
			get => TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressionLevel;
			set => TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressionLevel = value;
		}

		protected internal override ZDateTime SuppressUntilUtc
		{
			get => TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressUntilUtc;
			set => TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressUntilUtc = value;
		}
	}
}
