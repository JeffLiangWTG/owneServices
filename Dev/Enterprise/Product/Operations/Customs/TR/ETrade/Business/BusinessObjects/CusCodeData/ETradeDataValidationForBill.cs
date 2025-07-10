namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeDataValidationForBill : Customs.Business.CusCodeDataValidation
	{
		public ETradeDataValidationForBill(ETradeData parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
