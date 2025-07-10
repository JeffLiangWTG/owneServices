using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SenegalComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Senegal;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.SenegalCodeTypes.NIN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.SenegalCodeTypes.NIN;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
