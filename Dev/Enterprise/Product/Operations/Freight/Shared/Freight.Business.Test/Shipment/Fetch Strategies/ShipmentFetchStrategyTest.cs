using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	public class ShipmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			CombineAssertions(() =>
			{
				CheckDBHitCount(nameof(CommonShipment.NumbersAsString), 1, true);
				CheckDBHitCount(nameof(CommonShipment.NumbersAsString), 12, false);
			});
		}

		#region Implementation

		void CheckDBHitCount(string columnName, int maxDbHits, bool fetchForView)
		{
			var factory = new BusinessObjectFactory();

			var shipments = CreateShipments(factory);
			if (fetchForView)
			{
				foreach (var shipment in shipments)
				{
					shipment.FetchStrategy.FetchForView(new[] { new TableColumn("", columnName) });
				}
			}
			factory.ResetDatabaseLoadCount();

			foreach (var container in shipments)
			{
				object value = container[columnName];
			}

			var message = ZString.Format("column name: {0}, max db hit count: {1}, actual count: {2}", columnName, maxDbHits, factory.DatabaseLoadCount);

			Assert(message, factory.DatabaseLoadCount <= maxDbHits);
		}

		static List<CommonShipment> CreateShipments(BusinessObjectFactory factory)
		{
			List<CommonShipment> result = new List<CommonShipment>();

			for (int i = 0; i < 12; i++)
			{
				result.Add(factory.New<CommonShipment>());
			}

			factory.Save();

			return result;
		}

		#endregion
	}
}
