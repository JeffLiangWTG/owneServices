using CargoWise.Types;
using static Enterprise.MasterFiles.Business.CzechRepublicOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class CzechRepublicComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.CzechRepublic;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.DPH;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.DPH;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.DPH;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
