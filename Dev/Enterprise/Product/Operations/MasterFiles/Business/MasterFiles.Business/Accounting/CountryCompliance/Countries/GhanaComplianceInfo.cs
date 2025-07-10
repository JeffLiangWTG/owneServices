using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class GhanaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Ghana;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.GhanaCodeTypes.TIN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.GhanaCodeTypes.TIN;
		protected override bool? GetIsReciprocal() => true;

		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("b6fc6aba-53c9-4e9e-8da7-42d0c097b7ad", "NHIL/GETFL Amt", "", "NHIL/GETFL Amount");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("cd0da342-f175-4e53-b9f7-cdcf5d21b2b4", "NHIL/GETFL Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSVAT", "VAT Amt", "VAT Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalVAT", "VAT Local");

		#endregion

		public static string VATAndExtraTax => Res.GetString("ef1f11b6-a666-4755-a1fa-2f6ab6f6c347", "VAT/NHIL/GETFL");

		public static string ExtraTax => Res.GetString("fa24c0b9-498e-4cfd-b151-78e569938db7", "NHIL/GETFL");

		public static IEnumerable<CodeDescriptionPair> ExtraTypes =>
			new CodeDescriptionPair[]
			{
				new CodeDescriptionPair(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, Res.GetString("a10d5b97-282f-4aa9-8a16-75800a0505fa", "NHIL/GETFL"))
			};
	}
}
