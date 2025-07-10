using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		public void TestChangeControlsVisibilityWhenMessageTypeChanges()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			{
				var control = (InvoiceHeaderUserControl)GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				AssertEquals("TariffTypeLabel.Visible", true, control.TariffTypeLabel.Visible);
				AssertEquals("TariffTypeDropEdit.Visible", true, control.TariffTypeDropEdit.Visible);
				invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("TariffTypeLabel.Visible", false, control.TariffTypeLabel.Visible);
				AssertEquals("TariffTypeDropEdit.Visible", false, control.TariffTypeDropEdit.Visible);
			}
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}
