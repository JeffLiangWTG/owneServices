namespace Enterprise.Customs.ZA.Business
{
	public class CreditTermsCodeListCompleteList : CreditTermsCodeList
	{
		public CreditTermsCodeListCompleteList()
		{
			AddNumberOfDays();
		}

		protected void AddNumberOfDays()
		{
			AddRange(new CreditTermsCodeListDays());
		}
	}
}
