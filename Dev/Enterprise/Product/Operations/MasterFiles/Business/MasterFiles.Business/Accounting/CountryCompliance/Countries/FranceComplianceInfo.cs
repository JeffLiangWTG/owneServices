using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class FranceComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.France;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.FranceCodeTypes.TVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.FranceCodeTypes.TVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.FranceCodeTypes.TVA;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
