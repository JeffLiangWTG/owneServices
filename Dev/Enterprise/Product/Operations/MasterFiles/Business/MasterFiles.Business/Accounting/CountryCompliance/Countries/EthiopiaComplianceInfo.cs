using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class EthiopiaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Ethiopia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.EthiopiaCodeTypes.TaxIdentificationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
