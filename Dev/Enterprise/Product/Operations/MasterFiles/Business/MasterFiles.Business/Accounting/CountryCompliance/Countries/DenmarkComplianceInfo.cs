using CargoWise.Types;
using static Enterprise.MasterFiles.Business.DenmarkOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class DenmarkComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Denmark;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.MOM;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
