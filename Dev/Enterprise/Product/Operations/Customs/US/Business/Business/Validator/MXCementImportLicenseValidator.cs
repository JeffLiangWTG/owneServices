using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class MXCementImportLicenseValidator : PermitValidator
	{
		public MXCementImportLicenseValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._09, invoiceLine)
		{
		}

		protected override bool IsPermitNoRequired(ZString entryType)
		{
			return base.IsPermitNoRequired(entryType) && IsCountryMatch;
		}

		bool IsCountryMatch
		{
			get { return InvoiceLine.US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Mexico; }
		}
	}
}
