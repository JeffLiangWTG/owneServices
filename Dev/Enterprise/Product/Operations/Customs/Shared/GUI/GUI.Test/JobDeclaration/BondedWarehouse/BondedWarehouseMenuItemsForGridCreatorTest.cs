using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BondedWarehouseMenuItemsCreatorTest : TestCaseWithFactory
	{
		public void TestShortCutUpdateBondedWarehouseMenuItem()
		{
			var entry = Factory.New<CusEntryHeader>();
			var menu = new EDIMenu();
			var creator = new BondedWarehouseMenuItemsCreator(entry, menu, string.Empty, false);
			AssertNull("ShortCut menu is not supported", creator.ShortCutUpdateBondedWarehouseMenuItem);
			creator = new BondedWarehouseMenuItemsCreator(entry, menu, string.Empty, true);
			AssertNotNull("ShortCut menu is supported", creator.ShortCutUpdateBondedWarehouseMenuItem);
		}

		public void TestContextMenu_Visible()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_WarehouseTransactionStatus = "ABC";

			using var control = new EntriesWithMessagesOnDeclarationUserControl(declaration);
			try
			{
				var messageControl = control.NewMessageUserControl;
				messageControl.JobDeclaration = declaration;
				var grid = messageControl.Controls.Find("EntriesBoundGrid", true)[0] as ZGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText("Inventory Management");
				AssertNotNull("Should find the Inventory Management menu.", menuItem);

				AssertNoExceptionThrown("currentSupporter is null", () => grid.ContextMenu.DoPopup());
				AssertEquals("Should be hidden.", false, menuItem.Visible);

				grid.SetDataBinding(declaration, nameof(declaration.ActiveEntryHeaders), CusEntryHeaderSchema.Constants.TableName);
				grid.ContextMenu.DoPopup();
				AssertEquals("Should be visible.", true, menuItem.Visible);

				using (declaration.ActiveEntryHeaders.SuspendListChanged())
				{
					entry.Delete();
					grid.ContextMenu.DoPopup();
				}
				AssertEquals("Should be hidden.", false, menuItem.Visible);
			}
			finally
			{
				control.NewMessageUserControl.Dispose();
			}
		}
	}
}
