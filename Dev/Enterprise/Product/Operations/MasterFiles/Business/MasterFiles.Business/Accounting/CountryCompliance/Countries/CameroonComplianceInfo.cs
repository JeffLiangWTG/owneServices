using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CameroonComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Cameroon;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CameroonCodeTypes.NIU;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CameroonCodeTypes.NIU;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
