using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class UgandaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Uganda;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => UgandaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
