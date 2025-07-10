using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ZambiaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Zambia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.ZambiaCodeTypes.TaxIdentificationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
