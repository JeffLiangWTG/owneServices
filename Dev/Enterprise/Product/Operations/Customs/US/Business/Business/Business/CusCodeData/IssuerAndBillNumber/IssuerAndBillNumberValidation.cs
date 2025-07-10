namespace Enterprise.Customs.US.Business
{
	public class IssuerAndBillNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public IssuerAndBillNumberValidation(IssuerAndBillNumber bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckCY_Code()
		{
			//do not want to validate
		}
	}
}
