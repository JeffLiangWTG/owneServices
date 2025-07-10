namespace Enterprise.Customs.US.eManifest.Business
{
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;

	class ShipmentLogManager
	{
		internal ShipmentLogManager(Logs logs)
		{
			Argument.NotNull(logs, "logs");
			this.logs = logs;
		}

		internal void AddStatusChangedLog(ZString status, ZDateTimeOffset statusTime)
		{
			ShipmentStatusEvent.AddNew(status, statusTime);
		}

		LogsForNominatedEvent ShipmentStatusEvent
		{
			get { return shipmentStatusEvent ?? (shipmentStatusEvent = new LogsForNominatedEvent(logs, AutoEvents.CustomsEntryStatus, true)); }
		}

		LogsForNominatedEvent shipmentStatusEvent;

		readonly Logs logs;
	}
}
