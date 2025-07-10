using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MayotteComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Mayotte;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => false;

		#endregion
	}
}
