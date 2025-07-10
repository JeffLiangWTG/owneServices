using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class AngolaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Angola;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
