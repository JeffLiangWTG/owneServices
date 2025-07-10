using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ComInvoiceReconciliationForm))]
	sealed class ComInvoiceReconciliationFormTest : ZFormBasherTest
	{
		public void TestLinesSerialNumbers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var helper = new TestHelper();
			var selectedOrders = new Order[] { helper.CreateOrder(Factory, "ORDER1", 0, ZDateTime.Now, ZString.Empty, ZDateTime.Empty) };
			Factory.Save();

			var mock = new Mock<ComInvoiceReconciliator>(new object[] { declaration, selectedOrders, ComInvReconciliationQuantityType.OrderQuantity });
			mock.CallBase = true;
			var reconciliator = mock.Object;

			using (var testForm = new ComInvoiceReconciliationForm(reconciliator))
			{
				testForm.Show();
				var linesGrid = testForm.Controls.Find("InvoiceLinesGrid", true)[0] as ZGrid;

				var column = linesGrid.GetColumnStyle("JO_SerialNumber");
				AssertNotNull("Serial Number column should exist", column);
				Assert("Serial Number column should not be visible", !column.IsVisible);
			}
		}

		[ExpectNoExceptions()]
		public void TestImportInvoicesButton_ClickCallReconciliator_ImportInvoices()
		{
			CreateDummyDeclarationAndOrders();
			var mock = new Mock<ComInvoiceReconciliator>(new object[] { declaration, selectedOrders, ComInvReconciliationQuantityType.OrderQuantity });
			mock.CallBase = true;
			mock.Setup(m => m.ImportInvoices());
			var reconciliator = mock.Object;

			using (ComInvoiceReconciliationFormForTest form = new ComInvoiceReconciliationFormForTest(reconciliator))
			{
				form.ImportInvoicesButton_ClickTest(form, System.EventArgs.Empty);
			}

			mock.VerifyAll();
		}

		[ExpectNoExceptions()]
		public void TestImportInvoicesButton_ClickNotCallReconciliator_ImportInvoices()
		{
			CreateDummyDeclarationAndOrders();
			var mock = new Mock<ComInvoiceReconciliator>(new object[] { declaration, selectedOrders, ComInvReconciliationQuantityType.OrderQuantity });
			mock.CallBase = true;

			var reconciliator = mock.Object;
			reconciliator.ComInvHeaders[0].AddRowWarning("BLAH");

			mock.Verify(m => m.ImportInvoices(), Times.Never);

			using (ComInvoiceReconciliationFormForTest form = new ComInvoiceReconciliationFormForTest(reconciliator))
			{
				form.ImportInvoicesButton_ClickTest(form, System.EventArgs.Empty);
			}

			mock.VerifyAll();
		}

		protected override Form GetFormToBashCore()
		{
			CreateDummyDeclarationAndOrders();
			var reconciliator = new ComInvoiceReconciliator(declaration, selectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			return new ComInvoiceReconciliationForm(reconciliator);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestHelper();
		}

		void CreateDummyDeclarationAndOrders()
		{
			declaration = Factory.New<BaseJobDeclaration>();
			selectedOrders = new Order[] { helper.CreateOrder(Factory, "ORDER1", (byte)nextOrderSplitNo++, ZDateTime.Now, ZString.Empty, ZDateTime.Empty) };
			Factory.Save();
		}

		TestHelper helper;
		Order[] selectedOrders;
		BaseJobDeclaration declaration;
		int nextOrderSplitNo;

		sealed class ComInvoiceReconciliationFormForTest : ComInvoiceReconciliationForm
		{
			public ComInvoiceReconciliationFormForTest(ComInvoiceReconciliator invoiceReconciliator) : base(invoiceReconciliator)
			{
			}

			public void ImportInvoicesButton_ClickTest(object sender, EventArgs e) => ImportInvoicesButton_Click(sender, e);
		}
	}
}
