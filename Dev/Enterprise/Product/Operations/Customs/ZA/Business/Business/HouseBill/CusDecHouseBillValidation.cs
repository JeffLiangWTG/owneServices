namespace Enterprise.Customs.ZA.Business
{
	public class CusDecHouseBillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public CusDecHouseBillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		protected override void CheckCU_PackType()
		{
		}
	}
}
