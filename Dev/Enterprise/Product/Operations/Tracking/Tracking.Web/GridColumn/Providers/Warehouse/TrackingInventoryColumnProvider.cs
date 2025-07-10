using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingInventoryColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			// PRODUCT
			var productColumn = new ZHyperLinkColumn(Res.GetString("a5ae3177-39db-44be-afb9-35ab32ab863c", "Product"), TrackingInventorySummary.Schema.ProductCode)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.Product,
				DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductProfileDetailsPage) + (NoResString)"?Ref={0}", // URL parameter
				DataNavigateUrlFields = new[] { TrackingInventorySummary.Schema.WI_OP }
			};

			// PRODUCT IMAGE
			var productImage = new ZHyperLinkColumn(Res.GetString("66910a7c-882a-42cd-8b51-b91cdb376696", "Image"), TrackingInventorySummary.Schema.HasProductImage)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.HasProductImage,
				DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductImagePage) + (NoResString)"?Ref={0}", // URL parameter
				DataNavigateUrlFields = new[] { TrackingInventorySummary.Schema.WI_OP },
				ImageUrl = UrlFormatWithAppRoot("Images/ImageIcon.png"), // Image location
				Target = (NoResString)"_blank", // javascript code
				WindowStyle = Global.ProductImagePopupWindowStyle
			};

			// AVAILABLE PICK QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).WI_AvailableUnits);
			var availPickQuantityColumn = new ZCalcEditColumn(Res.GetString("c40145b9-0cda-4307-aa10-de0f35506e24", "Available Pick Qty"), TrackingInventorySummary.Schema.WI_AvailableUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.AvailableToPickQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			// ALLOCATED QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).WI_CrossDockQuantity);
			var allocatedQuantityColumn = new ZCalcEditColumn(Res.GetString("eb2f6dd8-4afe-48a2-a68e-7bd9516555bb", "Reserved Qty"), TrackingInventorySummary.Schema.WI_CrossDockQuantity)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.ReservedQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			// COMMITTED QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).WI_CommittedUnits);
			var comittedQuantityColumn = new ZCalcEditColumn(Res.GetString("50ef46ea-758a-4d08-9352-22bb691916d1", "Committed Qty"), TrackingInventorySummary.Schema.WI_CommittedUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.CommittedQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			// CLIENT QUANTITY & UQ
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).WI_ClientQuantity);
			ZBindToChecker.CheckBindTo((ZByte)((TrackingInventorySummary)null).SupplierPart.OP_CountDecimalPlaces);
			var clientQuantityColumn = new ZCalcEditColumn(Res.GetString("1b4cd351-eaed-4549-992b-0ec32ec394b6", "Client Qty"), TrackingInventorySummary.Schema.WI_ClientQuantity)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.ClientQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			// TOTAL QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).WI_TotalUnits);
			ZBindToChecker.CheckBindTo((ZByte)((TrackingInventorySummary)null).SupplierPart.OP_CountDecimalPlaces);
			var totalQuantityColumn = new ZCalcEditColumn(Res.GetString("9CC3EC48-2AC3-40CE-B44C-424FBA692AA6", "Total Qty"), TrackingInventorySummary.Schema.WI_TotalUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.TotalQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			// Add Columns
			AddToDictionaryAsRequired(new ZTextEditColumn(Res.GetString("05d9d39e-6a4e-41b2-8650-8790e48b51d9", "Warehouse"), TrackingInventorySummary.Schema.WarehouseName) { ColumnKey = WebTracker.Grids.TrackingInventory.Warehouse });
			AddToDictionaryAsRequired(productColumn);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("45dfafb0-4e53-44ff-8b25-84732365ba28", "Description"), TrackingInventorySummary.Schema.ProductDescription) { ColumnKey = WebTracker.Grids.TrackingInventory.Description });
			AddToDictionaryAsDefault(productImage);
			AddToDictionaryAsDefault(availPickQuantityColumn);
			AddToDictionaryAsDefault(allocatedQuantityColumn);
			AddToDictionaryAsDefault(comittedQuantityColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingInventorySummary)null).WI_ClientUQ);
			var clientQuantityUQColumn = new ZTextEditColumn(Res.GetString("a9e64b02-7156-4868-963c-4c374e4144d2", "Client UQ"), TrackingInventorySummary.Schema.WI_ClientUQ);
			var totalQuantityUQColumn = new ZTextEditColumn("UQ", TrackingInventorySummary.Schema.WI_UnitsUQ);
			AddToDictionaryAsDefault(new ZGroupColumn(Res.GetString("1b4cd351-eaed-4549-992b-0ec32ec394b6", "Client Qty"), clientQuantityColumn, clientQuantityUQColumn) { ColumnKey = WebTracker.Grids.TrackingInventory.ClientQuantity });
			AddToDictionaryAsDefault(new ZGroupColumn(Res.GetString("9CC3EC48-2AC3-40CE-B44C-424FBA692AA6", "Total Qty"), totalQuantityColumn, totalQuantityUQColumn) { ColumnKey = WebTracker.Grids.TrackingInventory.TotalQuantity });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).SupplierPart.OP_Weight);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("9c2d3826-8c63-46d4-8e0f-5619e7d5e59a", "Product Wt."), "SupplierPart+" + OrgSupplierPart.Schema.OP_Weight) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.ProductWeight });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).TotalWeight);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("69a5d699-3de1-4641-90ea-60b1ebf1471a", "Total Wt."), TrackingInventorySummary.Schema.TotalWeight) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.TotalWeight });

			ZBindToChecker.CheckBindTo((ZString)((TrackingInventorySummary)null).SupplierPart.OP_WeightUQ);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a4c6fbab-766a-439b-a472-125f7257fade", "Wt. UQ"), "SupplierPart+" + OrgSupplierPart.Schema.OP_WeightUQ) { ColumnKey = WebTracker.Grids.TrackingInventory.WeightUnit });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).SupplierPart.OP_Cubic);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("ce379b00-d0a0-4f63-be31-908561ec6695", "Product Vol."), "SupplierPart+" + OrgSupplierPart.Schema.OP_Cubic) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.ProductVolume });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).TotalVolume);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("8838525b-1361-4a03-a1c7-e57fe88d2e12", "Total Vol."), TrackingInventorySummary.Schema.TotalVolume) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.TotalVolume });

			ZBindToChecker.CheckBindTo((ZString)((TrackingInventorySummary)null).SupplierPart.OP_CubicUQ);
			AddToDictionary(new ZTextEditColumn(Res.GetString("c532099d-5643-4d96-bd0b-3470b8f49335", "Vol. UQ"), "SupplierPart+" + OrgSupplierPart.Schema.OP_CubicUQ) { ColumnKey = WebTracker.Grids.TrackingInventory.VolumeUnit });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingInventorySummary)null).WI_TotalValue);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("ee659c0e-4fbb-4806-a5c6-c38b98f5ccc0", "Total Value"), TrackingInventorySummary.Schema.WI_TotalValue) { ColumnKey = WebTracker.Grids.TrackingInventory.TotalValue });

			ZBindToChecker.CheckBindTo((ZString)((TrackingInventorySummary)null).WI_Currency);
			AddToDictionary(new ZTextEditColumn(Res.GetString("99b00b4e-8d51-4cf1-a017-e6551a6d91c9", "Currency"), TrackingInventorySummary.Schema.WI_Currency) { ColumnKey = WebTracker.Grids.TrackingInventory.Currency });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).SupplierPart.OP_LastCost);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("6cd8cdd5-6327-4406-a607-c0f232e65742", "Last Cost"), "SupplierPart+" + OrgSupplierPart.Schema.OP_LastCost) { Decimals = 2, ColumnKey = WebTracker.Grids.TrackingInventory.LastCost });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			var result = new List<int>
			{
				(int)WebTracker.Grids.TrackingInventory.Warehouse,
				(int)WebTracker.Grids.TrackingInventory.Product,
				(int)WebTracker.Grids.TrackingInventory.Description,
				(int)WebTracker.Grids.TrackingInventory.HasProductImage,
				(int)WebTracker.Grids.TrackingInventory.AvailableToPickQuantity,
				(int)WebTracker.Grids.TrackingInventory.ReservedQuantity,
				(int)WebTracker.Grids.TrackingInventory.CommittedQuantity,
				(int)WebTracker.Grids.TrackingInventory.ClientQuantity,
				(int)WebTracker.Grids.TrackingInventory.TotalQuantity,
				(int)WebTracker.Grids.TrackingInventory.ProductWeight,
				(int)WebTracker.Grids.TrackingInventory.TotalWeight,
				(int)WebTracker.Grids.TrackingInventory.WeightUnit,
				(int)WebTracker.Grids.TrackingInventory.ProductVolume,
				(int)WebTracker.Grids.TrackingInventory.TotalVolume,
				(int)WebTracker.Grids.TrackingInventory.VolumeUnit,
				(int)WebTracker.Grids.TrackingInventory.TotalValue,
				(int)WebTracker.Grids.TrackingInventory.Currency,
				(int)WebTracker.Grids.TrackingInventory.LastCost,
			};

			return result;
		}
	}
}
