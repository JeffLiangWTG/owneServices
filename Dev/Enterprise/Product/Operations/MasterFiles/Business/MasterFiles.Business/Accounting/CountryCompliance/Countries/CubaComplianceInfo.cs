using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CubaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Cuba;
		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;
		protected override string GetConsumptionTaxCode() => string.Empty;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
