using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	sealed class SundryChargesFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_Job_JH_ProfitLossReasonCode()
			=> AssertFetchForView("Job+JH_ProfitLossReasonCode", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_Job_JH_TotalProfitRevenueMargin()
			=> AssertFetchForView("Job+JH_TotalProfitRevenueMargin", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateSundryCharges();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var allSundryCharges = newFactory.Load<SundryCharges>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var sundryCharges in allSundryCharges)
			{
				sundryCharges.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var sundryCharges in allSundryCharges)
			{
				_ = sundryCharges[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		SundryCharges CreateSundryCharges() => Factory.NewWithValidTestData<SundryCharges>();
	}
}
