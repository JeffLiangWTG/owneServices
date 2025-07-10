using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Module.Testing.ReceiveConsignment
{
	sealed class WhsTransitReceiveConsignmentFilterControlTest : TestCaseWithFactory
	{
		public void TestHoldReasonColumn()
			=> TestColumnVisibility("Hold Reason",
				nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_HoldReason),
				expectedColumnVisible: false);

		public void TestProfitLossReasonColumn()
			=> TestColumnVisibility("Profit/Loss Reason",
				nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_ProfitLossReasonCode),
				expectedColumnVisible: false);

		public void TestMarginColumn()
			=> TestColumnVisibility("Margin%",
				nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_TotalProfitRevenueMargin),
				expectedColumnVisible: false);

		void TestColumnVisibility(string columnDisplayName, string columnName, bool expectedColumnVisible)
		{
			using (var filterControl = GetFilterControl())
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == columnName && col.IsVisible == expectedColumnVisible);

				Assert($"{columnDisplayName} column should exist and should {(expectedColumnVisible ? "" : "NOT ")}be visible.", columnExistsAndNotVisible);
			}
		}

		WhsTransitReceiveConsignmentFilterControl GetFilterControl()
			=> new (new WhsItemReceiveConsignmentCollection(Factory), new WhsTransitReceiveConsignmentFilterBusinessObject());
	}
}
