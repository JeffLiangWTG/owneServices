using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class NigerComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Niger;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.NigerCodeTypes.NIF;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.NigerCodeTypes.NIF;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
