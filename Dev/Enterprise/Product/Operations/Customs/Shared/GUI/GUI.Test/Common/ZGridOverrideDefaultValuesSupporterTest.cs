using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Moq.Protected;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ZGridOverrideDefaultValuesSupporterTest : TestCaseWithFactory
	{
		public void TestPerformClick()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMock.Object;
			var collection = new TestCollection(declaration);
			collection.IsOverrideDefaultValuesEnabled = true;
			declarationMock.Protected()
				.Setup<IInvoiceLineViewCollection<BaseJobComInvoiceLine>>("GetNewInvoiceLineViewCollection")
				.Returns(collection);
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new BaseInvoiceLineUserControl())
			using (var supporter = new ZGridOverrideDefaultValuesSupporter(userControl.CustomsInvoiceLinesBoundGrid))
			{
				form.Controls.Add(userControl);
				form.Show();
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				grid.ContextMenu.OnPopup_ForTest();
				var overrideMenuItem = grid.ContextMenu.MenuItems.FindByText("Override Default Values");
				AssertEquals(false, overrideMenuItem.Checked);
				AssertEquals(false, declaration.JE_OverrideFreightDefaults);
				overrideMenuItem.PerformClick();
				AssertEquals(true, overrideMenuItem.Checked);
				AssertEquals(true, declaration.JE_OverrideFreightDefaults);
				overrideMenuItem.PerformClick();
				AssertEquals(false, overrideMenuItem.Checked);
				AssertEquals(false, declaration.JE_OverrideFreightDefaults);
				declaration.JE_OverrideFreightDefaults = ZBool.True;
				AssertEquals(true, overrideMenuItem.Checked);
				declaration.JE_OverrideFreightDefaults = ZBool.False;
				AssertEquals(false, overrideMenuItem.Checked);
			}
		}

		public void TestOverrideDefaultValuesVisible()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMock.Object;
			var invoice = declaration.Invoices.AddNew();
			var collection = new TestCollection(declaration);
			collection.IsOverrideDefaultValuesEnabled = false;
			var invoiceLine1 = collection.AddNew();
			var invoiceLine2 = collection.AddNew();
			declarationMock.Protected()
				.Setup<IInvoiceLineViewCollection<BaseJobComInvoiceLine>>("GetNewInvoiceLineViewCollection")
				.Returns(collection);
			using (var form = new ZForm(declaration))
			using (var userControl = new BaseInvoiceLineUserControl())
			using (var supporter = new ZGridOverrideDefaultValuesSupporter(userControl.CustomsInvoiceLinesBoundGrid))
			{
				form.Controls.Add(userControl);
				form.Show();
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				grid.ContextMenu.OnPopup_ForTest();
				AssertNull("Do not create menu if disable", grid.ContextMenu.MenuItems.FindByText("Override Default Values"));
				collection.IsOverrideDefaultValuesEnabled = true;
				grid.ListManager.Position = 1;
				grid.ContextMenu.OnPopup_ForTest();
				var overrideMenuItem = grid.ContextMenu.MenuItems.FindByText("Override Default Values");
				AssertEquals(true, overrideMenuItem.Visible);
				AssertEquals(true, overrideMenuItem.Enabled);
				collection.IsOverrideDefaultValuesEnabled = false;
				grid.ListManager.Position = 0;
				AssertEquals(false, overrideMenuItem.Visible);
				AssertEquals(false, overrideMenuItem.Enabled);
				collection.IsOverrideDefaultValuesEnabled = true;
				grid.ListManager.Position = 1;
				AssertEquals(true, overrideMenuItem.Visible);
				AssertEquals(true, overrideMenuItem.Enabled);
			}
		}

		sealed class TestCollection : InvoiceLineViewCollection<BaseJobComInvoiceLine>, IOverrideDefaultValuesCollection
		{
			public TestCollection(BaseJobDeclaration declaration) : base(declaration)
			{
			}

			public bool IsOverrideDefaultValuesEnabled { get; set; }

			public ZPropertyInfoBool OverrideDefaultValuesInfo => (ZPropertyInfoBool)Declaration.JE_OverrideFreightDefaultsInfo;
		}
	}
}
