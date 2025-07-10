using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsJobPlanningReceiveTest : WhsTestCaseWithFactory
	{
		public void TestFunction()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var receives = SetupData(whs);

			var viewLoadedResult = LoadView(whs);
			var manuallyLoadedResult = LoadManually(whs, receives);

			AssertEquals("View and Manual selection of lines must have same Count", manuallyLoadedResult.Count,
				viewLoadedResult.Count);

			for (int i = 0; i < manuallyLoadedResult.Count; i++)
			{
				AssertEqualInformation(manuallyLoadedResult[i], viewLoadedResult[i]);
			}
		}

		public void TestFunction_SerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			AssertEquals("Receive should be finalised.", false, receive.IsFinalised);
			Factory.Save();

			var viewLoadedResult = LoadView(data.Whs1);
			AssertEquals("Has correct count of results", 3, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine1, viewLoadedResult[0]);
			AssertEqualInformation(receiveLine2, viewLoadedResult[1]);
			AssertEqualInformation(receiveLine3, viewLoadedResult[2]);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		public void TestFunction_SerialNumberParameter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			AssertEquals("Receive should be finalised.", false, receive.IsFinalised);
			Factory.Save();

			var viewLoadedResult = LoadView(data.Whs1, "SN2");
			AssertEquals("Has correct count of results", 1, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine2, viewLoadedResult[0]);

			viewLoadedResult = LoadView(data.Whs1, "SN3");
			AssertEquals("Has correct count of results", 1, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine3, viewLoadedResult[0]);

			viewLoadedResult = LoadView(data.Whs1, "SN1");
			AssertEquals("Has correct count of results", 1, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine1, viewLoadedResult[0]);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		public void TestFunction_ETA_DateRange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();

			var etaDateTimeOffsetSydney = new ZDateTimeOffset(2022, 06, 01, 06, 00, 00, TimeSpan.FromHours(10));
			var etaDateTimeOffsetUtc = new ZDateTimeOffset(2022, 06, 01, 06, 00, 00, TimeSpan.FromHours(0));

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_ETA = etaDateTimeOffsetSydney;
			receive1.WD_ArrivalDate = ZDateTimeOffset.Empty;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m);
			AssertEquals("Receive should be finalised.", false, receive1.IsFinalised);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ETA = etaDateTimeOffsetUtc;
			receive2.WD_ArrivalDate = ZDateTimeOffset.Empty;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m);
			AssertEquals("Receive should be finalised.", false, receive2.IsFinalised);
			Factory.Save();

			var viewLoadedResult = LoadView(data.Whs1, fromDate: "2022-06-01 00:00:00 +10:00", toDate: "2022-06-01 09:00:00 +10:00");
			AssertEquals("Has correct count of results", 1, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine1, viewLoadedResult[0]);

			viewLoadedResult = LoadView(data.Whs1, fromDate: "2022-06-01 00:00:00 +0:00", toDate: "2022-06-01 09:00:00 +0:00");
			AssertEquals("Has correct count of results", 1, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine2, viewLoadedResult[0]);
		}

		public void TestFunction_ArrivalDate_DateRange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_ArrivalDate = new ZDateTimeOffset(2022, 06, 01, 06, 00, 00, TimeSpan.FromHours(10));
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m);
			AssertEquals("Receive should be finalised.", false, receive1.IsFinalised);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ArrivalDate = new ZDateTimeOffset(2022, 06, 01, 06, 00, 00, TimeSpan.FromHours(0));
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m);
			AssertEquals("Receive should be finalised.", false, receive2.IsFinalised);
			Factory.Save();

			var viewLoadedResult = LoadView(data.Whs1, serialNumber: "", fromDate: "2022-06-01 00:00:00 +10:00", toDate: "2022-06-01 09:00:00 +10:00");
			AssertEquals("Offset is ignored for WD_ArrivalDate.", 1, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine1, viewLoadedResult[0]);

			viewLoadedResult = LoadView(data.Whs1, serialNumber: "", fromDate: "2022-06-01 00:00:00 +0:00", toDate: "2022-06-01 09:00:00 +0:00");
			AssertEquals("Offset is ignored for WD_ArrivalDate.", 1, viewLoadedResult.Count);
			AssertEqualInformation(receiveLine2, viewLoadedResult[0]);
		}

		[TestDate(2024, 07, 31)]
		[TestUtcOffset(8, 0, 0)]
		public void TestFunction_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", data.Part1, 5m);
			var kitOrderLine1 = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var kitReceiveLine = kitOrderLine1.PickLines[0].InventoryLine;
			AssertEquals("Precondition", DocketStatus.Codes.Entered, kitReceiveLine.Docket.WD_DocketStatus);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 07, 31, 8, 0, 0), kitReceiveLine.Docket.WD_ArrivalDate);

			var viewLoadedResult = LoadView(data.Whs1, serialNumber: "", fromDate: "2024-07-01 00:00:00 +8:00", toDate: "2024-09-01 00:00:00 +8:00");
			AssertEquals("Kit Receive created from Pick by BOM is ignored.", 0, viewLoadedResult.Count);
		}

		#region AssertEqualInformation

		void AssertEqualInformation(WhsReceiveLine receiveLine, DynamicBusinessObject obj)
		{
			var receive = receiveLine.Docket;
			var client = receive.Client;
			var part = receiveLine.SupplierPart;
			if (receive.WD_ArrivalDate.IsEmpty)
			{
				AssertEquals("DateField", receive.WD_ETA.Date, ((ZDateTimeOffset)obj["DateField"]).Date);
			}
			else
			{
				AssertEquals("DateField", receive.WD_ArrivalDate.Date, ((ZDateTimeOffset)obj["DateField"]).Date);
			}

			AssertEquals("ClientPK", client.PK, obj["ClientPK"]);
			AssertEquals("ClientName", client.OH_FullName, obj["ClientName"]);
			AssertEquals("ClientCode", client.OH_Code, obj["ClientCode"]);
			AssertEquals("DocketPK", receive.PK, obj["DocketPK"]);
			AssertEquals("DocketID", receive.WD_DocketID, obj["DocketID"]);
			AssertEquals("DocketType", receive.WD_DocketType, obj["DocketType"]);
			AssertEquals("ExternalReference", receive.WD_ExternalReference, obj["ExternalReference"]);
			AssertEquals("DocketStatus", receive.WD_DocketStatus, obj["DocketStatus"]);
			AssertEquals("CustomerReference", receive.WD_CustomerReference, obj["CustomerReference"]);
			AssertEquals("TransportReference", receive.WD_TransportReference, obj["TransportReference"]);
			AssertEquals("WarehousePK", receive.Warehouse.PK, obj["WarehousePK"]);
			AssertEquals("WarehouseName", receive.Warehouse.WW_WarehouseName, obj["WarehouseName"]);
			AssertEquals("TransportCoPK", receive.TransportCoDocAddress.OrganisationPK, obj["TransportCoPK"]);
			AssertEquals("TransportCoName", receive.TransportCoDocAddress.E2_CompanyName, obj["TransportCoName"]);
			AssertEquals("SupplierPK", receive.SupplierDocAddress.OrganisationPK, obj["SupplierPK"]);
			AssertEquals("SupplierName", receive.SupplierDocAddress.E2_CompanyName, obj["SupplierName"]);
			AssertEquals("ETADate", receive.WD_ETA.Date, ((ZDateTimeOffset)obj["ETADate"]).Date);
			AssertEquals("ArrivalDate", receive.WD_ArrivalDate.Date, ((ZDateTimeOffset)obj["ArrivalDate"]).Date);
			AssertEquals("ProductPK", part.PK, obj["ProductPK"]);
			AssertEquals("ProductCode", part.OP_PartNum, obj["ProductCode"]);
			AssertEquals("ProductDescription", part.OP_Desc, obj["ProductDescription"]);
			AssertEquals("PartAttrib1", receiveLine.WE_PartAttrib1, obj["PartAttrib1"]);
			AssertEquals("PartAttrib2", receiveLine.WE_PartAttrib2, obj["PartAttrib2"]);
			AssertEquals("PartAttrib3", receiveLine.WE_PartAttrib3, obj["PartAttrib3"]);
			AssertEquals("SerialNumber", receiveLine.WE_SerialNumber, obj["SerialNumber"]);
			AssertEquals("PartAttrib1Name", client.MiscServ.OM_IMPartAttrib1Name, obj["PartAttrib1Name"]);
			AssertEquals("PackingDate", receiveLine.WE_PackingDate, ((ZDateTime)obj["PackingDate"]).Date);
			AssertEquals("ExpiryDate", receiveLine.WE_ExpiryDate, ((ZDateTime)obj["ExpiryDate"]).Date);
			AssertEquals("Quantity", receiveLine.WE_TransactionQuantity, obj["Quantity"]);
			AssertEquals("ExpectedQuantity", receiveLine.WE_ClientOrderedUnits, obj["ExpectedQuantity"]);
			AssertEquals("UQ", part.OP_StockKeepingUnit, obj["UQ"]);
			AssertEquals("Packs",
				part.UnitConverter.Convert(receiveLine.WE_TransactionQuantity, part.OP_StockKeepingUnit,
					receiveLine.WE_F3_NKPackType), obj["Packs"]);
			AssertEquals("PackUQ", receiveLine.WE_F3_NKPackType, obj["PackUQ"]);
			AssertEquals("Weight", part.OP_Weight * receiveLine.WE_TransactionQuantity, obj["Weight"]);
			AssertEquals("WeightUQ", part.OP_WeightUQ, obj["WeightUQ"]);
			AssertEquals("Volume", part.OP_Cubic * receiveLine.WE_TransactionQuantity, obj["Volume"]);
			AssertEquals("VolumeUQ", part.OP_CubicUQ, obj["VolumeUQ"]);
			if (receiveLine.Notes.VisibleNotes.Count > 0)
			{
				AssertEquals("NoteDescription", receiveLine.Notes.VisibleNotes[0].ST_Description,
					obj["NoteDescription"]);
				AssertEquals("NoteData", receiveLine.Notes.VisibleNotes[0].ST_NoteData, obj["NoteData"]);
			}

			AssertEquals("CommodityPK", part.CommodityCode?.PK ?? ZGuid.Empty, obj["CommodityPK"]);
			AssertEquals("CommodityCode", part.CommodityCode?.RH_Code ?? ZString.Empty, obj["CommodityCode"]);
		}

		#endregion

		#region SetupData

		WhsReceive[] SetupData(WhsWarehouse whs)
		{
			var whs2 = Helper.CreateWarehouse("2", "B", 2, 2);

			var client1 = Helper.CreateClient("1", "Client 1");
			var client2 = Helper.CreateClient("2", "Client 2");

			var supplier1 = Helper.CreateClient("3", "Supplier 1");
			var supplier2 = Helper.CreateClient("4", "Supplier 2");

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

			var receive1 = SetupReceive(client1, whs, "R1", transportCo1, supplier1, ZDateTimeOffset.Today, ZDateTimeOffset.Empty,
				part1, 5m, part3, 15m);
			var receive2 = SetupReceive(client1, whs, "R2", transportCo1, supplier2, ZDateTimeOffset.Empty, ZDateTimeOffset.Today,
				part1, 20m, part3, 25m);
			var receive3 = SetupReceive(client2, whs, "R3", transportCo2, supplier1, ZDateTimeOffset.Now,
				ZDateTimeOffset.Today.AddDays(2), part2, 30m, part3, 35m);
			var receive4 = SetupReceive(client2, whs2, "R4", transportCo2, supplier2, ZDateTimeOffset.Empty, ZDateTimeOffset.Now,
				part2, 40m, part3, 45m);

			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 5m, ZDate.Today.AddDays(+10), ZDate.Today.AddDays(-1),
				"A1", "A2", "A3", "");
			Factory.Save();

			return new[] { receive1, receive2, receive3, receive4 };
		}

		WhsReceive SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgHeader transportCo,
			OrgHeader supplier, ZDateTimeOffset arrivalDate, ZDateTimeOffset eTA, OrgSupplierPart part1, ZDecimal units1,
			OrgSupplierPart part2, ZDecimal units2)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			receive.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;
			receive.WD_ArrivalDate = arrivalDate;
			receive.WD_ETA = eTA;

			Helper.CreateWhsReceiveInventoryLine(receive, part1, units1, ZDate.Today.AddDays(+10),
				ZDate.Today.AddDays(-1), "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, part2, units2, ZDate.Today.AddDays(+15),
				ZDate.Today.AddDays(-2), "B1", "B2", "B3", "");

			return receive;
		}

		#endregion

		#region Loaders

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, string serialNumber = "", string fromDate = null, string toDate = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			#region SQL query

			var sql = $@"
SELECT
	*
FROM
	WhsJobPlanningReceiveReport(@WarehousePK,null,null,null,'','','',@SerialNumber,null,null,{(fromDate == null ? "null" : "'" + fromDate + "'")},{(toDate == null ? "null" : "'" + toDate + "'")})
ORDER BY
	ClientCode,
	ExternalReference,
	ProductCode asc,
	SerialNumber";

			#endregion

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@SerialNumber", serialNumber, WhsDocketLineSchema.WE_SerialNumber }
			};

			result.Load(sql, sqlParams);

			return result;
		}

		IList<WhsReceiveLine> LoadManually(WhsWarehouse whs, WhsReceive[] receives)
		{
			var result = new List<WhsReceiveLine>();
			var resultBeforeAdding = new List<WhsReceiveLine>();

			resultBeforeAdding.AddRange(receives[0].Lines.Cast<WhsReceiveLine>());
			resultBeforeAdding.AddRange(receives[1].Lines.Cast<WhsReceiveLine>());
			resultBeforeAdding.AddRange(receives[2].Lines.Cast<WhsReceiveLine>());
			resultBeforeAdding.AddRange(receives[3].Lines.Cast<WhsReceiveLine>());

			foreach (var receiveLine in resultBeforeAdding)
			{
				if (receiveLine.WarehousePK == whs.PK &&
						receiveLine.Docket.WD_DocketStatus == DocketStatus.Codes.Entered)
				{
					var equalInventory = FindEqualReceiveLine(result, receiveLine);
					if (equalInventory != null)
					{
						equalInventory.WE_TransactionQuantity += receiveLine.WE_TransactionQuantity;
						equalInventory.WE_ClientOrderedUnits += receiveLine.WE_ClientOrderedUnits;
					}
					else
					{
						result.Add(receiveLine);
					}
				}
			}

			return result;
		}

		WhsDocketLine FindEqualReceiveLine(IEnumerable<WhsReceiveLine> receiveLinesToCompare,
			WhsReceiveLine receiveLine)
		{
			foreach (var docketLine in receiveLinesToCompare)
			{
				if (receiveLine.Docket.PK == docketLine.Docket.PK &&
					receiveLine.WE_OP == docketLine.WE_OP &&
					receiveLine.WE_PartAttrib1 == docketLine.WE_PartAttrib1 &&
					receiveLine.WE_PartAttrib2 == docketLine.WE_PartAttrib2 &&
					receiveLine.WE_PartAttrib3 == docketLine.WE_PartAttrib3 &&
					receiveLine.WE_SerialNumber == docketLine.WE_SerialNumber &&
					receiveLine.WE_ExpiryDate == docketLine.WE_ExpiryDate &&
					receiveLine.WE_PackingDate == docketLine.WE_PackingDate &&
					receiveLine.WE_F3_NKPackType == docketLine.WE_F3_NKPackType)
				{
					return docketLine;
				}
			}

			return null;
		}

		#endregion
	}
}
