using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(OrderInvoiceImporterForm))]
	sealed class OrderInvoiceImporterFormTest : ZFormBasherTest
	{
		public void TestSelectAllElements()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "1";
			order1.JD_JE = declaration.PK;
			order1.BuyerPK = org.PK;
			var order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "2";
			order2.JD_JE = declaration.PK;
			order2.BuyerPK = org.PK;
			Factory.Save();
			var importer = new OrderInvoiceImporter(declaration);
			AssertEquals("PreCondition - Importer Orders collection count should be 2", 2, importer.OrdersToImport.Count);
			using (var form = new OrderInvoiceImporterFormForTest(importer))
			{
				form.Show();
				form.SelectAllElements();
				AssertEquals("Should have 2 elements selected", 2, form.OrdersGridSelectedElementsCount);
			}
		}

		public void TestCopyElementToNewRow()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "1";
			order1.JD_JE = declaration.PK;
			order1.BuyerPK = org.PK;
			Factory.Save();
			var importer = new OrderInvoiceImporter(declaration);
			AssertEquals("PreCondition - Importer Orders collection count should be 1", 1, importer.OrdersToImport.Count);
			using (var form = new OrderInvoiceImporterFormForTest(importer))
			{
				form.Show();
				form.SelectAllElements();
				form.OrderGrid.InnerGrid.OnPopup_CallForTesting();
				var menuItem = form.OrderGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Copy to New Row");
				if (menuItem != null)
				{
					if (menuItem.Visible)
					{
						AssertNoExceptionThrown(delegate
						{
							menuItem.PerformClick();
						});
					}

					AssertEquals("'Copy to New Row' shouldn't be visible", false, menuItem.Visible);
				}
			}
		}

		public void TestValidateAfterEdit()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "1";
			order1.JD_JE = declaration.PK;
			order1.BuyerPK = org.PK;
			order1.OrderLines.AddNew();
			order1.OrderLines[0].JO_Quantity = 10m;
			Factory.Save();
			var importer = new OrderInvoiceImporter(declaration);
			using (var form = new OrderInvoiceImporterFormForTest(importer))
			{
				AssertEquals("Importer should have 1 record", 1, importer.OrdersToImport.Count);
				AssertEquals("Orders to import grid should have no errors", false, importer.OrdersToImport[0].HasRowErrors);
				var factory2 = new BusinessObjectFactory();
				var order2 = factory2.Load<Order>(order1.PK);
				order2.OrderLines.DeleteAll();
				factory2.Save();
				form.FireOrdersToImportGrid_EditFormClosed();
				AssertEquals("Orders To Import Grid should have a row error", true, importer.OrdersToImport[0].HasRowErrors);
			}
		}

		public void TestReconcileSelectedOrdersButton_Click()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "1";
			order1.JD_JE = declaration.PK;
			order1.BuyerPK = org.PK;
			var order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "2";
			order2.JD_JE = declaration.PK;
			order2.BuyerPK = org.PK;
			Factory.Save();
			var importer = new OrderInvoiceImporter(declaration);
			AssertEquals("PreCondition - Importer Orders collection count should be 2", 2, importer.OrdersToImport.Count);
			using (var form = new OrderInvoiceImporterFormForTest(importer))
			{
				form.Show();
				AssertEquals("PreCondition: ShowComInvoiceReconciliationForm was not called", false, form.ShowComInvoiceReconciliationFormWasCalled);
				form.ReconcileSelectedOrdersButton_ClickTest(form, EventArgs.Empty);
				AssertEquals("ShowComInvoiceReconciliationForm was not called because no Order was selected", false, form.ShowComInvoiceReconciliationFormWasCalled);
				form.SelectAllElements();
				form.ReconcileSelectedOrdersButton_ClickTest(form, EventArgs.Empty);
				AssertEquals("ShowComInvoiceReconciliationForm was called", true, form.ShowComInvoiceReconciliationFormWasCalled);
			}
		}

		public void TestUpdateSelectedButtonClickWhenNoneSelected()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "1";
			order1.JD_JE = declaration.PK;
			order1.BuyerPK = org.PK;
			Factory.Save();
			var importer = new OrderInvoiceImporter(declaration);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			using (var form = new OrderInvoiceImporterFormForTest(importer))
			{
				form.UpdateButton_ClickTest();
				AssertEquals("No Order has been selected", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGridId()
		{
			using (var form = GetFormToBashCore())
			{
				var grid = (ZModuleButtonGrid)form.Controls.Find("OrdersToImportGrid", true).Single();
				AssertEquals("369e94e8-151f-4a19-9d68-f81298a8b98a", grid.GridId);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var importer = new OrderInvoiceImporter(declaration);
			return new OrderInvoiceImporterForm(importer);
		}

		sealed class OrderInvoiceImporterFormForTest : OrderInvoiceImporterForm
		{
			public OrderInvoiceImporterFormForTest(OrderInvoiceImporter importer) : base(importer)
			{
			}

			public int OrdersGridSelectedElementsCount => OrdersToImportGrid.InnerGrid.SelectedRowCount;
			public ZModuleButtonGrid OrderGrid => OrdersToImportGrid;
			public void FireOrdersToImportGrid_EditFormClosed()
			{
				OrdersToImportGrid_EditFormClosed(OrdersToImportGrid, EventArgs.Empty);
			}

			public void ReconcileSelectedOrdersButton_ClickTest(object sender, EventArgs e)
			{
				ReconcileSelectedOrdersButton_Click(sender, e);
			}

			protected override void ShowComInvoiceReconciliationForm(Order[] selectedOrders)
			{
				ShowComInvoiceReconciliationFormWasCalled = true;
			}

			public bool ShowComInvoiceReconciliationFormWasCalled { get; private set; }
			public void UpdateButton_ClickTest()
			{
				UpdateButton_Click(null, null);
			}
		}
	}
}
