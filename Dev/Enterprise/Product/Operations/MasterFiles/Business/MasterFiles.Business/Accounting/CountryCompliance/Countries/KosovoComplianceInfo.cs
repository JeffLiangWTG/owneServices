using CargoWise.Types;
using static Enterprise.MasterFiles.Business.KosovoOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class KosovoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Kosovo;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.TVS;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.TVSH;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.TVS;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
