using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		public void TestHandleInvoiceLineUserControlVisibilityChangedCore()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				invoice.JZ_MessageType = "IMP";
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertEquals(form.InvoiceLineUserControl.GetType(), typeof(ImportInvoiceLineUserControl));
				form.MainTabControl.SelectedTab = form.HeaderTabPage;
				Application.DoEvents();
				invoice.JZ_MessageType = "EXP";
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertEquals(form.InvoiceLineUserControl.GetType(), typeof(ExportInvoiceLineUserControl));
			}
		}

		public void TestMinimumSize()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(725), form.MinimumSize.Height);
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

		public void TestTopLevelMenuType()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceFormForTest(invoice))
			{
				AssertType<CommercialInvoiceEDIMenu>(form.ExposeGetNewTopLevelMenuCore());
			}
		}

		class CommercialInvoiceFormForTest : CommercialInvoiceForm
		{
			public CommercialInvoiceFormForTest(JobComInvoiceHeader header) : base(header)
			{ }

			public Customs.GUI.CommercialInvoiceEDIMenu ExposeGetNewTopLevelMenuCore()
			{
				return GetNewTopLevelMenuCore();
			}
		}
	}
}
