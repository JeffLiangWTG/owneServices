using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Provides details of available inventory in a warehouse
	/// </summary>
	public class TrackingInventorySummary : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string PK = "WI_PK";
			public const string WI_WW_Whs = "WI_WW_Whs";
			public const string WI_OH_Client = "WI_OH_Client";
			public const string WI_OP = "WI_OP";

			public const string WI_TotalUnits = "WI_TotalUnits";
			public const string WI_CommittedUnits = "WI_CommittedUnits";
			public const string WI_AvailableUnits = "WI_AvailableUnits";
			public const string WI_CrossDockQuantity = "WI_CrossDockQuantity";
			public const string WI_ClientQuantity = "WI_ClientQuantity";

			public const string WI_UnitsUQ = "WI_UnitsUQ";
			public const string WI_ClientUQ = "WI_ClientUQ";
			public const string WI_ArrivalDate = "WI_ArrivalDate";

			public const string WarehouseName = "WarehouseName";
			public const string ProductCode = "ProductCode";
			public const string ProductDescription = "ProductDescription";
			public const string HasProductImage = "HasProductImage";

			public const string TotalWeight = "TotalWeight";
			public const string TotalVolume = "TotalVolume";

			public const string WI_TotalValue = "WI_TotalValue";
			public const string WI_Currency = "WI_Currency";
		}

		public TrackingInventorySummary(DynamicBusinessObject data, ZQuery additionalFilter)
			: base(data.Factory)
		{
			this.data = data;
			this.additionalFilter = additionalFilter;
		}

		readonly ZQuery additionalFilter;
		readonly DynamicBusinessObject data;

		public ZQuery AdditionalFilter => additionalFilter;

		public TrackingWhsInventoryCollection Inventories => inventories ?? (inventories = GetInventories());

		TrackingWhsInventoryCollection inventories;

		void ResetInventories()
		{
			if (inventories != null)
			{
				UnRegisterEditableChildObject(inventories);
			}
			inventories = null;
		}

		TrackingWhsInventoryCollection GetInventories()
		{
			var inventories = new TrackingWhsInventoryCollection(Factory);
			RegisterEditableChildObject(inventories);

			return inventories;
		}

		public void LoadInventories()
		{
			if (!Inventories.IsLoaded)
			{
				var filter = new ZQuery(WhsInventoryViewSchema.PK, InventoryPKs);
				Inventories.Load(filter);

				var strategy = (TrackingWhsInventoryCollectionFetchStrategy)Inventories.FetchStrategy;
				strategy.AddFetchHints(Inventories);

				foreach (TrackingWhsInventory inventory in Inventories)
				{
					inventory.SetSupplierPart(SupplierPart);
				}
			}
		}

		public void AddInventoryPK(ZGuid inventoryPK)
		{
			if (Inventories.IsLoaded && !InventoryPKs.Any(i => i == inventoryPK))
			{
				ResetInventories();
			}
			InventoryPKs.Add(inventoryPK);
			Factory.AddFetchHint(WhsInventoryViewSchema.PK, inventoryPK);
		}

		List<ZGuid> InventoryPKs => inventoryPKs ?? (inventoryPKs = new List<ZGuid>());
		List<ZGuid> inventoryPKs;

		public void SortInventories()
		{
			var sorter = new InventorySortComparer();
			Inventories.Sort(sorter);
		}

		public ZGuid WI_PK => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.PK);

		public ZPropertyInfo WI_PKInfo => GetZPropertyInfo(Schema.PK);

		public ZGuid WI_WW_Whs => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_WW_Whs);

		public ZPropertyInfo WI_WW_WhsInfo => GetZPropertyInfo(Schema.WI_WW_Whs);

		public ZGuid WI_OH_Client => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_OH_Client);

		public ZPropertyInfo WI_OH_ClientInfo => GetZPropertyInfo(Schema.WI_OH_Client);

		public ZGuid WI_OP => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_OP);

		public ZPropertyInfo WI_OPInfo => GetZPropertyInfo(Schema.WI_OP);

		public ZString WI_UnitsUQ => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_UnitsUQ);

		public ZPropertyInfo WI_UnitsUQInfo => GetZPropertyInfo(Schema.WI_UnitsUQ);

		public ZString WI_ClientUQ => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_ClientUQ);

		public ZPropertyInfo WI_ClientUQInfo => GetZPropertyInfo(Schema.WI_ClientUQ);

		public ZDateTimeOffset WI_ArrivalDate => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_ArrivalDate);

		public ZPropertyInfo WI_ArrivalDateInfo => GetZPropertyInfo(Schema.WI_ArrivalDate);

		public ZDecimal WI_TotalValue => WI_TotalUnits * (SupplierPart?.OP_LastCost ?? 0);

		public ZPropertyInfo WI_TotalValueInfo => GetZPropertyInfo(Schema.WI_TotalValue);

		public ZString WI_Currency => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_Currency);

		public ZPropertyInfo WI_CurrencyInfo => GetZPropertyInfo(Schema.WI_Currency);

		public ZDecimal TotalWeight => WI_TotalUnits * (SupplierPart?.OP_Weight ?? 0);

		public ZPropertyInfo TotalWeightInfo => GetZPropertyInfo(Schema.TotalWeight);

		public ZDecimal TotalVolume => WI_TotalUnits * (SupplierPart?.OP_Cubic ?? 0);

		public ZPropertyInfo TotalVolumeInfo => GetZPropertyInfo(Schema.TotalVolume);

		public ZDecimal WI_TotalUnits => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_TotalUnits);

		public ZPropertyInfo WI_TotalUnitsInfo => GetZPropertyInfo(Schema.WI_TotalUnits);

		public ZDecimal WI_CommittedUnits => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_CommittedUnits);

		public ZPropertyInfo WI_CommittedUnitsInfo => GetZPropertyInfo(Schema.WI_CommittedUnits);

		public ZDecimal WI_AvailableUnits => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_AvailableUnits);

		public ZPropertyInfo WI_AvailableUnitsInfo => GetZPropertyInfo(Schema.WI_AvailableUnits);

		public ZDecimal WI_CrossDockQuantity => GetColumnValue(WhsTrackingInventorySummaryItemViewSchema.WI_CrossDockQuantity);

		public ZPropertyInfo WI_CrossDockQuantityInfo => GetZPropertyInfo(Schema.WI_CrossDockQuantity);

		public ZDecimal WI_ClientQuantity => SupplierPart?.UnitConverter.Convert(WI_TotalUnits, SupplierPart.OP_StockKeepingUnit, WI_ClientUQ) ?? 0;

		public ZPropertyInfo WI_ClientQuantityInfo => GetZPropertyInfo(Schema.WI_ClientQuantity);

		#region Warehouse

		public WhsWarehouse Warehouse => warehouse ?? (warehouse = Factory.Load<WhsWarehouse>(WI_WW_Whs));

		WhsWarehouse warehouse;

		#endregion

		#region WarehouseName

		public ZString WarehouseName => Warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty;

		public ZPropertyInfo WarehouseNameInfo => GetZPropertyInfo(Schema.WarehouseName);

		#endregion

		#region ProductCode

		public ZString ProductCode => SupplierPart?.OP_PartNum ?? ZString.Empty;

		public ZPropertyInfo ProductCodeInfo => GetZPropertyInfo(Schema.ProductCode);

		#endregion

		#region ProductDescription

		public ZString ProductDescription => SupplierPart?.OP_Desc ?? ZString.Empty;

		public ZPropertyInfo ProductDescriptionInfo => GetZPropertyInfo(Schema.ProductDescription);

		#endregion

		#region ProductImage

		public ZBool HasProductImage
		{
			get
			{
				if (hasProductImage.HasValue)
				{
					return hasProductImage.Value;
				}

				var storageMain = ((IDocManagerSupport)SupplierPart)?.DocManagerInfo.MasterFactory.GetStorageMainForPK(SupplierPart.PK);

				if (storageMain == null)
				{
					hasProductImage = false;
				}
				else
				{
					hasProductImage = WhsProduct.GetWhsProduct(SupplierPart).ProductImage != null;
				}

				return hasProductImage.Value;
			}
		}
		bool? hasProductImage;

		public ZPropertyInfo HasProductImageInfo => GetZPropertyInfo(Schema.HasProductImage);

		#endregion

		public OrgSupplierPart SupplierPart => supplierPart ?? (supplierPart = Factory.Load<OrgSupplierPart>(WI_OP));

		OrgSupplierPart supplierPart;

		public override string TableName => WhsTrackingInventorySummaryItemViewSchema.Constants.TableName;

		public override string TablePrefix => WhsTrackingInventorySummaryItemViewSchema.Constants.Prefix;

		#region GetColumnValue

		ZDecimal GetColumnValue(SchemaDecimalColumn column) => GetColumnValue<ZDecimal>(column);

		ZDateTimeOffset GetColumnValue(SchemaDateTimeOffsetColumn column) => GetColumnValue<ZDateTimeOffset>(column);

		ZString GetColumnValue(SchemaStringColumn column) => GetColumnValue<ZString>(column);

		ZGuid GetColumnValue(SchemaGuidColumn column) => GetColumnValue<ZGuid>(column);

		ColumnType GetColumnValue<ColumnType>(SchemaColumn column) => (ColumnType)data[column];

		#endregion

		class InventorySortComparer : IComparer<TrackingWhsInventory>
		{
			public int Compare(TrackingWhsInventory x, TrackingWhsInventory y)
			{
				var result = 0;
				if (x.PK != y.PK)
				{
					foreach (var comparison in Comparisons)
					{
						result = comparison(x, y);
						if (result != 0)
						{
							break;
						}
					}
				}

				return result;
			}

			Comparison<TrackingWhsInventory>[] Comparisons
			{
				get
				{
					if (comparisons == null)
					{
						comparisons = new[]
						{
							(Comparison<TrackingWhsInventory>)CompareByArrivalDateOrETA,
							CompareByReceiptReference,
							CompareByPalletID,
							CompareByPartAttrib1,
							CompareByPartAttrib2,
							CompareByPartAttrib3,
							CompareBySerialNumber,
							CompareByPK,
						};
					}

					return comparisons;
				}
			}

			Comparison<TrackingWhsInventory>[] comparisons;

			int CompareByArrivalDateOrETA(TrackingWhsInventory x, TrackingWhsInventory y) => x.TrackingArrivalDateOrETA.CompareTo(y.TrackingArrivalDateOrETA);
			int CompareByReceiptReference(TrackingWhsInventory x, TrackingWhsInventory y) => x.ReceiptReference.CompareTo(y.ReceiptReference);
			int CompareByPalletID(TrackingWhsInventory x, TrackingWhsInventory y) => x.WI_PalletID.CompareTo(y.WI_PalletID);
			int CompareByPartAttrib1(TrackingWhsInventory x, TrackingWhsInventory y) => x.WI_PartAttrib1.CompareTo(y.WI_PartAttrib1);
			int CompareByPartAttrib2(TrackingWhsInventory x, TrackingWhsInventory y) => x.WI_PartAttrib2.CompareTo(y.WI_PartAttrib2);
			int CompareByPartAttrib3(TrackingWhsInventory x, TrackingWhsInventory y) => x.WI_PartAttrib3.CompareTo(y.WI_PartAttrib3);
			int CompareBySerialNumber(TrackingWhsInventory x, TrackingWhsInventory y) => x.WI_SerialNumber.CompareTo(y.WI_SerialNumber);
			int CompareByPK(TrackingWhsInventory x, TrackingWhsInventory y) => x.PK.CompareTo(y.PK);
		}
	}
}
