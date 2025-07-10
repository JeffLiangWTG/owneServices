using CargoWise.Types;
using static Enterprise.MasterFiles.Business.SwedenOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class SwedenComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Sweden;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.MOM;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
