using Enterprise.Customs.SG.V4.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm()
		{
			var invoiceForm = new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());
			invoiceForm.JobDeclaration.ApportionmentDirty = false;
			return invoiceForm;
		}
	}
}
