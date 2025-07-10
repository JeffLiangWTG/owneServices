using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MarshallIslandsComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.MarshallIslands;

		protected override string GetConsumptionTaxCode() => string.Empty;

		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;

		protected override string GetLocalBusinessRegNoCodeType() => MarshallIslandsOrgCusCodeInfo.OrgCusCodes.EIN;

		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
