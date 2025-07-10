namespace Enterprise.MasterFiles.Business
{
	public class CreditLimitDetails
	{
		public CreditLimitDetails(decimal creditLimit, decimal totalOutstandingAmount, bool onCreditHold,
			bool isGlobalCreditApproved = false, string globalCreditCurrency = null, decimal globalCreditLimit = decimal.Zero, decimal globalTotalOutstandingAmount = decimal.Zero, bool isOnGlobalCreditHold = false)
		{
			CreditLimit = creditLimit;
			TotalOutstandingAmount = totalOutstandingAmount;
			OnCreditHold = onCreditHold;

			IsGlobalCreditApproved = isGlobalCreditApproved;
			GlobalCreditLimit = globalCreditLimit;
			GlobalCreditCurrency = globalCreditCurrency;
			GlobalTotalOutstandingAmount = globalTotalOutstandingAmount;
			IsOnGlobalCreditHold = isOnGlobalCreditHold;
		}

		public decimal CreditLimit { get; }
		public decimal TotalOutstandingAmount { get; }
		public bool OnCreditHold { get; }
		public bool IsGlobalCreditApproved { get; }
		public string GlobalCreditCurrency { get; }
		public decimal GlobalCreditLimit { get; }
		public decimal GlobalTotalOutstandingAmount { get; }
		public bool IsOnGlobalCreditHold { get; }
	}
}
