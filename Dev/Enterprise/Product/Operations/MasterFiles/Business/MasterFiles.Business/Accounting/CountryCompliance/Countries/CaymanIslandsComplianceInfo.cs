using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CaymanIslandsComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.CaymanIslands;

		protected override string GetConsumptionTaxCode() => string.Empty;

		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;

		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;

		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
