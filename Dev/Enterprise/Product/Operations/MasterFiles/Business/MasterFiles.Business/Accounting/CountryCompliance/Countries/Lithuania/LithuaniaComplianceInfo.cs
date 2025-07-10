using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class LithuaniaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Lithuania;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.LithuaniaCodeTypes.PVM;
		protected override string GetConsumptionTaxCode() => OrgCusCode.LithuaniaCodeTypes.PVM;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.LithuaniaCodeTypes.PVM;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
