using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class PuertoRicoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.PuertoRico;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.PuertoRicoCodeTypes.NRC;
		protected override string GetConsumptionTaxCode() => string.Empty;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
