using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsStocktakeFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_Job_JH_ProfitLossReasonCode()
			=> AssertFetchForView(
				nameof(WhsStocktake.Job) + "+" + nameof(WhsStocktake.Job.JH_ProfitLossReasonCode),
				new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_JH_TotalProfitRevenueMargin()
			=> AssertFetchForView(
				nameof(WhsStocktake.Job) + "+" + nameof(WhsStocktake.Job.JH_TotalProfitRevenueMargin),
				new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateWhsStocktake();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var whsStocktakes = newFactory.Load<WhsStocktake>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var whsStocktake in whsStocktakes)
			{
				whsStocktake.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var whsStocktake in whsStocktakes)
			{
				_ = whsStocktake[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		WhsStocktake CreateWhsStocktake() => Factory.NewWithValidTestData<WhsStocktake>();
	}
}
