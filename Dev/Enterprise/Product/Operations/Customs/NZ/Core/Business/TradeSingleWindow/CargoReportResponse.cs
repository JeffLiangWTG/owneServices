namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public abstract class CargoReportResponse : TSWResponse
	{
		protected CargoReportResponse(BaseTSWResponse response)
			: base(response)
		{
		}

		protected override string InstructionsPath
		{
			get { return "p:OverallDeclaration/p:Declaration/p:Consignment/p:AdditionalInformation"; }
		}
	}
}
