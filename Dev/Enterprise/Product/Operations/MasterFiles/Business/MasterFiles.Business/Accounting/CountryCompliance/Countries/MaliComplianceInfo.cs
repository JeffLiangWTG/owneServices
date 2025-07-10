using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MaliComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Mali;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.MaliCodeTypes.NIF;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.MaliCodeTypes.NIF;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
