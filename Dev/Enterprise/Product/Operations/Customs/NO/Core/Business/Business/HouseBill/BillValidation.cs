namespace Enterprise.Customs.NO.Business
{
	public class BillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		protected override bool NeedToValidateHouseBillAndPackages => false;
	}
}
