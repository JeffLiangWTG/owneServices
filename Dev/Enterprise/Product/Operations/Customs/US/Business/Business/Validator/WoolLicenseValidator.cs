using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class WoolLicenseValidator : PermitValidator
	{
		public WoolLicenseValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._17, invoiceLine)
		{
		}

		protected override bool IsPermitNoRequired(ZString entryType)
		{
			var tariff = InvoiceLine.ImportSupTariff ?? InvoiceLine.ImportTariff;
			return tariff != null && tariff.Applies(TariffRuleList.Codes.WoolLicenseEligible, InvoiceLine.EffectiveDateForDutyRate);
		}

		protected override ZString GetErrorTextForExtraCondition(ZString permit)
		{
			var result = ZString.Empty;

			if (!permit.IsEmpty)
			{
				if (!IsPermitNoRequired(ZString.Empty))
				{
					result = WoolLicenseShouldNotBeEnteredForTariff;
				}
			}

			return result;
		}

		internal const string WoolLicenseShouldNotBeEnteredForTariff = "Wool Licence No should not be entered for this tariff no.";
	}
}
