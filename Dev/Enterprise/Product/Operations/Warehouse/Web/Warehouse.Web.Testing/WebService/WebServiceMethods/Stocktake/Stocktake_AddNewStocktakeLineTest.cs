using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Stocktake_AddNewStocktakeLineTest : WhsStocktakeSecureServiceTestCase
	{
		#region TestStocktake_AddNewStocktakeLine

		[TestDate(2012, 2, 10)]
		public void TestStocktake_AddNewStocktakeLine()
		{
			SetupEnvironmentDataForStocktake();
			LoadStocktakes();

			var originalLines = TestData.Stocktake3.Lines.ToList();

			var webService = GetNewWebService(TestData.Warehouse2, TestData.Staff1);
			var response = webService.Stocktake_AddNewStocktakeLine(
				stocktakePk: TestData.Stocktake3.PK.ToGuid(),
				supplierPartPk: TestData.Part1.PK.ToGuid(),
				locationString: "Row2-1",
				clientPK: TestData.Stocktake3.Client.PK.ToGuid(),
				packType: "UNT",
				attr1: string.Empty,
				attr2: string.Empty,
				attr3: string.Empty,
				serial: string.Empty,
				expiryDate: null,
				packingDate: null,
				palletID: "PLT",
				countQuantity: 1m,
				stocktakeInventoryStatus: InventoryStatus.Codes.Available);
			AssertSuccessfulResponse(response, webService);

			var updatedStocktake = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsStocktake>(TestData.Stocktake3.PK);
			AssertEquals("The stocktake should have one more line", originalLines.Count + 1, updatedStocktake.Lines.Count);

			var newLine = TestData.Stocktake3.Lines.First(l => !originalLines.Contains(l));
			AssertNotNull("The new line should be in the stocktake", newLine);
			AssertEquals("Not same PK", response.StocktakeLine.PK, newLine.PK);
			AssertStocktakeLineDidCount("The new line should be counted.", 1m, TestData.Staff1, TestDateAttribute.Date, newLine.PK.ToGuid());
			AssertEquals("Part", TestData.Part1.PK, newLine.SupplierPart.PK);
			AssertEquals("PackType", "UNT", newLine.WU_F3_NKPackType);
			AssertEquals("attr1", string.Empty, newLine.WU_PartAttrib1);
			AssertEquals("attr2", string.Empty, newLine.WU_PartAttrib2);
			AssertEquals("attr3", string.Empty, newLine.WU_PartAttrib3);
			AssertEquals("serial", string.Empty, newLine.WU_SerialNumber);
			AssertEquals("expiryDate", ZDateTime.Empty, newLine.WU_ExpiryDate);
			AssertEquals("packingDate", ZDateTime.Empty, newLine.WU_PackingDate);
			AssertEquals("palletId", "PLT", newLine.WU_PalletID);
			AssertEquals("inventoryStatus", InventoryStatus.Codes.Available, newLine.WU_InventoryStatus);
			AssertEquals("client", TestData.Stocktake3.Client.PK, newLine.WU_OH_Client);
		}

		#endregion
	}
}
