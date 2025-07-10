using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Tools;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Stocktake_SetStocktakeLineCountTest : WhsStocktakeSecureServiceTestCase
	{
		#region TestStocktake_SetStocktakeLineCount

		[TestDate(2012, 2, 10)]
		public void TestStocktake_SetStocktakeLineCount()
		{
			SetupEnvironmentDataForStocktake();
			LoadStocktakes();

			var stocktakeLine = TestData.Stocktake1.Lines[0];
			AssertEquals("The stocktake line should have 0 pack quatity.", 0m, stocktakeLine.WU_PackQty);

			var webService = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			var response = webService.Stocktake_SetStocktakeLineCount(stocktakeLine.PK.ToGuid(), "UNT", 2);

			AssertSuccessfulResponse(response, webService);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			AssertEquals("The stocktake line should have 2 pack quatity now.", 2m, newFactory.Load<WhsStocktakeLine>(stocktakeLine.PK).WU_PackQty);
		}

		public void TestStocktake_SetStocktakeLineCount_InvalidQuantity()
		{
			SetupEnvironmentDataForStocktake();
			LoadStocktakes();

			var stocktakeLine = TestData.Stocktake1.Lines[0];
			AssertEquals("The stocktake line should have 0 pack quatity.", 0m, stocktakeLine.WU_PackQty);

			var webService = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			var response = webService.Stocktake_SetStocktakeLineCount(stocktakeLine.PK.ToGuid(), "UNT", -2);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Negative quantity should not be valid", "The count quantity cannot be negative.", response.ErrorMessage);
		}

		public void TestStocktake_SetStocktakeLineCount_InvalidStaff()
		{
			SetupEnvironmentDataForStocktake();
			LoadStocktakes();

			var stocktakeManager = new StocktakeManager(Helper.Factory, "ANY", "ANY", TestData.Warehouse1, TestData.Staff1);
			stocktakeManager.FindAndAssignNextStocktake(TestData.Stocktake1.WS_StocktakeNumber);
			var stocktakeLine = TestData.Stocktake1.Lines[0];
			AssertEquals("The stocktake line should have 0 pack quatity.", 0m, stocktakeLine.WU_PackQty);

			var webService = GetNewWebService(TestData.Warehouse1, TestData.Staff2);
			var response = webService.Stocktake_SetStocktakeLineCount(stocktakeLine.PK.ToGuid(), "UNT", 2);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Another staff should not be able to set the quantity", "Unable to update stocktake line count quantity. This stocktake line is not assigned or assigned to another operator.", response.ErrorMessage);
		}

		public void TestStocktake_SetStocktakeLineCount_InvalidLinePK()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var whs = Helper.CreateWarehouse("W1");
			Helper.Factory.Save();

			var webService = GetNewWebService(whs, staff);
			var response = webService.Stocktake_SetStocktakeLineCount(new Guid(), "UNT", 2);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("PK should not be valid", "Stocktake Line could not be found.", response.ErrorMessage);
		}

		#endregion
	}
}
