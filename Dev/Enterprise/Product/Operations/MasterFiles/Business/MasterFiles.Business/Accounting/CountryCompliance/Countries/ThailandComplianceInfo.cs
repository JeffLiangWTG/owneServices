using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ThailandComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Thailand;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CompanyRegistrationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
