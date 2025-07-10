using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class PickFacesFilterBusinessObject : PickFaceViewFilterBusinessObject
	{
		#region Schema

		public abstract new class Schema : PickFaceViewFilterBusinessObject.Schema
		{
			public readonly static string UnassignedPickfaces = nameof(UnassignedPickfaces);
			public readonly static string PercentageFull = nameof(PercentageFull);
			public readonly static string PickNumber = nameof(PickNumber);
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filterCollection = base.GetModuleFiltersCore();
			AddUnassignedPickfacesFilter(filterCollection);
			AddPercentageFullFilter(filterCollection);
			AddPickFilter(filterCollection);
			return filterCollection;
		}

		void AddUnassignedPickfacesFilter(ModuleFilterCollection filters)
		{
			GetTextQuery assignmentQuery = value =>
			{
				var query = new ZQuery();
				switch (value)
				{
					case PickFaceStatuses.Codes.Assigned:
						return new ZQuery(WhsPickFaceViewSchema.WPV_WF, SQLComparisonOperator.NotEqual, null);

					case PickFaceStatuses.Codes.Unassigned:
						query = new ZQuery(WhsPickFaceViewSchema.WPV_WF, null);
						query.AddToFilter(WhsPickFaceViewSchema.WPV_TotalQuantity, 0m);
						query.AddToFilter(WhsPickFaceViewSchema.WPV_Incoming, 0m);
						return query;

					case PickFaceStatuses.Codes.UnassignedWithStock:
						var stockFilter = new ZQuery(WhsPickFaceViewSchema.WPV_TotalQuantity, SQLComparisonOperator.GreaterThan, 0m);
						stockFilter.AddToFilter(JoinCondition.Or, WhsPickFaceViewSchema.WPV_Incoming, SQLComparisonOperator.GreaterThan, 0m);

						query = new ZQuery(WhsPickFaceViewSchema.WPV_WF, null);
						query.AddToFilter(stockFilter);
						return query;

					default:
						return query;
				}
			};

			var filter = filters.AddTextFilter(Schema.UnassignedPickfaces, assignmentQuery, new PickFaceStatuses());
			filter.DefaultProperty = PickFaceStatuses.Codes.All;
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("ac9d3000-33ab-4387-b6b8-c2efdcbdc7d2", "Assignment Status");
		}

		protected override ModuleGuidFilter GetNewWarehouseFilter(ModuleFilterCollection filters, string description, WhsWarehouseCollectionWithSecurityCheck collection)
		{
			return filters.AddGuidFilter(description, ModuleIDs.WhsConfigWarehouse, WhsPickFaceViewSchema.WPV_WW_Whs, collection);
		}

		protected override SchemaGuidColumn LocationColumn => WhsPickFaceViewSchema.WPV_WL;

		protected override ModuleGuidFilter GetNewClientFilter(ModuleFilterCollection filters, string description, WarehouseClientCollectionWithSecurityCheck collection)
		{
			return filters.AddGuidFilter(description, ModuleIDs.Organisation, WhsPickFaceViewSchema.WPV_OH, collection);
		}

		protected override SchemaGuidColumn ProductColumn => WhsPickFaceViewSchema.WPV_OP;

		protected override SchemaStringColumn ABCCategoryColumn => WhsPickFaceViewSchema.WPV_ABCCategory;

		void AddPercentageFullFilter(ModuleFilterCollection filters)
		{
			var percentageFullFilter = filters.AddNumberRangeFilter(Schema.PercentageFull, WhsPickFaceViewSchema.WPV_PercentageFull);
			percentageFullFilter.MultilingualDescription = ResString.GetMultilingualString("63ec74c0-b00d-43f8-825f-8b168ddb7b33", "Percentage Full");
		}

		void AddPickFilter(ModuleFilterCollection filters)
		{
			var collection = ObjectFactory.Get<IWhsPickCollection>(nameof(IWhsPickCollection), Factory);

			var filter = filters.AddGuidFilter(Schema.PickNumber, ModuleIDs.WhsPicking, WhsPickSchema.PK, collection);
			filter.MultilingualDescription = ResString.GetMultilingualString("03286063-320a-46d9-b74b-3464ef6c67ac", "Pick Number");
			filter.SubGroup = pickSubGroup ?? new PickFilterSubGroup(filter);
			filter.ComparisonOperator_List.RemoveCode(ModuleGuidFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleGuidFilter.ComparisonConstants.IsNotBlank);
		}

		readonly ModuleFilterSubGroup pickSubGroup;

		class PickFilterSubGroup : ModuleFilterSubGroup
		{
			internal PickFilterSubGroup(ModuleGuidFilter pickGuidFilter)
			{
				PickGuidFilter = pickGuidFilter;
			}

			ModuleGuidFilter PickGuidFilter { get; }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var pickQuerySQL = $@"
WPV_PK IN
(
SELECT pickFacePK
FROM
(
	SELECT
		WF_PK AS pickFacePK,
		WWP_WP AS WP_PK
	FROM
		dbo.WhsPickFace
		JOIN dbo.WhsPickFaceAwaitingReplenishmentView ON WF_OH_Client = WWP_OH_Client AND WF_OP = WWP_OP
		JOIN dbo.WhsLocationView ON WLV_PK = WF_WL AND WLV_WW_Whs = WWP_WW_Whs

	UNION ALL

	SELECT
		WCP_WF_PickFace AS pickFacePK,
		WCP_WP AS WP_PK
	FROM
		dbo.WhsPickFaceCommittedStockView
) AS PickFaces
WHERE
	WP_PK = @PickPK
)
";

				var pickQuery = new ZDBOnlyQuery(typeof(WhsPickFaceView));
				var sqlParams = new ZSqlParameterCollection
				{
					{ "@PickPK", PickGuidFilter.Property, WhsPickSchema.PK }
				};
				pickQuery.AddFilterAndZSQLParameterCollection(pickQuerySQL, sqlParams);

				return pickQuery;
			}
		}
	}
}
