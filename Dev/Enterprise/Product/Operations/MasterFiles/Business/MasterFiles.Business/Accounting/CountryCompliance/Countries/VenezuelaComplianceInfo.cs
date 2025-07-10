using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class VenezuelaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Venezuela;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
