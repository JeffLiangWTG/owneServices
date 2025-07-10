using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TanzaniaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Tanzania;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.TanzaniaCodeTypes.VRN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.TanzaniaCodeTypes.VRN;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
