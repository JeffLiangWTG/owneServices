using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.CongoOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CongoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Congo;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.NIU;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NIU;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCodes.NIU;
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT NIU #";
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		protected override bool? HasExtraTaxInfo() => true;
		protected override string GetDescriptionForQCTExtraTaxType() => Res.GetString("b9e8a25a-12d1-4700-8cfb-1dbb0dd700a3", "SURTAX");
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("3eed1529-4d51-498d-8408-7523e5ff1455", "SURTAX Amt", "SURTAX Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("1d3c4e7e-809c-4cd8-a7c3-ebf1c38be41e", "SURTAX Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("ea7f0cbb-b44f-43c0-b08f-0593a2915885", "TVA Amt", "TVA Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("4e0b8999-79ed-4c78-9906-a6085d40cb8f", "TVA Local");

		#endregion
	}
}
