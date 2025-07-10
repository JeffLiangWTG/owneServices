namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceRegistryDefaultProvider
	{
		// NOTE: This interface has the potential to add many unrelated registry items over time.
		//       Which will make implementing the interface more and more cumbersome.
		//       If this becomes a problem, consider refactoring to a single method: object GetDefaultValue(string registryName)

		string GetDefaultValueForComplianceNumberAllocationDateRegistry();

		string GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled);

		/// <summary>
		/// Validates the proposed value. Return null to use default validation rule, empty string to indicate success, or any other string to use as an error message.
		/// </summary>
		string ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled);

		/// <summary>
		/// Validates the proposed value. Return null to use default validation rule, empty string to indicate success, or any other string to use as an error message.
		/// </summary>
		string ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled);

		bool? GetDefaultValueForEnableGovernmentChargeCodeRegistry();

		bool? GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry();

		bool? GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry();
	}
}
