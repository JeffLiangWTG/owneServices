using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	sealed class WhsItemReceiveTransportationUnitFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewForJobColums()
		{
			var jobStatus = nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + nameof(WhsItemReceiveTransportationUnit.JobHeader.JH_Status);
			var whsItemReceiveTransportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			whsItemReceiveTransportationUnit.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, jobStatus) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var holdReason = nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + nameof(WhsItemReceiveTransportationUnit.JobHeader.JH_HoldReason);
			whsItemReceiveTransportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			whsItemReceiveTransportationUnit.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, holdReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var profitLossReason = nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + nameof(WhsItemReceiveTransportationUnit.JobHeader.JH_ProfitLossReasonCode);
			whsItemReceiveTransportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			whsItemReceiveTransportationUnit.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, profitLossReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var margin = nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + nameof(WhsItemReceiveTransportationUnit.JobHeader.JH_TotalProfitRevenueMargin);
			whsItemReceiveTransportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			whsItemReceiveTransportationUnit.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, margin) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));
		}

		public void TestFetchForView_HoldReason()
		{
			AssertFetchForView(nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + JobHeader.Schema.JH_HoldReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_JobStatus()
		{
			AssertFetchForView(nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + JobHeader.Schema.JH_Status, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_ProfitLossReason()
		{
			var profitLossReason = nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + nameof(WhsItemReceiveTransportationUnit.JobHeader.JH_ProfitLossReasonCode);
			AssertFetchForView(profitLossReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_TotalProfitRevenueMargin()
		{
			var margin = nameof(WhsItemReceiveTransportationUnit.JobHeader) + "+" + nameof(WhsItemReceiveTransportationUnit.JobHeader.JH_TotalProfitRevenueMargin);
			AssertFetchForView(margin, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateWhsItemReceiveTransportationUnit();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var headers = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
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

		WhsItemReceiveTransportationUnit CreateWhsItemReceiveTransportationUnit()
			=> Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
	}
}
