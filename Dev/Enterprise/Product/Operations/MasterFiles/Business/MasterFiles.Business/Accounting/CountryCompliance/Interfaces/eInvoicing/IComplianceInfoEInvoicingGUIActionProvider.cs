using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceInfoEInvoicingGUIActionProvider
	{
		bool IsCountryEnableComplianceEInvoicing(bool isAPTransaction = false);

		IEnumerable<AccTransactionHeader> GetEligibleInvoices(IEnumerable<AccTransactionHeader> selectedTransactions);

		bool ExistActiveDocumentRequestPivot(ZDateTime lastSentTime);
	}
}
