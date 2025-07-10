using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TurksAndCaicosIslandsComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.TurksAndCaicosIslands;

		protected override string GetConsumptionTaxCode() => string.Empty;

		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;

		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;

		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
