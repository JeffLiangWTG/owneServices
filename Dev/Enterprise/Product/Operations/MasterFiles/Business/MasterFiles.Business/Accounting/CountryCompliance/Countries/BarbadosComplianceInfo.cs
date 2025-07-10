using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BarbadosComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Barbados;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.BarbadosCodeTypes.TIN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.BarbadosCodeTypes.TIN;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
