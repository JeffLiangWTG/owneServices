using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class BelgiumComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Belgium;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
		protected override string GetConsumptionTaxCode() => OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
