using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CustomsQuantityConverterTest : BaseCustomsQuantityConverterTest
	{
		protected override BaseCustomsQuantityConverter GetBaseCustomsQuantityConverter(JobComInvoiceLine invoiceLine) => new CustomsQuantityConverter(invoiceLine);
		protected override ZPropertyInfo GetQuantityPropertyInfo(JobComInvoiceLine invoiceLine) => invoiceLine.JI_CustomsQuantityInfo;
		protected override ZPropertyInfo GetUnitPropertyInfo(JobComInvoiceLine invoiceLine) => invoiceLine.JI_CustomsUnitQtyInfo;

		public override void TestQuantityDecimalPlace()
		{
			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_ConversionFactor = 1m;
			refPacks.RP_CustomsPack = "MTK";
			refPacks.RP_CommercialPack = "MTR";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "MTR";
			invoiceLine.JI_CustomsUnitQty = "MTK";

			var converter = GetBaseCustomsQuantityConverter(invoiceLine);
			invoiceLine.JI_InvoiceQuantity = 123.00049;
			converter.CalculateCustomsFactorAndQty();
			AssertEquals(123.0005m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 123.000449;
			converter.CalculateCustomsFactorAndQty();
			AssertEquals(123.0004m, invoiceLine.JI_CustomsQuantity);
		}
	}
}
