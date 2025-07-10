using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business.Accounting.CountryCompliance
{
	public interface IComplianceSubTypeTaxInvoiceRuleWithTaxIDAndZeroAmount
	{
		bool AllWithTaxIDAndZeroTaxAmount(IEnumerable<AccTransactionLines> lines);
	}
}
