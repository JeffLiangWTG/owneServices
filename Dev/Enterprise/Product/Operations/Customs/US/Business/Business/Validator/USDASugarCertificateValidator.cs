using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class USDASugarCertificateValidator : PermitValidator
	{
		public USDASugarCertificateValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._21, invoiceLine)
		{
		}

		protected override bool IsPermitNoRequired(ZString entryType)
		{
			return
				InvoiceLine.US_UC_NKCountryOfOrigin != Core.Constants.CountryCodes.Mexico
				&& InvoiceLine.US_SupTariff != "98220515"
				&& InvoiceLine.US_SupTariff != "98220520"
				&& base.IsPermitNoRequired(entryType);
		}
	}
}
