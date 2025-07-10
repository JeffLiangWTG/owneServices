using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BotswanaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Botswana;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
