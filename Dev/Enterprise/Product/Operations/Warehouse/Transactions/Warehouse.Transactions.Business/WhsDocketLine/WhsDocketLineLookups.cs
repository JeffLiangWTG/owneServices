using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketLineLookups : AutoWhsDocketLineLookups
	{
		#region Schema

		public static class Schema
		{
			public const string WI_WW_Whs = "WI_WW_Whs";
			public const string WI_OH_Client = "WI_OH_Client";
			public const string WI_OP = "WI_OP";
			public const string WI_LocationRow = "WI_LocationRow";
			public const string WI_LocationColumn = "WI_LocationColumn";
			public const string WI_LocationLevel = "WI_LocationLevel";
			public const string WI_LocationTray = "WI_LocationTray";
		}

		#endregion

		#region Constructors

		public WhsDocketLineLookups(AutoWhsDocketLine parent)
			: base(parent)
		{
		}

		#endregion

		#region Parent

		protected new WhsDocketLine Parent
		{
			get { return (WhsDocketLine)base.Parent; }
		}

		#endregion

		#region PutawayAreas

		public WhsAreaCollection PutawayAreas
		{
			get { return WhsAreaCollection.GetPutawayAreas(Factory, Parent.Docket.WD_WW_Whs); }
		}

		#endregion

		#region Supplier Parts

		public override OrgSupplierPartCollection SupplierParts
		{
			get
			{
				OrgSupplierPartCollection result;

				if (Parent.IsDeleted)
				{
					result = null;
				}
				else
				{
					var docket = Parent.Docket;
					if (docket != null)
					{
						var client = docket.Client;
						result = (client == null)
							? GetNewOrgSupplierPartCollectionWithClient(null)
							: Factory.GetCachedValue(GetCachedValueKeyString(client), () => GetNewOrgSupplierPartCollectionWithClient(client));

						if (client != null)
						{
							AddFiltersToResult(result, docket);
						}
					}
					else
					{
						result = GetNewOrgSupplierPartCollection();
					}
				}
				return result;
			}
		}

		void AddFiltersToResult(OrgSupplierPartCollection result, WhsDocket docket)
		{
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", docket.WD_OH_Client));
			AddFiltersToResultCore(result);
		}

		protected virtual void AddFiltersToResultCore(OrgSupplierPartCollection result)
		{
		}

		protected virtual string GetCachedValueKeyString(OrgHeader client)
		{
			return client.PK.ToStringKey();
		}

		protected virtual bool ExcludeProductsNotForResale
		{
			get { return false; }
		}

		protected virtual OrgSupplierPartCollection GetNewOrgSupplierPartCollection()
		{
			return new WhsOrgSupplierPartCollection(Factory);
		}

		protected virtual OrgSupplierPartCollection GetNewOrgSupplierPartCollectionWithClient(OrgHeader client)
		{
			return new WhsOrgSupplierPartCollection(Factory, null, client, false, FilterOptionsForPartCollection);
		}

		protected virtual PartFilterOptions FilterOptionsForPartCollection => ExcludeProductsNotForResale ? PartFilterOptions.ExcludeNotForResale : PartFilterOptions.None;

		#endregion

		#region Clients

		public OrgHeaderCollection Clients
		{
			get { return Factory.GetCachedValue("WhsDocketLineLookups|Clients", () => new WarehouseClientCollectionWithSecurityCheck(Factory)); }
		}

		#endregion

		#region Tasks

		public override ProcessTaskCollection Tasks => Factory.GetCachedValue("WhsDocketLineLookups|Tasks", () => new ProcessTaskCollection(Factory, ZQuery.NoResultQuery));

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return Factory.GetCachedValue("WhsDocketLineLookups|Warehouses", () => new WhsWarehouseCollectionWithSecurityCheck(Factory)); }
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

		#region Areas

		public WhsAreaCollection Areas
		{
			get
			{
				if (!Parent.IsDeleted)
				{
					return new WhsAreaCollection(Factory, Parent.Docket.Warehouse);
				}
				else
				{
					return new WhsAreaCollection(Factory);
				}
			}
		}

		#endregion

		#region Bonded Entry Lines

		public virtual WhsBondedWarehouseAttributeCollection BondedEntryLines
		{
			get
			{
				EntryLineCodeParser parser = new EntryLineCodeParser(Parent.WE_BondedEntryKey);
				WhsBondedWarehouseAttributeCollection list = new WhsBondedWarehouseAttributeCollection(new BusinessObjectFactory());

				if (Parent.Docket != null)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", Parent.Docket.WD_WW_Whs));
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Client", "Property", Parent.Docket.WD_OH_Client));
				}

				list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product", "Property", Parent.WE_OP));
				list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Customs Entry Key", "Property", parser.EntryNumber));

				return list;
			}
		}

		#endregion

		#region Pack Types

		public new CodeDescriptionPairList PackTypes
		{
			get
			{
				return Factory.GetCachedValue($"WhsDocketLineLookups|RefPackTypeCollection|{Parent.WE_OP}", // not localised, this is a key
					() => Parent.Product?.GetPackTypes() ?? new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits());
			}
		}

		#endregion

		#region Locations

		public WhsLocationCollection Locations
		{
			get
			{
				var warehouse = Parent.Warehouse;
				var result = (warehouse != null) ? new WhsLocationCollection(warehouse) : new WhsLocationCollection(Factory);
				AddLocationFiltersToResult(result);
				return result;
			}
		}

		protected virtual void AddLocationFiltersToResult(WhsLocationCollection result)
		{
		}

		#endregion

		#region InventoryHeldCodeCollection

		public UntranslatableCodeDescriptionPairList InventoryHeldCodeCollection
		{
			get
			{
				var clientPK = Parent.ClientPK;
				var key = $"WhsDocketLineLookups|InventoryHeldCodeCollection|{clientPK}";
				return Factory.GetCachedValue(key,
					() =>
					{
						var collection = new UntranslatableCodeDescriptionPairList((NoResString)"Contains a mix of system and user defined entries"); // Untranslatable reason
						collection.Add(new CodeDescriptionPair("", Res.GetString("WhsDocketLineLookups|None", "None")));

						var systemWideCodesOnly = clientPK == ZGuid.Empty;

						if (systemWideCodesOnly)
						{
							collection.AddRange(new WhsInventoryHeldCodeCollection(Factory));
						}
						else
						{
							collection.AddRange(new WhsInventoryHeldCodeCollection(Factory, clientPK));
						}

						return collection;
					});
			}
		}

		#endregion

		#region InventoryStatuses

		public CodeDescriptionPairList InventoryStatuses
		{
			get { return GetInventoryStatusesCore(); }
		}

		protected virtual CodeDescriptionPairList GetInventoryStatusesCore()
		{
			return Factory.GetCachedValue(
				"WhsDocketLineLookups|InventoryStatuses", () => new InventoryStatus());
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
						var docket = Parent.Docket;
						var client = docket != null ? docket.Client : null;
						return (client != null && client.BuyerLinks.Count > 0 && client.OH_IsConsignor) ? client.PK : ZGuid.Empty;
					}
				));
				return result;
			}
		}

		#endregion
	}
}
