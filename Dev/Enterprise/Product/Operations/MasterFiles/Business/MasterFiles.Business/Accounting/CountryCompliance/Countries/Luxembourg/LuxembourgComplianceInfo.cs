using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class LuxembourgComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Luxembourg;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.LuxembourgCodeTypes.TVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.LuxembourgCodeTypes.TVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
