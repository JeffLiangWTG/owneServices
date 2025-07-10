using CargoWise.Types;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsStocktakeLineInfo))]
	public class WhsStocktakeLineInfoTestCase : DataObjectInfoTestCase<WhsStocktakeLineInfo>
	{
		#region Constructors

		#region TestAdditionalConstructors

		public void TestAdditionalConstructors()
		{
			var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part1);

			stocktakeLine.LocationString = Data.Whs1.DefaultLocation.ToLocationString();
			stocktakeLine.WU_PalletID = "PLT1";
			stocktakeLine.WU_PartAttrib1 = "Att1";
			stocktakeLine.WU_PartAttrib2 = "Att2";
			stocktakeLine.WU_PartAttrib3 = "Att3";
			stocktakeLine.WU_SerialNumber = "SN1";
			stocktakeLine.WU_PackingDate = ZDate.Today;
			stocktakeLine.WU_ExpiryDate = ZDate.Today;
			stocktakeLine.WU_InventoryStatus = "AVL";
			stocktakeLine.CurrentCount = 1;
			stocktakeLine.CurrentCountVerifiedDate = ZDateTime.Today;
			stocktakeLine.WU_SystemUnits = 2;

			var stocktakeLineInfo = new WhsStocktakeLineInfo(new WhsStocktakeInfo(stocktake), stocktakeLine);

			AssertEquals("PK", stocktakeLine.PK.ToGuid(), stocktakeLineInfo.PK);
			AssertEquals("LocationString", Data.Whs1.DefaultLocation.ToLocationString(), stocktakeLineInfo.LocationString);
			AssertEquals("LocationString_UserFriendly", Data.Whs1.DefaultLocation.ToLocationString(), stocktakeLineInfo.LocationString_UserFriendly);
			AssertEquals("PalletID", stocktakeLine.WU_PalletID, stocktakeLineInfo.PalletID);
			AssertEquals("RfAttributeConfirm", RFAttributeHelper.GetRFAttributeConfirm(stocktakeLine.SupplierPart, stocktakeLine.Client), stocktakeLineInfo.RfAttributeConfirm);
			AssertEquals("PartAttribOne", "Att1", stocktakeLineInfo.PartAttribOne);
			AssertEquals("PartAttribTwo", "Att2", stocktakeLineInfo.PartAttribTwo);
			AssertEquals("PartAttribThree", "Att3", stocktakeLineInfo.PartAttribThree);
			AssertEquals("SerialNumber", "SN1", stocktakeLineInfo.SerialNumber);
			AssertEquals("PackingDate", ZDate.Today, stocktakeLineInfo.PackingDate);
			AssertEquals("ExpiryDate", ZDate.Today, stocktakeLineInfo.ExpiryDate);
			AssertEquals("InventoryStatus", "AVL", stocktakeLineInfo.InventoryStatus);
			AssertEquals("InventoryStatusDesc", "Available", stocktakeLineInfo.InventoryStatusDesc);
			AssertEquals("PackType", string.Empty, stocktakeLineInfo.PackType);
			AssertEquals("CurrentCount", new decimal(1), stocktakeLineInfo.CurrentCount);
			AssertEquals("DateVerified", ZDateTime.Today, stocktakeLineInfo.DateVerified);
			AssertEquals("VerifiedBy", string.Empty, stocktakeLineInfo.VerifiedBy);
			AssertEquals("SystemUnits", new decimal(2), stocktakeLineInfo.SystemUnits);
			AssertEquals("ProductPK", Data.Part1.PK, stocktakeLineInfo.ProductPK);
			AssertEquals("ClientPK", stocktake.Client.PK, stocktakeLineInfo.ClientPK);
			AssertEquals("ClientCode", stocktake.Client.OH_Code, stocktakeLineInfo.ClientCode);
		}

		public void TestAdditionalConstructors_LocationStringUserFriendly()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 3, 3, 2);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(Data.Org1, warehouse);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part1);

			stocktakeLine.LocationString = "Z030302";
			stocktakeLine.WU_PalletID = "PLT1";
			stocktakeLine.WU_PartAttrib1 = "Att1";
			stocktakeLine.WU_PartAttrib2 = "Att2";
			stocktakeLine.WU_PartAttrib3 = "Att3";
			stocktakeLine.WU_SerialNumber = "SN1";
			stocktakeLine.WU_PackingDate = ZDate.Today;
			stocktakeLine.WU_ExpiryDate = ZDate.Today;
			stocktakeLine.WU_InventoryStatus = "AVL";
			stocktakeLine.CurrentCount = 1;
			stocktakeLine.CurrentCountVerifiedDate = ZDateTime.Today;
			stocktakeLine.WU_SystemUnits = 2;

			var stocktakeLineInfo = new WhsStocktakeLineInfo(new WhsStocktakeInfo(stocktake), stocktakeLine);

			AssertEquals("PK", stocktakeLine.PK.ToGuid(), stocktakeLineInfo.PK);
			AssertEquals("LocationString", "Z030302", stocktakeLineInfo.LocationString);
			AssertEquals("LocationString_UserFriendly", "Z-03-03-02", stocktakeLineInfo.LocationString_UserFriendly);
			AssertEquals("PalletID", stocktakeLine.WU_PalletID, stocktakeLineInfo.PalletID);
			AssertEquals("RfAttributeConfirm", RFAttributeHelper.GetRFAttributeConfirm(stocktakeLine.SupplierPart, stocktakeLine.Client), stocktakeLineInfo.RfAttributeConfirm);
			AssertEquals("PartAttribOne", "Att1", stocktakeLineInfo.PartAttribOne);
			AssertEquals("PartAttribTwo", "Att2", stocktakeLineInfo.PartAttribTwo);
			AssertEquals("PartAttribThree", "Att3", stocktakeLineInfo.PartAttribThree);
			AssertEquals("SerialNumber", "SN1", stocktakeLineInfo.SerialNumber);
			AssertEquals("PackingDate", ZDate.Today, stocktakeLineInfo.PackingDate);
			AssertEquals("ExpiryDate", ZDate.Today, stocktakeLineInfo.ExpiryDate);
			AssertEquals("InventoryStatus", "AVL", stocktakeLineInfo.InventoryStatus);
			AssertEquals("InventoryStatusDesc", "Available", stocktakeLineInfo.InventoryStatusDesc);
			AssertEquals("PackType", string.Empty, stocktakeLineInfo.PackType);
			AssertEquals("CurrentCount", new decimal(1), stocktakeLineInfo.CurrentCount);
			AssertEquals("DateVerified", ZDateTime.Today, stocktakeLineInfo.DateVerified);
			AssertEquals("VerifiedBy", string.Empty, stocktakeLineInfo.VerifiedBy);
			AssertEquals("SystemUnits", new decimal(2), stocktakeLineInfo.SystemUnits);
			AssertEquals("ProductPK", Data.Part1.PK, stocktakeLineInfo.ProductPK);
			AssertEquals("ClientPK", stocktake.Client.PK, stocktakeLineInfo.ClientPK);
			AssertEquals("ClientCode", stocktake.Client.OH_Code, stocktakeLineInfo.ClientCode);
		}

		public void TestAdditionalConstructors_UsesWarehouseCountryFormatString()
		{
			Helper.SetClientAllAttributeType(Data.Org1, true);
			Helper.SetProductAllAttributeUse(Data.Org1, Data.Part1, true);
			Helper.SetProductAllAttributeUse(Data.Org1, Data.Part2, true);

			var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1);
			var stocktakeLine1 = Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part1);
			var stocktakeLine2 = Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part2);

			stocktakeLine1.LocationString = Data.Whs1.DefaultLocation.ToLocationString();
			stocktakeLine2.LocationString = Data.Whs1.DefaultLocation.ToLocationString();

			var stocktakeInfo1 = new WhsStocktakeInfo(stocktake);
			new WhsStocktakeLineInfo(stocktakeInfo1, stocktakeLine1);

			AssertEquals("ddMMyy", stocktakeInfo1.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", stocktakeInfo1.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing

			Data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var stocktakeInfo2 = new WhsStocktakeInfo(stocktake);
			new WhsStocktakeLineInfo(stocktakeInfo2, stocktakeLine2);

			AssertEquals("yyMMdd", stocktakeInfo2.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", stocktakeInfo2.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsStocktakeLineInfo Parent => (WhsStocktakeLineInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo() => new WhsStocktakeLineInfo();

		#endregion
	}
}
