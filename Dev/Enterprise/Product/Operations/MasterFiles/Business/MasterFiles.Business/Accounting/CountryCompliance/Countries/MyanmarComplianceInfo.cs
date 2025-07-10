using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MyanmarComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Myanmar;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.MyanmarCodeTypes.CMT;
		protected override string GetConsumptionTaxCode() => OrgCusCode.MyanmarCodeTypes.CMT;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CompanyRegistrationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
