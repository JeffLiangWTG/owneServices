using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	sealed class WhsItemDispatchConsignmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewForJobColums()
		{
			var jobStatus = nameof(WhsItemDispatchConsignment.JobHeader) + "+" + nameof(WhsItemDispatchConsignment.JobHeader.JH_Status);
			var whsItemDispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			whsItemDispatchConsignment.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, jobStatus) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var holdReason = nameof(WhsItemDispatchConsignment.JobHeader) + "+" + nameof(WhsItemDispatchConsignment.JobHeader.JH_HoldReason);
			whsItemDispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			whsItemDispatchConsignment.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, holdReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var profitLossReason = nameof(WhsItemDispatchConsignment.JobHeader) + "+" + nameof(WhsItemDispatchConsignment.JobHeader.JH_ProfitLossReasonCode);
			whsItemDispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			whsItemDispatchConsignment.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, profitLossReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var margin = nameof(WhsItemDispatchConsignment.JobHeader) + "+" + nameof(WhsItemDispatchConsignment.JobHeader.JH_TotalProfitRevenueMargin);
			whsItemDispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			whsItemDispatchConsignment.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, margin) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));
		}

		public void TestFetchForView_HoldReason()
		{
			AssertFetchForView(nameof(WhsItemDispatchConsignment.JobHeader) + "+" + JobHeader.Schema.JH_HoldReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_JobStatus()
		{
			AssertFetchForView(nameof(WhsItemDispatchConsignment.JobHeader) + "+" + JobHeader.Schema.JH_Status, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_ProfitLossReason()
		{
			var profitLossReason = nameof(WhsItemDispatchConsignment.JobHeader) + "+" + nameof(WhsItemDispatchConsignment.JobHeader.JH_ProfitLossReasonCode);
			AssertFetchForView(profitLossReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_TotalProfitRevenueMargin()
		{
			var margin = nameof(WhsItemDispatchConsignment.JobHeader) + "+" + nameof(WhsItemDispatchConsignment.JobHeader.JH_TotalProfitRevenueMargin);
			AssertFetchForView(margin, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateWhsItemDispatchConsignment();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var headers = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
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

		WhsItemDispatchConsignment CreateWhsItemDispatchConsignment()
			=> Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
	}
}
