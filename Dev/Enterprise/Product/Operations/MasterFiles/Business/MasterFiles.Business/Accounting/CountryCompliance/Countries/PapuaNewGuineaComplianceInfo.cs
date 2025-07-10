using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class PapuaNewGuineaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.PapuaNewGuinea;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => false;

		#endregion

		public static string RecipientTaxIdPrefix => Res.GetString("3ECD5FBC-2440-4D2D-B6FF-FC034B86737D", "TIN");
		public static string RecipientTaxIDHeading => Res.GetString("1e761c81-b33d-4088-8416-35d293ea961b", "Client TIN #:");
	}
}
