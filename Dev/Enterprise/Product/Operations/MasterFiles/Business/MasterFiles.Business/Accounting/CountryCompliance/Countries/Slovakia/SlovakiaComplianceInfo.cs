using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class SlovakiaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Slovakia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.SlovakiaCodeTypes.DPH;
		protected override string GetConsumptionTaxCode() => OrgCusCode.SlovakiaCodeTypes.DPH;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.SlovakiaCodeTypes.DPH;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
