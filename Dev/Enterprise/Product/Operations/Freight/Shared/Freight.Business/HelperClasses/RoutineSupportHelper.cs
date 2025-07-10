using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public static class RoutineSupportHelper
	{
		public static void AddTriggerCondition(this IMilestoneDateDefaultable processTask, bool isDepartureRelated,
			string legName, bool useTerminalFacility)
		{
			Argument.NotNull(processTask, nameof(processTask));
			Argument.NotNull(legName, nameof(legName));

			var originDestination = isDepartureRelated
				? TransportEventDataModel.Properties.Origin
				: TransportEventDataModel.Properties.Destination;

			var port = string.Format(CultureInfo.InvariantCulture, "<{0}.{1}>", legName, originDestination);
			var conditionValuesList = new List<KeyValuePair<string, string>>
				{
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location.AsKeyFor(port)
				};

			if (useTerminalFacility)
			{
				conditionValuesList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, CargoWise.EventReference.Constants.Facilities.Code.Terminal));
			}

			processTask.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			processTask.TriggerConditionValue = ProcessTask.GenerateRFPCondition(conditionValuesList.ToArray());
		}

		public static string GetLegIdentifier(this IMilestoneDateDefaultable processTask, int transportsLength,
			bool isDepartureRelated, bool isArrivalRelated,
			IList<string> condition1Codes, IList<string> legNames)
		{
			Argument.NotNull(processTask, nameof(processTask));
			Argument.NotNull(condition1Codes, nameof(condition1Codes));
			Argument.NotNull(legNames, nameof(legNames));

			if (condition1Codes.Count < 3 && legNames.Count < 4)
			{
				return null;
			}

			string leg = null;

			if (isDepartureRelated && processTask.TemplateCondition1 == condition1Codes[0] && transportsLength >= 2)
			{
				leg = legNames[1];
			}
			else if (isArrivalRelated && processTask.TemplateCondition1 == condition1Codes[0] && transportsLength >= 1)
			{
				leg = legNames[0];
			}
			else if (isDepartureRelated && processTask.TemplateCondition1 == condition1Codes[1] && transportsLength >= 3)
			{
				leg = legNames[2];
			}
			else if (isArrivalRelated && processTask.TemplateCondition1 == condition1Codes[1] && transportsLength >= 2)
			{
				leg = legNames[1];
			}
			else if (isDepartureRelated && processTask.TemplateCondition1 == condition1Codes[2] && transportsLength >= 4)
			{
				leg = legNames[3];
			}
			else if (isArrivalRelated && processTask.TemplateCondition1 == condition1Codes[2] && transportsLength >= 3)
			{
				leg = legNames[2];
			}

			return leg;
		}

		public static Transport[] GetTransportLegsFromLoadToDischarge(Transport[] transports, ZString loadPort, ZString dischargePort)
		{
			Argument.NotNull(transports, nameof(transports));

			var result = new List<Transport>();

			var firstLegFound = false;

			foreach (var transport in transports)
			{
				if (transport.JW_RL_NKLoadPort == loadPort)
				{
					firstLegFound = true;
				}

				if (firstLegFound)
				{
					result.Add(transport);
				}

				if (transport.JW_RL_NKDiscPort == dischargePort)
				{
					break;
				}
			}

			return result.ToArray();
		}

		public static bool IsCondition1Met(ZString conditionCode, int transportsLength, IList<string> condition1Codes)
		{
			Argument.NotNull(condition1Codes, nameof(condition1Codes));

			if (condition1Codes.Count < 3)
			{
				return false;
			}

			if (conditionCode == condition1Codes[0])
			{
				return transportsLength >= 2;
			}

			if (conditionCode == condition1Codes[1])
			{
				return transportsLength >= 3;
			}

			if (conditionCode == condition1Codes[2])
			{
				return transportsLength >= 4;
			}

			return false;
		}
	}
}
