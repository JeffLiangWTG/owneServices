using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class FrenchPolynesiaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.FrenchPolynesia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.FrenchPolynesiaCodeTypes.TAH;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.FrenchPolynesiaCodeTypes.TAH;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? HasExtraTaxInfo() => true;
		protected override string GetDescriptionForQCTExtraTaxType() => Res.GetString("d8814e3b-2cfe-4e42-a677-7945aee745a0", "CPS");
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("85851c63-2804-4185-aa20-f132f188f578", "CPS Amt", "CPS Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("255afc62-58dc-4928-8a68-6ca4033a5966", "CPS Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("b9e9ced9-5a7a-408a-9b2d-77a2974c5c49", "TVA Amt", "TVA Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("4bfd22a1-2750-4394-9a23-e191c3e20f67", "TVA Local");

		#endregion

	}
}
