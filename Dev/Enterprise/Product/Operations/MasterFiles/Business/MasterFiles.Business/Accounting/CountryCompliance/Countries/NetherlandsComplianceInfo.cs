using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class NetherlandsComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Netherlands;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
		protected override string GetConsumptionTaxCode() => OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
