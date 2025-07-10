using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DocumentaryQuantityConverterTest : BaseCustomsQuantityConverterTest
	{
		protected override BaseCustomsQuantityConverter GetBaseCustomsQuantityConverter(JobComInvoiceLine invoiceLine) => new DocumentaryQuantityConverter(invoiceLine);
		protected override ZPropertyInfo GetQuantityPropertyInfo(JobComInvoiceLine invoiceLine) => invoiceLine.AddInfoChild.TWL_DocumentaryQtyInfo;
		protected override ZPropertyInfo GetUnitPropertyInfo(JobComInvoiceLine invoiceLine) => invoiceLine.AddInfoChild.TWL_DocumentaryUQInfo;

		public override void TestQuantityDecimalPlace()
		{
			const string pce = "PCE";
			const string dzn = "DZN";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var converter = invoiceLine.DocumentaryQuantityConverter;
			var addInfo = invoiceLine.AddInfoChild;

			invoiceLine.JI_InvoiceQuantityInfo.SetValueFromString("12.00049");
			invoiceLine.JI_InvoiceUQInfo.SetValueFromString(pce);
			converter.CalculateCustomsFactorAndQty();
			AssertQuantitiesAndUnits(invoiceLine, 12.00049m, pce, 12.0005m, pce);

			invoiceLine.JI_InvoiceUQInfo.SetValueFromString(dzn);
			converter.CalculateCustomsFactorAndQty();
			AssertQuantitiesAndUnits(invoiceLine, 12.00049m, dzn, 144.0059m, pce);

			invoiceLine.JI_InvoiceUQInfo.SetValueFromString("");
			addInfo.TWL_DocumentaryUQInfo.SetValueFromString("");
			invoiceLine.JI_InvoiceUQInfo.SetValueFromString(dzn);
			converter.CalculateCustomsFactorAndQty();
			AssertQuantitiesAndUnits(invoiceLine, 12.00049m, dzn, 12.0005m, dzn);

			invoiceLine.JI_InvoiceUQInfo.SetValueFromString(pce);
			converter.CalculateCustomsFactorAndQty();
			AssertQuantitiesAndUnits(invoiceLine, 12.00049m, pce, 1m, dzn);
		}

		void AssertQuantitiesAndUnits(JobComInvoiceLine invoiceLine, ZDecimal invoiceQuantity, ZString invoiceUnit, ZDecimal documentaryQuantity, ZString documentaryUnit)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(invoiceLine.JI_InvoiceQuantity), invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
				AssertEquals(nameof(invoiceLine.JI_InvoiceUQ), invoiceUnit, invoiceLine.JI_InvoiceUQ);
				AssertEquals(nameof(invoiceLine.AddInfoChild.TWL_DocumentaryQty), documentaryQuantity, invoiceLine.AddInfoChild.TWL_DocumentaryQty);
				AssertEquals(nameof(invoiceLine.AddInfoChild.TWL_DocumentaryUQ), documentaryUnit, invoiceLine.AddInfoChild.TWL_DocumentaryUQ);
			});
		}
	}
}
