using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class KiribatiComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Kiribati;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.KiribatiCodeTypes.TaxIdentificationNumber;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.KiribatiCodeTypes.TaxIdentificationNumber;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
