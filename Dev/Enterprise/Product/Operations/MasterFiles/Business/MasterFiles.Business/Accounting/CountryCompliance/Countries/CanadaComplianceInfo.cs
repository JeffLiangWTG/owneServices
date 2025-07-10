using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CanadaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Canada;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CACodeTypes.BusinessNumberForImportExport;
		protected override bool? GetIsReciprocal() => true;

		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSQST", "QST Amt", "QST Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalQST", "QST Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSGST", "GST Amt", "GST Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalGST", "GST Local");

		#endregion
	}
}
