using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public static class ResourceStringHelper
	{
		public static ResourceStringData GetTotalInternationalFreightAmountInInvoiceCurrencyResString(ZBool isImport) => isImport ? Res.GetData("20c7c908-d756-4b97-bd18-c3eee5c47c39", "Freight (18)", "All freight fees paid or payable for transporting the shipment to its destination.") : Res.GetData("abd29cc6-2e0e-438a-9dcd-d2a43deb1917", "Freight (17)", "All freight fees paid or payable for transporting the shipment to its destination.");

		public static ResourceStringData GetTotalInternationalInsuranceAmountInInvoiceCurrencyCaption(ZBool isImport) => isImport ? Res.GetData("97aed13f-09ee-4b1e-a63e-ce6848882643", "Insurance (19)", "The cost of insurance of goods.") : Res.GetData("749edb25-6a1e-4618-b237-bb5a10d08447", "Insurance (18)", "The cost of insurance of goods.");

		public static ResourceStringData GetTotalAdditionsInInvoiceCurrencyCaption(ZBool isImport) => isImport ? Res.GetData("6fa4282e-9f18-4f33-bb60-cab31ebdbbe2", "Additions (20)", "The charge not included in the invoice which should be added according to the customs valuation rules.") : Res.GetData("0ef6aaa2-1e34-46d1-8598-eadbc4a6cad5", "Additions (19)", "The charge not included in the invoice which should be added according to the customs valuation rules.");

		public static ResourceStringData GetTotalDeductionsInInvoiceCurrencyCaption(ZBool isImport) => isImport ? Res.GetData("12d5b379-8089-45a1-9aa7-16fbbd8216a7", "Deductions (21)", "The charge included in the invoice which should be deducted according to the customs valuation rules.") : Res.GetData("65e63652-2ed3-478f-90d2-8907eba210e5", "Deductions (20)", "The charge included in the invoice which should be deducted according to the customs valuation rules.");

		public static ResourceStringData GetTotalCustomsValueInInvoiceCurrencyCaption(ZBool isImport) => isImport ? Res.GetData("21ba3f3e-cf67-4f04-abca-7977137ed015", "CIF (22)", "The total CIF value of this entry.") : Res.GetData("7e27ef77-5d48-47f1-aa23-d9090bbf0f0d", "FOB (21)", "The total FOB value of this entry.");

		public static ResourceStringData GetTotalCustomsValueInLocalCurrencyCaption(ZBool isImport) => isImport ? Res.GetData("d73a54bb-50f2-403c-91dd-c3fb99b73cf2", "CIF (TWD) (22)", "The total CIF value (TWD) of this entry.") : Res.GetData("74d3c960-83a2-40fa-b8fa-f829d6256a5f", "FOB (TWD) (21)", "The total FOB value (TWD) of this entry.");
	}
}
