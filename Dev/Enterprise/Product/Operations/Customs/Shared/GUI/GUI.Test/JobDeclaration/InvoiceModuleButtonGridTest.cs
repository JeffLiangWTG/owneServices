using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceModuleButtonGridTest : TestCaseForAttachGUI
	{
		public void TestReloadOnBinding()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			Factory.Save();
			Factory.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var declarationInFactory2 = factory2.Load<BaseJobDeclaration>(declaration.PK);
			declarationInFactory2.Invoices.DeleteAll();
			var invoice3 = declarationInFactory2.Invoices.AddNew();
			var invoice4 = declarationInFactory2.Invoices.AddNew();
			var invoice5 = declarationInFactory2.Invoices.AddNew();
			factory2.Save();

			using (var form = new ZForm())
			{
				var flag = false;
				var grid = CreateGrid();
				grid.InnerGrid.CheckDatabaseAfterFirstBinding = false;
				grid.InnerGrid.OnRowsChangedInDatabase += (o, e) => { flag = true; };
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();
				Application.DoEvents();
				AssertEquals("we don't check database for changes", false, flag);
				AssertCollectionContains(invoice1.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionContains(invoice2.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice3.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice4.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice5.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
			}

			using (var form = new ZForm())
			{
				var flag = false;
				var grid = CreateGrid();
				grid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
				grid.InnerGrid.OnRowsChangedInDatabase += (o, e) => { flag = true; };
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();
				Application.DoEvents();
				AssertEquals("collection surprisingly changed, so event was called", true, flag);
				AssertCollectionContains(invoice1.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionContains(invoice2.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice3.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice4.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice5.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
			}

			using (var form = new ZForm())
			{
				var flag = false;
				var grid = CreateGrid();
				grid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
				grid.InnerGrid.OnRowsChangedInDatabase += (o, e) => { flag = true; };
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();
				Application.DoEvents();
				AssertEquals("collection didn't change, so it's still different to db, so event was called", true, flag);
				AssertCollectionContains(invoice1.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionContains(invoice2.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice3.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice4.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice5.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
			}

			using (var form = new ZForm())
			{
				var flag = false;
				var grid = CreateGrid();
				grid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
				grid.InnerGrid.OnRowsChangedInDatabase += (o, e) => { flag = true; };
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declarationInFactory2, "Invoices");
				form.Show();
				Application.DoEvents();
				AssertEquals("collection is the same as db, so no event", false, flag);
				AssertCollectionNotContains(invoice1.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionNotContains(invoice2.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionContains(invoice3.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionContains(invoice4.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
				AssertCollectionContains(invoice5.PK, grid.InnerGrid.List.OfType<BusinessObject>().Select(x => x.PK));
			}
		}

		public void TestDetachButton_Click()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			using (ZForm form = new ZForm())
			{
				InvoiceModuleButtonGrid grid = CreateGrid();
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();

				declaration.Invoices.DeleteAll();
				declaration.Invoices.SetReadOnlyIncludingChildren(false);
				grid.DetachButton_ClickInternal(this, EventArgs.Empty);
				AssertEquals("Normal message shown", "Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.Invoices.SetReadOnlyIncludingChildren(true);
				grid.DetachButton_ClickInternal(this, EventArgs.Empty);
				AssertEquals("Correct message", "Sorry, Commercial Invoices can be viewed but not detached.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Correct message", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestDisableDoubleClickToOpenInvoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();

			using (var form = new ZForm())
			{
				var grid = CreateGrid();
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();
				Application.DoEvents();
				grid.InnerGrid.Select(1);
				var pos = grid.InnerGrid.GetCellBounds(1, 0).Location;
				grid.InnerGrid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 2, pos.X, pos.Y, 0));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDetachButton_ClickForUnsavedDeclarationWithRouting()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.Transports.AddNew();
			invoice.Factory.Save();

			using (ZForm form = new ZForm())
			{
				invoice.JZ_JE = declaration.PK;
				invoice.JZ_ClusterKey = declaration.JE_ClusterKey;

				InvoiceModuleButtonGrid grid = CreateGrid();
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				grid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				grid.DetachButton_ClickInternal(this, EventArgs.Empty);
				AssertEquals("Correct message", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("No 'Cannot detach notification' expected, Invoice can be detached", "Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Invoice detached", 0, declaration.Invoices.Count);
				AssertEquals("Transport should be in Invoice level", 1, invoice.Transports.Count);

				invoice.JZ_JE = declaration.PK;
				invoice.JZ_ClusterKey = declaration.JE_ClusterKey;
				Factory.Save();
				grid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				grid.DetachButton_ClickInternal(this, EventArgs.Empty);
				AssertEquals("No 'Cannot detach notification' expected, Invoice can be detached", "Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Invoice detached", 0, declaration.Invoices.Count);
				AssertEquals("Transport should be deleted from Invoice, because Routing and Notes deleted from Invoice during Factory.Save", 0, invoice.Transports.Count);
			}
		}

		public void TestClickingAttachButtonDoesNotThrowException()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			using (ZForm form = new ZForm())
			{
				InvoiceModuleButtonGrid grid = CreateGrid();
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();
				grid.FindBoxList = new DummyBaseJobComInvoiceHeaderCollection(Factory);

				AssertNoExceptionThrown(delegate
				{ grid.AttachButton_ClickInternal(this, EventArgs.Empty); });
			}
		}

		public void TestClickingAttachButtonDoesNotThrowException_IfDeclarationIsDeleted()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			using (ZForm form = new ZForm())
			{
				InvoiceModuleButtonGrid grid = CreateGrid();
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();
				declaration.Delete();

				AssertNoExceptionThrown(delegate
				{ grid.AttachButton_ClickInternal(this, EventArgs.Empty); });
			}

			ErrorReporter.Clear();
		}

		public void TestAddsMenuItemsIfAppropriate()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			using (ZForm form = new ZForm())
			{
				InvoiceModuleButtonGrid grid = CreateGrid();
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(declaration, "Invoices");
				form.Show();

				AssertEquals("Menu present", true, MenuItemsContains(grid.InnerGrid.ContextMenu.MenuItems, "Attach Commercial Invoices"));
				AssertEquals("Menu present", true, MenuItemsContains(grid.InnerGrid.ContextMenu.MenuItems, "Detach Commercial Invoice"));
			}
		}

		public void TestButtonsGoVisibleOnlyWhenGridIsVisibleToUser()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			using (ZForm form = new ZForm(declaration))
			{
				InvoiceModuleButtonGrid grid = CreateGrid();

				ZTabControl topLevelTabControl = new ZTabControl();
				ZTabPage topPage1 = new ZTabPage();
				ZTabPage topPage2 = new ZTabPage();
				topLevelTabControl.TabPages.Add(topPage1);
				topLevelTabControl.TabPages.Add(topPage2);

				ZTabControl secondLevelTabControl = new ZTabControl();
				ZTabPage level2Page1 = new ZTabPage();
				ZTabPage level2Page2 = new ZTabPage();
				level2Page2.Controls.Add(grid);
				secondLevelTabControl.TabPages.Add(level2Page1);
				secondLevelTabControl.TabPages.Add(level2Page2);
				topPage2.Controls.Add(secondLevelTabControl);

				form.Controls.Add(topLevelTabControl);
				form.Show();

				AssertButtonsNull(grid);
				topLevelTabControl.SelectedTab = topPage2;
				AssertButtonsNull(grid);
				secondLevelTabControl.SelectedTab = level2Page2;

				form.FireSaveButton();
				UserIdleWorker.Flush(); // binds everything up

				secondLevelTabControl.SelectedTab = level2Page1;
				AssertButtonsVisible(grid, false);
				secondLevelTabControl.SelectedTab = level2Page2;
				AssertButtonsVisible(grid, true);
				secondLevelTabControl.SelectedTab = level2Page1;
				AssertButtonsVisible(grid, false);
				secondLevelTabControl.SelectedTab = level2Page2;
				AssertButtonsVisible(grid, true);
				topLevelTabControl.SelectedTab = topPage1;
				AssertButtonsVisible(grid, false);
				topLevelTabControl.SelectedTab = topPage2;
				AssertButtonsVisible(grid, true);
			}
		}

		public void TestInvoiceInnerGridIsOfTheCorrectType()
		{
			using (InvoiceModuleButtonGrid grid = new InvoiceModuleButtonGrid())
			{
				AssertEquals("Correct type", typeof(BaseInvoiceArrayBoundGrid), grid.InvoiceInnerGrid.GetType());
			}
		}

		void AssertButtonsNull(InvoiceModuleButtonGrid grid)
		{
			AssertNull("Buttons lazy created when required", grid.attachInvoiceButton);
			AssertNull("Buttons lazy created when required", grid.detachInvoiceButton);
		}

		void AssertButtonsVisible(InvoiceModuleButtonGrid grid, bool isVisible)
		{
			AssertEquals("Attach button visible", isVisible, grid.attachInvoiceButton.Visible);
			AssertEquals("Detach button visible", isVisible, grid.detachInvoiceButton.Visible);
		}

		InvoiceModuleButtonGrid CreateGrid()
		{
			InvoiceModuleButtonGrid grid = new InvoiceModuleButtonGrid();
			grid.BindToGridList = "Invoices";
			grid.BindToFindBoxList = "Lookups+InvoicesToAttach";
			grid.ModuleID = ModuleIDs.CommercialInvoice;
			ZCalcEditColumnStyleInfo columnStyle = new ZCalcEditColumnStyleInfo();
			columnStyle.ColumnName = "JZ_Calc_Balance";
			grid.InnerGrid.ColumnStyles.Add(columnStyle);
			return grid;
		}

		sealed class DummyBaseJobComInvoiceHeaderCollection : ActiveBusinessObjectCollection<BaseJobComInvoiceHeader>
		{
			public DummyBaseJobComInvoiceHeaderCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
	}

	[TestedType(typeof(InvoiceModuleButtonGrid))]
	sealed class InvoiceModuleButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
