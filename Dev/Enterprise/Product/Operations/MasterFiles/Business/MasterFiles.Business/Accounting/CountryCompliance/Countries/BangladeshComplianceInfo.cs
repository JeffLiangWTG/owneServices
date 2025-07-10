using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BangladeshComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Bangladesh;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.BusinessRegistrationNumber;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region Static

		public static string RecipientTaxIdPrefix => Res.GetString("2176e71a-f527-462d-8b0f-9f57a920cfc9", "BIN");
		public static string RecipientTaxIDHeading => Res.GetString("01c02e90-1489-423d-8261-30b8b065f887", "Client BIN #:");

		#endregion
	}
}
