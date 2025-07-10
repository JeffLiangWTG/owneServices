namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	class NZCResponse : DeclarationResponse
	{
		public NZCResponse(BaseTSWResponse response)
			: base(response)
		{
		}

		protected override string GetJobName()
		{
			return "TSW Unsolicited Delivery Order";
		}

		protected override bool GetIsImportEntry() => false;    // Unsolicited Response processed by this class, direction is irrelevant

		protected override string GetJobID()
		{
			return string.Empty;
		}
	}
}
