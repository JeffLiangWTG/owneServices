using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class RequireTEUMonitor
	{
		bool oldRequireTEU;
		int monitorRequireTEUIndex;
		readonly ICO2eCalculationSupporter supporter;
		readonly Func<bool> isCopying;

		bool IsMonitoringRequireTEUChange => monitorRequireTEUIndex > 0;

		public RequireTEUMonitor(ICO2eCalculationSupporter supporter, Func<bool> isCopying)
		{
			this.supporter = supporter;
			this.isCopying = isCopying;
		}

		public IDisposable MonitorChange(CO2eStatusChangedReason reason, Action additionalAction = null, bool skip = false)
		{
			if (IsMonitoringRequireTEUChange || skip)
			{
				return DisposableAction.NoAction;
			}

			return new DisposableAction(() =>
			{
				oldRequireTEU = supporter.RequireTEU;
				monitorRequireTEUIndex++;
			}, () =>
			{
				var newRequireTEU = supporter.RequireTEU;
				if (oldRequireTEU != newRequireTEU && supporter is BusinessObject bizo && !bizo.IsDeleted && !bizo.IsDeleting)
				{
					supporter.UpdateCO2eStatusToNotCurrent(reason, isCopying());
					additionalAction?.Invoke();
				}
				monitorRequireTEUIndex--;
			});
		}
	}
}
