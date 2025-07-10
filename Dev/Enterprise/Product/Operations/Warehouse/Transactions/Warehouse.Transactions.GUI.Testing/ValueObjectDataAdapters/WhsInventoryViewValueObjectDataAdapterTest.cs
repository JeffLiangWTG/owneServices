using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

[TestedType(typeof(WhsInventoryViewValueObjectDataAdapter))]
sealed class WhsInventoryViewValueObjectDataAdapterTest : ValueObjectDataAdapterTest<WhsInventoryView, Xsd.WhsInventoryView>
{
	public void TestExport_FixedWidthLocation()
	{
		var data = new TestDataSimpleEnvironment(Factory, 2, 1);
		var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
		Helper.CreateRowAndGenerateLocations(warehouse, "AA", 5, 5, 5);
		Factory.Save();

		var receive = Helper.CreateWhsReceive(data.Org1, warehouse);
		var location = warehouse.FindLocation("AA050403");
		AssertNotNull("Precondition", location);
		var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);

		var xsdInventory = new WhsInventoryViewValueObjectDataAdapter().ExportToValueObject(inventoryLine, new ValueObjectExportContext(new NotificationBuffer()));
		AssertEquals("AA050403", xsdInventory.Location);
	}

	protected override string ExpectedRootCollectionElementName => "WhsInventoryViews";

	protected override string ExpectedRootElementName => "WhsInventoryView";

	protected override string[] XmlNodesToExcludeFromCoverageTest
	{
		get
		{
			var result = new List<string>();
			result.AddRange(base.XmlNodesToExcludeFromCoverageTest);
			result.Add("CustomAttribute1");
			result.Add("CustomAttribute2");
			result.Add("CustomAttribute3");
			return result.ToArray();
		}
	}

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		var emptyWhsInventoryPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsInventory.xml");
		return new BusinessObjectAndExpectedOutputFileName(Factory.New<WhsReceive>().Lines.AddNew().Inventory[0], emptyWhsInventoryPath, ValidationKind.None, "Empty Inventory");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		var populatedWhsInventoryPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsInventory.xml");
		return new BusinessObjectAndExpectedOutputFileName(PopulatedInventory, populatedWhsInventoryPath, ValidationKind.None, "Populated Inventory");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
	{
		var emptyWhsInventoryPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsInventory.xml");
		return new BusinessObjectAndExpectedOutputFileName(Factory.New<WhsReceive>().Lines.AddNew().Inventory[0], emptyWhsInventoryPath, ValidationKind.None, "Empty Inventory");
	}

	protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
	{
		return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
	}

	protected override ValueObjectDataAdapter<WhsInventoryView, Xsd.WhsInventoryView> GetNewBizObjXmlDataAdapter()
	{
		return new WhsInventoryViewValueObjectDataAdapter();
	}

	protected override bool IsImportFromValueObjectSupported => false;

	protected override bool IsCreateOrUpdateFromValueObjectSupported => false;

	protected override void SetUp()
	{
		base.SetUp();
		resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	protected override void TearDown()
	{
		base.TearDown();
		if (resourceRetriever.IsValueCreated)
		{
			resourceRetriever.Value.Dispose();
		}
	}

	Lazy<EmbeddedResourceRetriever> resourceRetriever;

	WhsInventoryView PopulatedInventory
	{
		get
		{
			if (populatedInventory == null)
			{
				var receive = Helper.CreateWhsReceive(Client, Warehouse);
				populatedInventory = Helper.CreateWhsReceiveInventoryLine(receive, Product, 100, new ZDate(2008, 12, 31), new ZDate(2008, 01, 01), "PA1", "PA2", "PA3", "BEK");
				populatedInventory.WI_TotalUnits = 100m;

				populatedInventory.WI_PalletID = "LP0001";
				populatedInventory.WI_WL = Warehouse.Areas[0].PickLocations[0].PK;
				populatedInventory.WI_ArrivalDate = new ZDateTimeOffset(2008, 08, 21);

				var classification = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();
				classification[CusClassificationSchema.CC_ClassificationType.Name] = "IMP";
				classification[CusClassificationSchema.CC_LookupCode.Name] = "LOOKUP";
				classification[CusClassificationSchema.CC_TariffNum.Name] = "9901.22.23";
				classification[CusClassificationSchema.CC_Description.Name] = "DESCRIPTION";

				var pivot = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
				pivot[CusClassPartPivotSchema.CI_CC.Name] = classification.PK;
				pivot[CusClassPartPivotSchema.CI_OP.Name] = Product.PK;

				populatedInventory.CustomsData.WB_EntryDate = new ZDateTime(2008, 02, 03);
				populatedInventory.CustomsData.WB_EntryLineNo = 1;
				populatedInventory.CustomsData.WB_EntryKey = "ENTRYKEY";
				populatedInventory.CustomsData.WB_DeclarationReference = "DEC REFERENCE";
				populatedInventory.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
				populatedInventory.CustomsData.WB_CustomsQty = 1.23;
				populatedInventory.CustomsData.WB_CustomsUnitOfQty = "EA";
				populatedInventory.CustomsData.WB_ValueForDuty = 190.89m;
				populatedInventory.CustomsData.WB_TILV = 100.67m;
				populatedInventory.CustomsData.WB_AddInfo = "Additional Info";
			}
			return populatedInventory;
		}
	}

	WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
	WhsTestHelperFunctions helper;

	WhsWarehouse Warehouse => warehouse ?? (warehouse = Helper.CreateWarehouse("TEST", "ROW1", 2, 2));
	WhsWarehouse warehouse;

	OrgHeader Client => client ?? (client = Helper.CreateClient());
	OrgHeader client;

	OrgSupplierPart Product
	{
		get
		{
			if (product == null)
			{
				product = Helper.CreateProduct(Client, "P0001");
				product.OP_RH_NKCommodityCode = "GOLD";
			}
			return product;
		}
	}
	OrgSupplierPart product;

	WhsInventoryView populatedInventory;
}
