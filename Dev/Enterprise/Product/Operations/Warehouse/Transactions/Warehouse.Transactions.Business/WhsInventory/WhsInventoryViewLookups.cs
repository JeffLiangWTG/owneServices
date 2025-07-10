using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryViewLookups : AutoWhsInventoryViewLookups
	{
		#region Schema

		public static class Schema
		{
			public const string WI_LocationRow = "WI_LocationRow";
			public const string WI_LocationColumn = "WI_LocationColumn";
			public const string WI_LocationLevel = "WI_LocationLevel";
			public const string WI_LocationTray = "WI_LocationTray";
			public const string WI_WW_Whs = "WI_WW_Whs";
		}

		#endregion

		#region Constructors

		public WhsInventoryViewLookups(AutoWhsInventoryView parent)
			: base(parent)
		{
		}

		#endregion

		#region Parent

		protected new WhsInventoryView Parent
		{
			get { return (WhsInventoryView)base.Parent; }
		}

		#endregion

		#region Clients

		public OrgHeaderCollection Clients
		{
			get { return Factory.GetCachedValue("WhsInventoryViewLookups|Clients", () => new WarehouseClientCollectionWithSecurityCheck(Factory)); }
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return new WhsWarehouseCollectionWithSecurityCheck(Factory); }
		}

		#endregion

		#region Pack Types

		public CodeDescriptionPairList PackTypesWithStandardUnits
		{
			get
			{
				return LookupsHelper.PackTypesWithStandardUnits(Factory);
			}
		}

		#endregion

		#region SupplierParts

		public OrgSupplierPartCollection SupplierParts
		{
			get
			{
				var result = new WhsOrgSupplierPartCollection(Factory, null, Parent.Client, Parent.WI_OP_Desc, Parent.WI_UnitsUQ, false);
				if (!Parent.WI_OH_Client.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", Parent.WI_OH_Client));
				}
				return result;
			}
		}

		#endregion

		#region Bonded Entry Lines

		public virtual WhsBondedWarehouseAttributeCollection BondedEntryLines
		{
			get
			{
				EntryLineCodeParser parser = new EntryLineCodeParser(Parent.WI_BondedEntryKey);
				WhsBondedWarehouseAttributeCollection list = new WhsBondedWarehouseAttributeCollection(new BusinessObjectFactory());

				if (Parent.Docket != null)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", Parent.Docket.WD_WW_Whs));
				}

				list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Client", "Property", Parent.WI_OH_Client));
				list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product", "Property", Parent.WI_OP));
				list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Customs Entry Key", "Property", parser.EntryNumber));

				return list;
			}
		}

		#endregion

		#region Possible Inventory

		public WhsInventoryViewCollection PossibleInventory
		{
			get
			{
				var list = new WhsInventoryViewCollection(Factory);
				if (Parent.Docket != null)
				{
					if (Parent.Docket.WD_WW_Whs.IsValid)
					{
						list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", Parent.Docket.WD_WW_Whs));
					}
				}

				if (Parent.Client != null && Parent.WI_OH_Client.IsValid)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Client", "Property", Parent.WI_OH_Client));
				}

				if (Parent.WI_OP.IsValid)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product", "Property", Parent.WI_OP));
				}

				if (!Parent.WI_ExpiryDate.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Expiry Date", "Property1", Parent.WI_ExpiryDate));
				}

				if (!Parent.WI_ExpiryDate.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Expiry Date", "Property2", Parent.WI_ExpiryDate));
				}

				if (!Parent.WI_PackingDate.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Packing Date", "Property1", Parent.WI_PackingDate));
				}

				if (!Parent.WI_PackingDate.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Packing Date", "Property2", Parent.WI_PackingDate));
				}

				if (!Parent.WI_PartAttrib1.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 1", "Property", Parent.WI_PartAttrib1));
				}

				if (!Parent.WI_PartAttrib2.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 2", "Property", Parent.WI_PartAttrib2));
				}

				if (!Parent.WI_PartAttrib3.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 3", "Property", Parent.WI_PartAttrib3));
				}

				if (Parent.Location != null)
				{
					if (Parent.Location.Row != null)
					{
						list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Row", "Property", Parent.Location.Row.WR_Name));
					}

					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Column", "Property", new ZString(Parent.Location.WLV_Column.ToString())));
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Level", "Property", new ZString(Parent.Location.WLV_Level.ToString())));
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tray", "Property", new ZString(Parent.Location.WLV_Tray.ToString())));
				}

				return list;
			}
		}

		#endregion

		#region Locations

		public WhsLocationCollection Locations
		{
			get
			{
				var docket = Parent.Docket;
				return docket != null && docket.Warehouse != null
					? new WhsLocationCollection(docket.Warehouse)
					: new WhsLocationCollection(Factory);
			}
		}

		#endregion

		#region CommodityCodes

		public RefCommodityCodeCollection CommodityCodes
		{
			get { return new RefCommodityCodeCollection(Factory); }
		}

		#endregion

		#region Consignees

		public OrgHeaderCollection Consignees
		{
			get
			{
				var result = new ConsigneeCollection(Factory);

				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
					"Consignee - Related Consignor"
				  , "Property"
				  , delegate
				  {
					  OrgHeader client = Parent.Client;
					  return (client != null && client.BuyerLinks.Count > 0 && client.OH_IsConsignor) ? Parent.WI_OH_Client : ZGuid.Empty;
				  }
				));
				return result;
			}
		}

		#endregion

		#region PutawayAreas

		public WhsAreaCollection PutawayAreas
		{
			get { return WhsAreaCollection.GetPutawayAreas(Factory, Parent.WI_WW_Whs); }
		}

		#endregion

		#region InventoryHeldCodeCollection

		public UntranslatableCodeDescriptionPairList InventoryHeldCodeCollection
		{
			get
			{
				var key = $"WhsInventoryViewLookups|WhsInventoryHeldCodeCollection|{Parent.WI_OH_Client}";
				return Factory.GetCachedValue(key,
					() =>
					{
						var collection = new UntranslatableCodeDescriptionPairList((NoResString)"Contains a mix of system and user defined entries"); // Untranslatable reason
						collection.Add(new CodeDescriptionPair("", Res.GetString("WhsInventoryViewLookups|None", "None")));
						collection.AddRange(new WhsInventoryHeldCodeCollection(Factory, Parent.WI_OH_Client));
						return collection;
					});
			}
		}

		#endregion
	}
}
