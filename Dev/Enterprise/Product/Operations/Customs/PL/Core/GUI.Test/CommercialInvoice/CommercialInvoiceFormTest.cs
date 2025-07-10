using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(CommercialInvoiceForm))]
sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
{
	public void TestGetImportInvoiceLineUserControl()
	{
		using (var control = new CommercialInvoiceFormForTest(Factory.New<JobComInvoiceHeader>()))
		{
			using (var invoiceLineControl = control.GetImportInvoiceLineUserControlExposed())
			{
				AssertType<ImportInvoiceLineUserControl>(invoiceLineControl);
			}
		}
	}

	public void TestGetExportInvoiceLineUserControl()
	{
		using (var control = new CommercialInvoiceFormForTest(Factory.New<JobComInvoiceHeader>()))
		{
			using (var invoiceLineControl = control.GetExportInvoiceLineUserControlExposed())
			{
				AssertType<ExportInvoiceLineUserControl>(invoiceLineControl);
			}
		}
	}

	protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

	sealed class CommercialInvoiceFormForTest : CommercialInvoiceForm
	{
		public CommercialInvoiceFormForTest(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public EUInvoiceLineUserControl GetImportInvoiceLineUserControlExposed() => base.GetImportInvoiceLineUserControl();

		public EUInvoiceLineUserControl GetExportInvoiceLineUserControlExposed() => base.GetExportInvoiceLineUserControl();
	}
}
