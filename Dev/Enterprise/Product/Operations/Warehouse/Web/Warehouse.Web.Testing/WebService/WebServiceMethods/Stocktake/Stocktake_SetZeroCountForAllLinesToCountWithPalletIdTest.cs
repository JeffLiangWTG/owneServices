using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Stocktake_SetZeroCountForAllLinesToCountWithPalletIdTest : WhsStocktakeSecureServiceTestCase
	{
		#region TestStocktake_SetZeroCountForAllLinesToCountWithPalletId

		[TestDate(2012, 2, 10)]
		public void TestStocktake_SetZeroCountForAllLinesToCountWithPalletId()
		{
			SetupEnvironmentDataForStocktake();
			LoadStocktakes();

			var sampleLine = TestData.Stocktake3.Lines.Find(l => !string.IsNullOrEmpty(l.WU_PalletID)).First();
			var palletId = sampleLine.WU_PalletID;
			var targetCount = TestData.Stocktake3.Lines.Find(l => l.WU_PalletID == palletId).Count();
			var linesToCountCount = TestData.Stocktake3.Lines.Find(l => l.WU_PalletID != palletId).Count();

			var webService = GetNewWebService(TestData.Warehouse2, TestData.Staff1);
			var response = webService.Stocktake_SetZeroCountForAllLinesToCountWithPalletId(
				TestData.Stocktake3.WS_StocktakeNumber,
				"ANY",
				"ANY",
				sampleLine.LocationString,
				palletId);

			AssertSuccessfulResponse(response, webService);
			foreach (var lineInfo in TestData.Stocktake3.Lines)
			{
				if (lineInfo.WU_PalletID == palletId)
				{
					AssertStocktakeLineDidCount(string.Format("line {0} should be set to 0.", lineInfo.WU_PalletID), 0, TestData.Staff1, TestDateAttribute.Date, lineInfo.PK.ToGuid());
					targetCount--;
				}
				else
				{
					var line = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsStocktakeLine>(new ZGuid(lineInfo.PK));
					AssertNotNull(line);
					AssertEquals("DateVerified should be empty", true, line.WU_DateVerified.IsEmpty);
					linesToCountCount--;
				}
			}

			AssertEquals("All target stocktake lines are verified (set to 0).", 0, targetCount);
			AssertEquals("All stocktake lines in response are verified (sent back to request sender to count)", 0, linesToCountCount);
		}

		#endregion

		#region TestStocktake_SetZeroCountForAllLinesToCountWithPalletId_Error

		public void TestStocktake_SetZeroCountForAllLinesToCountWithPalletId_Error()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var whs = Helper.CreateWarehouse("W1");
			Helper.Factory.Save();

			var webService = GetNewWebService(whs, staff);
			var response = webService.Stocktake_SetZeroCountForAllLinesToCountWithPalletId(
				"",
				"ANY",
				"ANY",
				"",
				"");
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Id should not be null or empty.", response.ErrorMessage);
		}

		#endregion
	}
}
