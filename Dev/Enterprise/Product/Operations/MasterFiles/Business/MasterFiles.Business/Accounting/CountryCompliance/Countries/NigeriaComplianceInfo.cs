using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class NigeriaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Nigeria;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.NigeriaCodeTypes.TaxIdentificationNumber;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.NigeriaCodeTypes.TaxIdentificationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
