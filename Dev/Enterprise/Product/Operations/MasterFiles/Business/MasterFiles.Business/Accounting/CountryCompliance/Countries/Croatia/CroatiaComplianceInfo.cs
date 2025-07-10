using CargoWise.Types;
using static Enterprise.MasterFiles.Business.CroatiaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class CroatiaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Croatia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.PDV;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
