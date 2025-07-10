using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class EquatorialGuineaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.EquatorialGuinea;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.EquatorialGuineaCodeTypes.NIF;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.EquatorialGuineaCodeTypes.NIF;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
