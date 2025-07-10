using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class IrelandComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Ireland;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GovBusinessCode;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
