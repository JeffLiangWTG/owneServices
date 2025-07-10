using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class EntriesAndEntryLinesUserControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			using (var control = new EntriesWithMessagesOnDeclarationUserControl())
			{
				try
				{
					var messageControl = control.NewMessageUserControl;
					messageControl.JobDeclaration = declaration;
					messageControl.Show();
					var grid = messageControl.Controls.Find("EntriesBoundGrid", true)[0] as ZGrid;
					var vatColumn = grid.GetColumnStyle(CusEntryHeader.Schema.VAT);
					Assert(!vatColumn.IsUnavailable);
					var dutyColumn = grid.GetColumnStyle(CusEntryHeader.Schema.Duty);
					Assert(!dutyColumn.IsUnavailable);
					var totalDutyColumn = grid.GetColumnStyle(CusEntryHeader.Schema.TotalDutyAmount);
					Assert(totalDutyColumn.IsUnavailable);
					var gstColumn = grid.GetColumnStyle(CusEntryHeader.Schema.GSTAmount);
					Assert(gstColumn.IsUnavailable);
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					Assert(vatColumn.IsUnavailable);
					Assert(dutyColumn.IsUnavailable);
					Assert(!totalDutyColumn.IsUnavailable);
					Assert(!gstColumn.IsUnavailable);
				}
				finally
				{
					control.NewMessageUserControl.Dispose();
				}
			}

			using (var control = new EntriesAndEntryLinesUserControl())
			{
				AssertNotNull(control);
			}
		}

		public void TestWarehouseTransactionStatusColumns()
		{
			var mock = new Mock<EntriesAndEntryLinesUserControl>() { CallBase = true };
			mock.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(true);
			using (var control = mock.Object)
			{
				AssertNotNull("CH_WarehouseTransactionStatus", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus));
				AssertNotNull("CH_WarehouseTransactionStatusDescription", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription));
			}
			mock = new Mock<EntriesAndEntryLinesUserControl>() { CallBase = true };
			mock.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(false);
			using (var control = mock.Object)
			{
				AssertNull("CH_WarehouseTransactionStatus", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus));
				AssertNull("CH_WarehouseTransactionStatusDescription", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription));
			}
		}

		public void TestHasManualWhsUpdateColumn()
		{
			var mock1 = new Mock<EntriesAndEntryLinesUserControl>() { CallBase = true };
			mock1.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(true);
			using (var control = mock1.Object)
			{
				AssertNotNull("CH_HasManualWhsUpdate", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate));
			}

			var mock2 = new Mock<EntriesAndEntryLinesUserControl> { CallBase = true };
			mock2.Protected().Setup<bool>("SupportWarehouseTransactionStatusColumns").Returns(false);
			using (var control = mock2.Object)
			{
				AssertNull("CH_HasManualWhsUpdate", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate));
			}
		}
	}
}
