using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class GuamComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Guam;
		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;
		protected override string GetConsumptionTaxCode() => string.Empty;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
