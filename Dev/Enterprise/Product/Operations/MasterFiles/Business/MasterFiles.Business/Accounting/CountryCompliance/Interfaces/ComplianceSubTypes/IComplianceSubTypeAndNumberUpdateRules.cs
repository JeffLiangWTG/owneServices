using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceSubTypeAndNumberUpdateRules
	{
		/// <summary>
		/// When true, the ClassAInvoice form (GovernmentInvoice bizo) will not be available for this country.
		/// </summary>
		bool IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed { get; }

		ZString GetErrorMessageForARComplianceSubTypeAndNumberUpdate(AccTransactionHeader[] transactionHeaders);

		bool IsComplianceNumberAllowed(AccTransactionHeader transactionHeader);

		bool IsComplianceDocDateAllowed(AccTransactionHeader transactionHeader);
	}
}
