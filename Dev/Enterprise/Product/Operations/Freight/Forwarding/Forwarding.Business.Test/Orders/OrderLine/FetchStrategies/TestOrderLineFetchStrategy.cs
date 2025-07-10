using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class TestOrderLineFetchStrategy : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_Description = "Pick me";
			var orderLine2 = order1.OrderLines.AddNew();
			orderLine2.JO_Description = "Pick me";

			var order2 = Factory.NewWithValidTestData<Order>();
			var orderLine3 = order2.OrderLines.AddNew();
			orderLine3.JO_Description = "Pick me";
			var orderLine4 = order2.OrderLines.AddNew();
			orderLine4.JO_Description = "Pick me";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var allOrderLines = newFactory.Load<OrderLine>(new ZQuery(JobOrderLineSchema.JO_Description, "Pick me"));

			foreach (var orderLine in allOrderLines)
			{
				orderLine.FetchStrategy.FetchForView(TableColumnsInFetchForView);
			}

			foreach (var orderLine in allOrderLines)
			{
				var poke = orderLine.JO_Calc_OrderLineNoAndSubLineNo;
			}

			var expectedDbHits = new Dictionary<string, int>
			{
				[JobOrderLineSchema.Constants.TableName] = 2,
				[JobOrderHeaderSchema.Constants.TableName] = 1
			};

			AssertDbHits(expectedDbHits, newFactory);
		}

		public void TestLoadChildEditableObjects()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_Description = "Pick me";
			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_Description = "Pick me";
			var orderLine3 = order.OrderLines.AddNew();
			orderLine3.JO_Description = "Pick me";
			Factory.Save();

			var factory2 = NewFactory();
			order = factory2.Load<Order>(order.PK);

			using (AssertDbHitsForAllFactories(ignoreUnspecified: true, thresholdForUnspecified: 2, includeFactoryPredicate: f => f == factory2, expectedHitCounts: new Dictionary<string, int>
				{
					{ JobOrderLineDeliverySchema.Constants.TableName, 1 },
					{ JobComInvHeaderChargeSchema.Constants.TableName, 2 /*one for order charges and one for line charges*/ },
					{ JobDocAddressSchema.Constants.TableName, 2 }, // once for order, and once for orderLines
					{ ProcessTasksSchema.Constants.TableName, 2 }
				}))
			{
				order.LoadChildEditableObjects();
			}
		}

		TableColumn[] TableColumnsInFetchForView
		{
			get
			{
				return new TableColumn[]
				{
						new TableColumn(JobOrderLineSchema.Constants.TableName, OrderLine.Schema.JO_Calc_OrderLineNoAndSubLineNo),
						new TableColumn(JobOrderLineSchema.Constants.TableName, "Order+JD_OrderNumber")
				};
			}
		}
	}
}
