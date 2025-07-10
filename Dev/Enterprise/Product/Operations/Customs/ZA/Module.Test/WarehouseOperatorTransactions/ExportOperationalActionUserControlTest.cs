using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module.Testing
{
	class ExportOperationalActionUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var context = new TestExportApplicator(Factory);
			context.Build(Array.Empty<ZGuid>());
			using (var form = new ZForm(context) { CaptionRenderingEnabled = true })
			using (var control = new ExportOperationalActionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = form.FindSingleOrDefault<ZGrid>("TransactionsDisplayGrid");
				AssertNotNull("Find TransactionsDisplayGrid", grid);

				AssertColumn(grid, "Select", "Select?", 60);
				AssertColumn(grid, "TransactionType", "Transaction Type", 75);
				AssertColumn(grid, "TransactionDate", "Transaction Date", 90);
				AssertColumn(grid, "ExportType", "Export Type", 75);
				AssertColumn(grid, "OwnerReference", "Owner Reference", 100);
			}
		}

		public void TestLayoutForLockedApplicator()
		{
			var lockedContext = new TestExportApplicator(Factory);
			lockedContext.Build(Array.Empty<ZGuid>());

			var blockedContext = new TestExportApplicator(Factory);
			blockedContext.Build(Array.Empty<ZGuid>());

			using (var form = new ZForm(lockedContext) { CaptionRenderingEnabled = true })
			using (var control = new ExportOperationalActionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("First user should own lock", lockedContext.HasLock);
				var grid = form.FindSingleOrDefault<ZGrid>("TransactionsDisplayGrid");
				AssertNotNull("Find TransactionsDisplayGridTransactionsDisplayGrid should be visible when lock set", grid);
				Assert("TransactionsDisplayGrid should be visible when lock set", grid.Visible);
				var lockInfoLabel = form.FindSingleOrDefault<ZLabel>("LockInfoLabel");
				AssertNotNull("Find LockInfoLabel", lockInfoLabel);
				Assert("LockInfoLabel should be hidden when lock set", !lockInfoLabel.Visible);
			}

			using (var form = new ZForm(blockedContext) { CaptionRenderingEnabled = true })
			using (var control = new ExportOperationalActionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("Second user should be blocked", !blockedContext.HasLock);
				var grid = form.FindSingleOrDefault<ZGrid>("TransactionsDisplayGrid");
				AssertNotNull("Find TransactionsDisplayGridTransactionsDisplayGrid should be visible when lock set", grid);
				Assert("TransactionsDisplayGrid should be hidden when lock not set", !grid.Visible);
				var lockInfoLabel = form.FindSingleOrDefault<ZLabel>("LockInfoLabel");
				AssertNotNull("Find LockInfoLabel", lockInfoLabel);
				Assert("LockInfoLabel should be visible when lock not set set", lockInfoLabel.Visible);
			}
		}

		void AssertColumn(ZGrid grid, string columnName, string expectedCaption, int expectedWidth)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			AssertEquals($"{columnName} Caption", expectedCaption, grid.GetColumnCaption(columnName));
			AssertEquals($"{columnName} Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedWidth), columnStyle.Width);
		}

		class TestExportApplicator : BaseExportApplicator
		{
			public TestExportApplicator(BusinessObjectFactory factory) : base("Test Applicator", factory, "EXP")
			{
			}

			protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
			{
			}
		}
	}
}
