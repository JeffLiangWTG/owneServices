using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobMawbFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_JH_ProfitLossReasonCode()
			=> AssertFetchForView("Job+JH_ProfitLossReasonCode", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_JH_TotalProfitRevenueMargin()
			=> AssertFetchForView("Job+JH_TotalProfitRevenueMargin", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateJobMawb();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var jobs = newFactory.Load<JobMawb>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var jobMawb in jobs)
			{
				jobMawb.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var jobMawb in jobs)
			{
				_ = jobMawb[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		JobMawb CreateJobMawb() => Factory.NewWithValidTestData<JobMawb>();
	}
}
