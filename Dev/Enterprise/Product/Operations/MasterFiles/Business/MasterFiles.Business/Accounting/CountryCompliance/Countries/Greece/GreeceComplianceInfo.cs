using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class GreeceComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Greece;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.GreeceCodeTypes.AFM;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GovBusinessCode;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
