using System;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Common.Business
{
	public class DefaultSailingManagerQueryProvider : ISailingManagerQueryProvider
	{
		public SailingManagerUpdateMode QueryFreshMatchBehaviour(QueryFreshMatchBehaviourArgs queryArgs)
		{
			var result = IsNewAirScheduleAllowedFromDataImportRegistry(queryArgs)
				? SailingManagerUpdateMode.NewSchedule
				: SailingManagerUpdateMode.ScheduleUnchanged;

			return result;
		}

		static bool IsNewAirScheduleAllowedFromDataImportRegistry(QueryFreshMatchBehaviourArgs queryArgs)
		{
			if (queryArgs != null
				&& queryArgs.IsImportingData
				&& (queryArgs.TransportMode == Constants.TransportModes.Air
					|| queryArgs.TransportMode == Constants.TransportModes.AirSea)
				&& SystemDataRegistry.Instance.UpdateSchedulesDuringAutomaticImport.Value)
			{
				var foundDate = queryArgs.FoundDate;
				var requestedDate = queryArgs.RequestedDate;

				if (!foundDate.IsEmpty && !requestedDate.IsEmpty)
				{
					var timeDifference = Math.Abs((foundDate - requestedDate).TotalHours);

					if (timeDifference >= SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.Value)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool SelectedByUser => false;
	}
}
