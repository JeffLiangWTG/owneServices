using CargoWise.Types;
using static Enterprise.MasterFiles.Business.SolomonIslandsOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SolomonIslandsComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.SolomonIslands;

		protected override string GetConsumptionTaxCode() => OrgCusCodes.TAX;
		protected override string GetConsumptionTaxRegistrationCode() => SolomonIslandsOrgCusCodeInfo.OrgCusCodes.TIN;
		protected override string GetLocalBusinessRegNoCodeType() => SolomonIslandsOrgCusCodeInfo.OrgCusCodes.TIN;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
