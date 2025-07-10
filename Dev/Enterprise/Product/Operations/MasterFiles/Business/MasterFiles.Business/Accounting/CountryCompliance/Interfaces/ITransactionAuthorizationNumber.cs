using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.CountryCompliance
{
	public interface ITransactionAuthorizationNumber
	{
		/// <summary>
		/// When true, the transaction authorization number is enabled (XD_PrintingAuthorizationNumber).
		/// It is used in DocEngine TransactionUniqueNumber macro, SAFT xml, validation and criticalValidation.
		/// </summary>
		bool IsTransactionAuthorizationNumberEnabled(ZGuid companyPK, ZDateTime transactionDate);

		/// <summary>
		/// Returns the default date on when the transaction authorization number is enabled.
		/// It is used in the registry to set the default date on companies.
		/// </summary>
		ZDate GetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate();

		/// <summary>
		/// Return the label of the transaction authorization number (e.g. label is "ATCUD" in Portugal companies).
		/// It is used in DocEngine TransactionUniqueNumber macro, SAFT xml and error messages.
		/// </summary>
		string GetTransactionAuthorizationNumberLabel();
	}
}
