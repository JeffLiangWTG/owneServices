using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class EventParentFinderHelper
	{
		public static ZString GetValueFromContextCollection(UniversalDataBuss.DataObjects.Universal.Event eventDataObject, string contextType, bool isMandatory = true)
		{
			var contectValue = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type == contextType)?.Value;

			if (isMandatory && (!contectValue.HasValue || contectValue.Value.IsEmpty))
			{
				throw new DataObjectReadFailureException($"{contextType} context must be provided.");
			}

			return contectValue ?? ZString.Empty;
		}

		public static bool IsGateEventSource(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var eventSource = eventDataObject.DataContext?.DataSourceCollection?.FirstOrDefault()?.Type;

			if (eventSource.HasValue && (eventSource.Value == nameof(DataContextType.GateBooking) || eventSource.Value == nameof(DataContextType.GateMovement) || eventSource.Value == nameof(DataContextType.GateMovementBooking) || eventSource.Value == nameof(DataContextType.GateVehicleMovement)))
			{
				return true;
			}

			return false;
		}

		public static bool RecipientIsATW(UniversalDataBuss.DataObjects.Universal.Event eventDataObject) => eventDataObject.DataContext?.RecipientRoleCollection?.Any(r => r.Code == RecipientRoleType.ATW) ?? false;

		public static bool IsGateEventCode(ZString eventCode) => eventCode == AutoEvents.GateInCode || eventCode == AutoEvents.GateOutCode || eventCode == AutoEvents.BookingCancelledCode || eventCode == AutoEvents.CancelledCode;
	}
}
