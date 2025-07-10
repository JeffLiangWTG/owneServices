using System;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Stocktake_SetStocktakeLineEmptyLocationConfirmTest : WhsStocktakeSecureServiceTestCase
	{
		#region TestStocktake_SetStocktakeLineEmptyLocation

		public void TestStocktake_SetStocktakeLineEmptyLocation()
		{
			var whs = Helper.CreateWarehouse("W1");
			var area = Helper.CreateArea(whs, "A1", Warehouse.Environment.CodeLists.AreaTypes.Codes.FreeStore);
			var row = Helper.CreateRowAndGenerateLocations(whs, "Row11", 3, 1);

			row.Locations[1].WLV_WA_PickingArea = area.PK;
			row.Locations[2].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PickingArea = area.PK;

			Helper.Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(whs, CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			stocktake.Load();
			Helper.Factory.Save();

			var pickArea = "ANY";
			var pickMethod = "ANY";
			var stocktakeNumber = stocktake.WS_StocktakeNumber;

			var webService1 = GetNewWebService(whs, staff);
			var response = webService1.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber, pickArea, pickMethod);
			AssertSuccessfulResponse(response, webService1);
			AssertNotNull("Stocktake", response.Stocktake);
			AssertEquals("LinesToCount Count", 1, response.LinesToCount.Count);

			var linePK = response.LinesToCount[0].PK;
			var webService2 = GetNewWebService(whs, staff);
			var lineInfo = webService2.Stocktake_SetStocktakeLineEmptyLocationConfirm(linePK, true).StocktakeLine;
			AssertEquals(lineInfo.VerifiedBy, "ST1");
			AssertNotNull(lineInfo.VerifiedBy);

			var webService3 = GetNewWebService(whs, staff);
			response = webService3.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber, pickArea, pickMethod);
			AssertEquals("LinesToCount Count", 1, response.LinesToCount.Count);

			linePK = response.LinesToCount[0].PK;
			var webService4 = GetNewWebService(whs, staff);
			lineInfo = webService4.Stocktake_SetStocktakeLineEmptyLocationConfirm(linePK, false).StocktakeLine;
			AssertNull(lineInfo.VerifiedBy);
		}

		#endregion

		#region TestStocktake_SetStocktakeLineEmptyLocation_Error

		public void TestStocktake_SetStocktakeLineEmptyLocation_Error()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var whs = Helper.CreateWarehouse("W1");
			Helper.Factory.Save();

			var webService = GetNewWebService(whs, staff);
			var response = webService.Stocktake_SetStocktakeLineEmptyLocationConfirm(new Guid(), true);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Stocktake Line could not be found.", response.ErrorMessage);
		}

		#endregion
	}
}
