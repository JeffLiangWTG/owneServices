using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TimorLesteComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.TimorLeste;
		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;
		protected override string GetConsumptionTaxCode() => string.Empty;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GovBusinessCode;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
