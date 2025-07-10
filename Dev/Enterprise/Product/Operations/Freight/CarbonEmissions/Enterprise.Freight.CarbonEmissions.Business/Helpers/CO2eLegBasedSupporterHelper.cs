using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class CO2eLegBasedSupporterHelper
	{
		public static void OnRequested(ICO2eLegBasedSupporter supporter)
		{
			supporter?.SetTotalCO2e(0m);
			supporter?.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			supporter?.RecordLog(CO2eEventType.Requested, string.Empty);

			if (supporter?.ShouldPopulateCO2eForLegs ?? false)
			{
				supporter.ForEachLeg(leg =>
				{
					if (!string.IsNullOrEmpty(leg.LoadPort) && !string.IsNullOrEmpty(leg.DiscPort))
					{
						leg.RecordLog(CO2eEventType.Requested, string.Empty);
					}
					leg.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				});
			}

			if (supporter?.EmptyContainers?.Any() ?? false)
			{
				supporter.EmptyContainers.ForEach(emptyContainer =>
				{
					if (emptyContainer.Container != null)
					{
						if (emptyContainer.HasPickup && emptyContainer.Container.GetCO2eStatus(CO2eTypes.EmptyPickup) != CO2eStatusList.Codes.Current)
						{
							emptyContainer.Container.SetCO2eStatus(CO2eStatusList.Codes.Pending, CO2eTypes.EmptyPickup);
						}

						if (emptyContainer.HasReturn && emptyContainer.Container.GetCO2eStatus(CO2eTypes.EmptyReturn) != CO2eStatusList.Codes.Current)
						{
							emptyContainer.Container.SetCO2eStatus(CO2eStatusList.Codes.Pending, CO2eTypes.EmptyReturn);
						}
					}
				});
			}
		}

		public static void OnRejected(ICO2eLegBasedSupporter supporter, string failureReason)
		{
			supporter?.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			supporter?.RecordLog(CO2eEventType.Rejected, failureReason);

			if (supporter?.ShouldPopulateCO2eForLegs ?? false)
			{
				supporter.ForEachLeg(leg =>
				{
					leg.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
					if (!leg.LoadPort.IsEmpty && !leg.DiscPort.IsEmpty)
					{
						leg.RecordLog(CO2eEventType.Rejected, failureReason);
					}
				});
			}

			if (supporter?.EmptyContainers?.Any() ?? false)
			{
				supporter.EmptyContainers.ForEach(emptyContainer =>
				{
					if (emptyContainer.Container != null)
					{
						if (emptyContainer.HasPickup && emptyContainer.Container.GetCO2eStatus(CO2eTypes.EmptyPickup) != CO2eStatusList.Codes.Current)
						{
							emptyContainer.Container.SetCO2eStatus(CO2eStatusList.Codes.Rejected, CO2eTypes.EmptyPickup);
						}

						if (emptyContainer.HasReturn && emptyContainer.Container.GetCO2eStatus(CO2eTypes.EmptyReturn) != CO2eStatusList.Codes.Current)
						{
							emptyContainer.Container.SetCO2eStatus(CO2eStatusList.Codes.Rejected, CO2eTypes.EmptyReturn);
						}
					}
				});
			}
		}
	}
}
