using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsCycleCountLocationHelper
	{
		#region InventoriesSQL

		const string InventoriesMatchingNegativeVariancesWithSerialNumber = @"
SELECT 
	WI_PK 
FROM 
	dbo.WhsCycleCountLocation
	JOIN dbo.WhsCycleCountLocationVariance ON WCC_WCL_CycleCountLocation = WCL_PK
	JOIN dbo.WhsInventoryView ON WI_OP = WCC_OP_Product AND WI_OH_Client = WCC_OH_Client
WHERE
	WCL_PK = @CycleCountPK AND
	WCL_WL_Location = WI_WL AND
	WCC_VarianceQty < 0 AND
	WCC_PalletID = WI_PalletID AND
	WCC_PartAttrib1 = WI_PartAttrib1 AND
	WCC_PartAttrib2 = WI_PartAttrib2 AND
	WCC_PartAttrib3 = WI_PartAttrib3 AND
	WCC_SerialNumber = WI_SerialNumber AND
	(ISNULL(WCC_ExpiryDate, WI_ExpiryDate) IS NULL OR WCC_ExpiryDate = WI_ExpiryDate) AND
	(ISNULL(WCC_PackingDate, WI_PackingDate) IS NULL OR WCC_PackingDate = WI_PackingDate) AND
	WI_TotalUnits > 0
";

		public const string UnexpectedInventoryWithPalletIDSQL = @"
SELECT 
	WI_PK 
FROM 
	dbo.WhsCycleCountLocation 
	JOIN dbo.WhsCycleCountLocationVariance ON WCC_WCL_CycleCountLocation = WCL_PK
	JOIN dbo.WhsInventoryView ON WI_PalletID = WCC_PalletID
	JOIN dbo.WhsLocationView ON WI_WL = WLV_PK
WHERE
	WCL_PK = @CycleCountPK AND
	WCC_VarianceQty > 0 AND
	WCC_PalletID <> '' AND
	WI_WL <> WCL_WL_Location AND
	WI_TotalUnits > 0 AND
	WLV_WW_WHS = @WhsPK";

		public const string UnexpectedInventoryWithSerialNumberSQL = @"
SELECT 
	WI_PK 
FROM 
	dbo.WhsCycleCountLocation
	JOIN dbo.WhsCycleCountLocationVariance ON WCC_WCL_CycleCountLocation = WCL_PK
	JOIN dbo.WhsInventoryView ON WI_OP = WCC_OP_Product AND WI_OH_Client = WCC_OH_Client
	JOIN dbo.OrgPartRelation ON OU_OP = WI_OP AND OU_OH = WI_OH_Client AND OU_Relationship IN ('OWN', 'BTH')
	JOIN dbo.OrgMiscServ ON OM_OH = WI_OH_Client
WHERE
	WCL_PK = @CycleCountPK AND
	WI_WL <> WCL_WL_Location AND
	WCC_VarianceQty > 0 AND
	WI_TotalUnits > 0 AND
	OM_IMUseSerialNumber = 1 AND OU_UseSerialNumber = 1 AND OU_IsSerialNumberReleaseCaptured = 0 AND WCC_SerialNumber = WI_SerialNumber";

		#endregion

		#region GetInventories

		public static WhsInventoryView[] GetUnexpectedInventoriesWithPalletID(BusinessObjectFactory factory, WhsCycleCountLocation cycleCountLocation)
		{
			var parameter = new ZSqlParameterCollection();
			parameter.Add("@CycleCountPK", cycleCountLocation.PK, WhsCycleCountLocationSchema.PK);
			parameter.Add("@WhsPK", cycleCountLocation.Location.WLV_WW_Whs, WhsLocationViewSchema.WLV_WW_Whs);

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsInventoryViewSchema.PK.Name, UnexpectedInventoryWithPalletIDSQL), parameter);

			return factory.Load<WhsInventoryView>(inventoryQuery);
		}

		public static WhsInventoryView[] GetInventoriesMatchingNegativeVariances(BusinessObjectFactory factory, WhsCycleCountLocation cycleCountLocation)
		{
			var parameter = new ZSqlParameterCollection();
			parameter.Add("@CycleCountPK", cycleCountLocation.PK, WhsCycleCountLocationSchema.PK);

			var inventoriesMatchingNegativeVariancesQuery = InventoriesMatchingNegativeVariancesWithSerialNumber;

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsInventoryViewSchema.PK.Name, inventoriesMatchingNegativeVariancesQuery), parameter);

			return factory.Load<WhsInventoryView>(inventoryQuery);
		}

		public static WhsInventoryView[] GetUnexpectedInventoriesWithSerialNumber(BusinessObjectFactory factory, WhsCycleCountLocation cycleCountLocation)
		{
			var parameter = new ZSqlParameterCollection();
			parameter.Add("@CycleCountPK", cycleCountLocation.PK, WhsCycleCountLocationSchema.PK);

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsInventoryViewSchema.PK.Name, UnexpectedInventoryWithSerialNumberSQL), parameter);

			return factory.Load<WhsInventoryView>(inventoryQuery);
		}

		#endregion

		public static WhsCycleCountLocationVariance GetOpenNegativeVariancesMatchingInventory(BusinessObjectFactory factory, WhsInventoryView inventory, decimal varianceQty)
		{
			var varianceQuery = new ZDBOnlyQuery(typeof(WhsCycleCountLocationVariance));
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_OP_Product, inventory.WI_OP);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_PalletID, inventory.WI_PalletID);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_PartAttrib1, inventory.WI_PartAttrib1);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_PartAttrib2, inventory.WI_PartAttrib2);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_PartAttrib3, inventory.WI_PartAttrib3);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_SerialNumber, inventory.WI_SerialNumber);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_Status, CycleCountVarianceStatus.Codes.Open);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_VarianceQty, SQLComparisonOperator.Equal, varianceQty);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_ExpiryDate, inventory.WI_ExpiryDate);
			varianceQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_PackingDate, inventory.WI_PackingDate);

			var cycleCountLocationSubQuery = new ZDBOnlySubQuery(typeof(WhsCycleCountLocation), WhsCycleCountLocationVarianceSchema.WCC_WCL_CycleCountLocation);
			cycleCountLocationSubQuery.AddToFilter(WhsCycleCountLocationSchema.WCL_WL_Location, inventory.WI_WL);
			varianceQuery.AddSubQuery(cycleCountLocationSubQuery, JoinCondition.And);

			return factory.LoadTop1<WhsCycleCountLocationVariance>(varianceQuery);
		}

		#region HoldInventory

		/// <summary>
		/// Attempts to hold quantity from qtyToHold
		/// </summary>
		/// <param name="inventory"></param>
		/// <param name="heldCode"></param>
		/// <param name="qtyToHold"></param>
		/// <returns>Amount actually held</returns>
		public static ZDecimal HoldInventory(WhsInventoryView inventory, ZString heldCode, ZDecimal qtyToHold)
		{
			var availableQty = inventory.WI_AvailableToTransferQuantity;
			var availableToHold = (qtyToHold < availableQty) ? qtyToHold : availableQty;

			var docketLine = inventory.InDocketLine;
			docketLine.HeldCodeChangeQuantity = availableToHold;
			docketLine.HeldCodeToChangeTo = heldCode;

			var result = docketLine.ChangeInventoryHeldCode(true);
			return result ? availableToHold : ZDecimal.Zero;
		}

		#endregion
	}
}
