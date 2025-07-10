using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MozambiqueComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Mozambique;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.MozambiqueCodeTypes.NUI;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.MozambiqueCodeTypes.NUI;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
