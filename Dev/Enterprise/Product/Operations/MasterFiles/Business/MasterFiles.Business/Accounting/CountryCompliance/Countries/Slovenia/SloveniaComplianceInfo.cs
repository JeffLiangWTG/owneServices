using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class SloveniaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Slovenia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.SloveniaCodeTypes.DDV;
		protected override string GetConsumptionTaxCode() => OrgCusCode.SloveniaCodeTypes.DDV;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.SloveniaCodeTypes.DDV;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
