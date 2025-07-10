using System;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccTaxConfigGLAccountsProvider
	{
		(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount) GetApplicableGLAccounts(AccTaxConfiguration config);
		Guid GetRegistryValue(GLAccountRegistryType type);
	}
}
