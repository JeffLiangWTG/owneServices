using System.Collections.Generic;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.DataTransfer.Freight.Universal
{
	public static class EventTransformerHelper
	{
		public static EventValue DepartureOrArrivalToStatusUpdated(EventValue sourceEventValue, ZString reference, ZString location)
		{
			if (!(sourceEventValue.Code == Events.DepartureCode || sourceEventValue.Code == Events.ArrivalCode))
			{
				throw new System.ArgumentException("Invalid argument.", nameof(sourceEventValue));
			}

			var parameters = new Dictionary<string, string>();

			var isDeparture = sourceEventValue.Code == Events.DepartureCode;
			parameters[Constants.EventReferenceParameters.Codes.Type] = isDeparture ? (NoResString)"Change of ETD rejected" : (NoResString)"Change of ETA rejected";
			parameters[Constants.EventReferenceParameters.Codes.Reason] = isDeparture ? (NoResString)"ETD received after ATD" : (NoResString)"ETA received after ATA";

			if (!location.IsEmpty)
			{
				parameters[Constants.EventReferenceParameters.Codes.Location] = location;
			}

			var freeText = reference.IsEmpty
				? ZString.Empty
				: StmALog.GetFreeTextFromReference(reference);
			var newReference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(freeText, parameters);

			return new EventValue(Events.StatusUpdated,
				sourceEventValue.IsEstimate,
				sourceEventValue.DeferFiringWorkflow,
				sourceEventValue.EventTime,
				newReference,
				parameters);
		}

		static HashSet<ZString> VesselEvents => new HashSet<ZString>
		{
			Events.CargoAvailableCode,
			Events.CutOffDateCode,
			Events.ReceiptCommencedCode,
			Events.StorageCommencedCode
		};

		public static ZBool IsVesselEvent(string eventType) => VesselEvents.Contains(eventType);
	}
}
