using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestWhsPickCollectionFetchStrategy_TransportCodes()
		{
			var columns = new[] { new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.TransportCodes)) };
			TestWhsPickCollectionFetchStrategyCore(columns, true);
		}

		public void TestWhsPickCollectionFetchStrategy_ConsigneeCodes()
		{
			var columns = new[] { new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.ConsigneeCodes)) };
			TestWhsPickCollectionFetchStrategyCore(columns, true);
		}

		public void TestWhsPickCollectionFetchStrategy_ConsigneeNames()
		{
			var columns = new[] { new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.ConsigneeNames)) };
			TestWhsPickCollectionFetchStrategyCore(columns, true);
		}

		public void TestWhsPickCollectionFetchStrategy_DistributionCentreCodes()
		{
			var columns = new[] { new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.DistributionCentreCodes)) };
			TestWhsPickCollectionFetchStrategyCore(columns, true);
		}

		public void TestWhsPickCollectionFetchStrategy_OtherColumn()
		{
			var columns = new[] {
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.ClientCodes)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.ClientNames)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.EarliestRequiredDate)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.WP_WL_DockDoor)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.ServiceLevels)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.PickPriority)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.SalesChannelDescriptions)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.TransportReferences)),
				new TableColumn(WhsPickSchema.Constants.TableName, nameof(WhsPick.WP_WW_Whs))
			};
			TestWhsPickCollectionFetchStrategyCore(columns, false);
		}

		void TestWhsPickCollectionFetchStrategyCore(TableColumn[] columns, bool expectAddFetchHint)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("Co");
			var part = helper.CreateProduct(client, "p1");
			var warehouse = helper.CreateWarehouse("W", "L");
			Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(client, warehouse, "Order1", part, 1);
			var order2 = helper.CreateWhsOrderWithOrderLine(client, warehouse, "Order2", part, 1);

			var transportCo = helper.CreateClient("T");
			order1.TransportCoPK = transportCo.PK;
			order2.TransportCoPK = transportCo.PK;
			var pick = helper.CreatePickNew(order1, order2);

			var viewFactory = new BusinessObjectFactory();
			var query = new ZQuery(WhsDocketSchema.WD_WP, pick.PK);

			var collection = new WhsLegacyPickableDocketCollection(viewFactory, query);
			var pickCollectionFetchStrategy = new WhsPickCollectionFetchStrategy(collection);
			AssertEquals("Precondition", 0, viewFactory.ActiveFetchHintsForTable(WhsDocketSchema.Constants.TableName));
			pickCollectionFetchStrategy.FetchForView(pick.Orders.ToArray(), columns);
			AssertEquals("Should add fetch hint when accessing WhsPick's column.", expectAddFetchHint ? 1 : 0, viewFactory.ActiveFetchHintsForTable(WhsDocketSchema.Constants.TableName));
		}
	}
}
