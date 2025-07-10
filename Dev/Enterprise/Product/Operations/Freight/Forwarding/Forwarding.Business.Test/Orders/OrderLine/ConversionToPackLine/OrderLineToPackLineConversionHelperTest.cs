using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineToPackLineConversionHelper))]
	sealed class OrderLineToPackLineConversionHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrderLinesCollection_NullOrdersList()
		{
			AssertNoExceptionThrown("Null Orders List should not throw null reference exception.", () => new OrderLineToPackLineConversionHelper(Factory, Shipment, null));
		}

		public void TestOrderLinesCollectionFilledProperly()
		{
			Helper = new OrderLineToPackLineConversionHelper(Factory, Shipment, new List<Order>());
			AssertEquals("No orderlines", 0, Helper.OrderLines.Count);

			Helper = CreateHelper();
			AssertEquals("All order's orderlines are in collection", 5, Helper.OrderLines.Count);
		}

		public void TestCreateDummyPackLine()
		{
			Helper = CreateHelper();
			AssertEquals("Precondition: All order's orderlines are in collection", 5, Helper.OrderLines.Count);

			List<OrderLine> mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[0]);
			mergeList.Add(Order1.OrderLines[1]);
			Helper.CreateDummyPackLine(mergeList);

			AssertEquals("Stand alone orderlines", 3, Helper.OrderLines.Count);
			AssertEquals("Dummy packlines", 1, Helper.DummyPackLines.Count);
			AssertEquals("Dummy packline one made from two orderlines", 2, Helper.DummyPackLines[0].MergedOrderLines.Count);

			mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[2]);
			mergeList.Add(Order2.OrderLines[0]);
			Helper.CreateDummyPackLine(mergeList);

			AssertEquals("Stand alone orderlines", 1, Helper.OrderLines.Count);
			AssertEquals("Dummy packlines", 2, Helper.DummyPackLines.Count);
			AssertEquals("Dummy packline one made from two orderlines", 2, Helper.DummyPackLines[0].MergedOrderLines.Count);
			AssertEquals("Dummy packline two made from two orderlines", 2, Helper.DummyPackLines[1].MergedOrderLines.Count);
		}

		public void TestUndoDummyPackLines()
		{
			Helper = CreateHelper();
			AssertEquals("Precondition: All order's orderlines are in collection", 5, Helper.OrderLines.Count);
			AssertEquals("Precondition: dummy packlines empty", 0, Helper.DummyPackLines.Count);

			List<OrderLine> mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[0]);
			mergeList.Add(Order1.OrderLines[1]);
			Helper.CreateDummyPackLine(mergeList);

			mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[2]);
			mergeList.Add(Order2.OrderLines[0]);
			Helper.CreateDummyPackLine(mergeList);

			AssertEquals("Stand alone orderlines", 1, Helper.OrderLines.Count);
			AssertEquals("Dummy packlines", 2, Helper.DummyPackLines.Count);

			List<DummyPackLine> dummyUndo = new List<DummyPackLine>();
			dummyUndo.Add(Helper.DummyPackLines[0]);
			Helper.UndoDummyPackLines(dummyUndo);
			AssertEquals("Stand alone orderlines", 3, Helper.OrderLines.Count);
			AssertEquals("Dummy packlines", 1, Helper.DummyPackLines.Count);

			dummyUndo = new List<DummyPackLine>();
			dummyUndo.Add(Helper.DummyPackLines[0]);
			Helper.UndoDummyPackLines(dummyUndo);
			AssertEquals("Stand alone orderlines", 5, Helper.OrderLines.Count);
			AssertEquals("Dummy packlines", 0, Helper.DummyPackLines.Count);
		}

		public void TestCreatePackLinesFromOrderLines()
		{
			Helper = CreateHelper();

			List<OrderLine> mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[0]);
			mergeList.Add(Order1.OrderLines[1]);
			Helper.CreateDummyPackLine(mergeList);

			mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[2]);
			mergeList.Add(Order2.OrderLines[0]);
			Helper.CreateDummyPackLine(mergeList);

			AssertEquals("Precondition: no packlines", 0, Shipment.OuterPackLines.Count);
			Helper.CreatePackLines();
			AssertEquals("New born packlines", 3, Shipment.OuterPackLines.Count);
			AssertEquals("New born inner packlines", 3, Shipment.InnerPackLines.Count);

			ForwardingPackLine firstPackLine = Shipment.OuterPackLines[0];
			ForwardingPackLine firstCombinedPackLine = Shipment.OuterPackLines[1];
			ForwardingPackLine secondCombinedPackLine = Shipment.OuterPackLines[2];

			PackLine firstInnerPackLine = Shipment.InnerPackLines[0];
			PackLine firstCombinedInnerPackLine = Shipment.InnerPackLines[1];
			PackLine secondCombinedInnerPackLine = Shipment.InnerPackLines[2];

			ZString firstPacklineDetailedDescription = "Additonal Info: order 2, line two";
			ZString firstCombinedPackLineDetailedDescription =
				Order1.JD_OrderNumber + "." + Order1.OrderLines[0].JO_LineNo + ": " + "Additonal Info: line one";
			ZString secondCombinedPackLineDetailedDescription =
				Order1.JD_OrderNumber + "." + Order1.OrderLines[2].JO_LineNo + ": " + "Additonal Info: line three\r\n" +
				Order2.JD_OrderNumber + "." + Order2.OrderLines[0].JO_LineNo + ": " + "Additonal Info: order 2, line one";

			AssertPackline(firstPackLine, 5, "BOX", 50m, "CF", 500m, "LB", "line five description", "AU", 500.50m, 1, new string[] { "SKECHERS" }, firstPacklineDetailedDescription);
			AssertPackline(firstCombinedPackLine, 3, "CTN", 10.566m, "M3", 190.718m, "KG", "", "AU", 300.30m, 2, new string[] { "BOOKS", "COOKIES" }, firstCombinedPackLineDetailedDescription);
			AssertPackline(secondCombinedPackLine, 7, "PKG", 1442.587m, "CF", 481.437m, "KG", "", "", 700.70m, 1, new string[] { "ICE-CREAM" }, secondCombinedPackLineDetailedDescription);

			AssertInnerPackline(firstInnerPackLine, 16, "CRT", "line five description");
			AssertInnerPackline(firstCombinedInnerPackLine, 23, "PKG", "");
			AssertInnerPackline(secondCombinedInnerPackLine, 27, "BOX", "");
		}

		public void TestCreatePacklineFromOrderlinesMergeDoesNotMergeDimensions()
		{
			Helper = CreateHelper();

			var mergeList = new List<OrderLine>();

			Order1.OrderLines[0].JO_OuterPacks = 1;
			Order1.OrderLines[0].JO_OuterPackHeight = 20;
			Order1.OrderLines[0].JO_OuterPackLength = 10;
			Order1.OrderLines[0].JO_OuterPackWidth = 30;
			Order1.OrderLines[0].JO_OuterPackUnitOfDimension = Core.Constants.Length.Metres;
			Order1.OrderLines[0].JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			Order1.OrderLines[0].JO_ActualVolume = 60;

			Order1.OrderLines[1].JO_OuterPacks = 1;
			Order1.OrderLines[1].JO_OuterPackHeight = 20;
			Order1.OrderLines[1].JO_OuterPackLength = 22;
			Order1.OrderLines[1].JO_OuterPackWidth = 30;
			Order1.OrderLines[1].JO_OuterPackUnitOfDimension = Core.Constants.Length.Metres;
			Order1.OrderLines[1].JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			Order1.OrderLines[1].JO_ActualVolume = 70;

			mergeList.Add(Order1.OrderLines[0]);
			mergeList.Add(Order1.OrderLines[1]);

			Helper.CreateDummyPackLine(mergeList);
			Helper.CreatePackLines();

			AssertEquals(0m, Shipment.OuterPackLines[Shipment.OuterPackLines.Count - 1].JL_Height);
			AssertEquals(0m, Shipment.OuterPackLines[Shipment.OuterPackLines.Count - 1].JL_Length);
			AssertEquals(0m, Shipment.OuterPackLines[Shipment.OuterPackLines.Count - 1].JL_Width);
			AssertEquals(Core.Constants.Length.Metres, Shipment.OuterPackLines[Shipment.OuterPackLines.Count - 1].JL_UnitOfDimension);
			AssertEquals(130m, Shipment.OuterPackLines[Shipment.OuterPackLines.Count - 1].JL_ActualVolume);
		}

		public void TestCreatePackLineFromStandAloneOrderLine()
		{
			Helper = CreateHelper();

			List<OrderLine> list = new List<OrderLine>();

			Order1.OrderLines[0].JO_OuterPacks = 1;
			Order1.OrderLines[0].JO_OuterPackHeight = 20;
			Order1.OrderLines[0].JO_OuterPackLength = 10;
			Order1.OrderLines[0].JO_OuterPackWidth = 30;
			Order1.OrderLines[0].JO_OuterPackUnitOfDimension = Core.Constants.Length.Metres;
			Order1.OrderLines[0].JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			Order1.OrderLines[0].JO_ActualVolume = 60;

			list.Add(Order1.OrderLines[0]);

			Helper.CreatePackLines();

			AssertEquals(20m, Shipment.OuterPackLines[0].JL_Height);
			AssertEquals(10m, Shipment.OuterPackLines[0].JL_Length);
			AssertEquals(30m, Shipment.OuterPackLines[0].JL_Width);
			AssertEquals(Core.Constants.Length.Metres, Shipment.OuterPackLines[0].JL_UnitOfDimension);
			AssertEquals(60m, Shipment.OuterPackLines[0].JL_ActualVolume);
			AssertEquals(Core.Constants.Volume.CubicMetres, Shipment.OuterPackLines[0].JL_ActualVolumeUQ);
		}

		public void TestProductsMaximumLength()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var helper = new OrderLineToPackLineConversionHelper(Factory, Factory.New<ForwardingShipment>(), new Order[] { order });

			int totalProductsLength = 0;

			Action<string> addOrderLine = partNo =>
				{
					var orderLine = order.OrderLines.AddNew();
					orderLine.JO_Partno = partNo;
					totalProductsLength += partNo.Length;
				};

			addOrderLine("ABC");
			addOrderLine("DEF");

			Assert("Combined length of order line part numbers is less than DummyPackLine.ProductsMaxLength", totalProductsLength < DummyPackLine.ProductsMaxLength);
			AssertNoExceptionThrown(() => helper.CreateDummyPackLine(order.OrderLines));
			AssertEquals("Products should not trim", "ABC, DEF", helper.DummyPackLines[0].Products);

			for (int i = 0; i < 60; i++)
			{
				addOrderLine("ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789");
			}

			Assert("Combined length of order line part numbers is greater than DummyPackLine.ProductsMaxLength", totalProductsLength > DummyPackLine.ProductsMaxLength);
			AssertNoExceptionThrown(() => helper.CreateDummyPackLine(order.OrderLines));
			AssertEquals("Products should trim to max length", DummyPackLine.ProductsMaxLength, helper.DummyPackLines[1].Products.Length);
		}

		public void TestGetReasonForNotAbleToConvert_ShipmentIsMaster_ShouldRetrunReason()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.OrderLines.AddNew();
			order.OrderLines.AddNew();

			var subShipment = Factory.New<ForwardingShipment>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = "ASM";
			shipment.CoLoadShipments.Add(subShipment);

			var helperToTest = new OrderLineToPackLineConversionHelper(Factory, shipment, new[] { order });

			AssertEquals(
				"Expected reason returned",
				"You have attached orders to the 'Assembly Master' shipment. Order lines cannot be linked to the pack lines of the 'Assembly Master' shipment",
				helperToTest.ReasonForNotAbleToConvert);
		}

		public void TestCreatePackLines_ShipmentIsMaster_ShouldNotCreatePackLines()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.OrderLines.AddNew();
			order.OrderLines.AddNew();

			var subShipment = Factory.New<ForwardingShipment>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = "ASM";
			shipment.CoLoadShipments.Add(subShipment);

			var helperToTest = new OrderLineToPackLineConversionHelper(Factory, shipment, new[] { order });
			helperToTest.CreatePackLines();

			AssertEquals("Pack lines should not be created", 0, shipment.InnerPackLines.Count);
			AssertEquals("Pack lines should not be created", 0, shipment.OuterPackLines.Count);
		}

		public void TestCreatePackLinesFromOrderLines_NotCopyEmptyUnits()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			FillOrderLine(line, 1, "CTN", 11, "ENV", 10m, string.Empty, 100m, string.Empty, "an order line ", "AU", 100.10m, "BOOKS", "CONT1", "Some");

			var shipment = Factory.New<ForwardingShipment>();
			var helperToTest = new OrderLineToPackLineConversionHelper(Factory, shipment, new[] { order });

			helperToTest.CreatePackLines();

			AssertEquals("New born packlines", 1, shipment.OuterPackLines.Count);
			AssertEquals("New born inner packlines", 1, shipment.InnerPackLines.Count);

			AssertEquals("KG", shipment.OuterPackLines[0].JL_ActualWeightUQ);
			AssertEquals("M3", shipment.OuterPackLines[0].JL_ActualVolumeUQ);
			AssertEquals("M", shipment.InnerPackLines[0].JL_UnitOfDimension);
			AssertEquals("KG", shipment.InnerPackLines[0].JL_ActualWeightUQ);
			AssertEquals("M3", shipment.InnerPackLines[0].JL_ActualVolumeUQ);
			AssertEquals("M", shipment.InnerPackLines[0].JL_UnitOfDimension);
		}

		void AssertPackline(ForwardingPackLine packline, ZInt outerPacks, ZString outerPacksUnit, ZDecimal volume, string volumeUnit, ZDecimal weight, string weightUnit, string description, string origin, ZDecimal linePrice, int productsCount, string[] products, ZString detailedDescription)
		{
			AssertEquals("Outer Packs", outerPacks, packline.JL_PackageCount);
			AssertEquals("Outer Packs Unit", outerPacksUnit, packline.JL_F3_NKPackType);

			AssertEquals("Volume", volume, packline.JL_ActualVolume);
			AssertEquals("Volume unit", volumeUnit, packline.JL_ActualVolumeUQ);

			AssertEquals("Weight", weight, packline.JL_ActualWeight);
			AssertEquals("Weight unit", weightUnit, packline.JL_ActualWeightUQ);

			AssertEquals("Description", description, packline.JL_Description);
			AssertEquals("DetailedDescription", detailedDescription, packline.JL_DetailedDescription);
			AssertEquals("Origin", origin, packline.JL_RN_NKOrigin);
			AssertEquals("LinePrice", linePrice, packline.JL_LinePrice);

			AssertEquals("PackProducts count", productsCount, packline.Products.Count);

			foreach (string productExpected in products)
			{
				bool productFound = false;
				foreach (PackProduct product in packline.Products)
				{
					if (productExpected == product.D2_ProductCode)
					{
						productFound = true;
					}
				}

				AssertEquals("Product code", true, productFound);
			}
		}

		void AssertInnerPackline(PackLine innerPackline, ZInt innerPacks, ZString innerPacksUnit, string description)
		{
			AssertEquals("Inner Packs", innerPacks, innerPackline.JL_PackageCount);
			AssertEquals("Inner Packs Unit", innerPacksUnit, innerPackline.JL_F3_NKPackType);
			AssertEquals("Description", description, innerPackline.JL_Description);
		}

		public void TestAddContainerLink()
		{
			Helper = CreateHelper();

			List<OrderLine> mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[0]);
			mergeList.Add(Order1.OrderLines[1]);
			Helper.CreateDummyPackLine(mergeList);

			mergeList = new List<OrderLine>();
			mergeList.Add(Order1.OrderLines[2]);
			mergeList.Add(Order2.OrderLines[0]);
			Helper.CreateDummyPackLine(mergeList);

			AssertEquals("Precondition: no packlines", 0, Shipment.OuterPackLines.Count);
			Helper.CreatePackLines();
			AssertEquals("New born packlines", 3, Shipment.OuterPackLines.Count);

			ForwardingPackLine firstPackLine = Shipment.OuterPackLines[0];
			ForwardingPackLine firstCombinedPackLine = Shipment.OuterPackLines[1];
			ForwardingPackLine secondCombinedPackLine = Shipment.OuterPackLines[2];

			AssertEquals("Container link", Container2.PK, firstPackLine.JL_JC);
			AssertEquals("Container link", Container1.PK, firstCombinedPackLine.JL_JC);
			AssertEquals("Container link to the last container from consol", Container3.PK, secondCombinedPackLine.JL_JC);
		}

		public void TestShouldShowSplitOrders()
		{
			var order1 = Factory.New<Order>();
			var line1 = order1.OrderLines.AddNew();
			FillOrderLine(line1, 1, "CTN", 11, "ENV", 10m, "M3", 100m, "KG", "an order line that's unsplittable", "AU", 100.10m, "BOOKS", "CONT1", "Some extra info");
			line1.JO_Quantity = 5m;

			var orderList = new List<Order>();
			orderList.Add(order1);
			var helper = new OrderLineToPackLineConversionHelper(Factory, Shipment, orderList);

			AssertEquals("Expected false as helper does not to contain any orders that can be split", false, helper.ShouldShowSplitOrders);

			line1.JO_QtyReceived = 2m;
			line1.JO_QtyInvoiced = 2m;

			AssertEquals("Expected true as ordr lines quantity received/invoiced does not meet quantity for the line and the line has not been split before", true, helper.ShouldShowSplitOrders);

			line1.JO_QtyReceived = 5m;
			line1.JO_QtyInvoiced = 5m;

			AssertEquals("Expected false as invoiced now meets quantity for the line", false, helper.ShouldShowSplitOrders);

			var order2 = Factory.New<Order>();
			var line2 = order1.OrderLines.AddNew();
			FillOrderLine(line2, 2, "CTN", 11, "ENV", 10m, "M3", 100m, "KG", "order to split", "AU", 100.10m, "BOOKS", "CONT1", "Some extra info again");
			line2.JO_Quantity = 20m;
			line2.JO_QtyReceived = 12m;
			line2.JO_QtyInvoiced = 10m;

			orderList.Add(order2);
			helper = new OrderLineToPackLineConversionHelper(Factory, Shipment, orderList);

			AssertEquals("Expected true as second order has a line that can be split", true, helper.ShouldShowSplitOrders);
		}

		OrderLineToPackLineConversionHelper Helper;
		Order Order1;
		Order Order2;
		ForwardingShipment Shipment;
		ForwardingContainer Container1;
		ForwardingContainer Container2;
		ForwardingContainer Container3;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Container1 = consol.Containers.AddNew();
			Container1.JC_ContainerNum = "CONT1";
			Container2 = consol.Containers.AddNew();
			Container2.JC_ContainerNum = "CONT2";
			Container3 = consol.Containers.AddNew();
			Container3.JC_ContainerNum = "CONT3";

			Shipment = consol.Shipments.AddNew();

			Order1 = Factory.New<Order>();
			OrderLine line = Order1.OrderLines.AddNew();
			FillOrderLine(line, 1, "CTN", 11, "ENV", 10m, "M3", 100m, "KG", "line one description", "AU", 100.10m, "BOOKS", "CONT1", "Additonal Info: line one");

			line = Order1.OrderLines.AddNew();
			FillOrderLine(line, 2, "CTN", 12, "DOZ", 20m, "CF", 200m, "LB", "line two description", "AU", 200.20m, "COOKIES", "CONT1", ZString.Empty);

			line = Order1.OrderLines.AddNew();
			FillOrderLine(line, 3, "PLT", 13, "BOX", 30m, "CF", 300m, "KG", "line three description", "NZ", 300.30m, "ICE-CREAM", "CONT1", "Additonal Info: line three");

			Order2 = Factory.New<Order>();
			line = Order2.OrderLines.AddNew();
			FillOrderLine(line, 4, "GRS", 14, "BOX", 40m, "M3", 400m, "LB", "line four description", "MD", 400.40m, "", "", "Additonal Info: order 2, line one");

			line = Order2.OrderLines.AddNew();
			FillOrderLine(line, 5, "BOX", 16, "CRT", 50m, "CF", 500m, "LB", "line five description", "AU", 500.50m, "SKECHERS", "CONT2", "Additonal Info: order 2, line two");
		}

		void FillOrderLine(OrderLine orderline, ZInt outerPacks, ZString outerPacksType, ZInt innerPacks, ZString innerPacksType, ZDecimal volume, string volumeUnit, ZDecimal weight, string weightUnit, string description, string origin, ZDecimal linePrice, string product, string containerNumber, ZString additionalInformation)
		{
			orderline.JO_OuterPacks = new ZDecimal(outerPacks);
			orderline.JO_OuterPacksUQ = outerPacksType;

			orderline.JO_InnerPacks = new ZDecimal(innerPacks);
			orderline.JO_InnerPacksUQ = innerPacksType;

			orderline.JO_ActualVolume = volume;
			orderline.JO_UnitOfVolume = volumeUnit;

			orderline.JO_ActualWeight = weight;
			orderline.JO_UnitOfWeight = weightUnit;

			orderline.JO_Description = description;
			orderline.JO_RN_NKCountryOfOrigin = origin;
			orderline.JO_LinePrice = linePrice;
			orderline.JO_Partno = product;
			orderline.JO_ContainerNumber = containerNumber;

			orderline.JO_AdditionalInformation = additionalInformation;
		}

		OrderLineToPackLineConversionHelper CreateHelper()
		{
			List<Order> list = new List<Order>();
			list.Add(Order1);
			list.Add(Order2);
			return new OrderLineToPackLineConversionHelper(Factory, Shipment, list);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrderLineToPackLineConversionHelper helper = new OrderLineToPackLineConversionHelper(Factory, Factory.New<ForwardingShipment>(), new List<Order>());
			return helper;
		}

		#endregion
	}
}
