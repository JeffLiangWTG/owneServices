using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.CountryCompliance
{
	public interface IOriginalInvoiceReference
	{
		/// <summary>
		/// When true, the transaction form will show the 3 fields: Original Reference (OriginalTransactionReference), Original Invoice Number (AH_OriginalTransactionNum) and Date (AH_OriginalInvoiceDate).
		/// </summary>
		bool ShouldShowOriginalInvoiceReferenceFields(string ledger, string transactionType);

		/// <summary>
		/// When true, the transaction form will show the 2 fields: Reference Date From (AH_OriginalReferenceStartDate) and Reference Date To (AH_OriginalReferenceEndDate)
		/// </summary>
		bool ShouldShowOriginalInvoiceReferenceDatesFields(string ledger, string transactionType);

		/// <summary>
		/// When true, the transaction form will show the 2 fields: Reason Code (ReasonCode) and Reason Description (ReasonDescription)
		/// </summary>
		bool ShouldShowOriginalInvoiceReferenceReasonFields(string ledger, string transactionType);

		/// <summary>
		/// When true, All 7 fields related to Original Invoice Reference will be enabled (not read only).
		/// </summary>
		bool GetAreAllOriginalInvoiceReferenceFieldsEnabled(string ledger, string transactionType, string complianceSubtype);

		/// <summary>
		/// When true, All 7 fields related to Original Invoice Reference are mandatory.
		/// </summary>
		bool GetAreOriginalTransactionReferenceFieldsMandatory(string ledger, string transactionType);

		/// <summary>
		/// DocEngine Macro <ARInvoice.ReversalReason> use this value as a fallback to print in the footer docstrip "Reversal Reason"
		/// </summary>
		string GetDocOriginalReferenceReason(string reasonDescription, ZDate originalReferenceStartDate, ZDate originalReferenceEndDate);
	}
}
