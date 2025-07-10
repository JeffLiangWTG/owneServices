using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceInfoElectronicInvoicingEligibleSubType : IComplianceInfoElectronicInvoicing
	{
		// 🚩🚩🚩 Obsolete 🚩🚩🚩
		// This interface is obsolete and no further additions should be made to it.
		// Please create new interfaces as part of Enterprise.Accounting.Business.AccountingCountryFactory.GlobalAccountingCountryFactory
		// OR, AccountingMasterFilesDependencyFactory
		// See also: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/1459/Country-Object-Factories

		IReadOnlyCollection<string> GetEligibleComplianceSubTypeListForEInvoicing();
		bool IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType);
	}
}
