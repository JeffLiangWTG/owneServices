using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsJobHistoryReceiveDetailTest : WhsTestCaseWithFactory
	{
		#region TestView_WithProductCategoryFilter

		public void TestView_WithProductCategoryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea", categorySoftDrinks);
			var productCoke = Helper.CreateProduct(client, "Coke", categorySoftDrinks);
			var productVB = Helper.CreateProduct(client, "VB", categoryBeers);

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productCoke, 200m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productTea, 10m, true, true);

			Factory.Save();

			AssertEquals("ResultSet Count", 1, LoadView_WithProductCategory(categoryBeers.PK).Count);
			AssertEquals("ResultSet Count", 2, LoadView_WithProductCategory(categorySoftDrinks.PK).Count);
			AssertEquals("ResultSet Count", 3, LoadView_WithProductCategory(categoryBeverages.PK).Count);
			AssertEquals("ResultSet Count", 3, LoadView_WithProductCategory(ZGuid.Empty).Count);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = categoryPK.IsEmpty
				? @"select * from WhsJobHistoryReceiveDetailReport(null)"
				: @"select * from WhsJobHistoryReceiveDetailReport(@ProductCategoryPK)";
			var sqlParams = new ZSqlParameterCollection();
			if (!categoryPK.IsEmpty)
			{
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView

		public void TestView()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("1", "1");
			var part = Helper.CreateProduct(client, "3");
			var receives = SetupData(whs, client, part, commodity);
			var result = LoadView(whs, client, part, commodity);

			AssertEquals(3, result.Count);

			var docketLines = new List<WhsDocketLine>();
			var tempDocketLines = new List<WhsDocketLine>();
			tempDocketLines.AddRange(receives[0].Lines);
			tempDocketLines.AddRange(receives[1].Lines);
			tempDocketLines.AddRange(receives[2].Lines);
			tempDocketLines.AddRange(receives[3].Lines);
			tempDocketLines.AddRange(receives[4].Lines);

			foreach (var docketLine in tempDocketLines)
			{
				var docket = docketLine.Docket;
				if (docketLine.WE_OP == part.PK && docket.WD_WW_Whs == whs.PK && docket.WD_OH_Client == client.PK)
				{
					docketLines.Add(docketLine);
				}
			}

			AssertEquals(3, docketLines.Count);

			docketLines.OrderBy(x => x.WE_TransactionQuantity);

			AssertResult(docketLines[0], result[0], commodity);
			AssertResult(docketLines[1], result[1], commodity);
			AssertResult(docketLines[2], result[2], commodity);
		}

		public void TestView_DistributionCentre()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("1", "1");
			var distributionCentre = Helper.CreateClient("C3");
			var part = Helper.CreateProduct(client, "3");
			var receives = SetupData(whs, client, part, commodity);
			receives[0].DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			Factory.Save();

			var result = LoadView(whs, client, part, commodity);
			var resultsWithDcAddress = result.Where(o => (CargoWise.Types.ZGuid)o["DistributionCentrePK"] == distributionCentre.PK).ToArray();

			AssertEquals("Should filter by distribution centre.", 1, resultsWithDcAddress.Length);

			var businessObject = resultsWithDcAddress[0];
			AssertEquals(receives[0].PK, businessObject["DocketPK"]);
			AssertEquals(distributionCentre.PK, businessObject["DistributionCentrePK"]);
			AssertEquals(distributionCentre.OH_FullName, businessObject["DistributionCentreCoName"]);
		}

		public void TestView_ProductCategory()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("2", "2");
			var part = Helper.CreateProduct(client, "2");
			var category = Helper.CreateProductCategory(client, part, "Cat1");
			category.OPC_CategoryDescription = "Category 1";

			var receive = SetupDataForProductCategory(whs, client, part);
			var result = LoadView_ProductCategory(whs, client, category);
			AssertEquals(2, result.Count);
			AssertResult_ProductCategory(receive.Lines[0], result[0]);
			AssertResult_ProductCategory(receive.Lines[1], result[0]);
		}

		public void TestView_SortByDateTimeOffsetField()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			var receive1 = Helper.CreateWhsReceive(client, warehouse, "R1", Notify);
			var receive2 = Helper.CreateWhsReceive(client, warehouse, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, product, 1);
			Helper.CreateWhsReceiveInventoryLine(receive2, product, 1);
			receive1.WD_ArrivalDate = new ZDateTimeOffset(2024, 10, 23, 9, 1, 0, TimeSpan.FromHours(8));
			receive2.WD_ArrivalDate = new ZDateTimeOffset(2024, 10, 23, 9, 2, 0, TimeSpan.FromHours(11));
			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"select * from WhsJobHistoryReceiveReport(null) order by DateTimeOffsetField asc";
			result.Load(sql);
			AssertEquals(2, result.Count);
			AssertEquals(receive2.WD_ArrivalDate.ToString(), result[0]["DateTimeOffsetField"].ToString());
			AssertEquals(receive1.WD_ArrivalDate.ToString(), result[1]["DateTimeOffsetField"].ToString());
		}

		void AssertResult_ProductCategory(WhsDocketLine reveiveLine, DynamicBusinessObject dynamicObject)
		{
			var partRelation =
				reveiveLine.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(
					reveiveLine.Docket.Client, OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation != null ? partRelation.Category : null;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;
			AssertEquals("ProductCategoryCode", expectedCategoryCode, dynamicObject["ProductCategoryCode"]);
			AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
				dynamicObject["ProductCategoryDescription"]);
			AssertEquals("Receive Category", reveiveLine.Docket.WD_ReceiveCategory, dynamicObject["ReceiveCategory"]);
		}

		void AssertResult(WhsDocketLine reveiveLine, DynamicBusinessObject obj, RefCommodityCode[] commodity)
		{
			var receive = (WhsReceive)reveiveLine.Docket;
			var whs = receive.Warehouse;
			var client = receive.Client;
			var part = reveiveLine.SupplierPart;
			if (receive.IsFinalised)
			{
				AssertEquals("IsFinalised", "Y", obj["IsFinalised"]);
				AssertEquals("DateTimeOffsetField", receive.WD_FinalisedDate, ((ZDateTimeOffset)obj["DateTimeOffsetField"]));
				AssertEquals("QuantityExpected", reveiveLine.WE_TransactionQuantity, obj["QuantityExpected"]);
				AssertEquals("QuantityActual", reveiveLine.WE_TransactionQuantity, obj["QuantityActual"]);
				AssertEquals("Packs",
					part.UnitConverter.Convert(reveiveLine.WE_TransactionQuantity, part.OP_StockKeepingUnit,
						reveiveLine.WE_F3_NKPackType), obj["Packs"]);
				AssertEquals("Weight", part.OP_Weight * reveiveLine.WE_TransactionQuantity, obj["Weight"]);
				AssertEquals("Volume", part.OP_Cubic * reveiveLine.WE_TransactionQuantity, obj["Volume"]);
			}
			else
			{
				AssertEquals("IsFinalised", "N", obj["IsFinalised"]);
				var expectedDateField =
					receive.WD_ArrivalDate.IsEmpty ? receive.WD_ETA : receive.WD_ArrivalDate;
				AssertEquals("DateTimeOffsetField", expectedDateField, ((ZDateTimeOffset)obj["DateTimeOffsetField"]));
				AssertEquals("QuantityExpected", reveiveLine.WE_TransactionQuantity, obj["QuantityExpected"]);
				AssertEquals("QuantityActual", 0m, obj["QuantityActual"]);
				AssertEquals("Packs", 0m, obj["Packs"]);
				AssertEquals("Weight", 0m, obj["Weight"]);
				AssertEquals("Volume", 0m, obj["Volume"]);
			}

			AssertEquals("WarehousePK", whs.PK, obj["WarehousePK"]);
			AssertEquals("DocketPK", receive.PK, obj["DocketPK"]);
			AssertEquals("WarehouseName", whs.WW_WarehouseName, obj["WarehouseName"]);
			AssertEquals("ClientPK", client.PK, obj["ClientPK"]);
			AssertEquals("ClientFullName", client.OH_FullName, obj["ClientFullName"]);
			AssertEquals("ClientCode", client.OH_Code, obj["ClientCode"]);
			AssertEquals("Reference", receive.WD_ExternalReference, obj["Reference"]);
			AssertEquals("DocketType", receive.WD_DocketType, obj["DocketType"]);
			AssertEquals("FinalisedDate", receive.WD_FinalisedDate.Date, ((ZDateTime)obj["FinalisedDate"]).Date);
			AssertEquals("PartAttrib1", reveiveLine.WE_PartAttrib1, obj["PartAttrib1"]);
			AssertEquals("PartAttrib2", reveiveLine.WE_PartAttrib2, obj["PartAttrib2"]);
			AssertEquals("PartAttrib3", reveiveLine.WE_PartAttrib3, obj["PartAttrib3"]);
			AssertEquals("SerialNumber", reveiveLine.WE_SerialNumber, obj["SerialNumber"]);
			AssertEquals("Product", part.OP_PartNum, obj["Product"]);
			AssertEquals("ProductDesc", part.OP_Desc, obj["ProductDesc"]);
			AssertEquals("ProductPK", part.PK, obj["ProductPK"]);
			AssertEquals("ExpiryDate", reveiveLine.WE_ExpiryDate, ((ZDateTime)obj["ExpiryDate"]).Date);
			AssertEquals("PackingDate", reveiveLine.WE_PackingDate, ((ZDateTime)obj["PackingDate"]).Date);
			AssertEquals("QuantityUQ", part.OP_StockKeepingUnit, obj["QuantityUQ"]);
			AssertEquals("PackUQ", reveiveLine.WE_F3_NKPackType, obj["PackUQ"]);
			AssertEquals("WeightUQ", part.OP_WeightUQ, obj["WeightUQ"]);
			AssertEquals("VolumeUQ", part.OP_CubicUQ, obj["VolumeUQ"]);
			AssertEquals("LineStatus", reveiveLine.WE_CurrentInventoryStatus, obj["LineStatus"]);
			AssertEquals("HeldCode", reveiveLine.WE_WHC_NKCurrentInventoryHeldCode, obj["HeldCode"]);
			AssertEquals("CommodityCode", commodity[0].RH_Code, obj["CommodityCode"]);
			AssertEquals("CommodityPK", commodity[0].PK, obj["CommodityPK"]);
			AssertEquals("ConsigneePK", ZGuid.Empty, obj["ConsigneePK"]);
			AssertEquals("ConsigneeName", ZString.Empty, obj["ConsigneeName"]);
			AssertEquals("DistributionCentrePK", ZGuid.Empty, obj["DistributionCentrePK"]);
			AssertEquals("DistributionCentreCoName", ZString.Empty, obj["DistributionCentreCoName"]);
			AssertEquals("Service Level", receive.WD_RS_NKServiceLevel, obj["ServiceLevel"]);
			AssertEquals("Receive Category", receive.WD_ReceiveCategory, obj["ReceiveCategory"]);
		}

		#endregion

		#region TestView_SerialNumber

		public void TestView_SerialNumber()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			data.Part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			data.Part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inv1 = CreateReceiveLine("SN1");
			var inv2 = CreateReceiveLine("SN2");
			var inv3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var result = LoadView(data.Whs1, data.Org1, data.Part1, commodity);
			AssertEquals("Should be 3 results", 3, result.Count);

			AssertResult(inv1, result[0], commodity);
			AssertResult(inv2, result[1], commodity);
			AssertResult(inv3, result[2], commodity);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView_ProductCategory(WhsWarehouse whs, OrgHeader client,
			OrgPartCategory category)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"
select
	*
from
	WhsJobHistoryReceiveDetailReport(@ProductCategoryPK)
where
	WarehousePK = @WarehousePK and
	ClientPK = @ClientPK and
	ProductCategoryPK = @ProductCategoryPK
order by
	Reference,
	QuantityActual asc";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client },
				{ "@ProductCategoryPK", category.PK, OrgPartCategorySchema.PK }
			};

			result.Load(sql, sqlParams);
			return result;
		}

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part,
			RefCommodityCode[] commodity)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"
select
	*
from
	WhsJobHistoryReceiveDetailReport(null)
where
	WarehousePK = @WarehousePK and
	ClientPK = @ClientPK and
	ProductPK = @ProductPK and
	CommodityCode = @CommodityCode
order by
	Reference,
	QuantityActual asc,
	SerialNumber";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client },
				{ "@ProductPK", part.PK, WhsDocketLineSchema.WE_OP },
				{ "@CommodityCode", commodity[0].RH_Code, OrgSupplierPartSchema.OP_RH_NKCommodityCode }
			};

			result.Load(sql, sqlParams);
			return result;
		}

		#endregion

		#region SetupData

		WhsReceive[] SetupData(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part, RefCommodityCode[] commodity)
		{
			var year = ZDateTime.Now.Year;
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			var client2 = Helper.CreateClient("2", "2");

			var consignee1 = Helper.CreateClient("3", "3");
			var consignee2 = Helper.CreateClient("4", "4");

			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo1.OH_FullName = "TransportCo1";
			transportCo1.Addresses.AddNewMainAddress();
			transportCo1.OH_IsShippingProvider = true;

			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo2.OH_FullName = "TransportCo2";
			transportCo2.Addresses.AddNewMainAddress();
			transportCo2.OH_IsShippingProvider = true;

			var part1 = Helper.CreateProduct(client, "1", OrgPartRelation.RelationshipTypes.Both);
			var part2 = Helper.CreateProduct(client2, "2");

			var category1 = Helper.CreateProductCategory(client2, part2, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 2");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			SetClientAttributesType(client);
			SetClientAttributesType(client2);
			SetProductAttributesUse(client, part1);
			SetProductAttributesUse(client2, part2);
			SetProductAttributesUse(client, part);

			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part.OP_RH_NKCommodityCode = commodity[0].RH_Code;

			part.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part, "UNT", "PLT", 10);

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part.PK;
			relation.OU_OH = client2.PK;

			var arriveDate = new ZDateTimeOffset(year, 1, 1);
			var emptyDateOffset = ZDateTimeOffset.Empty;

			var receives = new[]
			{
				SetupReceive(client, whs, "11", part1, 111, part, 112, true, arriveDate, emptyDateOffset, serviceLevel: "TST", receiveCategory: "RC1"),
				SetupReceive(client, whs, "12", part1, 121, part, 122, false, arriveDate, emptyDateOffset, heldCode: "HEL", receiveCategory: "RC2"),
				SetupReceive(client, whs, "13", part1, 121, part, 123, false, arriveDate, arriveDate),
				SetupReceive(client2, whs, "21", part2, 211, part2, 212, true, arriveDate, emptyDateOffset),
				SetupReceive(client2, whs2, "22", part2, 221, part2, 222, false, arriveDate, emptyDateOffset)
			};

			Factory.Save();

			SetupCancelledReceive(client, whs, "31", part, 100m);
			return receives;
		}

		WhsReceive SetupDataForProductCategory(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part)
		{
			var year = ZDateTime.Now.Year;

			var consignee1 = Helper.CreateClient("3", "3");
			var consignee2 = Helper.CreateClient("4", "4");

			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo1.OH_FullName = "TransportCo1";
			transportCo1.Addresses.AddNewMainAddress();
			transportCo1.OH_IsShippingProvider = true;

			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo2.OH_FullName = "TransportCo2";
			transportCo2.Addresses.AddNewMainAddress();
			transportCo2.OH_IsShippingProvider = true;

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 2");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			SetClientAttributesType(client);
			SetProductAttributesUse(client, part);

			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			part.OP_RH_NKCommodityCode = commodity.RH_Code;

			var arriveDate = new ZDateTimeOffset(year, 1, 1);
			var emptyDateOffset = ZDateTimeOffset.Empty;

			var receive = SetupReceive(client, whs, "21", part, 211, part, 212, true, arriveDate, emptyDateOffset, receiveCategory: "RC2");

			Factory.Save();
			return receive;
		}

		void SetClientAttributesType(OrgHeader client)
		{
			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, false);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
		}

		void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part)
		{
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, true);
		}

		void SetupCancelledReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart part,
			ZDecimal units)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, part, units);
			line.WI_TotalUnits = units;

			Factory.Save();

			receive.CancelReactivateDocket();

			Factory.Save();

			AssertEquals(true, receive.IsCancelled);
		}

		WhsReceive SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference,
			OrgSupplierPart warehousePart1,
			ZDecimal units1, OrgSupplierPart warehousePart2, ZDecimal units2, ZBool finalized, ZDateTimeOffset arrivalDate,
			ZDateTimeOffset etaDate, string serviceLevel = "", string heldCode = "", string receiveCategory = "")
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, warehousePart1, units1,
				ZDate.Today.AddDays(+10), ZDate.Today, "Attrib11", "Attrib12", "Attrib13", "BEK1");
			receiveLine1.OriginalInventoryHeldCode = heldCode;
			Helper.CreateWhsReceiveInventoryLine(receive, warehousePart2, units2, ZDate.Today.AddDays(+5),
				ZDate.Today.AddDays(-1), "Attrib21", "Attrib22", "Attrib23", "BEK2");

			receive.WD_ArrivalDate = arrivalDate;
			receive.WD_ETA = etaDate;
			receive.WD_RS_NKServiceLevel = serviceLevel;
			receive.WD_ReceiveCategory = receiveCategory;

			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPuttingAway", true, receive.IsPuttingAway);

			if (finalized)
			{
				receive.FinaliseDocket();
				AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
			}

			receive.RunPreSaveValidation();

			if (finalized)
			{
				receiveLine1.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode =
					!string.IsNullOrEmpty(heldCode) ? "CHANGED" : string.Empty;
			}
			else
			{
				receiveLine1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode =
					!string.IsNullOrEmpty(heldCode) ? "CHANGED" : string.Empty;
			}

			return receive;
		}

		#endregion
	}
}