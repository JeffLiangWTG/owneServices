using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class JapanComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Japan;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.JapanCodeTypes.CON;
		protected override string GetConsumptionTaxCode() => OrgCusCode.JapanCodeTypes.CON;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.JapanCodeTypes.CON;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
