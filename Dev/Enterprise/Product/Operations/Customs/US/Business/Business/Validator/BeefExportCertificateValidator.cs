namespace Enterprise.Customs.US.Business
{
	class BeefCertificateValidator : PermitValidator
	{
		public BeefCertificateValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._05, invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		protected override bool IsPermitNoRequired(CargoWise.Types.ZString entryType)
		{
			return base.IsPermitNoRequired(entryType) && CountryOfOriginRequiresBeefCertificate;
		}

		bool CountryOfOriginRequiresBeefCertificate
		{
			get
			{
				return invoiceLine.US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Argentina ||
					invoiceLine.US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Australia ||
					invoiceLine.US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.NewZealand ||
					invoiceLine.US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Uruguay;
			}
		}
	}
}
