using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BoliviaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Bolivia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.BoliviaCodeTypes.NIT;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.BoliviaCodeTypes.NIT;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
