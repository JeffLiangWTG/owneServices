using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetASNLinesFromPalletIDTest : WhsSecureServiceTestCase
	{
		#region GetASNLinesFromPalletID

		#region TestGetASNLinesFromPalletID_PalletAsnLineMissingMandatoryAttributes

		public void TestGetASNLinesFromPalletID_PalletAsnLineMissingMandatoryAttributes()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1");
			webService.Factory.Save();

			PopluateASNLines(receive);

			var response = webService.GetASNLinesFromPalletID(receive.PK.ToGuid(), "PLT1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.PalletAsnLineMissingMandatoryAttributes, response.Error);
			AssertNotNull(response.GroupedLines);
			AssertEquals(0, response.GroupedLines.Count);
		}

		#endregion

		#region TestGetUnloadPalletInfo_InvalidReceivePK

		public void TestGetUnloadPalletInfo_InvalidReceivePK()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Receive record could not be found.", webService.GetASNLinesFromPalletID(Guid.Empty, "PLT1"));
		}

		#endregion

		#region TestGetUnloadPalletInfo_FinalisedReceive

		public void TestGetUnloadPalletInfo_FinalisedReceive()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			AssertBusinessValidationError(webService, "Receive record has been finalized.", webService.GetASNLinesFromPalletID(receive.PK.ToGuid(), "PLT1"));
		}

		#endregion

		#region TestGetUnloadPalletInfo_BusinessValidationError

		public void TestGetUnloadPalletInfo_BusinessValidationError()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			webService.Factory.Save();

			var response = webService.GetASNLinesFromPalletID(receive.PK.ToGuid(), "PLT1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertNotNull(response.GroupedLines);
			AssertEquals(0, response.GroupedLines.Count);
		}

		#endregion

		#region TestGetUnloadPalletInfo_PalletWithUnloadedLines

		public void TestGetUnloadPalletInfo_PalletWithUnloadedLines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var year = ZDateTime.Now.Year - 1;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			var receiveLine1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			var receiveLine2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			webService.Factory.Save();

			// create ASN lines for receive.
			receive.PopulateASNLines();
			webService.Factory.Save();

			AssertEquals("Precondition:", 2, receive.AsnLines.Count);
			AssertEquals("Precondition:", 2, receive.Inventory.Count);

			var response = webService.GetASNLinesFromPalletID(receive.PK.ToGuid(), "PLT1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.YesNoEnquiry, response.Error);
			AssertEquals("Pallet Id 'PLT1' was already unloaded. Do you want to check it?", response.ErrorMessage);
			AssertNotNull(response.GroupedLines);
			AssertEquals(1, response.GroupedLines.Count);
		}

		public void TestGetUnloadPalletInfo_PalletWithUnloadedLines_ResultUsesWarehouseCountryFormatString()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var year = ZDateTime.Now.Year - 1;
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			var receiveLine1 = helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			var receiveLine2 = helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev2", Notify);
			var receiveLine3 = helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, data.Whs1.DefaultLocation, "PLT2", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			var receiveLine4 = helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, data.Whs1.DefaultLocation, "PLT2", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");

			webService.Factory.Save();

			// create ASN lines for receive.
			receive1.PopulateASNLines();
			receive2.PopulateASNLines();
			webService.Factory.Save();

			var response = webService.GetASNLinesFromPalletID(receive1.PK.ToGuid(), "PLT1");

			AssertEquals("ddMMyy", response.GroupedLines[0].PartAttributes.ExpiryDateFormatString);
			AssertEquals("ddMMyy", response.GroupedLines[0].PartAttributes.PackingDateFormatString);

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";
			Helper.Factory.Save();

			response = webService.GetASNLinesFromPalletID(receive2.PK.ToGuid(), "PLT2");

			AssertEquals("yyMMdd", response.GroupedLines[0].PartAttributes.ExpiryDateFormatString);
			AssertEquals("yyMMdd", response.GroupedLines[0].PartAttributes.PackingDateFormatString);
		}

		#endregion

		#region TestGetUnloadPalletInfo_PalletWithoutUnloadedLines

		public void TestGetUnloadPalletInfo_PalletWithoutUnloadedLines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var year = ZDate.Today.Year - 1;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			var receiveLine1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			var receiveLine2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			webService.Factory.Save();

			PopluateASNLines(receive);

			var response = webService.GetASNLinesFromPalletID(receive.PK.ToGuid(), "PLT1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotNull(response.GroupedLines);
			AssertEquals(1, response.GroupedLines.Count);
		}

		public void TestGetUnloadPalletInfo_PalletWithoutUnloadedLines_ResultUsesWarehouseCountryFormatString()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.PackingDate, true);

			var year = ZDate.Today.Year - 1;
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			var receiveLine1 = helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			var receiveLine2 = helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev2", Notify);
			var receiveLine3 = helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, data.Whs1.DefaultLocation, "PLT2", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			var receiveLine4 = helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, data.Whs1.DefaultLocation, "PLT2", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");

			webService.Factory.Save();

			PopluateASNLines(receive1);
			PopluateASNLines(receive2);

			var response = webService.GetASNLinesFromPalletID(receive1.PK.ToGuid(), "PLT1");

			AssertEquals("ddMMyy", response.GroupedLines[0].PartAttributes.ExpiryDateFormatString);
			AssertEquals("ddMMyy", response.GroupedLines[0].PartAttributes.PackingDateFormatString);

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";
			Helper.Factory.Save();

			response = webService.GetASNLinesFromPalletID(receive2.PK.ToGuid(), "PLT2");

			AssertEquals("yyMMdd", response.GroupedLines[0].PartAttributes.ExpiryDateFormatString);
			AssertEquals("yyMMdd", response.GroupedLines[0].PartAttributes.PackingDateFormatString);
		}

		#endregion

		#endregion
	}
}
