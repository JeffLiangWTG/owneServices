namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	class TSWNotificationResponse : NZCResponse
	{
		public TSWNotificationResponse(BaseTSWResponse response)
			: base(response)
		{
		}

		protected override string GetJobName()
		{
			return "TSW Notification Message";
		}

		protected override bool GetIsImportEntry() => EntryHeader?.Declaration?.IsImport ?? false;

		protected override string GetJobID()
		{
			return string.Empty;
		}
	}
}
