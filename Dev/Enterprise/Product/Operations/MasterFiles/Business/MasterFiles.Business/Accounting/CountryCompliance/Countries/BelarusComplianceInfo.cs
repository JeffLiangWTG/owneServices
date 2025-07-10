using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BelarusComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Belarus;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.BelarusCodeTypes.TIN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.BelarusCodeTypes.TIN;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
