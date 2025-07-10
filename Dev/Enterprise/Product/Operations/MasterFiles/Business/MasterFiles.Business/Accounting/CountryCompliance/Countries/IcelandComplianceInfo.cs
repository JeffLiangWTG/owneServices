using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class IcelandComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Iceland;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.IcelandCodeTypes.VSK;
		protected override string GetConsumptionTaxCode() => OrgCusCode.IcelandCodeTypes.VSK;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.IcelandCodeTypes.Kennitala;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
