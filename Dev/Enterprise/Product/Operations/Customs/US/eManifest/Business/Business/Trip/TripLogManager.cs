namespace Enterprise.Customs.US.eManifest.Business
{
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;

	class TripLogManager
	{
		internal TripLogManager(Logs logs)
		{
			Argument.NotNull(logs, "logs");
			this.logs = logs;
		}

		internal void AddStatusChangedLog(ZString status)
		{
			TripStatusEvent.AddNew(status);
		}

		LogsForNominatedEvent TripStatusEvent
		{
			get { return tripStatusEvent ?? (tripStatusEvent = new LogsForNominatedEvent(logs, AutoEvents.CustomsManifestStatus)); }
		}

		LogsForNominatedEvent tripStatusEvent;

		readonly Logs logs;
	}
}