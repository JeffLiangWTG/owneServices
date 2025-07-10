using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingInventoryColumnProvider))]
	class TrackingInventoryColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingInventory.LastCost],
				TestProvider[WebTracker.Grids.TrackingInventory.Product],
				TestProvider[WebTracker.Grids.TrackingInventory.ReservedQuantity],
				TestProvider[WebTracker.Grids.TrackingInventory.CommittedQuantity],
				TestProvider[WebTracker.Grids.TrackingInventory.TotalQuantity],
				TestProvider[WebTracker.Grids.TrackingInventory.AvailableToPickQuantity],
				TestProvider[WebTracker.Grids.TrackingInventory.Warehouse]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();

			var productColumn = new ZHyperLinkColumn("Product", TrackingInventorySummary.Schema.ProductCode)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.Product,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductProfileDetailsPage + "?Ref={0}", // URL parameter
				DataNavigateUrlFields = new[] { TrackingInventorySummary.Schema.WI_OP }
			};

			var productImage = new ZHyperLinkColumn("Image", TrackingInventorySummary.Schema.HasProductImage)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.HasProductImage,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductImagePage + "?Ref={0}", // URL parameter
				DataNavigateUrlFields = new[] { TrackingInventorySummary.Schema.WI_OP },
				ImageUrl = "Images/image.png", // Image location
				Target = "_blank", // javascript code
				WindowStyle = Global.ProductImagePopupWindowStyle
			};

			var availPickQuantityColumn = new ZCalcEditColumn("Available Pick Qty", TrackingInventorySummary.Schema.WI_AvailableUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.AvailableToPickQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			var allocatedQuantityColumn = new ZCalcEditColumn("Reserved Qty", TrackingInventorySummary.Schema.WI_CrossDockQuantity)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.ReservedQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			var committedQuantityColumn = new ZCalcEditColumn("Committed Qty", TrackingInventorySummary.Schema.WI_CommittedUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.CommittedQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			var clientQuantityColumn = new ZCalcEditColumn("Client Qty", TrackingInventorySummary.Schema.WI_ClientQuantity)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.ClientQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			var totalQuantityColumn = new ZCalcEditColumn("Total Qty", TrackingInventorySummary.Schema.WI_TotalUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.TotalQuantity,
				BindToDecimals = "SupplierPart+" + OrgSupplierPart.Schema.OP_CountDecimalPlaces
			};

			AddRequiredColumn(new ZTextEditColumn("Warehouse", TrackingInventorySummary.Schema.WarehouseName) { ColumnKey = WebTracker.Grids.TrackingInventory.Warehouse });
			AddRequiredColumn(productColumn);
			AddDefaultsColumn(new ZTextEditColumn("Description", TrackingInventorySummary.Schema.ProductDescription) { ColumnKey = WebTracker.Grids.TrackingInventory.Description });
			AddDefaultsColumn(productImage);
			AddDefaultsColumn(availPickQuantityColumn);
			AddDefaultsColumn(allocatedQuantityColumn);
			AddDefaultsColumn(committedQuantityColumn);

			var clientQuantityUQColumn = new ZTextEditColumn("Client UQ", TrackingInventorySummary.Schema.WI_ClientUQ);
			AddDefaultsColumn(new ZGroupColumn("Client Qty", clientQuantityColumn, clientQuantityUQColumn) { ColumnKey = WebTracker.Grids.TrackingInventory.ClientQuantity });

			var totalQuantityUQColumn = new ZTextEditColumn("UQ", TrackingInventorySummary.Schema.WI_UnitsUQ);
			AddDefaultsColumn(new ZGroupColumn("Total Qty", totalQuantityColumn, totalQuantityUQColumn) { ColumnKey = WebTracker.Grids.TrackingInventory.TotalQuantity });

			AddColumn(new ZCalcEditColumn("Product Wt.", "SupplierPart+" + OrgSupplierPart.Schema.OP_Weight) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.ProductWeight });
			AddColumn(new ZCalcEditColumn("Total Wt.", TrackingInventorySummary.Schema.TotalWeight) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.TotalWeight });
			AddColumn(new ZTextEditColumn("Wt. UQ", "SupplierPart+" + OrgSupplierPart.Schema.OP_WeightUQ) { ColumnKey = WebTracker.Grids.TrackingInventory.WeightUnit });
			AddColumn(new ZCalcEditColumn("Product Vol.", "SupplierPart+" + OrgSupplierPart.Schema.OP_Cubic) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.ProductVolume });
			AddColumn(new ZCalcEditColumn("Total Vol.", TrackingInventorySummary.Schema.TotalVolume) { Decimals = 3, ColumnKey = WebTracker.Grids.TrackingInventory.TotalVolume });
			AddColumn(new ZTextEditColumn("Vol. UQ", "SupplierPart+" + OrgSupplierPart.Schema.OP_CubicUQ) { ColumnKey = WebTracker.Grids.TrackingInventory.VolumeUnit });
			AddColumn(new ZCalcEditColumn("Total Value", TrackingInventorySummary.Schema.WI_TotalValue) { ColumnKey = WebTracker.Grids.TrackingInventory.TotalValue });
			AddColumn(new ZTextEditColumn("Currency", TrackingInventorySummary.Schema.WI_Currency) { ColumnKey = WebTracker.Grids.TrackingInventory.Currency });
			AddColumn(new ZCalcEditColumn("Last Cost", "SupplierPart+" + OrgSupplierPart.Schema.OP_LastCost) { Decimals = 2, ColumnKey = WebTracker.Grids.TrackingInventory.LastCost });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingInventory.ClientQuantity,
			WebTracker.Grids.TrackingInventory.TotalQuantity
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingInventoryColumnProvider();
		}

		#endregion
	}
}
