using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingPackingLineDataObjectReaderTest : ShipmentDataObjectReadingHelperTest
	{
		#region Pack Line Products From PackedItemCollection

		public void TestPackingLineCollectionAddsPackLineProducts()
		{
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3 },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 4 }
			});

			shipmentDataObject.PackingLineCollection[0].SetPackedItemCollection(() => new List<PackedItem>
			{
				 new PackedItem
				{
					PackedQuantity = 1,
					UnitOfQuantity = new PackageType { Code = "CNT" },
					Product = new Product { Code = "Camel Poodles" }
				},
				new PackedItem
				{
					PackedQuantity = 2,
					UnitOfQuantity = new PackageType { Code = "CNT" },
					Product = new Product { Code = "Panda Chow Chows" }
				}
			});

			shipmentDataObject.PackingLineCollection[1].SetPackedItemCollection(() => new List<PackedItem>
			{
				new PackedItem
				{
					PackedQuantity = 4,
					UnitOfQuantity = new PackageType { Code = "BAG" },
					Product = new Product { Code = "Shark Cats" }
				}
			});

			Logger.ClearLogs();

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(2, shipmentBO.OuterPackLines[0].Products.Count);
			AssertEquals(1, shipmentBO.OuterPackLines[1].Products.Count);

			var product = shipmentBO.OuterPackLines[0].Products[0];
			AssertEquals("Camel Poodles", product.D2_ProductCode);
			AssertEquals(1m, product.D2_ProductQuantity);
			AssertEquals("CNT", product.D2_ProductUnitOfQty);

			product = shipmentBO.OuterPackLines[0].Products[1];
			AssertEquals("Panda Chow Chows", product.D2_ProductCode);
			AssertEquals(2m, product.D2_ProductQuantity);
			AssertEquals("CNT", product.D2_ProductUnitOfQty);

			product = shipmentBO.OuterPackLines[1].Products[0];
			AssertEquals("Shark Cats", product.D2_ProductCode);
			AssertEquals(4m, product.D2_ProductQuantity);
			AssertEquals("BAG", product.D2_ProductUnitOfQty);
		}

		public void TestPackingLineCollectionUpdatesSinglePackLineProduct()
		{
			var shipmentBOToMatch = Factory.New<ForwardingShipment>();
			shipmentBOToMatch.JS_HouseBill = "EEVEELUTIONS";

			var outerPackLineToMatch = shipmentBOToMatch.OuterPackLines.AddNew();
			outerPackLineToMatch.JL_PackageCount = 7;
			outerPackLineToMatch.JL_Height = 10m;
			outerPackLineToMatch.JL_Width = 15m;
			outerPackLineToMatch.JL_ActualWeight = 250m;

			var productToOverride = outerPackLineToMatch.Products.AddNew();
			productToOverride.D2_ProductCode = "Eevee";
			productToOverride.D2_ProductQuantity = 7;
			productToOverride.D2_ProductUnitOfQty = "PLT";

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "EEVEELUTIONS";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 7,
				Weight = 250m,
				Height = 10m,
				Width = 15m,
			};
			packingLine.SetPackedItemCollection(() => new List<PackedItem>
			{
				new PackedItem
				{
					PackedQuantity = 1,
					UnitOfQuantity = new PackageType { Code = "CNT" },
					Product = new Product { Code = "Flareon" }
				},

				new PackedItem
				{
					PackedQuantity = 2,
					UnitOfQuantity = new PackageType { Code = "BOX" },
					Product = new Product { Code = "Espeon" }
				},

				new PackedItem
				{
					PackedQuantity = 4,
					UnitOfQuantity = new PackageType { Code = "BOX" },
					Product = new Product { Code = "Glaceon" }
				}
			});
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			Logger.ClearLogs();

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to only have one pack lines otherwise we can't match to existing pack line", 1, shipmentBO.OuterPackLines.Count);
			AssertEquals("Expected to have products on the pack lines", 3, shipmentBO.OuterPackLines[0].Products.Count);

			var product = shipmentBO.OuterPackLines[0].Products[0];
			AssertEquals("Expected to have overriden the original product", "Flareon", product.D2_ProductCode);
			AssertEquals("Expected to have overriden the original product", 1m, product.D2_ProductQuantity);
			AssertEquals("Expected to have overriden the original product", "CNT", product.D2_ProductUnitOfQty);

			product = shipmentBO.OuterPackLines[0].Products[1];
			AssertEquals("Espeon", product.D2_ProductCode);
			AssertEquals(2m, product.D2_ProductQuantity);
			AssertEquals("BOX", product.D2_ProductUnitOfQty);

			product = shipmentBO.OuterPackLines[0].Products[2];
			AssertEquals("Glaceon", product.D2_ProductCode);
			AssertEquals(4m, product.D2_ProductQuantity);
			AssertEquals("BOX", product.D2_ProductUnitOfQty);
		}

		public void TestPackingLineCollectionUpdatesSinglePackLineProduct_DoesNotDeleteExistingProductsIfNotImportingProducts()
		{
			var shipmentBOToMatch = Factory.New<ForwardingShipment>();
			shipmentBOToMatch.JS_HouseBill = "EEVEELUTIONS";

			var outerPackLineToMatch = shipmentBOToMatch.OuterPackLines.AddNew();
			outerPackLineToMatch.JL_PackageCount = 7;
			outerPackLineToMatch.JL_Height = 10m;
			outerPackLineToMatch.JL_Width = 15m;
			outerPackLineToMatch.JL_ActualWeight = 250m;

			var productToOverride = outerPackLineToMatch.Products.AddNew();
			productToOverride.D2_ProductCode = "Eevee";
			productToOverride.D2_ProductQuantity = 1;
			productToOverride.D2_ProductUnitOfQty = "PLT";

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "EEVEELUTIONS";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 7, PackType = new PackageType { Code = "PKG" }, Weight = 250m, Height = 10m, Width = 15m };
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			Logger.ClearLogs();

			AssertNull("Pre-condition", shipmentDataObject.PackingLineCollection[0].PackedItemCollection);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to only have one pack line or we can't match", 1, shipmentBO.OuterPackLines.Count);
			AssertEquals("Expected to have a product", 1, shipmentBO.OuterPackLines[0].Products.Count);

			var product = shipmentBO.OuterPackLines[0].Products[0];
			AssertEquals("Expected not to have removed the original product", "Eevee", product.D2_ProductCode);
			AssertEquals("Expected not to have removed the original product", 1m, product.D2_ProductQuantity);
			AssertEquals("Expected not to have removed the original product", "PLT", product.D2_ProductUnitOfQty);
		}

		public void TestPackingLineCollectionUpdatesSinglePackLineProduct_MissingOrInvalidPackedItemData()
		{
			var shipmentBOToMatch = Factory.New<ForwardingShipment>();
			shipmentBOToMatch.JS_HouseBill = "SHIP0000034";

			var outerPackLineToMatch = shipmentBOToMatch.OuterPackLines.AddNew();
			outerPackLineToMatch.JL_PackageCount = 1;

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "SHIP0000034";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var emptyPackedItemDataObject = new PackedItem();
			var invalidPackedItemDataObject = new PackedItem
			{
				PackedQuantity = -5,
				UnitOfQuantity = new PackageType { Code = "TOOLONG" },
				Product = new Product { Code = "!!!" }
			};

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 7,
				Weight = 250m,
				Height = 10m,
				Width = 15m,
			};
			packingLine.SetPackedItemCollection(() => new List<PackedItem> { emptyPackedItemDataObject, invalidPackedItemDataObject });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			Logger.ClearLogs();

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to only have one pack lines otherwise we can't match to existing pack line", 1, shipmentBO.OuterPackLines.Count);
			AssertEquals("Expected to have products on the pack lines", 2, shipmentBO.OuterPackLines[0].Products.Count);

			var product = shipmentBO.OuterPackLines[0].Products[0];
			AssertEquals("Expected to be default", "", product.D2_ProductCode);
			AssertEquals("Expected to be default", 0m, product.D2_ProductQuantity);
			AssertEquals("Expected to be default", "", product.D2_ProductUnitOfQty);

			product = shipmentBO.OuterPackLines[0].Products[1];
			AssertEquals("!!!", product.D2_ProductCode);
			AssertEquals(-5m, product.D2_ProductQuantity);
			AssertEquals("Expected to have cropped the text that was too long", "TOO", product.D2_ProductUnitOfQty);
		}

		#endregion

		public void TestPackingLineCollectionAddsInnerPackLines()
		{
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, PackType = new PackageType { Code = Core.Constants.PkgUnit.Package } };
			packingLine1.SetPackingLineCollection(() => new List<PackingLine>
			{
				new PackingLine { PackQty = 3, PackType = new PackageType { Code = Core.Constants.PkgUnit.Carton } },
				new PackingLine { PackQty = 4, PackType = new PackageType { Code = Core.Constants.PkgUnit.Bag } }
			});
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = Core.Constants.PkgUnit.Pallet } };
			packingLine2.SetPackingLineCollection(() => new List<PackingLine>
			{
				new PackingLine { PackQty = 5, PackType = new PackageType { Code = Core.Constants.PkgUnit.Box } }
			});
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });

			Logger.ClearLogs();
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();
			var outerPackLine = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackageCount == 1 && l.JL_F3_NKPackType == Core.Constants.PkgUnit.Package);
			AssertNotNull("An outer packline with 1 PKG should exist", outerPackLine);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_PackageCount == 3 && l.JL_F3_NKPackType == Core.Constants.PkgUnit.Carton);
			AssertNotNull("An inner packline with 3 CTN should exist", innerPackLine);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_PackageCount == 4 && l.JL_F3_NKPackType == Core.Constants.PkgUnit.Bag);
			AssertNotNull("An inner packline with 4 BAG should exist", innerPackLine);

			outerPackLine = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackageCount == 2 && l.JL_F3_NKPackType == Core.Constants.PkgUnit.Pallet);
			AssertNotNull("An outer packline with 2 PLT should exist", outerPackLine);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_PackageCount == 5 && l.JL_F3_NKPackType == Core.Constants.PkgUnit.Box);
			AssertNotNull("An inner packline with 5 BOX should exist", innerPackLine);
		}
	}
}
