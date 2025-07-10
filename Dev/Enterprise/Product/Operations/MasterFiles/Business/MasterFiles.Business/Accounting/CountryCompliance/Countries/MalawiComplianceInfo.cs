using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MalawiComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Malawi;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.MalawiCodeTypes.TIN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.MalawiCodeTypes.TIN;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
