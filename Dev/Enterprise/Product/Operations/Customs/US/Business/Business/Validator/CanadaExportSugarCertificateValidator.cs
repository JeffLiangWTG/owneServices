using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CanadaExportSugarCertificateValidator : PermitValidator
	{
		public CanadaExportSugarCertificateValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._16, invoiceLine)
		{
		}

		protected override ZString GetErrorTextForExtraCondition(ZString permit)
		{
			var result = string.Empty;

			if (InvoiceLine.US_UC_NKCountryOfExport != Core.Constants.CountryCodes.Canada)
			{
				result = CASugarCertCountryOfExport;
			}
			else if (!InvoiceLine.IsCountryOfOriginCanada)
			{
				result = CASugarCertCountryOfOrigin;
			}

			return result;
		}

		public const string CASugarCertCountryOfExport = "CA Sugar Certificate is only valid where the Country of Export is Canada.";
		public const string CASugarCertCountryOfOrigin = "CA Sugar Certificate is only valid where the Country of Origin is Canada.";
	}
}
