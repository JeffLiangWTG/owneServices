namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class IM1Response : DeclarationResponse
	{
		public IM1Response(BaseTSWResponse response)
			: base(response)
		{
		}

		protected override string GetJobName()
		{
			return "Customs Import Declaration";
		}

		protected override bool GetIsImportEntry() => true;
	}
}
