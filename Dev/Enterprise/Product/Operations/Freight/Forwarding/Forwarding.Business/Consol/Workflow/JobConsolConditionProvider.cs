using System.Collections.Immutable;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class JobConsolConditionProvider
	{
		public static string GetLegIdentifier(IMilestoneDateDefaultable defaultable, ForwardingConsol consol)
		{
			Transport[] transports = GetTransportLegsFromLoadToDischarge(consol);

			var isDepartureRelatedProcessTask = IsDepartureRelatedProcessTask(defaultable);
			var isArrivalRelatedProcessTask = IsArrivalRelatedProcessTask(defaultable);

			var leg = defaultable.GetLegIdentifier(transports.Length,
				isDepartureRelatedProcessTask,
				isArrivalRelatedProcessTask,
				Condition1Codes,
				legNames);

			if (leg == null)
			{
				if (isDepartureRelatedProcessTask && transports.Length >= 1)
				{
					leg = ConsolEventDataModel.Properties.FirstLeg;
				}
				else if (isArrivalRelatedProcessTask && transports.Length >= 1)
				{
					leg = ConsolEventDataModel.Properties.LastLeg;
				}
			}

			return leg;
		}

		public static bool IsDepartureRelatedProcessTask(IMilestoneDateDefaultable processTask)
		{
			var triggerFieldIsDeparture = processTask.TriggerFieldName == Transport.Schema.JW_ETD || processTask.TriggerFieldName == Transport.Schema.JW_ATD;

			return processTask.TriggerEventCode == Events.Departure.Code
				|| processTask.TriggerEventCode == Events.CutOffDate.Code
				|| processTask.TriggerEventCode == Events.ReceiptCommenced.Code
				|| (processTask.TriggerEventCode.IsEmpty && triggerFieldIsDeparture);
		}

		static bool IsArrivalRelatedProcessTask(IMilestoneDateDefaultable processTask)
		{
			var triggerFieldIsDeparture = processTask.TriggerFieldName == Transport.Schema.JW_ETA || processTask.TriggerFieldName == Transport.Schema.JW_ATA;

			return processTask.TriggerEventCode == Events.Arrival.Code
				|| processTask.TriggerEventCode == Events.StorageCommenced.Code
				|| (processTask.TriggerEventCode.IsEmpty && triggerFieldIsDeparture);
		}

		public static Transport[] GetTransportLegsFromLoadToDischarge(ForwardingConsol consol)
		{
			var transports = consol.Transports.Cast<Transport>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(transports);
			return RoutineSupportHelper.GetTransportLegsFromLoadToDischarge(transports,
				consol.JK_RL_NKLoadPort, consol.JK_RL_NKDischargePort);
		}

		public static ImmutableArray<string> Condition1Codes { get; } = new[]
		{
			JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg,
			JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg,
			JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg
		}.ToImmutableArray();

		static readonly ImmutableArray<string> legNames = new[]
		{
			ConsolEventDataModel.Properties.FirstLeg,
			ConsolEventDataModel.Properties.SecondLeg,
			ConsolEventDataModel.Properties.ThirdLeg,
			ConsolEventDataModel.Properties.FourthLeg,
			ConsolEventDataModel.Properties.LastLeg
		}.ToImmutableArray();
	}
}
