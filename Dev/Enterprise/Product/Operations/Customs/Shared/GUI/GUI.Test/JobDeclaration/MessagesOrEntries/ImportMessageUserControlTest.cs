using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TestImportMessageUserControl : TestCase
	{
		public void TestRemoveAction()
		{
			using (var control = new ImportMessageUserControl())
			{
				AssertEquals(RemoveAction.NoRemovePossible, control.FindSingle<ZGrid>("EntryLineGrid").RemoveAction);
			}
		}

		public void TestWarehouseTransactionStatusColumns()
		{
			var mock = new Mock<ImportMessageUserControl>() { CallBase = true };
			mock.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(true);
			using (var control = mock.Object)
			{
				AssertNotNull("CH_WarehouseTransactionStatus", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus));
				AssertNotNull("CH_WarehouseTransactionStatusDescription", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription));
			}
			mock = new Mock<ImportMessageUserControl>() { CallBase = true };
			mock.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(false);
			using (var control = mock.Object)
			{
				AssertNull("CH_WarehouseTransactionStatus", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus));
				AssertNull("CH_WarehouseTransactionStatusDescription", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription));
			}
		}

		public void TestHasManualWhsUpdateColumn()
		{
			var mock = new Mock<ImportMessageUserControl> { CallBase = true };
			mock.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(true);
			using (var control = mock.Object)
			{
				AssertNotNull("CH_HasManualWhsUpdate", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate));
			}
			mock = new Mock<ImportMessageUserControl> { CallBase = true };
			mock.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(false);
			using (var control = mock.Object)
			{
				AssertNull("CH_HasManualWhsUpdate", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate));
			}
		}

		public void TestBondedWarehouseGridContextMenuExists()
		{
			using (var control = new ImportMessageUserControl())
			{
				AssertNotNull("Should find the Inventory Management menu.", control.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Inventory Management"));
			}
		}

		public void TestAddEntryHeaderColumns()
		{
			using (var control = new ImportMessageUserControl())
			{
				CombineAssertions(() =>
				{
					var grid = control.EntriesBoundGrid;
					var bGMReferenceColumn = grid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference);
					AssertNotNull("User control should have CH_BGMReference column", bGMReferenceColumn);
					AssertEquals("CH_BGMReference Column is readonly", true, bGMReferenceColumn.IsReadOnly);

					var entryNumberColumn = grid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber);
					AssertNotNull("User control should have EntryNumber column", entryNumberColumn);
					AssertEquals("EntryNumber Column is readonly", true, entryNumberColumn.IsReadOnly);

					var packagesCountColumn = grid.GetColumnStyle(CusEntryHeader.Schema.PackagesCount);
					AssertNotNull("User control should have PackagesCount column", packagesCountColumn);
					AssertEquals("PackagesCount Column is readonly", true, packagesCountColumn.IsReadOnly);

					var messageTypeColumn = grid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageType);
					AssertNotNull("User control should have CH_MessageType column", messageTypeColumn);
					AssertEquals("CH_MessageType Column is readonly", true, messageTypeColumn.IsReadOnly);

					var messageTypeDescriptionColumn = grid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageTypeDescription);
					AssertNotNull("User control should have CH_MessageTypeDescription column", messageTypeDescriptionColumn);
					AssertEquals("CH_MessageTypeDescription Column is readonly", true, messageTypeDescriptionColumn.IsReadOnly);
				});
			}
		}

		public void TestGetBaseMessagesTabUserControlType()
		{
			using (var control = new ImportMessageUserControlForTest())
			{
				AssertEquals("MessagesTab is correct type", typeof(BaseMessagesTabUserControl), control.GetBaseMessagesTabUserControlTypeExposed());
			}
		}

		sealed class ImportMessageUserControlForTest : ImportMessageUserControl
		{
			public Type GetBaseMessagesTabUserControlTypeExposed() => base.GetBaseMessagesTabUserControlType();
		}
	}
}
