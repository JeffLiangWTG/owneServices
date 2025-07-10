using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class LayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestGetLayout()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var document = invoice.SupportingDocuments.AddNew();

		using (var form = new ZForm(document))
		using (var control = new LayoutSupportingDocumentsFieldsControlForTest(declaration))
		{
			form.Controls.Add(control);
			form.Show();

			AssertType<SupportingDocumentFieldsLayout>(control.GetLayoutExposed());
		}
	}

	class LayoutSupportingDocumentsFieldsControlForTest : LayoutSupportingDocumentsFieldsControl
	{
		public LayoutSupportingDocumentsFieldsControlForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public IPanelLayoutProvider GetLayoutExposed() => base.GetLayout();
	}
}
