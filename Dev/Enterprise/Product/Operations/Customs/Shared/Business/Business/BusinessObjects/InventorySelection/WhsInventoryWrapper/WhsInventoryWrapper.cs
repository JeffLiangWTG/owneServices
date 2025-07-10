using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class WhsInventoryWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public WhsInventoryWrapper(IWhsInventoryView inventory, InventorySelectionHeader header, WhsOrderLineWrapper relatedOrderLineWrapper = null)
			: base(header.Factory)
		{
			this.inventory = Argument.NotNull(inventory, "inventory");
			this.header = header;
			this.relatedOrderLineWrapper = relatedOrderLineWrapper;
		}
		readonly InventorySelectionHeader header;
		public readonly WhsOrderLineWrapper relatedOrderLineWrapper;

		public static class Schema
		{
			public const string ArrivalDate = "ArrivalDate";
			public const string CalculatedOriginalBondedQty = "CalculatedOriginalBondedQty";
			public const string CalculatedQuantityOnHand = "CalculatedQuantityOnHand";
			public const string CustomsEntryKey = "CustomsEntryKey";
			public const string AllocationKey = "AllocationKey";
			public const string OriginalBondedQty = "OriginalBondedQty";
			public const string Product = "Product";
			public const string Attribute1 = "Attribute1";
			public const string Attribute2 = "Attribute2";
			public const string Attribute3 = "Attribute3";
			public const string SerialNumber = "SerialNumber";
			public const string GroupingID = "GroupingID";
			public const string ProductDescription = "ProductDescription";
			public const string SupplierName = "SupplierName";
			public const string QuantityOnHand = "QuantityOnHand";
			public const string QuantityToDraw = "QuantityToDraw";
			public const string WarehouseName = "WarehouseName";
			public const string OriginalPackType = "OriginalPackType";
		}

		public IWhsInventoryView Inventory
		{
			get { return inventory; }
		}
		readonly IWhsInventoryView inventory;

		public IWhsDocket Receive
		{
			get
			{
				if (!receiveLoaded)
				{
					receiveLoaded = true;
					var receiveLine = ReceiveLine;
					if (receiveLine != null)
					{
						receive = Factory.Load<IWhsDocket>(receiveLine.WE_WD);
					}
				}
				return receive;
			}
		}
		bool receiveLoaded;
		IWhsDocket receive;

		public IWhsDocketLine ReceiveLine
		{
			get
			{
				if (!receiveLineLoaded)
				{
					receiveLineLoaded = true;
					receiveLine = Factory.Load<IWhsDocketLine>(Inventory.WI_WE_InDocketLine);
				}
				return receiveLine;
			}
		}
		bool receiveLineLoaded;
		IWhsDocketLine receiveLine;

		public IWhsWarehouse Warehouse
		{
			get
			{
				if (!warehouseLoaded)
				{
					warehouseLoaded = true;
					var receive = Receive;
					warehouse = receive == null ? null : Factory.Load<IWhsWarehouse>(receive.WD_WW_Whs);
				}
				return warehouse;
			}
		}
		bool warehouseLoaded;
		IWhsWarehouse warehouse;

		public OrgAddress WarehouseAddress
		{
			get
			{
				if (!warehouseAddressLoaded)
				{
					warehouseAddressLoaded = true;
					var warehouse = Warehouse;
					warehouseAddress = warehouse == null ? null : Factory.Load<OrgAddress>(warehouse.WW_OA_WarehouseAddress);
				}
				return warehouseAddress;
			}
		}
		bool warehouseAddressLoaded;
		OrgAddress warehouseAddress;

		public OrgSupplierPart Part
		{
			get
			{
				if (!partLoaded)
				{
					partLoaded = true;
					part = Factory.Load<OrgSupplierPart>(Inventory.WI_OP);
				}
				return part;
			}
		}
		bool partLoaded;
		OrgSupplierPart part;

		public ZString LineNoKey
		{
			get
			{
				if (!lineNoKey.HasValue)
				{
					var line = ReceiveLine;
					lineNoKey = ZString.Format("{0}_{1}_{2}", line?.WE_LineNo.ToString().PadRight(5), line?.WE_SubLineNo.ToString().PadRight(5), Inventory.PK.ToStringKey());
				}
				return lineNoKey.Value;
			}
		}
		ZString? lineNoKey;

		public ZString ProductGroupKey
		{
			get
			{
				if (!productGroupKey.HasValue)
				{
					productGroupKey = ZString.Format("{0}_{1}_{2}", ProductKey, Inventory.WI_BondedEntryKey, ReceiveLine?.WE_PerPackageQty);
				}
				return productGroupKey.Value;
			}
		}
		ZString? productGroupKey;

		public ZString ProductKey
		{
			get
			{
				if (!productKey.HasValue)
				{
					productKey = ZString.Format("{0}_{1}_{2}_{3}_{4}", Inventory.WI_OP.ToStringKey(), Inventory.WI_PartAttrib1, Inventory.WI_PartAttrib2, Inventory.WI_PartAttrib3, Inventory.WI_SerialNumber);
				}
				return productKey.Value;
			}
		}
		ZString? productKey;

		public ZString PackingGroupKey
		{
			get
			{
				if (!packingGroupKey.HasValue)
				{
					packingGroupKey = InventorySelectionHeader.GetPackingGroupKey(ReceiveLine);
				}
				return packingGroupKey.Value;
			}
		}
		ZString? packingGroupKey;

		public ZInt PerPackageQty
		{
			get
			{
				if (!perPackageQty.HasValue)
				{
					var perPkgQty = 1;
					var receiveLine = ReceiveLine;
					if (receiveLine != null && !receiveLine.WE_PackageGroupId.IsEmpty)
					{
						perPkgQty = receiveLine.WE_PerPackageQty.ToZInt();
						perPkgQty = perPkgQty <= 0 ? 1 : perPkgQty;
					}
					perPackageQty = perPkgQty;
				}
				return perPackageQty.Value;
			}
		}
		ZInt? perPackageQty;

		public ZString WarehouseKey
		{
			get
			{
				if (!warehouseKey.HasValue)
				{
					var receive = Receive;
					warehouseKey = (receive == null ? Inventory.PK : receive.WD_WW_Whs).ToStringKey();
				}
				return warehouseKey.Value;
			}
		}
		ZString? warehouseKey;

		public ZString BondedEntryKeyForGrouping
		{
			get
			{
				if (!bondedEntryKeyForGrouping.HasValue)
				{
					bondedEntryKeyForGrouping = Inventory.WI_BondedEntryKey.IsEmpty ? Inventory.PK.ToStringKey() : (string)Inventory.WI_BondedEntryKey;
				}
				return bondedEntryKeyForGrouping.Value;
			}
		}
		ZString? bondedEntryKeyForGrouping;

		public ZGuid SupplierPK
		{
			get
			{
				if (!supplierPK.HasValue)
				{
					JobDocAddress supplierDocAddress = null;
					var receiveLine = ReceiveLine;
					if (receiveLine != null)
					{
						supplierDocAddress = GetSupplierDocAddress(receiveLine.PK);
					}
					if (supplierDocAddress == null)
					{
						var receive = Receive;
						if (receive != null)
						{
							supplierDocAddress = GetSupplierDocAddress(receive.PK);
						}
					}
					if (supplierDocAddress == null)
					{
						supplierPK = GetSupplierPKFromPart();
					}
					else
					{
						var supplier = supplierDocAddress.Organisation;
						supplierPK = supplier == null ? ZGuid.Empty : supplier.PK;
					}
				}
				return supplierPK.Value;
			}
		}
		ZGuid? supplierPK;

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|OriginalPackType", Caption = "Pack Type")]
		public ZString OriginalPackType
		{
			get
			{
				return inventory.WI_F3_NKPackType;
			}
		}

		public ZPropertyInfo OriginalPackTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalPackType); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|SupplierPK", Caption = "Supplier")]
		public ZString SupplierName
		{
			get
			{
				if (!supplierName.HasValue)
				{
					var supplier = Factory.Load<OrgHeader>(SupplierPK);
					supplierName = supplier == null ? ZString.Empty : supplier.OH_FullName;
				}
				return supplierName.Value;
			}
		}
		ZString? supplierName;

		public ZPropertyInfo SupplierNameInfo
		{
			get { return GetZPropertyInfo(Schema.SupplierName); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|ArrivalDate", Caption = "Arrival Date")]
		public ZDateTime ArrivalDate
		{
			get { return Inventory.WI_ArrivalDate.ToZDateTime(); }
		}

		public ZPropertyInfo ArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalDate); }
		}

		public ZString AllocationKey => Inventory.WI_AllocationKey;

		public ZPropertyInfo AllocationKeyInfo => GetZPropertyInfo(Schema.AllocationKey);

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|CustomsEntryKey", Caption = "Customs Entry Key")]
		public ZString CustomsEntryKey
		{
			get { return Inventory.WI_BondedEntryKey; }
		}

		public ZPropertyInfo CustomsEntryKeyInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsEntryKey); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|OriginalBondedQty", Caption = "Original Bonded Qty", ShortCaption = "Original Qty")]
		public ZDecimal OriginalBondedQty
		{
			get { return Inventory.WI_InDocketLineUnits; }
		}

		public ZPropertyInfo OriginalBondedQtyInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalBondedQty); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|CalculatedOriginalBondedQty", Caption = "Original Bonded Qty", ShortCaption = "Original Qty")]
		public ZDecimal CalculatedOriginalBondedQty
		{
			get { return PackingGroupKey.IsEmpty ? OriginalBondedQty : header.GetOriginalBondedQty(PackingGroupKey, ProductGroupKey); }
		}

		public ZPropertyInfo CalculatedOriginalBondedQtyInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedOriginalBondedQty); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|Product", Caption = "Product")]
		public ZString Product
		{
			get
			{
				if (!productCached.HasValue)
				{
					var part = Part;
					productCached = part == null ? ZString.Empty : part.OP_PartNum;
				}
				return productCached.Value;
			}
		}
		ZString? productCached;

		public ZPropertyInfo ProductInfo
		{
			get { return GetZPropertyInfo(Schema.Product); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|Attribute1", Caption = "Attribute 1", ShortCaption = "Attrib. 1")]
		public ZString Attribute1 => Inventory.WI_PartAttrib1;
		public ZPropertyInfo Attribute1Info => GetZPropertyInfo(Schema.Attribute1);

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|Attribute2", Caption = "Attribute 2", ShortCaption = "Attrib. 2")]
		public ZString Attribute2 => Inventory.WI_PartAttrib2;
		public ZPropertyInfo Attribute2Info => GetZPropertyInfo(Schema.Attribute2);

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|Attribute3", Caption = "Attribute 3", ShortCaption = "Attrib. 3")]
		public ZString Attribute3 => Inventory.WI_PartAttrib3;
		public ZPropertyInfo Attribute3Info => GetZPropertyInfo(Schema.Attribute3);

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|SerialNumber", Caption = "Serial Number", MediumCaption = "Serial #", ShortCaption = "SN #")]
		public ZString SerialNumber => Inventory.WI_SerialNumber;
		public ZPropertyInfo SerialNumberInfo => GetZPropertyInfo(Schema.SerialNumber);

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|GroupingID", Caption = "Grouping ID")]
		public ZString GroupingID => ReceiveLine?.WE_PackageGroupId ?? ZString.Empty;
		public ZPropertyInfo GroupingIDInfo => GetZPropertyInfo(Schema.GroupingID);

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|ProductDescription", Caption = "Product Description")]
		public ZString ProductDescription
		{
			get
			{
				if (!productDescriptionCached.HasValue)
				{
					var part = Part;
					productDescriptionCached = part == null ? ZString.Empty : part.OP_Desc;
				}
				return productDescriptionCached.Value;
			}
		}
		ZString? productDescriptionCached;

		public ZPropertyInfo ProductDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ProductDescription); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|QuantityOnHand", Caption = "Quantity on Hand")]
		public ZDecimal QuantityOnHand
		{
			get { return Inventory.WI_AvailableToPickQuantity; }
		}

		public ZPropertyInfo QuantityOnHandInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityOnHand); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|CalculatedQuantityOnHand", Caption = "Quantity on Hand")]
		public ZDecimal CalculatedQuantityOnHand
		{
			get { return PackingGroupKey.IsEmpty ? QuantityOnHand : header.GetQuantityOnHand(PackingGroupKey, ProductGroupKey); }
		}

		public ZPropertyInfo CalculatedQuantityOnHandInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedQuantityOnHand); }
		}

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|QuantityToDraw", Caption = "Quantity to Draw")]
		public ZDecimal QuantityToDraw
		{
			get { return quantityToDraw; }
			set
			{
				var oldValue = QuantityToDraw;
				quantityToDraw = value;
				if (!IsCopying && oldValue != QuantityToDraw)
				{
					header.UpdateSelectedInventory(this);
				}
				QuantityToDrawInfo.RefreshBinding(oldValue);
			}
		}
		ZDecimal quantityToDraw;

		public ZPropertyInfo QuantityToDrawInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityToDraw); }
		}

		[ResourceStringData("NPBO:Enterprise.Customs.Business.WhsInventoryWrapper|WarehousePK", Caption = "Warehouse")]
		public ZString WarehouseName
		{
			get
			{
				if (!warehouseNameCached.HasValue)
				{
					var warehouse = Warehouse;
					warehouseNameCached = warehouse == null ? ZString.Empty : warehouse.WW_WarehouseName;
				}
				return warehouseNameCached.Value;
			}
		}
		ZString? warehouseNameCached;

		public ZPropertyInfo WarehouseNameInfo
		{
			get { return GetZPropertyInfo(Schema.WarehouseName); }
		}

		public IWhsDocket Order { get; set; }

		public IEnumerable<IWhsDocketLine> OrderLines { get; set; }

		ZGuid GetSupplierPKFromPart()
		{
			var result = ZGuid.Empty;
			var part = Part;
			if (part != null)
			{
				foreach (var orgPK in part.RelatedOrganisations.OfType<OrgPartRelation>().Where(x => x.IsSupplier && !x.OU_OH.IsEmpty).Select(x => x.OU_OH))
				{
					if (result.IsEmpty)
					{
						result = orgPK;
					}
					else if (result != orgPK)
					{
						result = ZGuid.Empty;
						break;
					}
				}
			}
			return result;
		}

		JobDocAddress GetSupplierDocAddress(ZGuid parentID)
		{
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, parentID);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.SupplierDocumentaryAddress);
			query.OrderBy = JobDocAddressSchema.Constants.E2_AddressSequence;
			var supplierDocAddress = Factory.LoadTop1<JobDocAddress>(query);
			return supplierDocAddress;
		}
	}
}
