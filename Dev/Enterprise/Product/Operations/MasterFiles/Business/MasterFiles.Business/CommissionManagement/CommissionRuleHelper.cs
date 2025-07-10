namespace Enterprise.MasterFiles.Business
{
	public static class CommissionRuleHelper
	{
		#region CommissionType

		public static void SetCommissionTypeDefaults(ICommissionRateOverridable commissionRate)
		{
			if (!commissionRate.IsCurrencyCommissionType())
			{
				commissionRate.CommissionCurrency = "";
			}
			else
			{
				DefaultCommissionCurrency(commissionRate);
			}

			if (!commissionRate.IsPercentageCommissionType())
			{
				commissionRate.CommissionPercentage = 0;
			}

			if (!commissionRate.IsAmountCommissionType())
			{
				commissionRate.CommissionAmount = 0;
			}
		}

		#endregion

		#region CommissionCurrency

		public static void DefaultCommissionCurrency(ICommissionRateOverridable commissionRate)
		{
			if (commissionRate.IsCurrencyCommissionType() && commissionRate.CommissionCurrency.IsEmpty)
			{
				var defaultingCompany = commissionRate.Company ?? GlbCompany.CurrentCompany;
				if (defaultingCompany != null)
				{
					commissionRate.CommissionCurrency = defaultingCompany.GC_RX_NKLocalCurrency;
				}
			}
		}

		#endregion
	}
}
