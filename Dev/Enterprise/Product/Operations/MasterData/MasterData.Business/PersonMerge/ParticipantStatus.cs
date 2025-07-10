namespace Enterprise.MasterData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Status text")]
	public static class ParticipantStatus
	{
		public const string Queued = "Queued";
		public const string Merging = "Merging";
		public const string MergedWithErrors = "MergedWithErrors";
		public const string FailedWithCriticalError = "FailedWithCriticalError";
		public const string Completed = "Completed";
	}
}
