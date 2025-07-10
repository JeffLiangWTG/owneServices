using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class AustraliaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Australia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
