using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Test.Consol
{
	sealed class CFSLoadListConsolFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForViewForJobColums()
		{
			var jobStatus = nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_Status);
			var cFSLoadListConsol = Factory.New<CFSLoadListConsol>();
			cFSLoadListConsol.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, jobStatus) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var holdReason = nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_HoldReason);
			cFSLoadListConsol = Factory.New<CFSLoadListConsol>();
			cFSLoadListConsol.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, holdReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var profitLossReason = nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_ProfitLossReasonCode);
			cFSLoadListConsol = Factory.New<CFSLoadListConsol>();
			cFSLoadListConsol.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, profitLossReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var margin = nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_TotalProfitRevenueMargin);
			cFSLoadListConsol = Factory.New<CFSLoadListConsol>();
			cFSLoadListConsol.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, margin) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));
		}

		public void TestFetchForView_HoldReason()
		{
			AssertFetchForView(nameof(CFSLoadListConsol.Job) + "+" + JobHeader.Schema.JH_HoldReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_JobStatus()
		{
			AssertFetchForView(nameof(CFSLoadListConsol.Job) + "+" + JobHeader.Schema.JH_Status, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_ProfitLossReason()
		{
			AssertFetchForView(nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_ProfitLossReasonCode), new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_Margin()
		{
			AssertFetchForView(nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_TotalProfitRevenueMargin), new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateCFSLoadListConsol();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var headers = newFactory.Load<CFSLoadListConsol>(new ZQuery());
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

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			_ = CreateCFSLoadListConsol();

			return new CFSLoadListConsolCollection(Factory);
		}

		CFSLoadListConsol CreateCFSLoadListConsol()
		{
			var header = Factory.New<CFSLoadListConsol>();

			return header;
		}
	}
}
