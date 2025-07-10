namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class EX1Response : DeclarationResponse
	{
		public EX1Response(BaseTSWResponse response)
			: base(response)
		{
		}

		protected override string GetJobName()
		{
			return "Customs Export Declaration";
		}

		protected override bool GetIsImportEntry() => false;
	}
}
