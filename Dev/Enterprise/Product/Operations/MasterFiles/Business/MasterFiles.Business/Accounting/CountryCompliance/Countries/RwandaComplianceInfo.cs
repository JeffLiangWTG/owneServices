using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class RwandaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Rwanda;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.RwandaCodeTypes.TIN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.RwandaCodeTypes.TIN;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
