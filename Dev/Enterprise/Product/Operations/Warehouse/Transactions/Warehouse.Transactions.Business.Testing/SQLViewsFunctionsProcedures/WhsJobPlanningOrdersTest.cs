using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsJobPlanningOrdersTest : WhsTestCaseWithFactory
	{
		public void TestView()
		{
			var whs = Helper.CreateWarehouse("1", "A", 3, 2);
			var orders = SetupData(whs);

			var viewLoadedResult = LoadView(whs);
			var manuallyLoadedResult = LoadManually(whs, orders);

			AssertEquals("View and Manual selection of lines must have same Count", manuallyLoadedResult.Count,
				viewLoadedResult.Count);
			for (int i = 0; i < manuallyLoadedResult.Count; i++)
			{
				AssertEqualInformation(manuallyLoadedResult[i], viewLoadedResult[i]);
			}
		}

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
			CreateReceiveLine("SN1");
			CreateReceiveLine("SN2");
			CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN21";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN22";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine3.WE_SerialNumber = "SN23";
			Factory.Save();

			var result = LoadView(data.Whs1);
			AssertEquals("Should be 3 results", 3, result.Count);

			AssertEqualInformation(orderLine1, result[0]);
			AssertEqualInformation(orderLine2, result[1]);
			AssertEqualInformation(orderLine3, result[2]);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		public void TestView_DistributionCentre()
		{
			var whs = Helper.CreateWarehouse("1", "A", 3, 2);

			var distributionCentre = Helper.CreateClient("C3");

			var orders = SetupData(whs);
			orders[0].DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			Factory.Save();

			var viewLoadedResult = LoadView(whs, distributionCentre.PK);
			AssertEquals("Should filter by distribution centre.", 2, viewLoadedResult.Count);

			for (var i = 0; i < viewLoadedResult.Count; i++)
			{
				var businessObject = viewLoadedResult[i];
				AssertEquals(orders[0].PK, businessObject["DocketPK"]);
				AssertEquals(distributionCentre.PK, businessObject["DistributionCentrePK"]);
				AssertEquals(distributionCentre.OH_FullName, businessObject["DistributionCentreCoName"]);
			}

			var result2 = LoadView(whs, ZGuid.NewZGuid());
			AssertEquals("Should not match with any distribution centre.", 0, result2.Count);
		}

		void AssertEqualInformation(WhsOrderLine orderLine, DynamicBusinessObject obj)
		{
			var order = orderLine.Order;
			var client = order.Client;
			var part = orderLine.SupplierPart;
			AssertEquals("DateField", order.WD_RequiredDate.Date, ((ZDateTime)obj["DateField"]).Date);
			AssertEquals("ClientPK", client.PK, obj["ClientPK"]);
			AssertEquals("ClientCode", client.OH_Code, obj["ClientCode"]);
			AssertEquals("ClientName", client.OH_FullName, obj["ClientName"]);
			AssertEquals("DocketPK", order.PK, obj["DocketPK"]);
			AssertEquals("DocketID", order.WD_DocketID, obj["DocketID"]);
			AssertEquals("DocketType", order.WD_DocketType, obj["DocketType"]);
			AssertEquals("ExternalReference", order.WD_ExternalReference, obj["ExternalReference"]);
			AssertEquals("DocketStatus", order.WD_DocketStatus, obj["DocketStatus"]);
			AssertEquals("CustomerReference", order.WD_CustomerReference, obj["CustomerReference"]);
			AssertEquals("TransportReference", order.WD_TransportReference, obj["TransportReference"]);
			AssertEquals("WarehousePK", order.Warehouse.PK, obj["WarehousePK"]);
			AssertEquals("WarehouseName", order.Warehouse.WW_WarehouseName, obj["WarehouseName"]);
			AssertEquals("TransportCoPK", order.TransportCoDocAddress.OrganisationPK, obj["TransportCoPK"]);
			AssertEquals("TransportCoName", order.TransportCoDocAddress.E2_CompanyName, obj["TransportCoName"]);
			AssertEquals("ConsigneePK", order.ConsigneeDocAddress.OrganisationPK, obj["ConsigneePK"]);
			AssertEquals("ConsigneeName", order.ConsigneeDocAddress.E2_CompanyName, obj["ConsigneeName"]);
			AssertEquals("RequiredDate", order.WD_RequiredDate.Date, ((ZDateTimeOffset)obj["RequiredDate"]).Date);
			AssertEquals("ProductPK", part.PK, obj["ProductPK"]);
			AssertEquals("ProductCode", part.OP_PartNum, obj["ProductCode"]);
			AssertEquals("ProductDescription", part.OP_Desc, obj["ProductDescription"]);
			AssertEquals("PartAttrib1", orderLine.WE_PartAttrib1, obj["PartAttrib1"]);
			AssertEquals("PartAttrib2", orderLine.WE_PartAttrib2, obj["PartAttrib2"]);
			AssertEquals("PartAttrib3", orderLine.WE_PartAttrib3, obj["PartAttrib3"]);
			AssertEquals("SerialNumber", orderLine.WE_SerialNumber, obj["SerialNumber"]);
			AssertEquals("PartAttrib1Name", client.MiscServ.OM_IMPartAttrib1Name, obj["PartAttrib1Name"]);
			AssertEquals("ExpiryDate", orderLine.WE_ExpiryDate, obj["ExpiryDate"]);
			AssertEquals("PackingDate", orderLine.WE_PackingDate, obj["PackingDate"]);
			AssertEquals("Quantity", orderLine.WE_TransactionQuantity, obj["Quantity"]);
			AssertEquals("QuantityUQ", part.OP_StockKeepingUnit, obj["QuantityUQ"]);
			AssertEquals("Packs",
				part.UnitConverter.Convert(orderLine.WE_TransactionQuantity, part.OP_StockKeepingUnit,
					orderLine.WE_F3_NKPackType), obj["Packs"]);
			AssertEquals("PackUQ", orderLine.WE_F3_NKPackType, obj["PackUQ"]);
			AssertEquals("Weight", part.OP_Weight * orderLine.WE_TransactionQuantity, obj["Weight"]);
			AssertEquals("WeightUQ", part.OP_WeightUQ, obj["WeightUQ"]);
			AssertEquals("Volume", part.OP_Cubic * orderLine.WE_TransactionQuantity, obj["Volume"]);
			AssertEquals("VolumeUQ", part.OP_CubicUQ, obj["VolumeUQ"]);
			AssertEquals("Comment", orderLine.WE_LineComment, obj["Comment"]);
		}

		#region SetupData

		WhsOrder[] SetupData(WhsWarehouse whs)
		{
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);

			var client1 = Helper.CreateClient("1", "Client 1");
			var client2 = Helper.CreateClient("2", "Client 2");

			var consignee1 = Helper.CreateClient("3", "Consignee 1");
			var consignee2 = Helper.CreateClient("4", "Consignee 2");

			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo1.OH_FullName = "TransportCo1";
			transportCo1.Addresses.AddNewMainAddress();
			transportCo1.OH_IsShippingProvider = true;

			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo2.OH_FullName = "TransportCo2";
			transportCo2.Addresses.AddNewMainAddress();
			transportCo2.OH_IsShippingProvider = true;

			var part1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Both);
			var part2 = Helper.CreateProduct(client2, "P2");
			var part3 = Helper.CreateProduct(client1, "P3");

			SetClientAttributesType(client1);
			SetClientAttributesType(client2);
			SetProductAttributesUse(client1, part1);
			SetProductAttributesUse(client2, part2);
			SetProductAttributesUse(client1, part3);

			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};

			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part3.OP_RH_NKCommodityCode = commodity[0].RH_Code;

			part3.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part3, "UNT", "PLT", 10);

			var relation = part3.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part3.PK;
			relation.OU_OH = client2.PK;

			SetupReceive(client1, whs, "R1", part1, 10m, part3, 15m);
			SetupReceive(client1, whs, "R2", part1, 20m, part3, 25m);
			SetupReceive(client2, whs, "R3", part2, 30m, part2, 35m);
			SetupReceive(client2, whs2, "R4", part2, 40m, part2, 45m);

			var order1 = SetupOrders(client1, whs, "O1", consignee1, transportCo1, ZDateTimeOffset.Today, "TR1", "CR1", "ER1",
				part1, 5m, part3, 15m);
			var order2 = SetupOrders(client1, whs, "O2", consignee2, transportCo2, ZDateTimeOffset.Today, "TR2", "CR2", "ER2",
				part1, 20m, part3, 25m);
			var order3 = SetupOrders(client2, whs, "O3", consignee1, transportCo1, ZDateTimeOffset.Today, "TR3", "CR3", "ER3",
				part2, 30m, part3, 35m);
			var order4 = SetupOrders(client2, whs2, "O4", consignee2, transportCo2, ZDateTimeOffset.Today, "TR4", "CR4",
				"ER4", part2, 40m, part3, 45m);

			Helper.CreateWhsOrderLine(order1, part1, 5m);

			Factory.Save();

			return new[] { order1, order2, order3, order4 };
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

		void SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart part1,
			ZDecimal units1, OrgSupplierPart part2, ZDecimal units2)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part1, units1, ZDate.Today.AddDays(+10),
				ZDate.Today.AddDays(-1), "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, part2, units2, ZDate.Today.AddDays(+15),
				ZDate.Today.AddDays(-2), "B1", "B2", "B3", "");

			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPuttingAway", true, receive.IsPuttingAway);

			receive.FinaliseDocket();
			AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
		}

		WhsOrder SetupOrders(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgHeader consignee,
			OrgHeader transportCo, ZDateTimeOffset requiredDate, ZString trasportReference, ZString customerReference,
			ZString externalReference, OrgSupplierPart part1, ZDecimal units1, OrgSupplierPart part2, ZDecimal units2)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			order.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			order.WD_RequiredDate = requiredDate;
			order.WD_TransportReference = trasportReference;
			order.WD_CustomerReference = customerReference;
			order.WD_ExternalReference = externalReference;

			Helper.CreateWhsOrderLine(order, part1, units1);
			Helper.CreateWhsOrderLine(order, part2, units2);
			return order;
		}

		#endregion

		#region Loaders

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, ZGuid? distributionCentrePK = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			#region Sql query

			var sql = $@"
SELECT
	*
FROM
	dbo.WhsJobPlanningOrdersReport
WHERE
	WarehousePK = @WarehousePK
{(distributionCentrePK != null ? "AND DistributionCentrePK = @DistributionCentrePK" : "")}
ORDER BY
	ExternalReference,
	ProductCode asc,
	SerialNumber";

			#endregion

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK }
			};

			if (distributionCentrePK != null)
			{
				sqlParams.Add("@DistributionCentrePK",  distributionCentrePK, OrgHeaderSchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		WhsOrderLineWorkCollection LoadManually(WhsWarehouse whs, WhsOrder[] orders)
		{
			var result = new WhsOrderLineWorkCollection(Factory);
			var tempOrderLines = new WhsOrderLineWorkCollection(Factory);

			tempOrderLines.AddRange(orders[0].Lines);
			tempOrderLines.AddRange(orders[1].Lines);
			tempOrderLines.AddRange(orders[2].Lines);
			tempOrderLines.AddRange(orders[3].Lines);

			foreach (WhsOrderLine orderLine in tempOrderLines)
			{
				if (orderLine.Docket.WD_WW_Whs == whs.PK &&
						(orderLine.Docket.WD_DocketStatus == DocketStatus.Codes.Entered ||
						orderLine.Docket.WD_DocketStatus == DocketStatus.Codes.Held ||
						orderLine.Docket.WD_DocketStatus == DocketStatus.Codes.Error))
				{
					var equalOrderLine = FindEqualOrderLine(result, orderLine);
					if (equalOrderLine != null)
					{
						equalOrderLine.WE_TransactionQuantity += orderLine.WE_TransactionQuantity;
					}
					else
					{
						result.Add(orderLine);
					}
				}
			}

			return result;
		}

		WhsOrderLine FindEqualOrderLine(WhsOrderLineWorkCollection orderLineCollection, WhsOrderLine orderLine)
		{
			foreach (WhsOrderLine equalOrderLine in orderLineCollection)
			{
				if (orderLine.WE_WD == equalOrderLine.WE_WD &&
					orderLine.WE_OP == equalOrderLine.WE_OP &&
					orderLine.WE_PartAttrib1 == equalOrderLine.WE_PartAttrib1 &&
					orderLine.WE_PartAttrib2 == equalOrderLine.WE_PartAttrib2 &&
					orderLine.WE_PartAttrib3 == equalOrderLine.WE_PartAttrib3 &&
					orderLine.WE_SerialNumber == equalOrderLine.WE_SerialNumber &&
					orderLine.WE_ExpiryDate == equalOrderLine.WE_ExpiryDate &&
					orderLine.WE_PackingDate == equalOrderLine.WE_PackingDate &&
					orderLine.WE_F3_NKPackType == equalOrderLine.WE_F3_NKPackType &&
					orderLine.WE_LineComment == equalOrderLine.WE_LineComment)
				{
					return equalOrderLine;
				}
			}

			return null;
		}

		#endregion
	}
}
