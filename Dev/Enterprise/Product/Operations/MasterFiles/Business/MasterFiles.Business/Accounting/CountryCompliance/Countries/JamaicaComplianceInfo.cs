using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class JamaicaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Jamaica;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.JamaicaCodeTypes.GCT;
		protected override string GetConsumptionTaxCode() => OrgCusCode.JamaicaCodeTypes.GCT;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.JamaicaCodeTypes.GCT;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
