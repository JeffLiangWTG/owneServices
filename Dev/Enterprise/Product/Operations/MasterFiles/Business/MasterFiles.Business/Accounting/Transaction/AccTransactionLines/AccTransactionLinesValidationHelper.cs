using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class AccTransactionLinesValidationHelper
	{
		public static bool IsNegativeChargeAllowed(AccTransactionLines parent)
		{
			if (parent.TransactionHeader != null && parent.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				return AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.Value == AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code
					|| (AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.Value == AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code && IsDebtorVATIsNotApplicable(parent));
			}

			return true;
		}

		static bool IsDebtorVATIsNotApplicable(AccTransactionLines parent)
		{
			return parent.TransactionHeader.Header?.CompanyData != null && parent.TransactionHeader.Header.CompanyData.OB_ARVATConfig == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
		}
	}
}
