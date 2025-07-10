using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CanadaSoftwoodLumberExportPermitValidator : PermitValidator
	{
		public CanadaSoftwoodLumberExportPermitValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._11, invoiceLine)
		{
		}

		protected override bool IsPermitNoRequired(ZString entryType)
		{
			return base.IsPermitNoRequired(entryType)
				&& CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(InvoiceLine.US_UC_NKCountryOfOrigin)
				&& InvoiceLine.IsSoftwoodLumberAgreementTariff
				&& InvoiceLine.US_LumberImporterDeclaration == YesNoDefaultList.Codes.Yes;
		}

		protected override bool MandatoryForEntry(string entryType)
		{
			return EntryTypeList.IsSoftwoodLumberPermitNumberRequired(entryType);
		}

		protected override ZString GetErrorTextForExtraCondition(ZString permit)
		{
			string result = "";

			if (!permit.IsEmpty)
			{
				if (!InvoiceLine.IsSoftwoodLumberAgreementTariff || !CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(InvoiceLine.US_UC_NKCountryOfOrigin))
				{
					result = LumberPermitNumberShouldNotBeEntered;
				}
			}

			return result;
		}

		protected override ZString GetWarningTextCore(ZString permit)
		{
			var result = base.GetWarningTextCore(permit);
			if (!permit.IsEmpty)
			{
				if (InvoiceLine.IsSoftwoodLumberAgreementTariff && InvoiceLine.US_UC_NKCountryOfOrigin == CanadaProvinceTerritoryCodes.Codes.XC)
				{
					result = OriginCannotBeBCForSoftwoodLumber;
				}
			}
			return result;
		}

		protected override ZString ExtraRequirementErrorMessage
		{
			get { return LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs; }
		}

		internal const string LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs = "A Canadian Softwood Lumber Permit Number is required for goods originating in a Canadian Province, for Tariffs requiring a Permit License and for Entry Types requiring a Permit License. (Entry Types '01', '02', '03', '06', '07', '11', '12', '21', '22').";

		internal const string OriginCannotBeBCForSoftwoodLumber = "Country of Origin code 'XC' (British Columbia) should not be reported for merchandise that is subject to the Softwood Lumber Agreement between Canada and the US. Use XD (Coastal Region of British Columbia) or XE (Interior Region of British Columbia) instead.";
		internal const string LumberPermitNumberShouldNotBeEntered = "A Lumber Permit Number is not required unless goods originate from a Canadian Province and the Tariff requires a Permit License and for Entry Types that require a Permit License. (Entry Types '01', '02', '03', '06', '07', '11', '12', '21', '22'). In other cases, entry message will be rejected.";
	}
}
