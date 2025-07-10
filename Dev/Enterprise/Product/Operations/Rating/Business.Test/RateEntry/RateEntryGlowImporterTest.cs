using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class RateEntryGlowImporterTest : RatingTestCase
	{
		public void TestWarehouseIsConverted()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "PRW";
			warehouse.WW_WarehouseCode = "WAR";
			Factory.Save();

			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			rateEntry.TI_ParentID = ZGuid.Empty;
			rateEntry.TI_ParentTableCode = ZString.Empty;
			var importer = new RateEntryGlowImporter(); 
			var logger = new Mock<INotifications>();

			importer.ConvertCustomLine(rateEntry, "WAR", logger.Object, 0, "Warehouse");
			AssertEquals("After conversion, the warehouse is entered and Parent ID is filled", warehouse.PK, rateEntry.TI_ParentID);
			AssertEquals("After conversion, the warehouse is entered and Parent Table Code is filled", "WW", rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.IsAny<INotification>()), Times.Never());
		}

		public void TestWarehouseNotFound_ShowLog()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			rateEntry.TI_ParentID = ZGuid.Empty;
			rateEntry.TI_ParentTableCode = ZString.Empty;
			var importer = new RateEntryGlowImporter();
			var logger = new Mock<INotifications>();

			importer.ConvertCustomLine(rateEntry, "WAR", logger.Object, 0, "Warehouse");
			AssertEquals("After conversion, parent id is still empty as there's nothing found", ZGuid.Empty, rateEntry.TI_ParentID);
			AssertEquals("After conversion, parent table code is still empty as there's nothing found", ZString.Empty, rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.Is<INotification>(x => x.Message == "Row 0, No Warehouse exists with the code: WAR")), Times.Once);
		}

		public void TestYardIsConverted()
		{
			var yard = Factory.NewWithValidTestData<WhsWarehouse>();
			yard.WW_WarehouseType = "CYD";
			yard.WW_WarehouseCode = "YAR";
			Factory.Save();

			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			rateEntry.TI_ParentID = ZGuid.Empty;
			rateEntry.TI_ParentTableCode = ZString.Empty;
			var importer = new RateEntryGlowImporter();
			var logger = new Mock<INotifications>();

			importer.ConvertCustomLine(rateEntry, "YAR", logger.Object, 0, "Yard");
			AssertEquals("After conversion, the yard is entered and Parent ID is filled", yard.PK, rateEntry.TI_ParentID);
			AssertEquals("After conversion, the yard is entered and Parent Table Code is filled", "WW", rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.IsAny<INotification>()), Times.Never());
		}

		public void TestYardNotFound_ShowLog()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			rateEntry.TI_ParentID = ZGuid.Empty;
			rateEntry.TI_ParentTableCode = ZString.Empty;
			var importer = new RateEntryGlowImporter();
			var logger = new Mock<INotifications>();

			importer.ConvertCustomLine(rateEntry, "YAR", logger.Object, 0, "Yard");
			AssertEquals("After conversion, parent id is still empty as there's nothing found", ZGuid.Empty, rateEntry.TI_ParentID);
			AssertEquals("After conversion, parent table code is still empty as there's nothing found", ZString.Empty, rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.Is<INotification>(x => x.Message == "Row 0, No Yard exists with the code: YAR")), Times.Once);
		}

		public void TestYardNotFoundButWarehouseFound_ShowLog()
		{
			var yard = Factory.NewWithValidTestData<WhsWarehouse>();
			yard.WW_WarehouseType = "PRW";
			yard.WW_WarehouseCode = "YAR";
			Factory.Save();

			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			rateEntry.TI_ParentID = ZGuid.Empty;
			rateEntry.TI_ParentTableCode = ZString.Empty;
			var importer = new RateEntryGlowImporter();
			var logger = new Mock<INotifications>();

			importer.ConvertCustomLine(rateEntry, "YAR", logger.Object, 0, "Yard");
			AssertEquals("After conversion, the warehouse is entered and Parent ID is filled", yard.PK, rateEntry.TI_ParentID);
			AssertEquals("After conversion, the warehouse is entered and Parent Table Code is filled", "WW", rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.IsAny<INotification>()), Times.Never());
		}

		public void TestWarehouseAndYardEntered_ShowLog()
		{
			var yard = Factory.NewWithValidTestData<WhsWarehouse>();
			yard.WW_WarehouseType = "CYD";
			yard.WW_WarehouseCode = "YAR";

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "PRW";
			warehouse.WW_WarehouseCode = "WAR";
			Factory.Save();

			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			rateEntry.TI_ParentID = ZGuid.Empty;
			rateEntry.TI_ParentTableCode = ZString.Empty;
			var importer = new RateEntryGlowImporter();
			var logger = new Mock<INotifications>();

			importer.ConvertCustomLine(rateEntry, "YAR", logger.Object, 0, "Yard");
			AssertEquals("After conversion, the yard is entered and Parent ID is filled", yard.PK, rateEntry.TI_ParentID);
			AssertEquals("After conversion, the yard is entered and Parent Table Code is filled", "WW", rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.IsAny<INotification>()), Times.Never());
			importer.ConvertCustomLine(rateEntry, "WAR", logger.Object, 0, "Warehouse");
			AssertEquals("After conversion, the warehouse is entered and Parent ID is filled", warehouse.PK, rateEntry.TI_ParentID);
			AssertEquals("After conversion, the warehouse is entered and Parent Table Code is filled", "WW", rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.Is<INotification>(x => x.Message == "A previous value has been set, for Yard and has been overwritten with a Warehouse, with code: WAR")), Times.Once);
		}

		public void TestAllWarehouses()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			rateEntry.TI_ParentID = new ZGuid();
			rateEntry.TI_ParentTableCode = ZString.Empty;
			var importer = new RateEntryGlowImporter();
			var logger = new Mock<INotifications>();

			importer.ConvertCustomLine(rateEntry, "", logger.Object, 0, "WAR");
			AssertEquals("After conversion, the warehouse is entered and Parent ID is empty", ZGuid.Empty, rateEntry.TI_ParentID);
			AssertEquals("After conversion, the warehouse is entered and Parent Table Code is filled", "WW", rateEntry.TI_ParentTableCode);
			logger.Verify(x => x.Add(It.IsAny<INotification>()), Times.Never());
		}

		public void TestImportChildlessChildren_ShowLog()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var importer = new RateEntryGlowImporter();
			var logger = new Mock<INotifications>();

			AssertEquals("Returns false for import childless children", false, importer.ImportChildlessChildren(rateEntry, logger.Object, 0, [], []));
			logger.Verify(x => x.Add(It.Is<INotification>(x => x.Message == "Row 0, RateEntry: RateLines without RateLineItems are not supported.")), Times.Once);
		}
	}
}
