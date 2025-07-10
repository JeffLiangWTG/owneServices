using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLinesTotalByProductCollection))]
	sealed class OrderLinesTotalByProductCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrderLinesTotalByProductCollection>
	{
		public void TestPopulation()
		{
			var order = Factory.New<Order>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			order.SupplierPK = org.PK;

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "111222";
			part1.OP_Desc = "desc";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "111333";
			part2.OP_Desc = "desc 2";
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_Quantity = 3.00M;
			orderLine1.JO_QtyInvoiced = 2.00M;
			orderLine1.JO_QtyReceived = 1.00M;
			orderLine1.JO_Partno = part1.OP_PartNum;

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_Quantity = 4.50M;
			orderLine2.JO_QtyInvoiced = 3.50M;
			orderLine2.JO_QtyReceived = 1.50M;
			orderLine2.JO_Partno = part1.OP_PartNum;

			var orderLine3 = order.OrderLines.AddNew();
			orderLine3.JO_Quantity = 5M;
			orderLine3.JO_QtyInvoiced = 4M;
			orderLine3.JO_QtyReceived = 3M;
			orderLine3.JO_Partno = part2.OP_PartNum;

			var orderLine4 = order.OrderLines.AddNew();
			orderLine4.JO_Quantity = 6M;
			orderLine4.JO_QtyInvoiced = 5M;
			orderLine4.JO_QtyReceived = 4M;
			orderLine4.JO_Description = "no product";

			var orderLine5 = order.OrderLines.AddNew();
			orderLine5.JO_Quantity = 8M;
			orderLine5.JO_QtyInvoiced = 5M;
			orderLine5.JO_QtyReceived = 4.9M;
			orderLine5.JO_Partno = "111444";
			orderLine5.JO_Description = "manually added part";

			var totals = new OrderLinesTotalByProductCollection(order, Factory);
			AssertEquals(0, totals.Count);

			totals.Populate();
			AssertEquals(4, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(7.5M, total.Quantity);
					AssertEquals(5.5M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(5M, total.Quantity);
					AssertEquals(4M, total.QuantityInvoiced);
					AssertEquals(3M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else if (total.OrderLines[0].JO_Partno == "111444")
				{
					AssertCorrectForOrderLine5(total);
				}
				else if (total.Product == "")
				{
					AssertEquals("no product", total.ProductDescription);
					AssertEquals(6M, total.Quantity);
					AssertEquals(5M, total.QuantityInvoiced);
					AssertEquals(4M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else
				{
					Fail("Incorrect Product Code");
				}
			}

			orderLine2.JO_QtyInvoiced = 2.55M;
			orderLine2.JO_QtyReceived = 1.50M;
			AssertEquals(4, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(7.5M, total.Quantity);
					AssertEquals(5.5M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(5M, total.Quantity);
					AssertEquals(4M, total.QuantityInvoiced);
					AssertEquals(3M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else if (total.OrderLines[0].JO_Partno == "111444")
				{
					AssertCorrectForOrderLine5(total);
				}
				else if (total.Product == "")
				{
					AssertEquals("no product", total.ProductDescription);
					AssertEquals(6M, total.Quantity);
					AssertEquals(5M, total.QuantityInvoiced);
					AssertEquals(4M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else
				{
					Fail("Incorrect Product Code");
				}
			}

			totals.Populate();
			AssertEquals(4, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(7.5M, total.Quantity);
					AssertEquals(4.55M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(5M, total.Quantity);
					AssertEquals(4M, total.QuantityInvoiced);
					AssertEquals(3M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else if (total.OrderLines[0].JO_Partno == "111444")
				{
					AssertCorrectForOrderLine5(total);
				}
				else if (total.Product == "")
				{
					AssertEquals("no product", total.ProductDescription);
					AssertEquals(6M, total.Quantity);
					AssertEquals(5M, total.QuantityInvoiced);
					AssertEquals(4M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else
				{
					Fail("Incorrect Product Code");
				}
			}

			order.OrderLines.Delete(orderLine5);
			totals.Populate();
			AssertEquals(3, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(7.5M, total.Quantity);
					AssertEquals(4.55M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(5M, total.Quantity);
					AssertEquals(4M, total.QuantityInvoiced);
					AssertEquals(3M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else if (total.Product == "")
				{
					AssertEquals("no product", total.ProductDescription);
					AssertEquals(6M, total.Quantity);
					AssertEquals(5M, total.QuantityInvoiced);
					AssertEquals(4M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else
				{
					Fail("Incorrect Product Code");
				}
			}

			order.OrderLines.Delete(orderLine4);
			AssertEquals(3, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(7.5M, total.Quantity);
					AssertEquals(4.55M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(5M, total.Quantity);
					AssertEquals(4M, total.QuantityInvoiced);
					AssertEquals(3M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else if (total.Product != "")
				{
					Fail("Incorrect Product Code");
				}
			}

			totals.Populate();
			AssertEquals(2, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(7.5M, total.Quantity);
					AssertEquals(4.55M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(5M, total.Quantity);
					AssertEquals(4M, total.QuantityInvoiced);
					AssertEquals(3M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else
				{
					Fail("Incorrect Product Code");
				}
			}

			orderLine1.JO_Quantity = 4.00M;
			totals.Populate();
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(8.5M, total.Quantity);
					AssertEquals(4.55M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(6.0M, total.QuantityRemaining);
				}
			}

			orderLine2.JO_Partno = part2.OP_PartNum;
			totals.Populate();
			AssertEquals(2, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(4M, total.Quantity);
					AssertEquals(2M, total.QuantityInvoiced);
					AssertEquals(1M, total.QuantityReceived);
					AssertEquals(3M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(9.5M, total.Quantity);
					AssertEquals(6.55M, total.QuantityInvoiced);
					AssertEquals(4.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else
				{
					Fail("Incorrect Product Code");
				}
			}
		}

		#region Population Assert Statements

		void AssertCorrectForOrderLine5(OrderLinesTotalByProduct total)
		{
			AssertEquals("manually added part", total.ProductDescription);
			AssertEquals(8M, total.Quantity);
			AssertEquals(5M, total.QuantityInvoiced);
			AssertEquals(4.9M, total.QuantityReceived);
			AssertEquals(3.1M, total.QuantityRemaining);
		}

		#endregion

		#region Implementation

		protected override OrderLinesTotalByProductCollection GetCollectionToTest()
		{
			return new OrderLinesTotalByProductCollection(Factory.New<Order>(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrderLinesTotalByProduct(Factory);
		}

		#endregion
	}
}
