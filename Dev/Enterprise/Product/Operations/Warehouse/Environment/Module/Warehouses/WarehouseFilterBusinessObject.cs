using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WarehouseFilterBusinessObject : FilterStripBusinessObject
	{
		public WarehouseFilterBusinessObject()
		{
		}

		public abstract class Schema
		{
			public readonly static string Code = nameof(Code);
			public readonly static string Name = nameof(Name);
			public readonly static string WarehouseType = nameof(WarehouseType);
			public readonly static string TransactionType = nameof(TransactionType);
			public readonly static string Branch = nameof(Branch);
			public readonly static string ReleaseGroup = nameof(ReleaseGroup);
		}

		internal WarehouseFilterBusinessObject(WarehouseCollectionType warehouseCollectionType)
		{
			WarehouseCollectionType = warehouseCollectionType;
		}

		readonly WarehouseCollectionType WarehouseCollectionType = WarehouseCollectionType.ProductWarehouse;

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddFlagsFilters(result);
			AddRelatedItemFilters(result);

			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			AddCodeFilter(filters);
			AddNameFilter(filters);
		}

		void AddCodeFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter warehouseCodeFilter;
			if (!Env.Security.WhsAllowedWarehouses.IsAllowed && !Globals.IsWeb)
			{
				warehouseCodeFilter = filters.AddTextFilter(Schema.Code, WhsWarehouseSchema.WW_WarehouseCode, Warehouses);
				warehouseCodeFilter.Visibility = FilterVisibility.AlwaysVisible;
				warehouseCodeFilter.PropertyValidation = CodeFilterValidation;
			}
			else
			{
				warehouseCodeFilter = filters.AddTextFilter(Schema.Code, WhsWarehouseSchema.WW_WarehouseCode);
			}

			warehouseCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Warehouse|WarehouseFilter|Code", "Code");
		}

		void CodeFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("7c24fa34-a084-4924-ae67-66e612b4bac5", "Please select a warehouse to filter by"));
			}
		}

		void AddNameFilter(ModuleFilterCollection filters)
		{
			filters.AddFiltersForTranslatableText(Schema.Name, WhsWarehouseSchema.WW_WarehouseName, typeof(WhsWarehouse), ResString.GetMultilingualString("Warehouse|WarehouseFilter|Name", "Name"));
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			AddWarehouseTypeFilter(filters);
			AddTransactionTypeFilter(filters);
		}

		#region AddWarehouseTypeFilter

		void AddWarehouseTypeFilter(ModuleFilterCollection filters)
		{
			var warehouseTypeFilter = filters.AddTextFilter(Schema.WarehouseType, GetWarehouseTypeFilter, WarehouseTypeList);
			warehouseTypeFilter.Category = FilterCategories.StatusAndFlags;

			if (WarehouseCollectionType == WarehouseCollectionType.TransitWarehouse)
			{
				warehouseTypeFilter.DefaultProperty = WarehouseTypes.Codes.Transit;
			}
			else if (WarehouseCollectionType == WarehouseCollectionType.CYDWarehouse)
			{
				warehouseTypeFilter.DefaultProperty = WarehouseTypes.Codes.ContainerYard;
			}
			else if (WarehouseCollectionType == WarehouseCollectionType.ProductWarehouse)
			{
				warehouseTypeFilter.DefaultProperty = WarehouseTypes.Codes.Product;
			}
			else
			{
				warehouseTypeFilter.DefaultProperty = WarehouseTypeCodes.All;
			}

			warehouseTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Warehouse|WarehouseFilter|WarehouseType", "Warehouse Type");
		}

		ZQuery GetWarehouseTypeFilter(ZString value)
		{
			ZQuery query;

			if (value == WarehouseTypeCodes.All)
			{
				query = new ZQuery();
			}
			else
			{
				query = new ZQuery(WhsWarehouseSchema.WW_WarehouseType, value);
			}

			return query;
		}

		#endregion

		#region AddTransactionTypeFilter

		void AddTransactionTypeFilter(ModuleFilterCollection filters)
		{
			var transactionTypeFilter = filters.AddTextFilter(Schema.TransactionType, GetTransactionTypeFilter, TransactionTypeList);
			transactionTypeFilter.Category = FilterCategories.StatusAndFlags;
			transactionTypeFilter.DefaultProperty = TransactionTypeCodes.All;
			transactionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Warehouse|WarehouseFilter|TransactionType", "Transaction Type");
		}

		ZQuery GetTransactionTypeFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsWarehouse));
			switch (value)
			{
				case TransactionTypeCodes.FreeStore:
					AddAreaTypeSubQuery(query, AreaTypes.Codes.FreeStore);
					break;
				case TransactionTypeCodes.Bonded:
					AddAreaTypeSubQuery(query, AreaTypes.Codes.Bonded);
					break;
				case TransactionTypeCodes.Excise:
					AddAreaTypeSubQuery(query, AreaTypes.Codes.Excise);
					break;
				case TransactionTypeCodes.InwardProcessing:
					AddAreaTypeSubQuery(query, AreaTypes.Codes.InwardProcessing);
					break;
				case TransactionTypeCodes.Virtual:
					query.AddToFilter(WhsWarehouseSchema.WW_IsVirtualWarehouse, ZBool.True);
					break;
				default:
					break;
			}
			return query;
		}

		void AddAreaTypeSubQuery(ZDBOnlyQuery query, string areaType)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsAreaSchema.WA_WW_Whs);
			subQuery.AddToFilter(WhsAreaSchema.WA_AreaType, areaType);
			query.AddSubQuery(subQuery, JoinCondition.And);
		}

		#endregion

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var branchFilter = filters.AddGuidFilter(Schema.Branch, ModuleIDs.GlbBranch, WhsWarehouseSchema.WW_GB_RelatedCompanyBranch, Branches);
			branchFilter.IsPublishedOnWeb = false;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Warehouse|WarehouseFilter|Branch", "Branch");

			if (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value)
			{
				var releaseGroupFilter = filters.AddGuidFilter(Schema.ReleaseGroup, ModuleIDs.GlbGroup, WhsWarehouseSchema.WW_GG_ReleaseGroup, ReleaseGroups);
				releaseGroupFilter.IsPublishedOnWeb = false;
				releaseGroupFilter.MultilingualDescription = ResString.GetMultilingualString("Warehouse|WarehouseFilter|ReleaseGroup", "Release Group");
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region Warehouses

		WhsWarehouseCollection Warehouses
		{
			get
			{
				var result = new WhsWarehouseCollectionWithSecurityCheck(Factory, WarehouseCollectionType);
				result.Load();
				return result;
			}
		}

		#endregion

		#region TransactionTypeList

		CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (transactionTypeList == null)
				{
					transactionTypeList = new CodeDescriptionPairList();
					transactionTypeList.AddPair(TransactionTypeCodes.All, Res.GetString("Warehouse|WarehouseFilter|TransactionType|All", "All"));
					transactionTypeList.AddPair(TransactionTypeCodes.FreeStore, Res.GetString("Warehouse|WarehouseFilter|TransactionType|FreeStore", "Free Store"));
					transactionTypeList.AddPair(TransactionTypeCodes.Bonded, Res.GetString("Warehouse|WarehouseFilter|TransactionType|Bonded", "Bonded"));
					transactionTypeList.AddPair(TransactionTypeCodes.Excise, Res.GetString("Warehouse|WarehouseFilter|TransactionType|Excise", "Excise"));
					transactionTypeList.AddPair(TransactionTypeCodes.Virtual, Res.GetString("Warehouse|WarehouseFilter|TransactionType|Virtual", "Virtual"));
					transactionTypeList.AddPair(TransactionTypeCodes.InwardProcessing, Res.GetString("Warehouse|WarehouseFilter|TransactionType|InwardProcessing", "Inward Processing"));
				}

				return transactionTypeList;
			}
		}

		CodeDescriptionPairList transactionTypeList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		class TransactionTypeCodes
		{
			public const string All = "All";
			public const string FreeStore = "Free Store";
			public const string Bonded = "Bonded";
			public const string Excise = "Excise";
			public const string Virtual = "Virtual";
			public const string InwardProcessing = "Inward Processing";
		}

		#endregion

		#region WarehouseTypeList

		CodeDescriptionPairList WarehouseTypeList
		{
			get { return Factory.GetCachedValue($"WarehouseFilterBusinessObject|WarehouseTypeList|{WarehouseCollectionType}", () => GetWarehouseTypeList()); }
		}

		CodeDescriptionPairList GetWarehouseTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(WarehouseTypeCodes.All, Res.GetString("Warehouse|WarehouseFilter|WarehouseType|All", "All"));

			if (WarehouseCollectionType == WarehouseCollectionType.TransitWarehouse)
			{
				result.AddPair(WarehouseTypes.Codes.Transit, WarehouseTypes.Descriptions.Transit);
			}
			else if (WarehouseCollectionType == WarehouseCollectionType.ProductWarehouse)
			{
				result.AddPair(WarehouseTypes.Codes.Product, WarehouseTypes.Descriptions.Product);
				result.AddPair(WarehouseTypes.Codes.FreeTradeZone, WarehouseTypes.Descriptions.FreeTradeZone);
			}
			else if (WarehouseCollectionType == WarehouseCollectionType.CYDWarehouse)
			{
				result.AddPair(WarehouseTypes.Codes.ContainerYard, WarehouseTypes.Descriptions.ContainerYard);
			}
			else if (WarehouseCollectionType == WarehouseCollectionType.All)
			{
				result.AddRange(new WarehouseTypes());
			}

			return result;
		}

		abstract class WarehouseTypeCodes : WarehouseTypes.Codes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code")]
			public const string All = "All";
		}

		#endregion

		#region Branches

		public GlbBranchCollection Branches => branches ??= new GlbBranchCollection(Factory);
		GlbBranchCollection branches;

		#endregion

		#region ReleaseGroups

		public GlbGroupCollection ReleaseGroups => releaseGroups ??= new GlbGroupCollection(Factory);
		GlbGroupCollection releaseGroups;

		#endregion

		#endregion
	}
}

