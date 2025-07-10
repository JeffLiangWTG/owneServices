using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GLJournalExchangeRateTypeValidation
	{
		public GLJournalExchangeRateTypeValidation(GLJournalExchangeRateType parent)
		{
			Parent = parent;
		}
		readonly GLJournalExchangeRateType Parent;

		public void ValidateAll()
		{
			ValidateBalanceSheetAccountTypeExchangeRateType();
			ValidateProfitAndLossAccountTypeExchangeRateType();
		}

		public void ValidateBalanceSheetAccountTypeExchangeRateType()
		{
			Parent.BalanceSheetAccountTypeExchangeRateTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.BalanceSheetAccountTypeExchangeRateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BalanceSheetAccountTypeExchangeRateTypeInfo, Parent.Lookups.ExchangeRateTypes);
		}

		public void ValidateProfitAndLossAccountTypeExchangeRateType()
		{
			Parent.ProfitAndLossAccountTypeExchangeRateTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.ProfitAndLossAccountTypeExchangeRateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ProfitAndLossAccountTypeExchangeRateTypeInfo, Parent.Lookups.ExchangeRateTypes);
		}
	}
}
