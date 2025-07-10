
namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public static class ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators
	{
		public static string CheckCannotChangeDefaultValue(string proposed, string defaultValue, string countryCode, string alternateErrorMessage = null)
			=> proposed != defaultValue
				? alternateErrorMessage ?? Res.GetString("B00A5F8B-A3CB-40C3-BEE1-9DB20F287023", "Cannot change default value for country/region '{0}'.", countryCode)
				: string.Empty;

		public static string CheckCannotBeGovernmentNumberAllocate(string proposed, string countryCode, string alternateErrorMessage = null)
			=> proposed == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate
				? alternateErrorMessage ?? Res.GetString("C415B086-4396-48D3-BDA5-2A4E6A7D85A7", "'{0}' is not valid for country/region '{1}'.", proposed, countryCode)
				: string.Empty;

		public static string CheckCannotBePrint(string proposed, string countryCode, string alternateErrorMessage = null)
			=> proposed == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print
				? alternateErrorMessage ?? Res.GetString("B7321A4B-C1F5-4B66-8646-40D25E5A1617", "'{0}' is not valid for country/region '{1}'.", proposed, countryCode)
				: string.Empty;
	}
}
