using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class InvoicingFilterControlTest : TestCaseWithFactory
	{
		#region Column Initialisation

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = new InvoicingFilterControl())
			{
				var profitLossReason = nameof(WhsInvoice.JobHeader) + "+" + nameof(WhsInvoice.JobHeader.JH_ProfitLossReasonCode);
				var column = filterControl.FilteredGrid.GetColumnStyle(profitLossReason);
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = new InvoicingFilterControl())
			{
				var margin = nameof(WhsInvoice.JobHeader) + "+" + nameof(WhsInvoice.JobHeader.JH_TotalProfitRevenueMargin);
				var column = filterControl.FilteredGrid.GetColumnStyle(margin);
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		#endregion

		public void TestFetchForView_JH_ProfitLossReasonCode()
		{
			var profitLossReason = nameof(WhsInvoice.JobHeader) + "+" + nameof(WhsInvoice.JobHeader.JH_ProfitLossReasonCode);
			AssertFetchForView(profitLossReason, new Dictionary<string, int> { { JobHeaderSchema.Constants.TableName, 1 } });
		}

		public void TestFetchForView_JH_TotalProfitRevenueMargin()
		{
			var margin = nameof(WhsInvoice.JobHeader) + "+" + nameof(WhsInvoice.JobHeader.JH_TotalProfitRevenueMargin);
			AssertFetchForView(margin, new Dictionary<string, int> { { JobHeaderSchema.Constants.TableName, 1 } });
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateWhsInvoice();
			}

			Factory.Save();

			var newFactory = NewFactory();
			var headers = newFactory.Load<WhsInvoice>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var header in headers)
			{
				header.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, propertyName) });
			}

			foreach (var header in headers)
			{
				_ = header[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		WhsInvoice CreateWhsInvoice() => Factory.NewWithValidTestData<WhsInvoice>();
	}
}
