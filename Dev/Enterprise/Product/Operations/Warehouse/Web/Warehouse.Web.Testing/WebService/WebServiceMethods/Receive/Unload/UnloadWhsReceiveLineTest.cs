using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class UnloadWhsReceiveLineTest : WhsSecureServiceTestCase
	{
		#region UnloadWhsReceiveLine

		#region TestUnloadWhsReceiveLine

		[TestDate(2010, 1, 1)]
		public void TestUnloadWhsReceiveLine_UsingPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);

			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			data.Part2.OP_StockKeepingUnit = "KG";
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.CreateProductUnit(data.Part2, "KG", "UNT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			var year = ZDateTime.Today.Year - 2;
			receive.PopulateASNHasBeenCalled_TestsOnly = false;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "KG", "MAN", "NONMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "abc", "") }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response1, webService1);
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P1", 10m, "KG", "MAN", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "ABC", "", receive.PK, 1, 5m, 5m, "UNT", null, webService1);
			AssertEquals(false, receive.PopulateASNHasBeenCalled_TestsOnly);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 20, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(), InventoryHoldCodes.Codes.Held, "PAllETid", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response2.InventoryLinePK, "P2", 20m, "UNT", "MAN", "", "TEST", "",
				ZDate.Empty, ZDate.Empty, "HEL", "PALLETID", "", receive.PK, 2, 100m, 100m, "KG", null, webService2);
			AssertEquals(false, receive.PopulateASNHasBeenCalled_TestsOnly);

			var webService3 = GetNewWebService();
			SetupSecurityHeader(webService3, data.Whs1, staff);
			var response3 = webService3.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "KG", "MAN", "NONMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "ABc", "") }, Array.Empty<Guid>(), false);
			var line1 = webService3.Factory.Load<WhsInventoryView>(new ZGuid(response1.InventoryLinePK));
			AssertUnloadWhsReceiveLine(response3.InventoryLinePK, "P1", 20m, "KG", "MAN", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "ABC", "", receive.PK, 2, 10m, 5m, "UNT", line1, webService3);
			AssertEquals(false, receive.PopulateASNHasBeenCalled_TestsOnly);

			var webService4 = GetNewWebService();
			SetupSecurityHeader(webService4, data.Whs1, staff);
			var response4 = webService4.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(), InventoryHoldCodes.Codes.Held, "PALLETID", "") }, Array.Empty<Guid>(), false);
			var line2 = webService4.Factory.Load<WhsInventoryView>(new ZGuid(response2.InventoryLinePK));
			AssertUnloadWhsReceiveLine(response4.InventoryLinePK, "P2", 30m, "UNT", "MAN", "", "TEST", "",
				ZDate.Empty, ZDate.Empty, "HEL", "PALLETID", "", receive.PK, 2, 150m, 100m, "KG", line2, webService4);
			AssertEquals(false, receive.PopulateASNHasBeenCalled_TestsOnly);
		}

		[TestDate(2010, 1, 1)]
		public void TestUnloadWhsReceiveLine_UsingPalletID_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);

			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			data.Part2.OP_StockKeepingUnit = "KG";
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.CreateProductUnit(data.Part2, "KG", "UNT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			var year = ZDateTime.Today.Year - 2;
			receive.PopulateASNHasBeenCalled_TestsOnly = false;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "man", "NonMaN", "TesT", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P1", 20m, "KG", "MAN", "NONMAN", "TEST", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "", receive.PK, 1, 10m, 10m, "UNT", null, webService1);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "MAn", "NONman2", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response2.InventoryLinePK, "P1", 20m, "KG", "MAN", "NONMAN2", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "", receive.PK, 2, 10m, 10m, "UNT", null, webService2);

			var webService3 = GetNewWebService();
			SetupSecurityHeader(webService3, data.Whs1, staff);
			var response3 = webService3.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "MaN", "", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response3.InventoryLinePK, "P1", 20m, "KG", "MAN", "", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "", receive.PK, 3, 10m, 10m, "UNT", null, webService3);

			var webService4 = GetNewWebService();
			SetupSecurityHeader(webService4, data.Whs1, staff);
			var response4 = webService4.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "mAN2", "nonMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response4.InventoryLinePK, "P1", 20m, "KG", "MAN2", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "", receive.PK, 4, 10m, 10m, "UNT", null, webService4);

			var webService5 = GetNewWebService();
			SetupSecurityHeader(webService5, data.Whs1, staff);
			var response5 = webService5.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "", "NONMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response5.InventoryLinePK, "P1", 20m, "KG", "", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "", receive.PK, 5, 10m, 10m, "UNT", null, webService5);

			var webService6 = GetNewWebService();
			SetupSecurityHeader(webService6, data.Whs1, staff);
			var response6 = webService6.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "UNT", "MAN", "NONMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response6.InventoryLinePK, "P1", 20m, "UNT", "MAN", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "", receive.PK, 6, 20m, 20m, "UNT", null, webService6);
		}

		[TestDate(2010, 1, 1)]
		public void TestUnloadWhsReceiveLine_UsingPalletID_PackingAndExpiryDates()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);

			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			data.Part2.OP_StockKeepingUnit = "KG";
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.CreateProductUnit(data.Part2, "KG", "UNT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			var year = ZDateTime.Today.Year - 2;
			receive.PopulateASNHasBeenCalled_TestsOnly = false;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "MAN", "NONMAN", "", "", new ZDate(year, 4, 23), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P1", 20m, "KG", "MAN", "NONMAN", "", "",
				new ZDate(year, 4, 23), new ZDate(year, 4, 23), "", "123", "", receive.PK, 1, 10m, 10m, "UNT", null, webService1);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "MAN", "NONMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 24), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response2.InventoryLinePK, "P1", 20m, "KG", "MAN", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 24), "", "123", "", receive.PK, 2, 10m, 10m, "UNT", null, webService2);

			var webService3 = GetNewWebService();
			SetupSecurityHeader(webService3, data.Whs1, staff);
			var response3 = webService3.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 20, "KG", "MAN", "NONMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "1234", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response3.InventoryLinePK, "P1", 20m, "KG", "MAN", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "1234", "", receive.PK, 3, 10m, 10m, "UNT", null, webService3);

			var webService4 = GetNewWebService();
			SetupSecurityHeader(webService4, data.Whs1, staff);
			var response4 = webService4.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 20, "KG", "MAN", "NONMAN", "", "", new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response4.InventoryLinePK, "P2", 20m, "KG", "MAN", "NONMAN", "", "",
				new ZDate(year, 4, 22), new ZDate(year, 4, 23), "", "123", "", receive.PK, 4, 20m, 20m, "KG", null, webService4);

			var webService5 = GetNewWebService();
			SetupSecurityHeader(webService5, data.Whs1, staff);
			var response5 = webService5.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(), "", "123456", "") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response5.InventoryLinePK, "P2", 10m, "UNT", "MAN", "", "TEST", "",
				ZDate.Empty, ZDate.Empty, "", "123456", "", receive.PK, 5, 50m, 50m, "KG", null, webService5);
		}

		[TestDate(2010, 1, 1)]
		public void TestUnloadWhsReceiveLine_UsingLocationString()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);

			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			data.Part2.OP_StockKeepingUnit = "KG";
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.CreateProductUnit(data.Part2, "KG", "UNT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			var year = ZDateTime.Today.Year - 2;
			receive.PopulateASNHasBeenCalled_TestsOnly = false;

			Helper.Factory.Save();

			// if entered Location String and not Pallet ID
			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(), "", "", "A-1-1") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P2", 10m, "UNT", "MAN", "", "TEST", "", ZDate.Empty, ZDate.Empty,
				"", "", "A-1-1", receive.PK, 1, 50m, 50m, "KG", null, webService1);

			// same as the inventory above, should be rolled into one line.
			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(), "", "", "A-1-1") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response2.InventoryLinePK, "P2", 20m, "UNT", "MAN", "", "TEST", "", ZDate.Empty, ZDate.Empty,
				"", "", "A-1-1", receive.PK, 1, 100m, 50m, "KG", null, webService2);
		}

		[TestDate(2010, 1, 1)]
		public void TestUnloadWhsReceiveLine_WithHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);

			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			data.Part2.OP_StockKeepingUnit = "KG";
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.CreateProductUnit(data.Part2, "KG", "UNT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			var year = ZDateTime.Today.Year - 2;
			receive.PopulateASNHasBeenCalled_TestsOnly = false;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(),
				InventoryStatus.Codes.Held, "", "A-1-2") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P2", 10m, "UNT", "MAN", "", "TEST", "", ZDate.Empty, ZDate.Empty,
				InventoryStatus.Codes.Held, "", "A-1-2", receive.PK, 1, 50m, 50m, "KG", null, webService1);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(),
				InventoryHoldCodes.Codes.Damaged, "", "A-3-2") }, Array.Empty<Guid>(), false);
			AssertUnloadWhsReceiveLine(response2.InventoryLinePK, "P2", 10m, "UNT", "MAN", "", "TEST", "", ZDate.Empty, ZDate.Empty,
				InventoryHoldCodes.Codes.Damaged, "", "A-3-2", receive.PK, 2, 50m, 50m, "KG", null, webService2);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_InactiveProduct

		public void TestUnloadWhsReceiveLine_InactiveProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			data.Part1.OP_IsActive = false;
			Helper.Factory.Save();

			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "", "");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertEquals(response1.InventoryErrorInfos.Length, 1);
			AssertEquals(response1.InventoryErrorInfos[0].Sequence, 0);
			AssertEquals(response1.InventoryErrorInfos[0].ErrorMessage, "Product P1 could not be found for client 111.");
			AssertEquals(response1.InventoryErrorInfos[0].ErrorType, "BusinessValidationError");

			data.Part1.OP_IsActive = true;
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "", "") }, Array.Empty<Guid>(), false);
			AssertEquals("P1 is now active so should be considered a valid product", true, response2.InventoryLinePK != Guid.Empty);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_InvalidProduct

		public void TestUnloadWhsReceiveLine_InvalidProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory); //only contains Part 1 and Part 2
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "12345";
			Helper.Factory.Save();

			var receiveLine1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "Part3", 10, "KG", "MAN", "NONMAN", "", "", new ZDate(2008, 4, 22), new ZDate(2008, 4, 23), "", "123", "");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { receiveLine1 }, Array.Empty<Guid>(), false);
			AssertEquals(response1.InventoryErrorInfos.Length, 1);
			AssertEquals(response1.InventoryErrorInfos[0].Sequence, 0);
			AssertEquals(response1.InventoryErrorInfos[0].ErrorMessage, "Product Part3 could not be found for client 111.");
			AssertEquals(response1.InventoryErrorInfos[0].ErrorType, "BusinessValidationError");

			var client2 = Helper.CreateClient("222");
			var part = Helper.CreateProduct(client2, "Part3");
			Helper.Factory.Save();

			var receiveLine2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "Part3", 10, "KG", "MAN", "NONMAN", "", "", new ZDate(2008, 4, 22), new ZDate(2008, 4, 23), "", "123", "");
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { receiveLine2 }, Array.Empty<Guid>(), false);
			AssertEquals(response2.InventoryErrorInfos.Length, 1);
			AssertEquals(response2.InventoryErrorInfos[0].Sequence, 0);
			AssertEquals(response2.InventoryErrorInfos[0].ErrorMessage, "Product Part3 could not be found for client 111.");
			AssertEquals(response2.InventoryErrorInfos[0].ErrorType, "BusinessValidationError");
		}

		#endregion

		#region TestUnloadWhsReceiveLine_InvalidReceivePK

		public void TestUnloadWhsReceiveLine_InvalidReceivePK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			AssertBusinessValidationError(webService, "Receive record could not be found.",
				webService.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(Guid.NewGuid(), "P1", 10, "KG", "MAN", "NONMAN", "", "", new ZDate(2008, 4, 22), new ZDate(2008, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false));
		}

		#endregion

		#region TestUnloadWhsReceiveLine_FinalisedReceive

		public void TestUnloadWhsReceiveLine_FinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			AssertBusinessValidationError(webService, "Receive record has been finalized.",
				webService.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "KG", "MAN", "NONMAN", "", "", new ZDate(2008, 4, 22), new ZDate(2008, 4, 23), "", "123", "") }, Array.Empty<Guid>(), false));
		}

		#endregion

		#region TestUnloadWhsReceiveLine_BlankArrivalDate

		public void TestUnloadWhsReceiveLine_BlankArrivalDate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.Factory.Save();

			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "", "");
			Helper.Factory.Save();

			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertBusinessValidationError(webService1, "Receive record arrival date is blank. Please suspend and reload this job.", response1);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_WeightTooLarge

		[TestDate(2022, 1, 1)]
		public void TestUnloadWhsReceiveLine_WeightTooLarge()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Part1.OP_Weight = 600000.6m; // Heavy Product
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "12345";
			Helper.Factory.Save();

			var receiveLine1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "123", "");
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { receiveLine1 }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response1, webService1);
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P1", 1m, "UNT", "", "", "", "",
				ZDate.Empty, ZDate.Empty, "", "123", "", receive.PK, 1, 1m, 1m, "UNT", null, webService1);
			AssertEquals(response1.InventoryErrorInfos.Length, 0);
			AssertEquals("1 heavy product can be unloaded as the weight is still under the column limit.", 600000.6m, receive.WD_TotalWeight);

			var receiveLine2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "456", "");
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { receiveLine2 }, Array.Empty<Guid>(), false);
			AssertEquals(response2.InventoryErrorInfos.Length, 1);
			AssertEquals(response2.InventoryErrorInfos[0].Sequence, 0);
			AssertEquals(response2.InventoryErrorInfos[0].ErrorMessage, "Error - WD_TotalWeight: The number 1,200,001.200 is too large, the maximum value allowed for Weight is 999,999.999.");
			AssertEquals(response2.InventoryErrorInfos[0].ErrorType, "BusinessValidationError");
		}

		[TestDate(2022, 1, 1)]
		public void TestUnloadWhsReceiveLine_WeightTooLarge_DoesNotProcessMoreLinesIfReceiveIsInError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Part1.OP_Weight = 600000.6m; // Heavy Product
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "12345";
			Helper.Factory.Save();

			var receiveLine1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "123", "");
			var receiveLine2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "456", "");
			var receiveLine3 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "789", "");
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { receiveLine1, receiveLine2, receiveLine3 }, Array.Empty<Guid>(), false);
			AssertEquals(response1.InventoryErrorInfos.Length, 1);
			AssertEquals(response1.InventoryErrorInfos[0].Sequence, 1);
			AssertEquals(response1.InventoryErrorInfos[0].ErrorMessage, "Error - WD_TotalWeight: The number 1,200,001.200 is too large, the maximum value allowed for Weight is 999,999.999.");
			AssertEquals(response1.InventoryErrorInfos[0].ErrorType, "BusinessValidationError");
		}

		#endregion

		#region TestUnloadWhsReceiveLine_VolumeTooLarge

		[TestDate(2022, 1, 1)]
		public void TestUnloadWhsReceiveLine_VolumeTooLarge()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Part1.OP_Cubic = 600000.6m; // Huge Product
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "12345";
			Helper.Factory.Save();

			var receiveLine1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "123", "");
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { receiveLine1 }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response1, webService1);
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P1", 1m, "UNT", "", "", "", "",
				ZDate.Empty, ZDate.Empty, "", "123", "", receive.PK, 1, 1m, 1m, "UNT", null, webService1);
			AssertEquals(response1.InventoryErrorInfos.Length, 0);
			AssertEquals("1 huge product can be unloaded as the volume is still under the column limit.", 600000.6m, receive.WD_TotalCubic);

			var receiveLine2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "456", "");
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { receiveLine2 }, Array.Empty<Guid>(), false);
			AssertEquals(response2.InventoryErrorInfos.Length, 1);
			AssertEquals(response2.InventoryErrorInfos[0].Sequence, 0);
			AssertEquals(response2.InventoryErrorInfos[0].ErrorMessage, "Error - WD_TotalCubic: The number 1,200,001.200 is too large, the maximum value allowed for Volume is 999,999.999.");
			AssertEquals(response2.InventoryErrorInfos[0].ErrorType, "BusinessValidationError");
		}

		#endregion

		#region TestUnloadWhsReceiveLine_LogsAreAddedCorrectly

		public void TestUnloadWhsReceiveLine_LogsAreAddedCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			Helper.Factory.Save();

			// Create new inventory and put it away
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1") }, Array.Empty<Guid>(), false);
			var inventoryLine1 = webService1.Factory.Load<WhsInventoryView>(new ZGuid(response1.InventoryLinePK)).InDocketLine;

			var logsEvents_Add = Helper.FindLogs(inventoryLine1.Logs, Events.AddedARecordToTheSystem, "RF");
			AssertEquals(1, logsEvents_Add.Length);

			var logsEvents_Edit1 = Helper.FindLogs(inventoryLine1.Logs, Events.EditedARecord, "RF");
			AssertEquals("Should not have edited a record event as inventory is just getting added, not edited.", 0, logsEvents_Edit1.Length);

			var logsEvents_Confirm = Helper.FindLogs(inventoryLine1.Logs, Events.WarehouseReceiptConfirmedPutaway);
			AssertEquals(1, logsEvents_Confirm.Length);
			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Location A-1.", logsEvents_Confirm[0].SL_Reference);

			var addRecordEventTime = logsEvents_Add[0].SL_EventTime;
			var confirmPutAwayEventTime = logsEvents_Confirm[0].SL_EventTime;

			// Add some more units to the same inventory
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1") }, Array.Empty<Guid>(), false);
			var inventoryLine2 = webService2.Factory.Load<WhsInventoryView>(new ZGuid(response2.InventoryLinePK)).InDocketLine;

			var logsEvents_Add2 = Helper.FindLogs(inventoryLine2.Logs, Events.AddedARecordToTheSystem, "RF");
			AssertEquals(1, logsEvents_Add2.Length);
			AssertEquals("When adding some units to the existing Inventory no new AddedARecordToTheSystem event should be generated", addRecordEventTime, logsEvents_Add2[0].SL_EventTime);

			var logsEvents_Edit2 = Helper.FindLogs(inventoryLine2.Logs, Events.EditedARecord, "RF");
			AssertEquals("Should now have edited a record event as inventory got edited.", 1, logsEvents_Edit2.Length);

			var logsEvents_Confirm2 = Helper.FindLogs(inventoryLine2.Logs, Events.WarehouseReceiptConfirmedPutaway);
			AssertEquals(1, logsEvents_Confirm2.Length);
			AssertNotEquals("When adding some units to the existing Inventory the WarehouseReceiptConfirmedPutAway should be refreshed", confirmPutAwayEventTime, logsEvents_Confirm2[0].SL_EventTime);

			// Create other inventory but don't put it away
			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response3 = webService3.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT_1", "") }, Array.Empty<Guid>(), false);
			var inventoryLine3 = webService3.Factory.Load<WhsInventoryView>(new ZGuid(response3.InventoryLinePK)).InDocketLine;

			var logsEvents_Add3 = Helper.FindLogs(inventoryLine3.Logs, Events.AddedARecordToTheSystem, "RF");
			AssertEquals(1, logsEvents_Add3.Length);
			AssertNotEquals("New inventory created, so new AddedARecordToTheSystem event should be generated", addRecordEventTime, logsEvents_Add3[0].SL_EventTime);

			var logsEvents_Edit3 = Helper.FindLogs(inventoryLine3.Logs, Events.EditedARecord, "RF");
			AssertEquals("Should not have edited a record event as new inventory is created.", 0, logsEvents_Edit3.Length);

			var logsEvents_Confirm3 = Helper.FindLogs(inventoryLine3.Logs, Events.WarehouseReceiptConfirmedPutaway);
			AssertEquals("If location wasn't set, then no put away confirmation event should be generated.", 0, logsEvents_Confirm3.Length);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ActualStartAddedCorrectly

		[TestDate(2014, 9, 2, 5, 5, 0)]
		public void TestUnloadWhsReceiveLine_ActualStartAddedCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1") }, Array.Empty<Guid>(), false);
			var inventory1 = webService1.Factory.Load<WhsInventoryView>(new ZGuid(response1.InventoryLinePK));
			AssertNotNull("Inventory added successfully", inventory1);

			var logsEvents1 = Helper.FindLogs(receive.Logs, Events.WarehouseReceiptUnloaded);
			AssertEquals("Unloaded event added successfully", 1, logsEvents1.Length);
			var dateTime = logsEvents1[0].SL_EventTime;
			AssertEquals("Check event time is added correctly", new DateTime(2014, 9, 2, 5, 5, 0), dateTime);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1") }, Array.Empty<Guid>(), false);
			var inventory2 = webService2.Factory.Load<WhsInventoryView>(new ZGuid(response2.InventoryLinePK));
			AssertNotNull("Inventory added successfully", inventory2);

			var logsEvents2 = Helper.FindLogs(receive.Logs, Events.WarehouseReceiptUnloaded);
			AssertEquals("Only one unloaded event added", 1, logsEvents2.Length);
			AssertEquals("Event start time still the same", dateTime, logsEvents2[0].SL_EventTime);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_EmptyPackingAndExpiryDates

		[TestDate(2014, 9, 2, 5, 5, 0)]
		public void TestUnloadWhsReceiveLine_EmptyPackingAndExpiryDates()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1") }, Array.Empty<Guid>(), false);
			var inventory = webService.Factory.Load<WhsInventoryView>(new ZGuid(response.InventoryLinePK));
			AssertEquals(ZDateTime.Empty, inventory.WI_PackingDate);
			AssertEquals(ZDateTime.Empty, inventory.WI_ExpiryDate);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_WorksForLocationsOldBarcodes

		public void TestUnloadWhsReceiveLine_WorksForLocationsOldBarcodes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var oldLocationBarcode = data.Whs1.Rows[0].Locations[0].OldBarcode;
			var locationString = data.Whs1.Rows[0].Locations[0].ToLocationString();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			receive.PopulateASNHasBeenCalled_TestsOnly = false;
			webService.AllowedToRunServiceHasBeenCalled = false;

			var response = webService.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PalletID", oldLocationBarcode) }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var line = webService.Factory.Load<WhsInventoryView>(new ZGuid(response.InventoryLinePK));
			AssertNotEquals(oldLocationBarcode, line.LocationString);
			AssertEquals(locationString, line.LocationString);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoaded

		public void TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoaded_PalletID()
		{
			TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedCore("PLT-01", "");
		}

		public void TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoaded_Location()
		{
			TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedCore("", "A");
		}

		void TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedCore(string palletID, string location)
		{
			const int totalReceiveLines = 100;
			var org = Helper.CreateClient("MXF");
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Helper.Factory);
			data.Part1 = Helper.CreateProduct(org, "P1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetClientAttributeType(org, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(org, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);

			var differentCompany = Helper.Factory.New<GlbCompany>();
			differentCompany.GC_Code = "CO2";

			var differentBranch = differentCompany.Branches.AddNew();
			differentBranch.GB_Code = "BR2";

			Helper.Factory.Save();

			// when saving receive under different branch than the one that created the receive the system will try to recreate milestones every single time.
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var year = ZDateTime.Today.Year - 1;
				int lastNumOfBzObjLoaded = 0;

				for (int i = 1; i <= totalReceiveLines; i++)
				{
					var service = GetNewWebService();
					service.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
					service.SecurityHeader.BranchCode = "BR2";

					var docketLines = new List<WhsLightDocketLineInfo>();
					docketLines.Add(CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "Not Serial", "Red", "Doesn't Matter", "SerialNo" + i, new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", palletID, location));

					WhsEnvironment.IsRF = true;
					try
					{
						var response = service.UnloadWhsReceiveLines(docketLines.ToArray(), Array.Empty<Guid>(), false);
						AssertSuccessfulResponse(response, service);
						AssertEquals(ErrorTypes.None, response.Error);
						AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					}
					finally
					{
						WhsEnvironment.IsRF = false;
					}

					var numOfBzObjLoaded = (service.Factory as IBusinessObjectFactoryInternals).NumberOfBusinessObjects;
					Assert(string.Format("Number of BizO loaded for New Line {0} ({1}) is equal to the number of BizO loaded for the previous Line {2} ({3})", i, numOfBzObjLoaded, (i - 1), lastNumOfBzObjLoaded), i < 4 || numOfBzObjLoaded == lastNumOfBzObjLoaded);

					lastNumOfBzObjLoaded = numOfBzObjLoaded;
				}
			}
		}

		public void TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedWithFailure_PalletID()
		{
			TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedWithFailureCore("PLT-01", "");
		}

		public void TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedWithFailure_Location()
		{
			TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedWithFailureCore("", "A");
		}

		void TestUnloadWhsReceiveLine_Performance_NumberOfBusinessObjectsLoadedWithFailureCore(string palletID, string location)
		{
			const int totalReceiveLines = 100;
			var org = Helper.CreateClient("MXF");
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Helper.Factory);
			data.Part1 = Helper.CreateProduct(org, "P1");
			data.Part1.OP_StockKeepingUnit = "UNT";

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetClientAttributeType(org, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(org, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);

			var differentCompany = Helper.Factory.New<GlbCompany>();
			differentCompany.GC_Code = "CO2";

			var differentBranch = differentCompany.Branches.AddNew();
			differentBranch.GB_Code = "BR2";

			Helper.Factory.Save();

			// when saving receive under different branch than the one that created the receive the system will try to recreate milestones every single time.
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var year = ZDateTime.Today.Year - 1;
				int lastNumOfBzObjLoaded = 0;

				for (int i = 1; i <= totalReceiveLines; i++)
				{
					var service = GetNewWebService();
					service.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
					service.SecurityHeader.BranchCode = "BR2";

					var docketLines = new List<WhsLightDocketLineInfo>();
					docketLines.Add(CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "Not Serial", "Red", "Doesn't Matter", "SerialNo" + i, new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", palletID, location));
					docketLines.Add(CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "WRG", "Not Serial", "Red", "Doesn't Matter", "SerialNo" + i, new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", palletID, location));

					WhsEnvironment.IsRF = true;
					try
					{
						var response = service.UnloadWhsReceiveLines(docketLines.ToArray(), Array.Empty<Guid>(), false);
						AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
						AssertEquals(@"Cannot convert 'WRG' to 'UNT' for the product 'P1'. Please add this conversion and try again.", response.ErrorMessage);
					}
					finally
					{
						WhsEnvironment.IsRF = false;
					}

					var numOfBzObjLoaded = (service.Factory as IBusinessObjectFactoryInternals).NumberOfBusinessObjects;
					var objects = (service.Factory as IBusinessObjectFactoryInternals).AllBusinessObjects;
					Assert(string.Format("Number of BizO loaded for New Line {0} ({1}) is higher than for the previous Line {2} ({3})", i, numOfBzObjLoaded, (i - 1), lastNumOfBzObjLoaded), i < 4 || numOfBzObjLoaded - 1 == lastNumOfBzObjLoaded);

					lastNumOfBzObjLoaded = numOfBzObjLoaded;
				}
			}
		}

		#endregion

		#region TestUnloadWhsReceiveLine_PackageSize

		public void TestUnloadWhsReceiveLine_PackageSize()
		{
			var pk = Guid.NewGuid();
			var year = ZDateTime.Today.Year - 1;
			var lightDocketLine = CreateWhsDocketLineInfo(pk, "PART1", 10, "KG", "NoMandatoryAttribute1", "SerialNo1", "NoMandatoryAttribute2", "SerialNumber1", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "");

			long size = 0;
			using (var ms = new MemoryStream())
			{
				JsonSerializer.Serialize(ms, lightDocketLine);
				size = ms.Length;
			}

			Assert("Expected DockeLine size <= 999 bytes. Actual DocketLine size: " + size, size <= 999);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_HandleBusinessValidationErrorWhenBatchingRequest

		public void TestUnloadWhsReceiveLine_HandleBusinessValidationErrorWhenBatchingRequest()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.OP_Weight = 1.0m;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);

			Helper.Factory.Save();
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			WhsEnvironment.IsRF = true;
			try
			{
				var year = ZDateTime.Today.Year - 1;
				// should succeed
				var receiveLine1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1, "KG", "", "", "", "SerialNo1", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "");
				var receiveLine2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1, "KG", "", "", "", "SerialNo1", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "456", "");
				var response = webService.UnloadWhsReceiveLines(new[] { receiveLine1, receiveLine2 }, Array.Empty<Guid>(), false);
				AssertEquals(response.InventoryErrorInfos.Length, 1);
				AssertEquals("Error - WE_SerialNumber: Serial # already used.", response.ErrorMessage);
				AssertEquals(response.Error, ErrorTypes.BusinessValidationError);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(1, receiveInNewFactory.Lines.Count);
		}

		public void TestUnloadWhsReceiveLine_HandleLineValidationErrorWhenBatchingRequest_ErrorIsFirstInCollection()
		{
			TestUnloadWhsReceiveLine_HandleLineValidationErrorWhenBatchingRequestCore(indexToInsert: 0);
		}

		public void TestUnloadWhsReceiveLine_HandleLineValidationErrorWhenBatchingRequest_ErrorIsMiddleOfCollection()
		{
			TestUnloadWhsReceiveLine_HandleLineValidationErrorWhenBatchingRequestCore(indexToInsert: 4);
		}

		public void TestUnloadWhsReceiveLine_HandleLineValidationErrorWhenBatchingRequest_ErrorIsLastInCollection()
		{
			TestUnloadWhsReceiveLine_HandleLineValidationErrorWhenBatchingRequestCore(indexToInsert: 9);
		}

		void TestUnloadWhsReceiveLine_HandleLineValidationErrorWhenBatchingRequestCore(int indexToInsert)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			data.Part1.OP_StockKeepingUnit = "UNT";
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m);
			receiveLine1.WE_SerialNumber = "S1";
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			WhsEnvironment.IsRF = true;
			try
			{
				var year = ZDateTime.Today.Year - 1;

				var lines = new List<WhsLightDocketLineInfo>();
				for (var i = 2; i <= 10; i++)
				{
					lines.Add(CreateWhsDocketLineInfo(receive2.PK.ToGuid(), "P1", 1, "KG", "", "", "", "S" + i, new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "456", ""));
				}
				var lineInError = CreateWhsDocketLineInfo(receive2.PK.ToGuid(), "P1", 1, "KG", "", "", "", "S1", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "");

				lines.Insert(indexToInsert, lineInError);

				var response = webService.UnloadWhsReceiveLines(lines.ToArray(), Array.Empty<Guid>(), false);
				AssertEquals(response.InventoryErrorInfos.Length, 1);
				AssertEquals("Error - WE_SerialNumber: Serial # already used.", response.ErrorMessage);
				AssertEquals(response.Error, ErrorTypes.BusinessValidationError);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive2.PK);
			AssertEquals(9, receiveInNewFactory.Lines.Count);
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S2"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S3"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S4"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S5"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S6"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S7"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S8"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S9"));
			AssertNotNull(receiveInNewFactory.Lines.FirstOrDefault(l => l.WE_SerialNumber == "S10"));
		}

		#endregion

		#region TestUnloadWhsReceiveLine_HandleExceptionWhenBatchingRequests

		public void TestUnloadWhsReceiveLine_HandleExceptionWhenBatchingRequests()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			data.Part1.OP_StockKeepingUnit = "UNT";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);

			Helper.Factory.Save();

			var year = ZDateTime.Today.Year - 1;
			var receiveLine1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P3", 10, "KG", "", "", "", "", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "");
			var receiveLine2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "KG", "", "", "", "", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "");

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.UnloadWhsReceiveLines(new[] { receiveLine1, receiveLine2 }, Array.Empty<Guid>(), false);
			AssertEquals(response.InventoryErrorInfos.Length, 1);
			AssertEquals(response.InventoryErrorInfos[0].Sequence, 0);
			AssertEquals(response.InventoryErrorInfos[0].ErrorMessage, "Product P3 could not be found for client 111.");
			AssertEquals(response.InventoryErrorInfos[0].ErrorType, "BusinessValidationError");

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(1, receiveInNewFactory.Lines.Count);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_UsingSerialNumber

		[TestDate(2010, 1, 1)]
		public void TestUnloadWhsReceiveLine_UsingSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.OP_StockKeepingUnit = "UNT";
			data.Part1.OP_Weight = 1.0m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			Helper.Factory.Save();

			var year = ZDateTime.Today.Year - 1;
			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			// should succeed
			var response1 = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1, "KG", "NoMandatoryAttribute", "MandatoryAttribute", "", "SerialNo1", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "") }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertUnloadWhsReceiveLine(response1.InventoryLinePK, "P1", 1m, "KG", "NOMANDATORYATTRIBUTE", "MANDATORYATTRIBUTE", "", "SERIALNO1",
				new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "", receive.PK, 1, 1m, 1m, "UNT", null, webService1);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1, "KG", "ITDoesntMatter", "MandatoryAttribute", "", "SerialNo1", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "456", "") }, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Error - WE_SerialNumber: Serial # already used.", response2.ErrorMessage);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			// To avoid interference between next and previous web call we have to re-create factory
			var response3 = webService3.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1, "KG", "NoMandatoryAttribute", "MandatoryAttribute", "", "SerialNo2", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "456", "") }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals(ErrorTypes.None, response3.Error);
			AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
			AssertUnloadWhsReceiveLine(response3.InventoryLinePK, "P1", 1m, "KG", "NOMANDATORYATTRIBUTE", "MANDATORYATTRIBUTE", "", "SERIALNO2",
				new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "456", "", receive.PK, 2, 1m, 1m, "UNT", null, webService3);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_LocationCapacityOverfill

		public void TestUnloadWhsReceiveLine_LocationCapacityOverfill()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 1);
			data.Whs1.FindLocation("A").WLV_MaxQuantity = 1m;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			WhsEnvironment.IsRF = true;
			try
			{
				var response = webService.UnloadWhsReceiveLines(new[]
				{
					CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 2, "UNT", "", "", "", "", ZDate.Empty,
						ZDate.Empty, "", "", "A")
				}, Array.Empty<Guid>(), false);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(
					"Error - WE_WL: The Maximum Qty(1) for the destination location will be exceeded. Please select another location.", response.ErrorMessage);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestUnloadWhsReceiveLine_DockDoorLocation

		public void TestUnloadWhsReceiveLine_DockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF123");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var location = data.Whs1.DefaultLocation;
			location.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT5", "", location.PK.ToGuid());
			var response = webService.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Error not error in reponse.", true, string.IsNullOrEmpty(response.ErrorMessage));

			var inventoryFromFactory = webService.Factory.Load<WhsInventoryView>(response.InventoryLinePK);
			AssertEquals("Inventory location is dockdoor location.", location.PK.ToGuid(), inventoryFromFactory.WI_WL);
			AssertEquals("Receive line dockdoor location is empty.", ZGuid.Empty, inventoryFromFactory.InDocketLine.WE_WL_TransferFrom);
		}

		#endregion

		#region TestUnloadReceiveLine_WarehouseReceiptConfirmedPutawayEvent

		public void TestUnloadReceiveLine_WarehouseReceiptConfirmedPutawayEvent_DockDoorLocation_HasNone()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF123");
			var ddlocation = data.Whs1.DefaultOutboundDockDoorLocation;
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT5", "", ddlocation.PK.ToGuid());
			var response = webService.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Error not error in reponse.", true, string.IsNullOrEmpty(response.ErrorMessage));

			var receiveLineFromFactory = webService.Factory.Load<WhsReceiveLine>(response.InventoryLinePK);
			AssertNull("WarehouseReceiptConfirmedPutawayEvent Log", WebServiceHelper.FindExistingLog(receiveLineFromFactory, AutoEvents.WarehouseReceiptConfirmedPutaway, "RF"));
		}

		public void TestUnloadReceiveLine_WarehouseReceiptConfirmedPutawayEvent_NormalLocation_HasOne()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF123");
			var location = data.Whs1.DefaultLocation;
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 1m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT5", $"{location.ToLocationString()}");
			var response = webService.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Error not error in reponse.", true, string.IsNullOrEmpty(response.ErrorMessage));

			var receiveLineFromFactory = webService.Factory.Load<WhsReceiveLine>(response.InventoryLinePK);
			AssertNotNull("WarehouseReceiptConfirmedPutawayEvent Log", WebServiceHelper.FindExistingLog(receiveLineFromFactory, AutoEvents.WarehouseReceiptConfirmedPutaway, "RF"));
		}

		#endregion

		#region TestUnloadWhsReceiveLine_PalletValidation

		public void TestUnloadWhsReceiveLine_PalletValidation_TotalPallets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			receive.WD_TotalPallets = 2;
			Helper.Factory.Save();

			receive.PopulateASNLines();
			Helper.Factory.Save();

			AssertEquals("Precondition: ASN lines is not empty.", true, receive.AsnLines.Any());

			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService = GetNewWebService(data.Whs1);
				var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "PLT123", "A-1-1");
				var response = webService.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
				AssertEquals("Error is returned in the web service response.", false, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Correct error is returned.", "Total number of pallets unloaded is already equal to or more than the expected number of pallets. You cannot unload more pallets.", response.ErrorMessage);
				AssertEquals("Error is a business validation error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("There's 1 Inventory error info in the response.", 1, response.InventoryErrorInfos.Length);
				AssertEquals("Error in the Inventory error info is the same as error on response.", response.ErrorMessage, response.InventoryErrorInfos[0].ErrorMessage);
				AssertEquals("Error type in the Inventory error info is business validation error.", nameof(ErrorTypes.BusinessValidationError), response.InventoryErrorInfos[0].ErrorType);
			}
		}

		public void TestUnloadWhsReceiveLine_PalletValidation_TotalPallets_NoPalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			receive.WD_TotalPallets = 2;
			Helper.Factory.Save();

			receive.PopulateASNLines();
			Helper.Factory.Save();

			AssertEquals("Precondition: ASN lines is not empty.", true, receive.AsnLines.Any());
			AssertEquals("Precondition: There are 2 receive lines.", 2, receive.Lines.Count);

			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService = GetNewWebService();
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "", "A-1-1");
				var response = webService.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
				AssertEquals("No error is returned in the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("There's no Inventory error info in the response.", false, response.InventoryErrorInfos.Any());
				AssertEquals("ReceiveLine is unloaded and there are now 3 receive lines.", 3, receive.Lines.Count);
			}
		}

		public void TestUnloadWhsReceiveLine_PalletValidation_MaxPalletIdLengthExceeded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var palletId = "1234567890123456789012345678901";

			Assert("Precondition: Pallet id exceeds maximum length for pallet ids.", palletId.Length > WhsDocketLineSchema.WE_PalletID.MaxLength);
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", palletId, "A-1-1");

			WhsInventoryWebServiceResponse response = null;
			AssertNoExceptionThrown("No exception is thrown.", () => response = webService.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false));

			AssertEquals("No error reported.", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Error is returned in the web service response.", false, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Correct error is returned.",
				"WE_PalletID exceeds maximum length allowed. The maximum length of this property is 30 characters, but 31 were entered.", response.ErrorMessage);
			AssertEquals("Error is a business validation error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("There's 1 Inventory error info in the response.", 1, response.InventoryErrorInfos.Length);
			AssertEquals("Error in the Inventory error info is the same as error on response.", response.ErrorMessage, response.InventoryErrorInfos[0].ErrorMessage);
			AssertEquals("Error type in the Inventory error info is business validation error.", nameof(ErrorTypes.BusinessValidationError), response.InventoryErrorInfos[0].ErrorType);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			Helper.Factory.Save();

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 1, receive.Lines.Count);
			AssertEquals("Receiveline has 10 transaction quantity.", 10m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has 10 stock on hand.", 10m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is assigned with location.", locationPK, receiveLine.WE_WL);
		}

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_MultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 line number.", (ZShort)1, receiveLine1.WE_LineNo);
			AssertEquals("Precondition: Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 line number.", (ZShort)2, receiveLine2.WE_LineNo);
			AssertEquals("Precondition: Receiveline2 has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline1 has 10 transaction quantity.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 10 stock on hand.", 10m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);
			AssertEquals("Receiveline2 has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
		}

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_MultipleLines_UnloadQuantityDistributedToMultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 line number.", (ZShort)1, receiveLine1.WE_LineNo);
			AssertEquals("Precondition: Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 line number.", (ZShort)2, receiveLine2.WE_LineNo);
			AssertEquals("Precondition: Receiveline2 has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 50m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", null, response.ErrorMessage);
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline1 has 40 transaction quantity.", 40m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 40 stock on hand.", 40m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);
			AssertEquals("Receiveline2 has location.", locationPK, receiveLine2.WE_WL);
			AssertEquals("Receiveline2 has 10 transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 10 stock on hand.", 10m, receiveLine2.WE_StockOnHand);
		}

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_MultipleLines_MatchBySerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine1.WE_SerialNumber = "SER1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine2.WE_SerialNumber = "SER2";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 line number.", (ZShort)1, receiveLine1.WE_LineNo);
			AssertEquals("Precondition: Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 line number.", (ZShort)2, receiveLine2.WE_LineNo);
			AssertEquals("Precondition: Receiveline2 has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "", "", "", "SER2", ZDate.Empty, ZDate.Empty, "", "", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);

			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
			AssertEquals("Receiveline2 has 1 transaction quantity.", 1m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 1 stock on hand.", 1m, receiveLine2.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LinesWithLocationAndNoLocation

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LinesWithLocationAndNoLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			var location = data.Whs1.FindLocation("A-1-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline2 has 10 transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 10 stock on hand.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
		}

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_MatchingLinesHaveUnmatchedLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var location1 = data.Whs1.FindLocation("A-1-1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location1);
			Helper.Factory.Save();

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has location.", location1.PK, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);

			var location2 = data.Whs1.FindLocation("A-1-2");
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-1-2");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());

			AssertEquals("Receive line has been unloaded, new receive line is created.", 2, receive.Lines.Count);
			var newReceiveLine = receive.Lines.Single(line => line.WE_TransactionQuantity == 10);
			AssertEquals("New receiveline has 10 stock on hand.", 10m, newReceiveLine.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", location2.PK, newReceiveLine.WE_WL);

			AssertEquals("Receiveline has location.", location1.PK, receiveLine.WE_WL);
			AssertEquals("Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
		}

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_UnloadedLineWithUnmatchedLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var location1 = data.Whs1.FindLocation("A-1-1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
			var location2 = data.Whs1.FindLocation("A-1-2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			receiveLine3.WE_TransactionQuantity = 0m;
			receiveLine3.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has location.", location1.PK, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has location.", location2.PK, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline3 has no location.", ZGuid.Empty, receiveLine3.WE_WL);
			AssertEquals("Precondition: Receiveline3 has 0 transaction quantity.", 0m, receiveLine3.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline3 has 0 stock on hand.", 0m, receiveLine3.WE_StockOnHand);

			var location3 = data.Whs1.FindLocation("A-2-1");
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "A-2-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", null, response.ErrorMessage);
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());

			AssertEquals("Receive line has been unloaded, no new receive line is created.", 3, receive.Lines.Count);
			AssertEquals("New receiveline has 10 stock on hand.", 10m, receiveLine3.WE_TransactionQuantity);
			AssertEquals("New receiveline has 10 stock on hand.", 10m, receiveLine3.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", location3.PK, receiveLine3.WE_WL);

			AssertEquals("Receiveline has location.", location1.PK, receiveLine1.WE_WL);
			AssertEquals("Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);

			AssertEquals("Receiveline has location.", location2.PK, receiveLine2.WE_WL);
			AssertEquals("Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LineWithReservedQuantity

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LineWithReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 1;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 2;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(receiveLine2.Inventory[0]);
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;

			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has 20 expected quantity.", 20m, receiveLine1.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receiveline2 has reserved quantity.", 10m, receiveLine2.ReservedQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 10 expected quantity.", 10m, receiveLine2.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline2 has unloaded quantity as transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has unloaded quantity as stock on hand.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LineWithReservedQuantity_PartialReservedAllocation

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LineWithReservedQuantity_PartialReservedAllocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 1;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 2;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.ReserveStockIfAbleTo(receiveLine2.Inventory[0]);
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;

			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receive line 1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receive line 1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receive line 1 has 20 expected quantity.", 20m, receiveLine1.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receive line 2 has reserved quantity.", 5m, receiveLine2.ReservedQuantity);
			AssertEquals("Precondition: Receive line 2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receive line 2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receive line 2 has 10 expected quantity.", 10m, receiveLine2.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 30m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline2 has reserved quantity as transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has reserved quantity as stock on hand.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline1 has (unloaded quantity - reserved quantity) as transaction quantity.", 20m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has (unloaded quantity - reserved quantity) as stock on hand.", 20m, receiveLine1.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LineWithReservedQuantity_MultiplePartialReservedAllocation

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_LineWithReservedQuantity_MultiplePartialReservedAllocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 1;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 2;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine1.ReserveStockIfAbleTo(receiveLine1.Inventory[0]);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine2.ReserveStockIfAbleTo(receiveLine2.Inventory[0]);
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;

			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receive line 1 has reserved quantity.", 10m, receiveLine1.ReservedQuantity);
			AssertEquals("Precondition: Receive line 1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receive line 1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receive line 1 has 10 expected quantity.", 20m, receiveLine1.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receive line 2 has reserved quantity.", 5m, receiveLine2.ReservedQuantity);
			AssertEquals("Precondition: Receive line 2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receive line 2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receive line 2 has 10 expected quantity.", 10m, receiveLine2.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 12m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline1 has reserved quantity as transaction quantity.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has reserved quantity as stock on hand.", 10m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline2 has (unloaded quantity - receiveLine1 reserved quantity) as transaction quantity.", 2m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has (unloaded quantity - receiveLine1 reserved quantity) as stock on hand.", 2m, receiveLine2.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_WithPartiallyReceivedLine

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_WithPartiallyReceivedLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 1;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockdoorLocation, "PLT123");
			receiveLine2.WE_LineNo = 2;
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine1.WE_StockOnHand = 5m;

			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receiveline1 has 5 transaction quantity.", 5m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 5 stock on hand.", 5m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has 10 expected quantity.", 10m, receiveLine1.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 20 expected quantity.", 20m, receiveLine2.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 20m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline1 has expected quantity as transaction quantity.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has expected quantity as stock on hand.", 10m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline2 has (unloaded quantity - receiveLine1 expected quantity deficit) as transaction quantity.", 15m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has (unloaded quantity - receiveLine1 expected quantity deficit) as stock on hand.", 15m, receiveLine2.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_WithExpectedQuantity

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_WithExpectedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 1;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockdoorLocation, "PLT123");
			receiveLine2.WE_LineNo = 2;
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;

			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has 10 expected quantity.", 10m, receiveLine1.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 20 expected quantity.", 20m, receiveLine2.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 15m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline1 has expected quantity as transaction quantity.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has expected quantity as stock on hand.", 10m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline2 has excess unload quantity as transaction quantity.", 5m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has excess unload quantity as stock on hand.", 5m, receiveLine2.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_WithNoExpectedQuantity

		public void TestUnloadWhsReceiveLine_ReceiveWithExistingMatchingLines_WithNoExpectedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 1;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockdoorLocation, "PLT123");
			receiveLine2.WE_LineNo = 2;
			Helper.Factory.Save();

			receiveLine1.WE_ClientOrderedUnits = 0m;

			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has 0 expected quantity.", 0m, receiveLine1.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 20 expected quantity.", 20m, receiveLine2.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline2 has unload quantity as transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has unload quantity as stock on hand.", 10m, receiveLine2.WE_StockOnHand);
		}

		#endregion

		#region TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLines

		public void TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLines_QtyEnoughForLineWithReservedQty()
		{
			TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLinesCore(10m, 10m, 5m, 0m);
		}

		public void TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLines_QtyEnoughForLineWithReservedQtyAndPartialReceived()
		{
			TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLinesCore(25m, 10m, 20m, 0m);
		}

		public void TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLines_QtyEnoughForAllLines()
		{
			TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLinesCore(30m, 10m, 20m, 5m);
		}

		public void TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLines_QtyEnoughForAllLinesWithExcess()
		{
			TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLinesCore(60m, 15m, 20m, 30m);
		}

		void TestUnload_WithReservedQtyAndPartiallyReceivedLineAndExpectedQuantityMatchingLinesCore(ZDecimal unloadQty,
			ZDecimal expectedQtyOnLineWithReservedQty, ZDecimal expectedQtyPartialReceivedLine, ZDecimal expectedQtyOnLineWithExpectedQty)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLineWithReservedQuantity = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLineWithReservedQuantity.WE_LineNo = 1;
			var receiveLinePartiallyReceived = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockdoorLocation, "PLT123");
			receiveLinePartiallyReceived.WE_LineNo = 2;
			var receiveLineWithExpectedQuantity = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, dockdoorLocation, "PLT123");
			receiveLinePartiallyReceived.WE_LineNo = 3;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine1.ReserveStockIfAbleTo(receiveLineWithReservedQuantity.Inventory[0]);

			receiveLineWithReservedQuantity.WE_TransactionQuantity = 0m;
			receiveLineWithReservedQuantity.WE_StockOnHand = 0m;

			receiveLinePartiallyReceived.WE_TransactionQuantity = 5m;
			receiveLinePartiallyReceived.WE_StockOnHand = 5m;

			receiveLineWithExpectedQuantity.WE_TransactionQuantity = 0m;
			receiveLineWithExpectedQuantity.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receiveline1 has reserved quantity.", 10m, receiveLineWithReservedQuantity.ReservedQuantity);
			AssertEquals("Precondition: Receiveline1 has 5 transaction quantity.", 0m, receiveLineWithReservedQuantity.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 5 stock on hand.", 0m, receiveLineWithReservedQuantity.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has 10 expected quantity.", 10m, receiveLineWithReservedQuantity.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 5m, receiveLinePartiallyReceived.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 5m, receiveLinePartiallyReceived.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 20 expected quantity.", 20m, receiveLinePartiallyReceived.WE_ClientOrderedUnits);

			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLineWithExpectedQuantity.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLineWithExpectedQuantity.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 20 expected quantity.", 30m, receiveLineWithExpectedQuantity.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, unloadQty, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 3, receive.Lines.Count);
			AssertEquals(expectedQtyOnLineWithReservedQty, receiveLineWithReservedQuantity.WE_TransactionQuantity);
			AssertEquals(expectedQtyOnLineWithReservedQty, receiveLineWithReservedQuantity.WE_StockOnHand);
			AssertEquals(expectedQtyPartialReceivedLine, receiveLinePartiallyReceived.WE_TransactionQuantity);
			AssertEquals(expectedQtyPartialReceivedLine, receiveLinePartiallyReceived.WE_StockOnHand);
			AssertEquals(expectedQtyOnLineWithExpectedQty, receiveLineWithExpectedQuantity.WE_TransactionQuantity);
			AssertEquals(expectedQtyOnLineWithExpectedQty, receiveLineWithExpectedQuantity.WE_StockOnHand);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ReceiveWithNoExistingMatchingLines

		public void TestUnloadWhsReceiveLine_ReceiveWithNoExistingMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PLT123");
			receiveLine1.WE_LineNo = 1;
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has 10 expected quantity.", 10m, receiveLine1.WE_ClientOrderedUnits);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 50m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT456", dockdoorLocation.ToLocationString());
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);
			AssertEquals("Receiveline1 still has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 still has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			var receiveLine2 = receive.Lines.Single(line => line.WE_PalletID == "PLT456");
			AssertEquals("Receiveline2 has unloaded quantity as transaction quantity.", 50m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has unloaded quantity as stock on hand.", 50m, receiveLine2.WE_StockOnHand);
		}

		#endregion

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadMatchesStockUnit

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadMatchesStockUnit()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadMatchesStockUnitCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_EmptyPalletIDMatching_WithStockUnit_UnloadMatchesStockUnit()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadMatchesStockUnitCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadMatchesStockUnitCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_F3_NKPackType = "UNT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 25m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has 15 transaction quantity.", 15m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 15 stock on hand.", 15m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has unchanged pack type.", "UNT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 has expected pallet ID.", "PID", receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 10 transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 10 stock on hand.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has unchanged pack type.", "UNT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 has expected pallet ID.", "PID", receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_EmptyPalletIDMatching_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_F3_NKPackType = "UNT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has 15 transaction quantity.", 15m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 15 stock on hand.", 15m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has changed pack type.", "PLT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 has expected pallet ID.", "PID", receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 10 transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 10 stock on hand.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has changed pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 has expected pallet ID.", "PID", receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadMatchesPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadMatchesPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadMatchesPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_EmptyPalletIDMatching_WithDifferentPackUQ_UnloadMatchesPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadMatchesPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadMatchesPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine1.WE_F3_NKPackType = "PLT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			receiveLine2.WE_F3_NKPackType = "PLT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 1m);
			asnLine1.WN_QuantityUQ = "PLT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 2m);
			asnLine2.WN_QuantityUQ = "PLT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has 15 transaction quantity.", 15m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 15 stock on hand.", 15m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has unchanged pack type.", "PLT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 has expected pallet ID.", "PID", receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 10 transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 10 stock on hand.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has unchanged pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 has expected pallet ID.", "PID", receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadWithDifferentPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadWithDifferentPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_EmptyPalletIDMatching_WithDifferentPackUQ_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadWithDifferentPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_WithDifferentPackUQ_UnloadWithDifferentPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Helper.CreateProductUnit(data.Part1, "BAG", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			Assert("Precondition", data.Part1.UnitConverter.Convertible("BAG", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "BAG", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine1.WE_F3_NKPackType = "PLT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			receiveLine2.WE_F3_NKPackType = "PLT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 1m);
			asnLine1.WN_QuantityUQ = "PLT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 2m);
			asnLine2.WN_QuantityUQ = "PLT";
			Helper.Factory.Save();
			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "BAG", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive lines are created.", 3, receive.Lines.Count);

			AssertEquals("Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 is not assigned pallet ID.", expectedPalletID, receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is not assigned with location.", Guid.Empty, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 is not assigned pallet ID.", expectedPalletID, receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is not assigned with location.", Guid.Empty, receiveLine2.WE_WL);

			var receiveLine3 = receive.Lines[2];
			AssertEquals("Receiveline3 has 25 transaction quantity.", 25m, receiveLine3.WE_TransactionQuantity);
			AssertEquals("Receiveline3 has 25 stock on hand.", 25m, receiveLine3.WE_StockOnHand);
			AssertEquals("Receiveline3 expected no units.", 0m, receiveLine3.WE_ClientOrderedUnits);
			AssertEquals("Receiveline3 has 5 packs.", 5m, receiveLine3.WE_PackQuantity);
			AssertEquals("Receiveline3 is assigned pallet ID.", "PID", receiveLine3.WE_PalletID);
			AssertEquals("Receiveline3 has unchanged pack type.", "BAG", receiveLine3.WE_F3_NKPackType);
			AssertEquals("Receiveline3 is not assigned with location.", locationPK, receiveLine3.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadMatchesPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadMatchesPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadMatchesPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_EmptyPalletIDMatching_WithStockUnitAndDifferentPackUQ_UnloadMatchesPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadMatchesPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadMatchesPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			receiveLine2.WE_F3_NKPackType = "PLT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 2m);
			asnLine2.WN_QuantityUQ = "PLT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has unchanged pack type.", "UNT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 is not assigned pallet ID.", expectedPalletID, receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is not assigned with location.", Guid.Empty, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 25 transaction quantity.", 25m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 25 stock on hand.", 25m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has unchanged pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline1 is assigned pallet ID.", "PID", receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadDoesNotMatchPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadDoesNotMatchPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadDoesNotMatchPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_EmptyPalletIDMatching_WithStockUnitAndDifferentPackUQ_UnloadDoesNotMatchPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadDoesNotMatchPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentPackUQ_UnloadDoesNotMatchPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Helper.CreateProductUnit(data.Part1, "BAG", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			Assert("Precondition", data.Part1.UnitConverter.Convertible("BAG", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "BAG", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			receiveLine2.WE_F3_NKPackType = "PLT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 2m);
			asnLine2.WN_QuantityUQ = "PLT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "BAG", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive lines are created.", 3, receive.Lines.Count);

			AssertEquals("Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has unchanged pack type.", "UNT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 is not assigned pallet ID.", expectedPalletID, receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is not assigned with location.", Guid.Empty, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has unchanged pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 is not assigned pallet ID.", expectedPalletID, receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is not assigned with location.", Guid.Empty, receiveLine2.WE_WL);

			var receiveLine3 = receive.Lines[2];
			AssertEquals("Receiveline3 has 25 transaction quantity.", 25m, receiveLine3.WE_TransactionQuantity);
			AssertEquals("Receiveline3 has 25 stock on hand.", 25m, receiveLine3.WE_StockOnHand);
			AssertEquals("Receiveline3 expected no units.", 0m, receiveLine3.WE_ClientOrderedUnits);
			AssertEquals("Receiveline3 has 5 packs.", 5m, receiveLine3.WE_PackQuantity);
			AssertEquals("Receiveline3 has unload pack type.", "BAG", receiveLine3.WE_F3_NKPackType);
			AssertEquals("Receiveline1 is assigned pallet ID.", "PID", receiveLine3.WE_PalletID);
			AssertEquals("Receiveline3 is assigned with location.", locationPK, receiveLine3.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentProductWithDifferentPackUQ_UnloadDoesNotMatchPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnit_AndDifferentProductWithDifferentPackUQ_UnloadDoesNotMatchPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentProductWithDifferentPackUQ_UnloadDoesNotMatchPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentProductWithDifferentPackUQ_UnloadDoesNotMatchPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentProductWithDifferentPackUQ_UnloadDoesNotMatchPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_WithStockUnitAndDifferentProductWithDifferentPackUQ_UnloadDoesNotMatchPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "BAG", 5m);
			Helper.CreateProductUnit(data.Part2, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("BAG", "UNT"));
			Assert("Precondition", data.Part2.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "BAG", "UNT"));
			AssertEquals("Precondition", 5m, data.Part2.UnitConverter.Convert(1m, "PLT", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			receiveLine2.WE_F3_NKPackType = "PLT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part2, 2m);
			asnLine2.WN_QuantityUQ = "PLT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "BAG", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, new[] { data.Part1.PK.ToGuid(), data.Part2.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive lines are created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has 25 transaction quantity.", 25m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 25 stock on hand.", 25m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has unload pack type.", "BAG", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 is assigned pallet ID.", "PID", receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has unchanged pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 is not assigned pallet ID.", expectedPalletID, receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is not assigned with location.", Guid.Empty, receiveLine2.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQ

		public void TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLines_EmptyPalletIDMatching_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLines_WithStockUnit_UnloadWithDifferentPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));

			var location = data.Whs1.FindLocation("A-1-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, location, expectedPalletID);
			receiveLine1.WE_F3_NKPackType = "UNT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, expectedPalletID);
			receiveLine2.WE_F3_NKPackType = "UNT";

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 2m;
			receiveLine1.WE_StockOnHand = 2m;
			receiveLine2.WE_TransactionQuantity = 5m;
			receiveLine2.WE_StockOnHand = 5m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has location A-1-1.", location.PK, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 2 transaction quantity.", 2m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 2 stock on hand.", 2m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has location A-1-1.", location.PK, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 5 transaction quantity.", 5m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 5 stock on hand.", 5m, receiveLine2.WE_StockOnHand);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive lines are created.", 3, receive.Lines.Count);

			AssertEquals("Receiveline1 has 2 transaction quantity.", 2m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 2 stock on hand.", 2m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has unchanged pack type.", "UNT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 has expected pallet ID.", expectedPalletID, receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", location.PK, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 5 transaction quantity.", 5m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 5 stock on hand.", 5m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has unchanged pack type.", "UNT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 has expected pallet ID.", expectedPalletID, receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is assigned with location.", location.PK, receiveLine2.WE_WL);

			var receiveLine3 = receive.Lines[2];
			AssertEquals("Receiveline3 has 25 transaction quantity.", 25m, receiveLine3.WE_TransactionQuantity);
			AssertEquals("Receiveline3 has 25 stock on hand.", 25m, receiveLine3.WE_StockOnHand);
			AssertEquals("Receiveline3 expected no units.", 0m, receiveLine3.WE_ClientOrderedUnits);
			AssertEquals("Receiveline3 has pack type.", "PLT", receiveLine3.WE_F3_NKPackType);
			AssertEquals("Receiveline3 has expected pallet ID.", "PID", receiveLine3.WE_PalletID);
			AssertEquals("Receiveline3 is assigned with location.", locationPK, receiveLine3.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_UnloadUnder_WithDifferentPackUQ_UnloadMatchesPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_UnloadUnder_WithDifferentPackUQ_UnloadMatchesPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_UnloadUnder_WithDifferentPackUQ_UnloadMatchesPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_UnloadUnder_EmptyPalletIDMatching_WithDifferentPackUQ_UnloadMatchesPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLines_UnloadUnder_WithDifferentPackUQ_UnloadMatchesPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLines_UnloadUnder_WithDifferentPackUQ_UnloadMatchesPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_F3_NKPackType = "UNT";
			receiveLine2.WE_PalletID = expectedPalletID;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 20m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);

			var isEmptyAsnPalletIdMatching = string.IsNullOrEmpty(expectedPalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatching);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());

			AssertEquals("Receiveline1 has 20 transaction quantity.", 20m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 20 stock on hand.", 20m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 20 units.", 20m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has changed pack type.", "PLT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 has expected pallet ID.", "PID", receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);

			if (isEmptyAsnPalletIdMatching)
			{
				AssertEquals("Receive line has been unloaded, new receive lines are created.", 3, receive.Lines.Count);

				AssertEquals("Receiveline2 has 5 transaction quantity.", 5m, receiveLine2.WE_TransactionQuantity);
				AssertEquals("Receiveline2 has 5 stock on hand.", 5m, receiveLine2.WE_StockOnHand);
				AssertEquals("Receiveline2 changed expected to 5 units.", 5m, receiveLine2.WE_ClientOrderedUnits);
				AssertEquals("Receiveline2 has changed pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
				AssertEquals("Receiveline2 has expected pallet ID.", "PID", receiveLine2.WE_PalletID);
				AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);

				var receiveLine3 = receive.Lines[2];
				AssertEquals("Receiveline3 has 0 transaction quantity.", 0m, receiveLine3.WE_TransactionQuantity);
				AssertEquals("Receiveline3 has 0 stock on hand.", 0m, receiveLine3.WE_StockOnHand);
				AssertEquals("Receiveline3 expected 5 units.", 5m, receiveLine3.WE_ClientOrderedUnits);
				AssertEquals("Receiveline3 has changed pack type.", "UNT", receiveLine3.WE_F3_NKPackType);
				AssertEquals("Receiveline3 has expected pallet ID.", "", receiveLine3.WE_PalletID);
				AssertEquals("Receiveline3 is not assigned with location.", Guid.Empty, receiveLine3.WE_WL);
			}
			else
			{
				AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);

				AssertEquals("Receiveline2 has 5 transaction quantity.", 5m, receiveLine2.WE_TransactionQuantity);
				AssertEquals("Receiveline2 has 5 stock on hand.", 5m, receiveLine2.WE_StockOnHand);
				AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
				AssertEquals("Receiveline2 has changed pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
				AssertEquals("Receiveline2 has expected pallet ID.", "PID", receiveLine2.WE_PalletID);
				AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
			}
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQ

		public void TestUnloadWhsReceiveLine_ExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_ExistingAsnLinesWithReservedStock_EmptyPalletIDMatching_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_ExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_ExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = expectedPalletID;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_F3_NKPackType = "UNT";
			receiveLine2.WE_PalletID = expectedPalletID;

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine.ReserveStockIfAbleTo(receiveLine1.Inventory[0]);
			orderLine.ReserveStockIfAbleTo(receiveLine2.Inventory[0]);

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has 5 stock reserved.", 5m, receiveLine1.ReservedQuantity);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has 10 stock reserved.", 10m, receiveLine2.ReservedQuantity);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has 15 transaction quantity.", 15m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 15 stock on hand.", 15m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has changed pack type.", "PLT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 has expected pallet ID.", "PID", receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 10 transaction quantity.", 10m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 10 stock on hand.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has changed pack type.", "PLT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 has expected pallet ID.", "PID", receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQ

		public void TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "PID");
		}

		public void TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLinesWithReservedStock_EmptyPalletIDMatching_WithStockUnit_UnloadWithDifferentPackUQ()
		{
			TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQCore(expectedPalletID: "");
		}

		void TestUnloadWhsReceiveLine_PartiallyFilledExistingAsnLinesWithReservedStock_WithStockUnit_UnloadWithDifferentPackUQCore(string expectedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));

			var location = data.Whs1.FindLocation("A-1-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, location, expectedPalletID);
			receiveLine1.WE_F3_NKPackType = "UNT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, expectedPalletID);
			receiveLine2.WE_F3_NKPackType = "UNT";

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine.ReserveStockIfAbleTo(receiveLine1.Inventory[0]);
			orderLine.ReserveStockIfAbleTo(receiveLine2.Inventory[0]);

			receiveLine1.WE_TransactionQuantity = 2m;
			receiveLine1.WE_StockOnHand = 2m;
			receiveLine2.WE_TransactionQuantity = 5m;
			receiveLine2.WE_StockOnHand = 5m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has location A-1-1.", location.PK, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline has 2 transaction quantity.", 2m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 2 stock on hand.", 2m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has 5 stock reserved.", 5m, receiveLine1.ReservedQuantity);
			AssertEquals("Precondition: Receiveline has location A-1-1.", location.PK, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline has 5 transaction quantity.", 5m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 5 stock on hand.", 5m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has 10 stock reserved.", 10m, receiveLine2.ReservedQuantity);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PID", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive lines are created.", 3, receive.Lines.Count);

			AssertEquals("Receiveline1 has 2 transaction quantity.", 2m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has 2 stock on hand.", 2m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 expected 5 units.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has unchanged pack type.", "UNT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Receiveline1 has expected pallet ID.", expectedPalletID, receiveLine1.WE_PalletID);
			AssertEquals("Receiveline1 is assigned with location.", location.PK, receiveLine1.WE_WL);

			AssertEquals("Receiveline2 has 5 transaction quantity.", 5m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has 5 stock on hand.", 5m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 expected 10 units.", 10m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has unchanged pack type.", "UNT", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Receiveline2 has expected pallet ID.", expectedPalletID, receiveLine2.WE_PalletID);
			AssertEquals("Receiveline2 is assigned with location.", location.PK, receiveLine2.WE_WL);

			var receiveLine3 = receive.Lines[2];
			AssertEquals("Receiveline3 has 25 transaction quantity.", 25m, receiveLine3.WE_TransactionQuantity);
			AssertEquals("Receiveline3 has 25 stock on hand.", 25m, receiveLine3.WE_StockOnHand);
			AssertEquals("Receiveline3 expected no units.", 0m, receiveLine3.WE_ClientOrderedUnits);
			AssertEquals("Receiveline3 has pack type.", "PLT", receiveLine3.WE_F3_NKPackType);
			AssertEquals("Receiveline3 has expected pallet ID.", "PID", receiveLine3.WE_PalletID);
			AssertEquals("Receiveline3 is assigned with location.", locationPK, receiveLine3.WE_WL);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_ExistingAsnLines_MissingUnitConversion

		public void TestUnloadWhsReceiveLine_ExistingAsnLines_MissingUnitConversion()
		{
			var expectedPalletID = "PID";
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_F3_NKPackType = "UNT";
			receiveLine.WE_PalletID = expectedPalletID;

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine.ReserveStockIfAbleTo(receiveLine.Inventory[0]);

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService1 = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", expectedPalletID, "A-1-1");
			var response1 = webService1.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo1 }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("Web service response should have error when cannot convert.", "Cannot convert 'PLT' to 'UNT' for the product 'P1'. Please add this conversion and try again.", response1.ErrorMessage);
			AssertEquals("Web service response should have error when cannot convert", true, response1.InventoryErrorInfos.Any());

			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			Assert("Precondition", data.Part1.UnitConverter.Convertible("PLT", "UNT"));
			AssertEquals("Precondition", 5m, data.Part1.UnitConverter.Convert(1m, "PLT", "UNT"));
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has 5 stock reserved.", 5m, receiveLine.ReservedQuantity);

			var webService2 = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 5m, "PLT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", expectedPalletID, "A-1-1");
			var response2 = webService2.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo2 }, new[] { data.Part1.PK.ToGuid() }, isEmptyAsnPalletIdMatchingEnabledForUnloadLine: string.IsNullOrEmpty(expectedPalletID));
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response2.InventoryErrorInfos.Any());

			AssertEquals("Receiveline has 25 transaction quantity.", 25m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has 25 stock on hand.", 25m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline expected 5 units.", 5m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline has changed pack type.", "PLT", receiveLine.WE_F3_NKPackType);
			AssertEquals("Receiveline has expected pallet ID.", expectedPalletID, receiveLine.WE_PalletID);
			AssertEquals("Receiveline is assigned with location.", locationPK, receiveLine.WE_WL);
		}

		#endregion

		#endregion

		#region TestUnloadedWhsReceiveLine_UnloadedTime

		[TestDate(2019, 09, 24)]
		public void TestUnloadedWhsReceiveLine_HasUnloadedTime()
		{
			TestUnloadedWhsReceiveLine_UnloadedTimeCore(true);
		}

		[TestDate(2019, 09, 24)]
		public void TestUnloadedWhsReceiveLine_NoUnloadedTime()
		{
			TestUnloadedWhsReceiveLine_UnloadedTimeCore(false);
		}

		void TestUnloadedWhsReceiveLine_UnloadedTimeCore(bool hasUnloadedTime)
		{
			var unloadedTime = ZDateTime.Now.AddHours(-1);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);

			if (hasUnloadedTime)
			{
				receive.CreateUnloadTime(unloadedTime.ToOffset());

				var logEvents_BeforeUnload = Helper.FindLogs(receive.Logs, Events.WarehouseReceiptUnloaded, "");
				AssertEquals("Precondition: Receive has unload time", 1, logEvents_BeforeUnload.Length);
			}
			Helper.Factory.Save();

			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT123", dockdoorLocation.ToLocationString());
			var webService = GetNewWebService(data.Whs1);
			webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);

			var receiveInNewFactory = webService.Factory.Load<WhsReceive>(receive.PK);
			var logEvents_AfterUnload = Helper.FindLogs(receiveInNewFactory.Logs, Events.WarehouseReceiptUnloaded, "");
			AssertEquals("Should only have 1 WarehouseReceiptUnloaded event", 1, logEvents_AfterUnload.Length);
			if (hasUnloadedTime)
			{
				AssertEquals("Should not update unload time", unloadedTime, logEvents_AfterUnload[0].SL_EventTime);
			}
			else
			{
				AssertEquals("Should create a new unload time", ZDateTime.Now, logEvents_AfterUnload[0].SL_EventTime);
			}
		}

		#endregion

		#region TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_AllowedEmptyPalletIdMatching()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive lines are created.", 1, receive.Lines.Count);

			AssertEquals("Receiveline has 10 transaction quantity.", 10m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has 10 stock on hand.", 10m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is assigned with location.", locationPK, receiveLine.WE_WL);
			AssertEquals("Receiveline is assigned with a pallet id.", "ABC", receiveLine.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_NotAllowedEmptyPalletIdMatching()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), false);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has no transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has expected quantity.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline has no stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is not assigned with location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Receiveline is not assigned with a pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity > 0);
			AssertEquals("New receiveline has no expected quantity.", 0m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New receiveline has transaction quantity.", 10m, newInventory.WE_TransactionQuantity);
			AssertEquals("New receiveline has stock on hand.", 10m, newInventory.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", locationPK, newInventory.WE_WL);
			AssertEquals("New receiveline is assigned with a pallet id.", "ABC", newInventory.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_AllowedEmptyPalletIdMatching_WithPartAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine.WE_PartAttrib1 = "ABCD1234";

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);
			AssertEquals("Precondition: Receiveline has serial number.", "ABCD1234", receiveLine.WE_PartAttrib1);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "ABCD4567", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo1 }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has no transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has expected quantity.", 1m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline has no stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is not assigned with location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Receiveline is not assigned with a pallet id.", string.Empty, receiveLine.WE_PalletID);

			var newReceiveLine = receive.Lines.Single(line => line.WE_TransactionQuantity > 0);
			AssertEquals("New receiveline has transaction quantity.", 1m, newReceiveLine.WE_TransactionQuantity);
			AssertEquals("New receiveline has no expected quantity.", 0m, newReceiveLine.WE_ClientOrderedUnits);
			AssertEquals("New receiveline has stock on hand.", 1m, newReceiveLine.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", locationPK, newReceiveLine.WE_WL);
			AssertEquals("New receiveline is assigned with a pallet id.", "ABC", newReceiveLine.WE_PalletID);
			AssertEquals("New receiveline has serial number.", "ABCD4567", newReceiveLine.WE_PartAttrib1);

			var webService2 = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1m, "UNT", "ABCD1234", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response2 = webService2.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo2 }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has transaction quantity.", 1m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has expected quantity.", 1m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline has stock on hand.", 1m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is assigned with location.", locationPK, receiveLine.WE_WL);
			AssertEquals("Receiveline is assigned with a pallet id.", "ABC", receiveLine.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_InventoryWithLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var location = data.Whs1.FindLocation("A-1-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			Helper.Factory.Save();

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has location.", location.PK, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has no transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has expected quantity.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline has no stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is not assigned with a pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity > 0);
			AssertEquals("New receiveline has transaction quantity.", 10m, newInventory.WE_TransactionQuantity);
			AssertEquals("New receiveline has no expected quantity.", 0m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New receiveline has stock on hand.", 10m, newInventory.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", location.PK, newInventory.WE_WL);
			AssertEquals("New receiveline is assigned with a pallet id.", "ABC", newInventory.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_InventoryWithNoExpectedQty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has 0 expected quantity.", 0m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has no transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has no stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is not assigned with a pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity > 0);
			AssertEquals("New receiveline has transaction quantity.", 10m, newInventory.WE_TransactionQuantity);
			AssertEquals("New receiveline has stock on hand.", 10m, newInventory.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", locationPK, newInventory.WE_WL);
			AssertEquals("New receiveline is assigned with a pallet id.", "ABC", newInventory.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_InventoryWithTransactionQty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has transaction quantity.", 20m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has stock on hand.", 20m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline's transaction quantity remains the same.", 20m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline's stock on hand remains the same.", 20m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is not assigned with location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Receiveline is not assigned with a pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var newInventory = receive.Lines.Single(line => !line.WE_PalletID.IsEmpty);
			AssertEquals("New receiveline has no expected quantity.", 0m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New receiveline has transaction quantity.", 10m, newInventory.WE_TransactionQuantity);
			AssertEquals("New receiveline has stock on hand.", 10m, newInventory.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", locationPK, newInventory.WE_WL);
			AssertEquals("New receiveline is assigned with a pallet id.", "ABC", newInventory.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_InventoryWithMatchingPalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "ABC");

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;

			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has no transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has no stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has no pallet id.", ZString.Empty, receiveLine1.WE_PalletID);

			AssertEquals("Precondition: Receiveline2 has no transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has no stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has pallet id.", "ABC", receiveLine2.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService1 = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 20m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response1 = webService1.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo1 }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response1.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has no transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has no stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 is not assigned with location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Receiveline1 is not assigned with a pallet id.", string.Empty, receiveLine1.WE_PalletID);

			AssertEquals("ReceiveLine2 has expected quantity.", 20m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("ReceiveLine2 has transaction quantity.", 20m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("ReceiveLine2 has stock on hand.", 20m, receiveLine2.WE_StockOnHand);
			AssertEquals("ReceiveLine2 is assigned with location.", locationPK, receiveLine2.WE_WL);

			var webService2 = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response2 = webService2.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo2 }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response2.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has no transaction quantity.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has no stock on hand.", 10m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);
			AssertEquals("Receiveline1 is assigned with a pallet id.", "ABC", receiveLine1.WE_PalletID);

			AssertEquals("ReceiveLine2 has expected quantity remain the same.", 20m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("ReceiveLine2 has transaction quantity.", 20m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("ReceiveLine2 has stock on hand.", 20m, receiveLine2.WE_StockOnHand);
			AssertEquals("ReceiveLine2 is assigned with location.", locationPK, receiveLine2.WE_WL);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_InventoryWithMatchingPalletIdAndLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var location = data.Whs1.FindLocation("A-1-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "ABC");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location, "ABC");

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has no transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has no stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has pallet id.", "ABC", receiveLine1.WE_PalletID);

			AssertEquals("Precondition: Receiveline2 has transaction quantity.", 20m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has expected quantity.", 20m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline2 has stock on hand.", 20m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has pallet id.", "ABC", receiveLine2.WE_PalletID);
			AssertEquals("Precondition: Receiveline2 has location.", location.PK, receiveLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, no new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline1 has no transaction quantity.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has no stock on hand.", 10m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1 is assigned with location.", location.PK, receiveLine1.WE_WL);

			AssertEquals("ReceiveLine2 has expected quantity remain the same.", 20m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("ReceiveLine2 has transaction quantity remain the same.", 20m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("ReceiveLine2 has stock on hand remain the same.", 20m, receiveLine2.WE_StockOnHand);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_NoQtyUnloaded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);
			AssertEquals("Precondition: Receiveline has expected quantity.", 10m, receiveLine.WE_ClientOrderedUnits);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 0m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has no transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has expected quantity.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline has no stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline is not assigned with location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Receiveline is not assigned with a pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var newInventoryWithNoQty = receive.Lines.Single(line => !line.WE_PalletID.IsEmpty);
			AssertEquals("New receiveline still has no transaction quantity.", 0m, newInventoryWithNoQty.WE_TransactionQuantity);
			AssertEquals("New receiveline still has no stock on hand.", 0m, newInventoryWithNoQty.WE_StockOnHand);
			AssertEquals("New receiveline is assigned with location.", locationPK, newInventoryWithNoQty.WE_WL);
			AssertEquals("New receiveline is assigned with a pallet id.", "ABC", newInventoryWithNoQty.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_ExpectedQtyGreaterThanUnloadQty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 25m);

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has expected quantity.", 25m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created from splitting the existing inventory.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has transaction quantity.", 10m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has stock on hand.", 10m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline's expected quantity is reduced.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline is assigned with location.", locationPK, receiveLine.WE_WL);
			AssertEquals("Receiveline is assigned with a pallet id.", "ABC", receiveLine.WE_PalletID);

			var newInventoryFromSplit = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals("New receiveline has no transaction quantity.", 0m, newInventoryFromSplit.WE_TransactionQuantity);
			AssertEquals("New receiveline has no stock on hand.", 0m, newInventoryFromSplit.WE_StockOnHand);
			AssertEquals("New receiveline has expected quantity (original line's expected qty - unload qty).", 15m, newInventoryFromSplit.WE_ClientOrderedUnits);
			AssertEquals("New receiveline is not assigned with location.", ZGuid.Empty, newInventoryFromSplit.WE_WL);
			AssertEquals("New receiveline is not assigned with a pallet id.", string.Empty, newInventoryFromSplit.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_ExpectedQtyGreaterThanUnloadQty_WithReservedQty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 25m);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine.ReserveStockIfAbleTo(receiveLine.Inventory[0]);
			Helper.Factory.Save();

			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline has no location.", ZGuid.Empty, receiveLine.WE_WL);
			AssertEquals("Precondition: Receiveline has 0 transaction quantity.", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline has 0 stock on hand.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline has expected quantity.", 25m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline has no pallet id.", ZString.Empty, receiveLine.WE_PalletID);
			AssertEquals("Precondition: Receiveline has reserved quantity.", 15m, receiveLine.ReservedQuantity);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created from splitting the existing inventory.", 2, receive.Lines.Count);

			AssertEquals("Receiveline has transaction quantity.", 10m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Receiveline has stock on hand.", 10m, receiveLine.WE_StockOnHand);
			AssertEquals("Receiveline's expected quantity is reduced.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receiveline is assigned with location.", locationPK, receiveLine.WE_WL);
			AssertEquals("Receiveline is assigned with a pallet id.", "ABC", receiveLine.WE_PalletID);
			AssertEquals("Receiveline's reserved quantity is reduced.", 10m, receiveLine.ReservedQuantity);

			var newInventoryFromSplit = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals("New receiveline has no transaction quantity.", 0m, newInventoryFromSplit.WE_TransactionQuantity);
			AssertEquals("New receiveline has no stock on hand.", 0m, newInventoryFromSplit.WE_StockOnHand);
			AssertEquals("New receiveline has expected quantity (original line's expected qty - unload qty).", 15m, newInventoryFromSplit.WE_ClientOrderedUnits);
			AssertEquals("New receiveline is not assigned with location.", ZGuid.Empty, newInventoryFromSplit.WE_WL);
			AssertEquals("New receiveline is not assigned with a pallet id.", string.Empty, newInventoryFromSplit.WE_PalletID);
			AssertEquals("New receiveline has reserved quantity.", 5m, newInventoryFromSplit.ReservedQuantity);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_UnloadOnMultipleInventories()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has expected quantity.", 10m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline1 has no pallet id.", ZString.Empty, receiveLine1.WE_PalletID);

			AssertEquals("Precondition: Receiveline2 has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has expected quantity.", 15m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline2 has no pallet id.", ZString.Empty, receiveLine2.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 20m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, a new receive line is created from splitting of an existing inventory.", 3, receive.Lines.Count);

			AssertEquals("Receiveline1 has transaction quantity.", 5m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has stock on hand.", 5m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1's expected quantity is reduced.", 5m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);
			AssertEquals("Receiveline1 is assigned with a pallet id.", "ABC", receiveLine1.WE_PalletID);

			AssertEquals("Receiveline2 has transaction quantity.", 15m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has stock on hand.", 15m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
			AssertEquals("Receiveline2 is assigned with a pallet id.", "ABC", receiveLine2.WE_PalletID);

			var newInventoryFromSplit = receive.Lines.Single(line => line.WE_PalletID.IsEmpty);
			AssertEquals("New receiveline has no transaction quantity.", 0m, newInventoryFromSplit.WE_TransactionQuantity);
			AssertEquals("New receiveline has no stock on hand.", 0m, newInventoryFromSplit.WE_StockOnHand);
			AssertEquals("New receiveline has expected quantity.", 5m, newInventoryFromSplit.WE_ClientOrderedUnits);
			AssertEquals("New receiveline is not assigned with location.", ZGuid.Empty, newInventoryFromSplit.WE_WL);
			AssertEquals("New receiveline is not assigned with a pallet id.", ZString.Empty, newInventoryFromSplit.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_EmptyAsnPalletIdMatching_UnloadOnMultipleInventoriesWithReservedQty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);

			receiveLine1.WE_TransactionQuantity = 0m;
			receiveLine1.WE_StockOnHand = 0m;
			receiveLine2.WE_TransactionQuantity = 0m;
			receiveLine2.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 12m);
			orderLine1.ReserveStockIfAbleTo(receiveLine1.Inventory[0]);
			orderLine2.ReserveStockIfAbleTo(receiveLine2.Inventory[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has no location.", ZGuid.Empty, receiveLine1.WE_WL);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline1 has expected quantity.", 15m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline1 has reserved quantity.", 10m, receiveLine1.ReservedQuantity);
			AssertEquals("Precondition: Receiveline1 has no pallet id.", ZString.Empty, receiveLine1.WE_PalletID);

			AssertEquals("Precondition: Receiveline2 has no location.", ZGuid.Empty, receiveLine2.WE_WL);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has expected quantity.", 20m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Precondition: Receiveline2 has reserved quantity.", 12m, receiveLine2.ReservedQuantity);
			AssertEquals("Precondition: Receiveline2 has no pallet id.", ZString.Empty, receiveLine2.WE_PalletID);

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 27m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID: "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);
			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive lines are created from inventory splits.", 4, receive.Lines.Count);

			AssertEquals("Receiveline1 has transaction quantity.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has stock on hand.", 10m, receiveLine1.WE_StockOnHand);
			AssertEquals("Receiveline1's expected quantity is reduced.", 10m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1's reserved quantity is reduced.", 10m, receiveLine1.ReservedQuantity);
			AssertEquals("Receiveline1 is assigned with location.", locationPK, receiveLine1.WE_WL);
			AssertEquals("Receiveline1 is assigned with a pallet id.", "ABC", receiveLine1.WE_PalletID);

			AssertEquals("Receiveline2 has transaction quantity.", 12m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has stock on hand.", 12m, receiveLine2.WE_StockOnHand);
			AssertEquals("Receiveline2's expected quantity remains the same.", 12m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2's reserved quantity remains the same.", 12m, receiveLine2.ReservedQuantity);
			AssertEquals("Receiveline2 is assigned with location.", locationPK, receiveLine2.WE_WL);
			AssertEquals("Receiveline2 is assigned with a pallet id.", "ABC", receiveLine2.WE_PalletID);

			var newInventoryFromReceiveLine1Split = receive.Lines.Single(line => line.WE_TransactionQuantity == 5m);
			AssertEquals("New receiveline has transaction quantity.", 5m, newInventoryFromReceiveLine1Split.WE_TransactionQuantity);
			AssertEquals("New receiveline has stock on hand.", 5m, newInventoryFromReceiveLine1Split.WE_StockOnHand);
			AssertEquals("New receiveline has expected quantity.", 5m, newInventoryFromReceiveLine1Split.WE_ClientOrderedUnits);
			AssertEquals("New receiveline has no reserved quantity.", 0m, newInventoryFromReceiveLine1Split.ReservedQuantity);
			AssertEquals("New receiveline is assigned with location.", locationPK, newInventoryFromReceiveLine1Split.WE_WL);
			AssertEquals("New receiveline is assigned with a pallet id.", "ABC", newInventoryFromReceiveLine1Split.WE_PalletID);

			var newInventoryFromReceiveLine2Split = receive.Lines.Single(line => line.WE_PalletID.IsEmpty);
			AssertEquals("New receiveline has no transaction quantity.", 0m, newInventoryFromReceiveLine2Split.WE_TransactionQuantity);
			AssertEquals("New receiveline has no stock on hand.", 0m, newInventoryFromReceiveLine2Split.WE_StockOnHand);
			AssertEquals("New receiveline has expected quantity.", 8m, newInventoryFromReceiveLine2Split.WE_ClientOrderedUnits);
			AssertEquals("New receiveline has no reserved quantity.", 0m, newInventoryFromReceiveLine2Split.ReservedQuantity);
			AssertEquals("New receiveline is not assigned with location.", ZGuid.Empty, newInventoryFromReceiveLine2Split.WE_WL);
			AssertEquals("New receiveline is not assigned with a pallet id.", string.Empty, newInventoryFromReceiveLine2Split.WE_PalletID);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_MatchingInventoryFallback

		public void TestUnloadWhsReceiveLine_MatchingInventoryFallback()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var lineWithMatchingLocationAndPalletId = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "ABC");
			var notMatchingInventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, location2);
			var lineWithEmptyLocationAndMatchingPalletId = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "ABC");
			var lineWithEmptyLocationAndPalletId = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);

			lineWithMatchingLocationAndPalletId.WE_TransactionQuantity = 0m;
			lineWithMatchingLocationAndPalletId.WE_StockOnHand = 0m;
			notMatchingInventory.WE_TransactionQuantity = 0m;
			notMatchingInventory.WE_StockOnHand = 0m;
			lineWithEmptyLocationAndMatchingPalletId.WE_TransactionQuantity = 0m;
			lineWithEmptyLocationAndMatchingPalletId.WE_StockOnHand = 0m;
			lineWithEmptyLocationAndPalletId.WE_TransactionQuantity = 0m;
			lineWithEmptyLocationAndPalletId.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 4, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, lineWithMatchingLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, lineWithMatchingLocationAndPalletId.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, notMatchingInventory.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, notMatchingInventory.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline3 has no location.", ZGuid.Empty, lineWithEmptyLocationAndMatchingPalletId.WE_WL);
			AssertEquals("Precondition: Receiveline3 has 0 transaction quantity.", 0m, lineWithEmptyLocationAndMatchingPalletId.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline3 has 0 stock on hand.", 0m, lineWithEmptyLocationAndMatchingPalletId.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline4 has no location.", ZGuid.Empty, lineWithEmptyLocationAndPalletId.WE_WL);
			AssertEquals("Precondition: Receiveline4 has no pallet id.", string.Empty, lineWithEmptyLocationAndPalletId.WE_PalletID);
			AssertEquals("Precondition: Receiveline4 has 0 transaction quantity.", 0m, lineWithEmptyLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline4 has 0 stock on hand.", 0m, lineWithEmptyLocationAndPalletId.WE_StockOnHand);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 40m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);

			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive line is created from inventory split.", 5, receive.Lines.Count);

			AssertEquals("Receiveline1 has transaction quantity.", 10m, lineWithMatchingLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has stock on hand.", 10m, lineWithMatchingLocationAndPalletId.WE_StockOnHand);
			AssertEquals("Receiveline1 has expected quantity.", 10m, lineWithMatchingLocationAndPalletId.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has location.", location1.PK, lineWithMatchingLocationAndPalletId.WE_WL);

			AssertEquals("Receiveline2 has no transaction quantity.", 0m, notMatchingInventory.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has no stock on hand.", 0m, notMatchingInventory.WE_StockOnHand);
			AssertEquals("Receiveline2 has expected quantity.", 15m, notMatchingInventory.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has location.", location2.PK, notMatchingInventory.WE_WL);

			AssertEquals("Receiveline3 has transaction quantity.", 20m, lineWithEmptyLocationAndMatchingPalletId.WE_TransactionQuantity);
			AssertEquals("Receiveline3 has stock on hand.", 20m, lineWithEmptyLocationAndMatchingPalletId.WE_StockOnHand);
			AssertEquals("Receiveline3 has expected quantity.", 20m, lineWithEmptyLocationAndMatchingPalletId.WE_ClientOrderedUnits);
			AssertEquals("Receiveline3 has location.", location1.PK, lineWithEmptyLocationAndMatchingPalletId.WE_WL);

			AssertEquals("Receiveline4 has transaction quantity.", 10m, lineWithEmptyLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Receiveline4 has stock on hand.", 10m, lineWithEmptyLocationAndPalletId.WE_StockOnHand);
			AssertEquals("Receiveline4 has expected quantity.", 10m, lineWithEmptyLocationAndPalletId.WE_ClientOrderedUnits);
			AssertEquals("Receiveline4 has location.", location1.PK, lineWithEmptyLocationAndPalletId.WE_WL);
			AssertEquals("Receiveline4 has pallet id.", "ABC", lineWithEmptyLocationAndPalletId.WE_PalletID);

			var newInventoryFromInventorySplit = receive.Lines.Single(line => line.WE_ClientOrderedUnits == 10m && line.WE_TransactionQuantity == 0m);
			AssertEquals("New inventory from split has no stock on hand.", 0m, newInventoryFromInventorySplit.WE_StockOnHand);
			AssertEquals("New inventory from split has expected quantity.", 10m, newInventoryFromInventorySplit.WE_ClientOrderedUnits);
			AssertEquals("New inventory from split has no location.", ZGuid.Empty, newInventoryFromInventorySplit.WE_WL);
			AssertEquals("New inventory from split has no pallet id.", string.Empty, newInventoryFromInventorySplit.WE_PalletID);
		}

		public void TestUnloadWhsReceiveLine_MatchingInventoryFallback_OverloadLineWithExcess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			var lineWithMatchingLocationAndPalletId = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "ABC");
			lineWithMatchingLocationAndPalletId.WE_LineNo = 4;

			var notMatchingInventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, location2);
			notMatchingInventory.WE_LineNo = 3;

			var lineWithEmptyLocationAndMatchingPalletId = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "ABC");
			lineWithEmptyLocationAndMatchingPalletId.WE_LineNo = 2;

			var lineWithEmptyLocationAndPalletId = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			lineWithEmptyLocationAndPalletId.WE_LineNo = 1;

			lineWithMatchingLocationAndPalletId.WE_TransactionQuantity = 0m;
			lineWithMatchingLocationAndPalletId.WE_StockOnHand = 0m;
			notMatchingInventory.WE_TransactionQuantity = 0m;
			notMatchingInventory.WE_StockOnHand = 0m;
			lineWithEmptyLocationAndMatchingPalletId.WE_TransactionQuantity = 0m;
			lineWithEmptyLocationAndMatchingPalletId.WE_StockOnHand = 0m;
			lineWithEmptyLocationAndPalletId.WE_TransactionQuantity = 0m;
			lineWithEmptyLocationAndPalletId.WE_StockOnHand = 0m;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			AssertEquals("Precondition", 4, receive.Lines.Count);
			AssertEquals("Precondition: Receiveline1 has 0 transaction quantity.", 0m, lineWithMatchingLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline1 has 0 stock on hand.", 0m, lineWithMatchingLocationAndPalletId.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline2 has 0 transaction quantity.", 0m, notMatchingInventory.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline2 has 0 stock on hand.", 0m, notMatchingInventory.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline3 has no location.", ZGuid.Empty, lineWithEmptyLocationAndMatchingPalletId.WE_WL);
			AssertEquals("Precondition: Receiveline3 has 0 transaction quantity.", 0m, lineWithEmptyLocationAndMatchingPalletId.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline3 has 0 stock on hand.", 0m, lineWithEmptyLocationAndMatchingPalletId.WE_StockOnHand);
			AssertEquals("Precondition: Receiveline4 has no location.", ZGuid.Empty, lineWithEmptyLocationAndPalletId.WE_WL);
			AssertEquals("Precondition: Receiveline4 has no pallet id.", string.Empty, lineWithEmptyLocationAndPalletId.WE_PalletID);
			AssertEquals("Precondition: Receiveline4 has 0 transaction quantity.", 0m, lineWithEmptyLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Precondition: Receiveline4 has 0 stock on hand.", 0m, lineWithEmptyLocationAndPalletId.WE_StockOnHand);

			var webService = GetNewWebService(data.Whs1);
			var unloadedReceiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 50m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "ABC", "A-1-1");
			var response = webService.UnloadWhsReceiveLines(new[] { unloadedReceiveLineInfo }, Array.Empty<Guid>(), true);

			AssertEquals("No error from the web service response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No inventory errors from the response.", false, response.InventoryErrorInfos.Any());
			AssertEquals("Receive line has been unloaded, new receive line is created from inventory split.", 4, receive.Lines.Count);

			AssertEquals("Receiveline1 has transaction quantity.", 10m, lineWithMatchingLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Receiveline1 has stock on hand.", 10m, lineWithMatchingLocationAndPalletId.WE_StockOnHand);
			AssertEquals("Receiveline1 has expected quantity.", 10m, lineWithMatchingLocationAndPalletId.WE_ClientOrderedUnits);
			AssertEquals("Receiveline1 has location.", location1.PK, lineWithMatchingLocationAndPalletId.WE_WL);

			AssertEquals("Receiveline2 has no transaction quantity.", 0m, notMatchingInventory.WE_TransactionQuantity);
			AssertEquals("Receiveline2 has no stock on hand.", 0m, notMatchingInventory.WE_StockOnHand);
			AssertEquals("Receiveline2 has expected quantity.", 15m, notMatchingInventory.WE_ClientOrderedUnits);
			AssertEquals("Receiveline2 has location.", location2.PK, notMatchingInventory.WE_WL);

			AssertEquals("Receiveline3 has transaction quantity.", 20m, lineWithEmptyLocationAndMatchingPalletId.WE_TransactionQuantity);
			AssertEquals("Receiveline3 has stock on hand.", 20m, lineWithEmptyLocationAndMatchingPalletId.WE_StockOnHand);
			AssertEquals("Receiveline3 has expected quantity.", 20m, lineWithEmptyLocationAndMatchingPalletId.WE_ClientOrderedUnits);
			AssertEquals("Receiveline3 has location.", location1.PK, lineWithEmptyLocationAndMatchingPalletId.WE_WL);

			AssertEquals("Receiveline4 has transaction quantity.", 20m, lineWithEmptyLocationAndPalletId.WE_TransactionQuantity);
			AssertEquals("Receiveline4 has stock on hand.", 20m, lineWithEmptyLocationAndPalletId.WE_StockOnHand);
			AssertEquals("Receiveline4 has expected quantity.", 10m, lineWithEmptyLocationAndPalletId.WE_ClientOrderedUnits);
			AssertEquals("Receiveline4 has location.", location1.PK, lineWithEmptyLocationAndPalletId.WE_WL);
			AssertEquals("Receiveline4 has pallet id.", "ABC", lineWithEmptyLocationAndPalletId.WE_PalletID);
		}

		#endregion

		#region TestUnloadWhsReceiveLines_IsPutawayTransferCreatedForThisPalletID_CheckWarehouse

		public void TestUnloadWhsReceiveLines_IsPutawayTransferCreatedForThisPalletID_CheckWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;
			var palletID = "PLT-123";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RN", data.Part1, 10m, dockDoorLocation, palletID, true, false);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, palletID, 10m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();

			var receiveSameWarehouse = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveOtherWarehouse = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			var webService1 = GetNewWebService(data.Whs1);
			var response = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receiveSameWarehouse.PK.ToGuid(), data.Part1.OP_PartNum, 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID, "") }, Array.Empty<Guid>(), false);

			AssertSuccessfulResponse(response, webService1);
			AssertEquals("Error - WE_PalletID: Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.", response.ErrorMessage);
			AssertEquals(response.Error, ErrorTypes.BusinessValidationError);

			var webService2 = GetNewWebService(whs2);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receiveOtherWarehouse.PK.ToGuid(), data.Part1.OP_PartNum, 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID, "") }, Array.Empty<Guid>(), false);

			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("No error for other warehouse.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals(ErrorTypes.None, response2.Error);
		}

		#endregion

		#region TestUnloadWhsReceiveLines_IsPutawayTransferCreatedForThisPalletID_ZeroStock

		public void TestUnloadWhsReceiveLines_IsPutawayTransferCreatedForThisPalletID_ZeroStock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;
			var palletID = "PLT-123";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RN", data.Part1, 10m, dockDoorLocation, palletID, true, false);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, palletID, 10m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();

			var receiveWebService = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			var webService1 = GetNewWebService(data.Whs1);
			var response = webService1.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID, "") }, Array.Empty<Guid>(), false);

			AssertSuccessfulResponse(response, webService1);
			AssertEquals("Error - WE_PalletID: Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.", response.ErrorMessage);
			AssertEquals(response.Error, ErrorTypes.BusinessValidationError);

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			Helper.Factory.Save();
			AssertEquals("Precondition: transfer Line status is Putaway.", InventoryStatus.Codes.Putaway, transferLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();
			AssertEquals("Precondition: transfer Line status is Available.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O01", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Helper.Factory.Save();

			AssertEquals("Precondition - Zero stock on hand.", 0m, transferLine.WE_StockOnHand);
			// Send same request
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receiveWebService.PK.ToGuid(), data.Part1.OP_PartNum, 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID, "") }, Array.Empty<Guid>(), false);

			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("No error as WE_StockOnHand is zero.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals(ErrorTypes.None, response2.Error);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_OneReceiveLine_ReceivedQuantityExceedsProductAllowedQuantity

		public void TestUnloadWhsReceiveLine_OneReceiveLine_ReceivedQuantityExceedsProductAllowedQuantity()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 3;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product, 5m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var response = webService.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), product.OP_PartNum, 6m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", "") }, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Error - ReceivedQuantity: Received quantity of product 'P1' exceeds the allowed quantity.", response.ErrorMessage);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_MultipleReceiveLinesWithSameProduct_PartOfLinesExceedProductAllowedQuantity

		public void TestUnloadWhsReceiveLine_MultipleReceiveLinesWithSameProduct_PartOfLinesExceedProductAllowedQuantity()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 100;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product, 5m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var lines = new[]
			{
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product.OP_PartNum, 6m, "UNT", "B1", "", "AT31", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product.OP_PartNum, 5m, "UNT", "B1", "", "AT32", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product.OP_PartNum, 1m, "UNT", "B1", "", "AT33", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product.OP_PartNum, 4m, "UNT", "B1", "", "AT34", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product.OP_PartNum, 2m, "UNT", "B1", "", "AT35", "", ZDate.Empty, ZDate.Empty, "", "", ""),
			};
			var response = webService.UnloadWhsReceiveLines(lines, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Error - ReceivedQuantity: Received quantity of product 'P1' exceeds the allowed quantity.", response.ErrorMessage);

			var productSummary = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().FirstOrDefault(ps => ps.ProductPk == product.PK);
			AssertEquals(5m, productSummary.ExpectedQuantity);
			AssertEquals(9m, productSummary.ReceivedQuantity);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_MultipleReceiveLinesWithDifferentProducts_PartOfLinesExceedProductAllowedQuantity

		public void TestUnloadWhsReceiveLine_MultipleReceiveLinesWithDifferentProducts_PartOfLinesExceedProductAllowedQuantity()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 100;
			var partRelation2 = Helper.CreateProductClientRelationShip(client, product2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_PreventReceivingOvers = true;
			partRelation2.OU_ReceiveOverageTolerancePercent = 0;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product1, 5m);
			Helper.CreateWhsReceiveLine(receive, product2, 10m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var lines = new[]
			{
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 6m, "UNT", "B11", "", "AT311", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 4m, "UNT", "B11", "", "AT312", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 7m, "UNT", "B21", "", "AT321", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 4m, "UNT", "B22", "", "AT322", "", ZDate.Empty, ZDate.Empty, "", "", ""),
			};
			var response = webService.UnloadWhsReceiveLines(lines, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Error - ReceivedQuantity: Received quantity of product 'P2' exceeds the allowed quantity.", response.ErrorMessage);

			var productSummaryCollection = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToArray();
			var product1Summary = productSummaryCollection.Where(ps => ps.ProductPk == product1.PK).FirstOrDefault();
			AssertEquals(5m, product1Summary.ExpectedQuantity);
			AssertEquals(10m, product1Summary.ReceivedQuantity);

			var product2Summary = productSummaryCollection.Where(ps => ps.ProductPk == product2.PK).FirstOrDefault();
			AssertEquals(10m, product2Summary.ExpectedQuantity);
			AssertEquals(7m, product2Summary.ReceivedQuantity);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_MultipleReceiveLinesWithUnexpectedProducts_PartOfLinesExceedProductAllowedQuantity

		public void TestUnloadWhsReceiveLine_MultipleReceiveLinesWithUnexpectedProducts_PartOfLinesExceedProductAllowedQuantity()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 100;
			var partRelation2 = Helper.CreateProductClientRelationShip(client, product2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_PreventReceivingOvers = true;
			partRelation2.OU_ReceiveOverageTolerancePercent = 200;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product1, 5m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var lines = new[]
			{
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 6m, "UNT", "B11", "", "AT311", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 4m, "UNT", "B11", "", "AT312", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 7m, "UNT", "B21", "", "AT321", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 4m, "UNT", "B22", "", "AT322", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product3.OP_PartNum, 10m, "UNT", "B31", "", "AT331", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product3.OP_PartNum, 11m, "UNT", "B32", "", "AT332", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product3.OP_PartNum, 21m, "UNT", "B32", "", "AT333", "", ZDate.Empty, ZDate.Empty, "", "", ""),
			};
			var response = webService.UnloadWhsReceiveLines(lines, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Error - ReceivedQuantity: Received quantity of product 'P2' exceeds the allowed quantity.", response.ErrorMessage);

			var productSummaryCollection = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToArray();
			var product1Summary = productSummaryCollection.Where(ps => ps.ProductPk == product1.PK).FirstOrDefault();
			AssertEquals(5m, product1Summary.ExpectedQuantity);
			AssertEquals(10m, product1Summary.ReceivedQuantity);

			var hasProduct2Summary = productSummaryCollection.Any(ps => ps.ProductPk == product2.PK);
			AssertEquals(false, hasProduct2Summary);

			var product3Summary = productSummaryCollection.Where(ps => ps.ProductPk == product3.PK).FirstOrDefault();
			AssertEquals(0m, product3Summary.ExpectedQuantity);
			AssertEquals(42m, product3Summary.ReceivedQuantity);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_OneReceiveLineWithIncorrectPackUnit_ReceivedQuantityIsWithinProductAllowedQuantity

		public void TestUnloadWhsReceiveLine_OneReceiveLineWithIncorrectPackUnit_ReceivedQuantityIsWithinProductAllowedQuantity()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 0;
			var partRelation2 = Helper.CreateProductClientRelationShip(client, product2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_PreventReceivingOvers = true;
			partRelation2.OU_ReceiveOverageTolerancePercent = 100;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product1, 5m);
			Helper.CreateWhsReceiveLine(receive, product2, 5m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var lines = new[]
			{
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 2m, "UNT", "B11", "", "AT311", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 3m, "UNT", "B11", "", "AT312", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 2m, "WRG", "B21", "", "AT321", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 10m, "UNT", "B22", "", "AT322", "", ZDate.Empty, ZDate.Empty, "", "", ""),
			};
			var response = webService.UnloadWhsReceiveLines(lines, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Cannot convert 'WRG' to 'UNT' for the product 'P2'. Please add this conversion and try again.", response.ErrorMessage);

			var productSummaryCollection = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToArray();
			var product1Summary = productSummaryCollection.Where(ps => ps.ProductPk == product1.PK).FirstOrDefault();
			AssertEquals(5m, product1Summary.ExpectedQuantity);
			AssertEquals(5m, product1Summary.ReceivedQuantity);

			var product2Summary = productSummaryCollection.Where(ps => ps.ProductPk == product2.PK).FirstOrDefault();
			AssertEquals(5m, product2Summary.ExpectedQuantity);
			AssertEquals(10m, product2Summary.ReceivedQuantity);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_MultipleReceiveLinesWithNonStockKeepingUnit_PartOfLinesExceedProductAllowedQuantity

		public void TestUnloadWhsReceiveLine_MultipleReceiveLinesWithNonStockKeepingUnit_PartOfLinesExceedProductAllowedQuantity()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			product1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(product1, product1.OP_StockKeepingUnit, "BOX", 5m);
			Helper.CreateProductUnit(product1, "BOX", "CTN", 6m);
			Helper.CreateProductUnit(product1, "CTN", "PLT", 10m);
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 0;

			var product2 = Helper.CreateProduct(client, "P2");
			product2.PartUnits.DeleteAll();
			Helper.CreateProductUnit(product2, product2.OP_StockKeepingUnit, "CTN", 3m);
			Helper.CreateProductUnit(product2, "CTN", "PLT", 3m);
			var partRelation2 = Helper.CreateProductClientRelationShip(client, product2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_PreventReceivingOvers = true;
			partRelation2.OU_ReceiveOverageTolerancePercent = 100;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product1, 900m);
			Helper.CreateWhsReceiveLine(receive, product2, 20m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var lines = new[]
			{
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 2m, "PLT", "B11", "", "AT311", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 9m, "CTN", "B11", "", "AT312", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 5m, "BOX", "B11", "", "AT313", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 5m, "UNT", "B11", "", "AT314", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 2m, "PLT", "B21", "", "AT321", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 3m, "PLT", "B22", "", "AT322", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 8m, "CTN", "B22", "", "AT323", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 23m, "UNT", "B22", "", "AT324", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 7m, "CTN", "B22", "", "AT325", "", ZDate.Empty, ZDate.Empty, "", "", ""),
			};
			var response = webService.UnloadWhsReceiveLines(lines, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Error - ReceivedQuantity: Received quantity of product 'P2' exceeds the allowed quantity.", response.ErrorMessage);

			var productSummaryCollection = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToArray();
			var product1Summary = productSummaryCollection.Where(ps => ps.ProductPk == product1.PK).FirstOrDefault();
			AssertEquals(900m, product1Summary.ExpectedQuantity);
			AssertEquals(900m, product1Summary.ReceivedQuantity);

			var product2Summary = productSummaryCollection.Where(ps => ps.ProductPk == product2.PK).FirstOrDefault();
			AssertEquals(20m, product2Summary.ExpectedQuantity);
			AssertEquals(39m, product2Summary.ReceivedQuantity);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_BlindReceiveMultipleLinesWithPreventReceivingOversEnabled

		public void TestUnloadWhsReceiveLine_BlindReceiveMultipleLinesWithPreventReceivingOversEnabled()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 0;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var lines = new[]
			{
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 2000m, "UNT", "B11", "", "AT311", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 5000m, "UNT", "B11", "", "AT312", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 1000m, "UNT", "B21", "", "AT321", "", ZDate.Empty, ZDate.Empty, "", "", ""),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 3000m, "UNT", "B21", "", "AT322", "", ZDate.Empty, ZDate.Empty, "", "", ""),
			};
			var response = webService.UnloadWhsReceiveLines(lines, Array.Empty<Guid>(), false);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

			var tempLines = receive.Lines.Cast<WhsReceiveLine>().ToArray();
			var line1 = tempLines[0];
			var line3 = tempLines[2];
			var productSummaryCollection = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToArray();
			var product1Summary = productSummaryCollection.Where(ps => ps.ProductPk == product1.PK).FirstOrDefault();
			AssertEquals(0m, product1Summary.ExpectedQuantity);
			AssertEquals(7000m, product1Summary.ReceivedQuantity);

			var product2Summary = productSummaryCollection.Where(ps => ps.ProductPk == product2.PK).FirstOrDefault();
			AssertEquals(0m, product2Summary.ExpectedQuantity);
			AssertEquals(4000m, product2Summary.ReceivedQuantity);
		}

		#endregion

		#region TestUnloadWhsReceiveLine_SaveFails

		public void TestUnloadWhsReceiveLine_SaveFails()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);

			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			data.Part2.OP_StockKeepingUnit = "KG";
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.CreateProductUnit(data.Part2, "KG", "UNT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			Helper.Factory.Save();

			var mockFactoryProvider = new Mock<IFactoryService>();
			using (ObjectFactory.Substitute(mockFactoryProvider.Object))
			{
				var factoryForMock = new BusinessObjectFactory();
				factoryForMock.Saving += f => throw new ZCannotSaveException("Test", "Test");

				mockFactoryProvider
					.Setup(fp => fp.GetFactory<Func<BusinessObjectFactory>>())
					.Returns(() => factoryForMock);

				var webService = GetNewWebService();
				SetupSecurityHeader(webService, data.Whs1, staff);

				var response = webService.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(), "", "", "A-1-1") }, Array.Empty<Guid>(), false);
				AssertEquals("ZCannotSaveException should be logged as an Error.", "Test", response.ErrorMessage);
				AssertEquals("ZCannotSaveException should be logged as an Error.", ErrorTypes.BusinessValidationError, response.Error);
			}
		}

		public void TestUnloadWhsReceiveLine_ZSaveConcurrencyExceptionThrown()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);

			data.Part1.OP_StockKeepingUnit = "UNT";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			data.Part2.OP_StockKeepingUnit = "KG";
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.CreateProductUnit(data.Part2, "KG", "UNT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345", Notify);
			Helper.Factory.Save();

			var mockFactoryProvider = new Mock<IFactoryService>();
			using (ObjectFactory.Substitute(mockFactoryProvider.Object))
			{
				var factoryForMock = new BusinessObjectFactory();
				var innerException = new Exception();
				var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)receive).Row, TestConnection);
				factoryForMock.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

				mockFactoryProvider
					.Setup(fp => fp.GetFactory<Func<BusinessObjectFactory>>())
					.Returns(() => factoryForMock);

				var webService = GetNewWebService();
				SetupSecurityHeader(webService, data.Whs1, staff);
				var response = webService.UnloadWhsReceiveLines(new[] { CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P2", 10, "UNT", "MAN", "", "TEST", "", new ZDate(), new ZDate(), "", "", "A-1-1") }, Array.Empty<Guid>(), false);
				AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has made changes to the receive while you have been working on it. Please restart the operation and try again.", response.ErrorMessage);
				AssertEquals("ZSaveConcurrencyException should be logged as an Error.", ErrorTypes.BusinessValidationError, response.Error);
			}
		}

		#endregion

		#region AssertUnloadWhsReceiveLine

		void AssertUnloadWhsReceiveLine(Guid inventoryLinePK, ZString expectedProduct, ZDecimal expectedPacks, ZString expectedPacksUQ,
			ZString expectedAttribute1, ZString expectedAttribute2, ZString expectedAttribute3, ZString expectedSerialNumber,
			ZDate expectedExpiryDate, ZDate expectedPackingDate, ZString expectedHeldCode,
			ZString expectedPalletID, ZString expectedLocation, ZGuid expectedReceivePK, ZInt expectedInventoryCount,
			ZDecimal expectedTxnQty, ZDecimal expectedUnits, ZString expectedUnitsUQ, WhsInventoryView expectedLine, WhsSecureService service)
		{
			var line = service.Factory.Load<WhsInventoryView>(new ZGuid(inventoryLinePK));
			AssertNotNull(line);
			AssertEquals(expectedReceivePK, line.InDocketLine.Docket.PK);

			// we can't check inventory count by accessing receive.Inventory collection as it may not be updated during web service call (for perfomance reasons)
			AssertEquals("Inventory count", expectedInventoryCount, Helper.Factory.GetDatabaseCount(typeof(WhsInventoryView), new ZQuery(WhsInventoryViewSchema.WI_WD, expectedReceivePK)));
			AssertNotNull("SupplierPart", line.SupplierPart);
			AssertEquals("Product", expectedProduct, line.SupplierPart.OP_PartNum);
			AssertEquals("WE_PackQuantity", expectedPacks, line.InDocketLine.WE_PackQuantity);
			AssertEquals("WI_F3_NKPackType", expectedPacksUQ, line.WI_F3_NKPackType);
			AssertEquals("WI_InDocketLineUnits", expectedTxnQty, line.WI_InDocketLineUnits);
			AssertEquals("WI_ExpectedReceiptQuantity", expectedUnits, line.WI_ExpectedReceiptQuantity);
			AssertEquals("WI_UnitsUQ", expectedUnitsUQ, line.WI_UnitsUQ);
			AssertEquals("WI_PartAttrib1", expectedAttribute1, line.WI_PartAttrib1);
			AssertEquals("WI_PartAttrib2", expectedAttribute2, line.WI_PartAttrib2);
			AssertEquals("WI_PartAttrib3", expectedAttribute3, line.WI_PartAttrib3);
			AssertEquals("WI_SerialNumber", expectedSerialNumber, line.WI_SerialNumber);
			AssertEquals("WI_ExpiryDate", expectedExpiryDate, line.WI_ExpiryDate);
			AssertEquals("WI_PackingDate", expectedPackingDate, line.WI_PackingDate);
			AssertEquals("WE_UnloadedTime", ZDateTimeOffset.Now, line.InDocketLine.WE_UnloadedTime);
			AssertEquals("OriginalInventoryHeldCode", expectedHeldCode, line.OriginalInventoryHeldCode);
			AssertEquals("WI_PalletID", expectedPalletID, line.WI_PalletID);
			AssertEquals("Location", expectedLocation, line.LocationString);
			AssertEquals("Logs", 1, Helper.FindLogs(line.InDocketLine.Logs, Events.AddedARecordToTheSystem, "RF").Length);
			if (expectedLine != null)
			{
				AssertEquals(expectedLine, line);
			}
		}

		#endregion

		public void TestUnloadWhsReceiveLine_SaveFail_DBHits()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 0;
			var partRelation2 = Helper.CreateProductClientRelationShip(client, product2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_PreventReceivingOvers = true;
			partRelation2.OU_ReceiveOverageTolerancePercent = 100;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product1, 5m);
			Helper.CreateWhsReceiveLine(receive, product2, 5m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var lines = new LinkedList<WhsLightDocketLineInfo>();
			for (int i = 0; i < 4; i++)
			{
				lines.AddFirst(CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 1m, "UNT", "B1" + i, "", "AT31" + i, "", ZDate.Empty, ZDate.Empty, "", "", ""));
				lines.AddFirst(CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 1m, "UNT", "B2" + i, "", "AT32" + i, "", ZDate.Empty, ZDate.Empty, "", "", ""));
			}
			lines.AddFirst(CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 1m, "UNT", "B15", "", "AT315", "", ZDate.Empty, ZDate.Empty, "", "", ""));
			lines.AddFirst(CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 1m, "WRG", "B25", "", "AT325", "", ZDate.Empty, ZDate.Empty, "", "", ""));

			var expectedDBHits = new Dictionary<string, int>()
			{
				// 1 Hit per WhsReceiveLine created successfully
				// on or after validating the docket line infos
				// or when saving
				{ JobDocAddressSchema.Constants.TableName, 9 },
				{ OrgAddressSchema.Constants.TableName, 9 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 9 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 9 },
				{ StmALogSchema.Constants.TableName, 9 },
				{ StmEventSchema.Constants.TableName, 9 },
				{ OrgMiscServSchema.Constants.TableName, 9 },
				{ OrgPartUnitSchema.Constants.TableName, 9 },
				// Number of WhsReceive saves (9) + 1
				// - CustomLabelProvider.GetLabelFallbackToCompanyOrgProxy x1
				// - OrgHeader.get_CustomLabels() x9
				{ OrgCustomLabelsSchema.Constants.TableName, 10 },
				// 1 Hit when doing batching
				// 1 hit per docket line info when a WhsReceiveLine is being created successfully - (9)
				{ OrgPartRelationSchema.Constants.TableName, 10 },
				// 1 Hit when doing batching
				// 1 Hit when runnig PreSaveValidation
				// 2 Hits per docket line info when a WhsReceiveLine is being created successfully - (18)
				// - when setting WI_OH_Client
				// - when setting WE_OP
				{ OrgHeaderSchema.Constants.TableName, 20 },
				// 1 Hit when doing batching
				// 2 Hits per docket line info when a WhsReceiveLine is being created successfully - (18)
				// - when setting WE_PackQuantity
				// - when setting WE_OP
				{ OrgSupplierPartSchema.Constants.TableName, 19 },
				// 2 Hits when creating a WhsReceiveLine (LoadAllMilestonesAndTriggersForEvent & UpdateTriggerEventDates)
				// 2 Hits per WhsReceiveLine when saving (CreateTasksAndMilestonesFromTemplateIfRequired) - (18)
				{ ProcessTasksSchema.Constants.TableName, 20 },
				// 1 Hit when doing batching (load)
				// 1 Hit when processing docket line info line by line (load) - (10)
				// 1 Hit when saving
				{ WhsDocketSchema.Constants.TableName, 12 },
				// 1 Hit when doing batching (ReceiveProductSummaryCollection)
				// 3 Hits per docket line info when a WhsReceiveLine is being created successfully
				// - Load - (9)
				// - 2 Hits when calling set_WE_PackQuantity (TableHitCounter management) - (18)
				{ WhsDocketLineSchema.Constants.TableName, 28 },
				// 1 Hit during batching
				// 1 hit per docket line info when a WhsReceiveLine is being created successfully - (9)
				{ WhsAsnLineSchema.Constants.TableName, 10 },
			};

			WhsInventoryWebServiceResponse response;
			using (TestCaseWithFactory.AssertDbHitsForAllFactories(expectedDBHits, ignoreUnspecified: true, useOnlyNewFactories: true, tablesToCollectQueriesFor: expectedDBHits.Keys.ToArray()))
			{
				response = webService.UnloadWhsReceiveLines(lines.ToArray(), Array.Empty<Guid>(), false);
			}

			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Cannot convert 'WRG' to 'UNT' for the product 'P2'. Please add this conversion and try again.", response.ErrorMessage);

			var productSummaryCollection = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToArray();
			var product1Summary = productSummaryCollection.Where(ps => ps.ProductPk == product1.PK).FirstOrDefault();
			AssertEquals(5m, product1Summary.ExpectedQuantity);
			AssertEquals(5m, product1Summary.ReceivedQuantity);

			var product2Summary = productSummaryCollection.Where(ps => ps.ProductPk == product2.PK).FirstOrDefault();
			AssertEquals(5m, product2Summary.ExpectedQuantity);
			AssertEquals(4m, product2Summary.ReceivedQuantity);
		}

		public void TestUnloadWhsReceiveLine_Save_DBHits()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 0;
			var partRelation2 = Helper.CreateProductClientRelationShip(client, product2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_PreventReceivingOvers = true;
			partRelation2.OU_ReceiveOverageTolerancePercent = 100;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product1, 5m);
			Helper.CreateWhsReceiveLine(receive, product2, 5m);
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(warehouse);
			var lines = new LinkedList<WhsLightDocketLineInfo>();
			for (var i = 0; i < 5; i++)
			{
				lines.AddFirst(CreateWhsDocketLineInfo(receive.PK.ToGuid(), product1.OP_PartNum, 1m, "UNT", "B1" + i, "", "AT31" + i, "", ZDate.Empty, ZDate.Empty, "", "", ""));
				lines.AddFirst(CreateWhsDocketLineInfo(receive.PK.ToGuid(), product2.OP_PartNum, 1m, "UNT", "B2" + i, "", "AT32" + i, "", ZDate.Empty, ZDate.Empty, "", "", ""));
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				// 1 DB Hit / receive
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				// Number of WhsReceive saves (1) + 1
				// - CustomLabelProvider.GetLabelFallbackToCompanyOrgProxy x1
				// - OrgHeader.get_CustomLabels() x1
				{ OrgCustomLabelsSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 2 }, 
				// 1 Hit for Load + 1 Hit for Save
				{ WhsDocketSchema.Constants.TableName, 2 }, 
				// 1 Hit / Receive line (FindExistingEligibleReceiveLines) = 10
				// 1 Hit Load Product summaries = 1
				{ WhsDocketLineSchema.Constants.TableName, 11 }, 
			};

			WhsInventoryWebServiceResponse response;
			using (TestCaseWithFactory.AssertDbHitsForAllFactories(expectedDBHits, ignoreUnspecified: true, useOnlyNewFactories: true, tablesToCollectQueriesFor: expectedDBHits.Keys.ToArray()))
			{
				response = webService.UnloadWhsReceiveLines(lines.ToArray(), Array.Empty<Guid>(), false);
			}

			AssertNullOrEmpty(response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);

			var productSummaryCollection = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToArray();
			var product1Summary = productSummaryCollection.Where(ps => ps.ProductPk == product1.PK).FirstOrDefault();
			AssertEquals(5m, product1Summary.ExpectedQuantity);
			AssertEquals(5m, product1Summary.ReceivedQuantity);

			var product2Summary = productSummaryCollection.Where(ps => ps.ProductPk == product2.PK).FirstOrDefault();
			AssertEquals(5m, product2Summary.ExpectedQuantity);
			AssertEquals(5m, product2Summary.ReceivedQuantity);
		}

		#endregion
	}
}
