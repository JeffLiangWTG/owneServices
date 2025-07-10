using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes
{
	public interface IComplianceSubTypeGUIProvider
	{
		bool ComplianceSubTypeIsReadOnly(bool hasBeenCreatedAsAmending, string ledger, string transactionType, bool originalTransactionReferenceIsEmpty);

		bool ShouldClearComplianceSubType(ZGuid originalTransactionReference);
	}
}
