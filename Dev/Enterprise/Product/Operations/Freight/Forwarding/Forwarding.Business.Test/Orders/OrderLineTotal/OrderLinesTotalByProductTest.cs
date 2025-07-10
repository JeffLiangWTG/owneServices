using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLinesTotalByProduct))]
	sealed class OrderLinesTotalByProductTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWithoutOrderLines()
		{
			var orderLinesTotalByProduct = new OrderLinesTotalByProduct(Factory);
			AssertEquals("", orderLinesTotalByProduct.Product);
			AssertEquals("", orderLinesTotalByProduct.ProductDescription);
			AssertEquals(0M, orderLinesTotalByProduct.Quantity);
			AssertEquals(0M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(0M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(0M, orderLinesTotalByProduct.QuantityRemaining);
		}

		public void TestProductDescription()
		{
			var order = Factory.New<Order>();
			List<OrderLine> lines = new List<OrderLine>();
			var orderLine = order.OrderLines.AddNew();
			lines.Add(orderLine);
			var orderLinesTotalByProduct = new OrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals("", orderLinesTotalByProduct.ProductDescription);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "111222";
			part.OP_Desc = "desc";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			order.SupplierPK = org.PK;
			part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Supplier);
			orderLine.JO_Partno = part.OP_PartNum;

			orderLinesTotalByProduct = new OrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals("desc", orderLinesTotalByProduct.ProductDescription);

			orderLine.JO_Partno = "22-333";
			orderLine.JO_Description = "STLTH 5 CS";
			orderLinesTotalByProduct = new OrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;

			AssertEquals("Expected order line product description to be overriden by order line description", "STLTH 5 CS", orderLinesTotalByProduct.ProductDescription);
		}

		public void TestQuantities()
		{
			var order = Factory.New<Order>();
			List<OrderLine> lines = new List<OrderLine>();
			var orderLine = order.OrderLines.AddNew();
			lines.Add(orderLine);
			orderLine.JO_Quantity = 3.00M;
			orderLine.JO_QtyInvoiced = 2.00M;
			orderLine.JO_QtyReceived = 1.00M;
			orderLine.JO_InnerPacks = 4;
			orderLine.JO_OuterPacks = 5;

			orderLine = order.OrderLines.AddNew();
			lines.Add(orderLine);
			orderLine.JO_Quantity = 4.50M;
			orderLine.JO_QtyInvoiced = 3.50M;
			orderLine.JO_QtyReceived = 1.50M;
			orderLine.JO_InnerPacks = 6;
			orderLine.JO_OuterPacks = 7;

			var orderLinesTotalByProduct = new OrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;

			AssertEquals(7.5M, orderLinesTotalByProduct.Quantity);
			AssertEquals(5.5M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(2.5M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(5.0M, orderLinesTotalByProduct.QuantityRemaining);
			AssertEquals(10, orderLinesTotalByProduct.InnerPacks);
			AssertEquals(12, orderLinesTotalByProduct.OuterPacks);

			//orderLine.JO_Quantity = 5.50M;
			orderLine.JO_QtyInvoiced = 4.50M;
			orderLine.JO_QtyReceived = 2.50M;
			orderLine.JO_InnerPacks = 7;
			orderLine.JO_OuterPacks = 8;

			AssertEquals(7.5M, orderLinesTotalByProduct.Quantity);
			AssertEquals(5.5M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(2.5M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(5.0M, orderLinesTotalByProduct.QuantityRemaining);
			AssertEquals(10, orderLinesTotalByProduct.InnerPacks);
			AssertEquals(12, orderLinesTotalByProduct.OuterPacks);

			orderLinesTotalByProduct = new OrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals(7.5M, orderLinesTotalByProduct.Quantity);
			AssertEquals(6.5M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(3.5M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(4.0M, orderLinesTotalByProduct.QuantityRemaining);
			AssertEquals(11, orderLinesTotalByProduct.InnerPacks);
			AssertEquals(13, orderLinesTotalByProduct.OuterPacks);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrderLinesTotalByProduct(Factory);
		}

		#endregion
	}
}
