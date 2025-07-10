using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.CommercialInvoice.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

		protected override void BashScenario(Form testForm)
		{
			base.BashScenario(testForm);
			var invoiceHeader = ((testForm as ZForm)?.BusinessEntity as JobComInvoiceHeader);
			if (invoiceHeader != null)
			{
				Assert("Should create a non-persistent declaration on the new commercial invoice header.", !invoiceHeader.JobDeclaration.IsPersistent);
				AssertEquals("Should not create any entry headers on the new commercial invoice header.", 0, invoiceHeader.JobDeclaration.CustomsEntryHeaders.Count);
			}
		}
	}
}
