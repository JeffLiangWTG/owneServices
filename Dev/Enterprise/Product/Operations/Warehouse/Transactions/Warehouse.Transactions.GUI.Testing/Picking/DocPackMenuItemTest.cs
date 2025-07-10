using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class DocPackMenuItemTest : TestCaseWithFactory
	{
		#region TestReprintWhenDocumentPackCannotPrint

		public void TestReprintWhenDocumentPackCannotPrint()
		{
			Pick.WP_PickStatus = PickStatus.Codes.Created;
			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			//Unpublish the DocBuilder Pick Documents Pack
			CargoWise.Database.TestFramework.ObjectModel.StmMenuItem
				.UpdateWhere(Guid.Parse("15F5C2FC-0CEC-4D4F-8818-461592DD97BE"))
				.Set(l => l.SU_IsPublished, false)
				.Post(TestConnection);

			using (MenuItem)
			{
				MenuItem.PerformClick();
				AssertEquals("Error: Unable to find Document to print. Please make sure the 'Pick Documents Pack' Document is published.", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region TestConstructor

		public void TestConstructor()
		{
			AssertEquals("Reprint Pick Documents Pack", MenuItem.Text);
		}

		#endregion

		#region TestOnClick_WithPickStatusOfBuilding

		public void TestOnClick_WithPickStatusOfBuilding()
		{
			Pick.WP_PickStatus = PickStatus.Codes.Building;
			MenuItem.PerformClick();
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Pick documents cannot be printed while pick is being built"));
		}

		#endregion

		#region TestOnClick

		public void TestOnClick()
		{
			Pick.WP_PickStatus = PickStatus.Codes.Created;
			MenuItem.PerformClick();
			AssertEquals("Pick Documents Pack", WhsDocumentPrinter.LastPrintedDocumentName);
		}

		#endregion

		#region Implementation

		WhsPick Pick => pick ?? (pick = Factory.New<WhsPick>());
		WhsPick pick;

		DocPackMenuItem MenuItem => menuItem ?? (menuItem = new DocPackMenuItem(Pick));
		DocPackMenuItem menuItem;

		#endregion
	}
}
