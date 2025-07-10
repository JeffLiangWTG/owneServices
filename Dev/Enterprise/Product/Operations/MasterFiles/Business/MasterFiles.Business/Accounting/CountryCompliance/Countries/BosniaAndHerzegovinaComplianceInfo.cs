using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BosniaAndHerzegovinaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.BosniaAndHerzegovina;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV;
		protected override string GetConsumptionTaxCode() => OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
