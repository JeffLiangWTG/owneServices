using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Freight.Universal
{
	public static class JobVoyageEventTransformer
	{
		public static EventValue Transform(EventValue sourceEventValue, JobVoyage voyage)
		{
			Argument.NotNull(sourceEventValue, nameof(sourceEventValue));
			Argument.NotNull(voyage, nameof(voyage));

			var reference = sourceEventValue.Reference;

			IDictionary<string, string> parameters;
			if (sourceEventValue.Parameters.Count == 0)
			{
				parameters = StmALog.GetParametersFromReference(reference);
			}
			else
			{
				parameters = sourceEventValue.Parameters.ToDictionary(s => s.Key, s => s.Value);
			}

			if (!NeedTransform(sourceEventValue, parameters, sourceEventValue.IsEstimate, voyage))
			{
				return sourceEventValue;
			}

			return EventTransformerHelper.DepartureOrArrivalToStatusUpdated(sourceEventValue,
					reference,
					parameters[Constants.EventReferenceParameters.Codes.Location]);
		}

		static bool NeedTransform(EventValue sourceEventValue, IDictionary<string, string> parameters, ZBool isEstimate, JobVoyage voyage)
		{
			if (isEstimate
				&& (sourceEventValue.Code == Events.DepartureCode || sourceEventValue.Code == Events.ArrivalCode)
				&& parameters != null
				&& parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out var facility)
				&& parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Location, out var eventLocation)
				&& facility.Equals(Constants.Facilities.Code.Terminal, StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrEmpty(eventLocation))
			{
				if (sourceEventValue.Code == Events.DepartureCode)
				{
					var voyageOrigin = voyage.Origins.Cast<VoyageOrigin>()
						.FirstOrDefault(origin => origin.JA_RL_NKPortOfLoading.EqualsIgnoringCase(eventLocation) && !origin.JA_A_DEP.IsEmpty);
					if (voyageOrigin != null)
					{
						return true;
					}
				}
				else
				{
					var voyageDest = voyage.Destinations.Cast<VoyageDestination>()
						.FirstOrDefault(dest => dest.JB_RL_NKPortOfDischarge.EqualsIgnoringCase(eventLocation) && !dest.JB_A_ARV.IsEmpty);
					if (voyageDest != null)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
