using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Test.Shipment
{
	sealed class GatePassShipmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_JH_ProfitLossReasonCode()
			=> AssertFetchForView("Job+JH_ProfitLossReasonCode", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_JH_TotalProfitRevenueMargin()
			=> AssertFetchForView("Job+JH_TotalProfitRevenueMargin", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateGatePassShipment();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var gatePassShipments = newFactory.Load<GatePassShipment>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var gatePassShipment in gatePassShipments)
			{
				gatePassShipment.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var gatePassShipment in gatePassShipments)
			{
				_ = gatePassShipment[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		GatePassShipment CreateGatePassShipment() => Factory.NewWithValidTestData<GatePassShipment>();
	}
}
