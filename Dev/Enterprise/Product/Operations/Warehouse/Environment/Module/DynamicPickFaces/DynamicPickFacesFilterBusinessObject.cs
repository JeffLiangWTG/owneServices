using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class DynamicPickFacesFilterBusinessObject : PickFaceViewFilterBusinessObject
	{
		#region Schema

		public abstract new class Schema : PickFaceViewFilterBusinessObject.Schema
		{
			public const string Area = "Area";
			public const string Assignment = "Assignment";
			public const string LocationType = "LocationType";
		}

		#endregion

		#region Constructor

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var moduleFilterCollection = base.GetModuleFiltersCore();

			AddAreaFilter(moduleFilterCollection);
			AddAssignmentFilter(moduleFilterCollection);
			AddLocationTypeFilter(moduleFilterCollection);

			return moduleFilterCollection;
		}

		#endregion

		#region Warehouse filter

		protected override ModuleGuidFilter GetNewWarehouseFilter(ModuleFilterCollection filters, string description, WhsWarehouseCollectionWithSecurityCheck collection)
		{
			ZQuery WarehouseQuery(ZGuid value) => new ZQuery(WhsWarehouseSchema.PK, value);

			var warehouseFilter = filters.AddGuidFilter(description, ModuleIDs.WhsConfigWarehouse, WarehouseQuery, collection);
			warehouseFilter.SubGroup = new WarehouseFiltersSubGroup();
			warehouseFilter.PropertyInfo.ValueChanged += (s, e) =>
			{
				AreaCollection = null;
				AreaFilter.Property = ZGuid.Empty;
			};

			return warehouseFilter;
		}

		class WarehouseFiltersSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_WarehouseCode);
				warehouseSubQuery.AddToFilter(filter);

				var viewQuery = new ZDBOnlyQuery(typeof(WhsDynamicPickFaceView));
				viewQuery.AddSubQuery(WhsDynamicPickFaceViewSchema.WDP_WarehouseCode, warehouseSubQuery, JoinCondition.And);

				return viewQuery;
			}
		}

		#endregion

		#region Area filter

		void AddAreaFilter(ModuleFilterCollection filters)
		{
			ZQuery AreaQuery(ZGuid value) => new ZQuery(WhsAreaSchema.PK, value);

			var areaFilter = filters.AddGuidFilter(Schema.Area, ModuleIDs.WhsConfigArea, AreaQuery, GetAreaCollection);
			areaFilter.MultilingualDescription = ResString.GetMultilingualString("aef93703-7e9e-4ddc-98a9-b4fbf7a95a36", "Area");
			areaFilter.SubGroup = new AreaFiltersSubGroup();

			areaFilter.PropertyValidation = info =>
			{
				if (IsParentFilterInvalid(WarehouseFilter, areaFilter))
				{
					info.AddError(ResString.GetMultilingualString("415a5566-84c6-4e9e-a258-a2aabfcc070a", "Area can not be entered without a Warehouse Code."));
				}
			};
		}

		class AreaFiltersSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var areaSubQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsAreaSchema.WA_Name);
				areaSubQuery.AddToFilter(filter);

				var viewQuery = new ZDBOnlyQuery(typeof(WhsDynamicPickFaceView));
				viewQuery.AddSubQuery(WhsDynamicPickFaceViewSchema.WDP_AreaName, areaSubQuery, JoinCondition.And);

				return viewQuery;
			}
		}

		WhsAreaCollection GetAreaCollection()
		{
			if (AreaCollection == null)
			{
				var filter = WarehouseFilter;
				var warehouse = filter.IsActive ? Factory.Load<WhsWarehouse>(filter.Property) : null;
				if (warehouse != null)
				{
					AreaCollection = new WhsAreaCollection(Factory, warehouse);
				}
			}

			return AreaCollection;
		}

		#endregion

		#region Location filter

		protected override SchemaGuidColumn LocationColumn => WhsDynamicPickFaceViewSchema.WDP_WL_Location;

		#endregion

		#region Client filter

		protected override ModuleGuidFilter GetNewClientFilter(ModuleFilterCollection filters, string description, WarehouseClientCollectionWithSecurityCheck collection)
		{
			ZQuery ClientQuery(ZGuid value) => new ZQuery(OrgHeaderSchema.PK, value);

			var clientFilter = filters.AddGuidFilter(description, ModuleIDs.Organisation, ClientQuery, collection);
			clientFilter.SubGroup = new ClientFiltersSubGroup();
			return clientFilter;
		}

		class ClientFiltersSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var clientSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.OH_Code);
				clientSubQuery.AddToFilter(filter);

				var viewQuery = new ZDBOnlyQuery(typeof(WhsDynamicPickFaceView));
				viewQuery.AddSubQuery(WhsDynamicPickFaceViewSchema.WDP_ClientCode, clientSubQuery, JoinCondition.And);

				return viewQuery;
			}
		}

		#endregion

		#region Product filter

		protected override SchemaGuidColumn ProductColumn => WhsDynamicPickFaceViewSchema.WDP_OP_Product;

		#endregion

		#region Assignment Status filter

		void AddAssignmentFilter(ModuleFilterCollection filters)
		{
			ZQuery AssignmentQuery(ZString value)
			{
				var query = new ZQuery();
				switch (value)
				{
					case DynamicPickFaceAssignmentStatus.Codes.Assigned:
						return query.AddToFilter(WhsDynamicPickFaceViewSchema.WDP_IsAssigned, true);
					case DynamicPickFaceAssignmentStatus.Codes.Unassigned:
						return query.AddToFilter(WhsDynamicPickFaceViewSchema.WDP_IsAssigned, false);
					case DynamicPickFaceAssignmentStatus.Codes.All:
					default:
						return query;
				}
			}

			var assignmentFilter = filters.AddTextFilter(Schema.Assignment, AssignmentQuery, new DynamicPickFaceAssignmentStatus());
			assignmentFilter.MultilingualDescription = ResString.GetMultilingualString("95dd6564-6dc4-4791-b04a-a020a3663ebc", "Assignment Status");
			assignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			assignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			assignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			assignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			assignmentFilter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region ABCCategory filter

		protected override SchemaStringColumn ABCCategoryColumn => WhsDynamicPickFaceViewSchema.WDP_ABCCategory;

		#endregion

		#region LocationType filter

		void AddLocationTypeFilter(ModuleFilterCollection filters)
		{
			ZQuery LocationTypeQuery(ZGuid value) => new ZQuery(WhsLocationTypeSchema.PK, value);

			var locationTypeFilter = filters.AddGuidFilter(Schema.LocationType, ModuleIDs.WhsConfigLocationType, LocationTypeQuery, new WhsLocationTypeCollection(Factory));
			locationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("782898df-a7bf-4eca-8f75-d711c69a3abf", "Location Type");
			locationTypeFilter.SubGroup = new LocationTypeFiltersSubGroup();
		}

		class LocationTypeFiltersSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var locationTypeSubQuery = new ZDBOnlySubQuery(typeof(WhsLocationType), WhsLocationTypeSchema.WLT_Code);
				locationTypeSubQuery.AddToFilter(filter);

				var viewQuery = new ZDBOnlyQuery(typeof(WhsDynamicPickFaceView));
				viewQuery.AddSubQuery(WhsDynamicPickFaceViewSchema.WDP_LocationType, locationTypeSubQuery, JoinCondition.And);

				return viewQuery;
			}
		}

		#endregion

		ModuleGuidFilter AreaFilter => (ModuleGuidFilter)ModuleFilters[Schema.Area];

		WhsAreaCollection AreaCollection;
	}
}
