using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	sealed class WhsItemReceiveConsignmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewForJobColums()
		{
			var jobStatus = nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_Status);
			var whsItemReceiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			whsItemReceiveConsignment.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, jobStatus) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var holdReason = nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_HoldReason);
			whsItemReceiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			whsItemReceiveConsignment.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, holdReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var profitLossReason = nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_ProfitLossReasonCode);
			whsItemReceiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			whsItemReceiveConsignment.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, profitLossReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var margin = nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_TotalProfitRevenueMargin);
			whsItemReceiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			whsItemReceiveConsignment.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, margin) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));
		}

		public void TestFetchForView_HoldReason()
		{
			AssertFetchForView(nameof(WhsItemReceiveConsignment.JobHeader) + "+" + JobHeader.Schema.JH_HoldReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_JobStatus()
		{
			AssertFetchForView(nameof(WhsItemReceiveConsignment.JobHeader) + "+" + JobHeader.Schema.JH_Status, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_ProfitLossReason()
		{
			var profitLossReason = nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_ProfitLossReasonCode);
			AssertFetchForView(profitLossReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_TotalProfitRevenueMargin()
		{
			var margin = nameof(WhsItemReceiveConsignment.JobHeader) + "+" + nameof(WhsItemReceiveConsignment.JobHeader.JH_TotalProfitRevenueMargin);
			AssertFetchForView(margin, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateWhsItemReceiveConsignment();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var headers = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var header in headers)
			{
				header.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var header in headers)
			{
				_ = header.ZPropertyInfoHash.GetPropertySafe(propertyName).Value;
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		WhsItemReceiveConsignment CreateWhsItemReceiveConsignment()
			=> Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
	}
}
