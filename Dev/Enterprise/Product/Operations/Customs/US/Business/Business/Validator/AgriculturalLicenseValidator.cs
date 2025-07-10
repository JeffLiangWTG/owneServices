using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AgriculturalLicenseValidator : PermitValidator
	{
		public AgriculturalLicenseValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._14, invoiceLine)
		{
		}

		protected override ZString GetErrorTextForExtraCondition(ZString permit)
		{
			var result = ZString.Empty;

			if (InvoiceLine.IsEntrySummaryValidationMode && !permit.IsEmpty)
			{
				if (!EntryTypeList.IsAgriculturalLicenseEntryTypes(InvoiceLine.ImportEntryType))
				{
					result = AgricultureLicenseNoInvalidEntryType;
				}
				else if (!InvoiceLine.US_VisaNo.IsEmpty || !InvoiceLine.US_TextileCategoryNo.IsEmpty || !InvoiceLine.US_DateOfExportFromCountryOfOrigin.IsEmpty)
				{
					result = AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered;
				}
			}

			return result;
		}

		internal const string AgricultureLicenseNoInvalidEntryType = "Invalid Entry Type for Agriculture License Number.";
		internal const string AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered = "An Agriculture License Number cannot be entered if a Textile Classification Visa Number or Category Number or a Textile Date is entered and vice versa.";
	}
}
