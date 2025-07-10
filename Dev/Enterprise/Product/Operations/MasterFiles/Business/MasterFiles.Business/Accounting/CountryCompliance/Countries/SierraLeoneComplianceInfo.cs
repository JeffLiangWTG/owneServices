using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SierraLeoneComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.SierraLeone;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.SierraLeoneCodeTypes.TIN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.SierraLeoneCodeTypes.TIN;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
