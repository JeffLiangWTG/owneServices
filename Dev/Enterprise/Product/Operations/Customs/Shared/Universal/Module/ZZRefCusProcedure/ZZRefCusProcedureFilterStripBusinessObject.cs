using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusProcedureFilterStripBusinessObject : FilterStripBusinessObject
	{
		public ZZRefCusProcedureFilterStripBusinessObject() { }

		public ZZRefCusProcedureFilterStripBusinessObject(BusinessObjectFactory factory) : base(factory) { }

		public ZZRefCusProcedureFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}
		ZZRefCusProcedureFilterLookups lookups;

		#region Implementation

		protected ZZRefCusProcedureFilterLookups GetNewLookups()
		{
			return new ZZRefCusProcedureFilterLookups(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			var dataGroupFilter = filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.DataGrouping, RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping);
			dataGroupFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			dataGroupFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|DataGrouping", Constants.ZZRefCusProcedureFilters.DataGrouping);

			var cpcFilter = filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.CPC, GetCPCQuery);
			cpcFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			cpcFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			cpcFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			cpcFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain);
			cpcFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
			cpcFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith);
			cpcFilter.Visibility = FilterVisibility.AlwaysVisible;
			cpcFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|CPC", Constants.ZZRefCusProcedureFilters.CPC);

			filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.Category, RefCusProcedureSchema.ZZ6_Category, Lookups.CategoryCodeList).MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|Category", Constants.ZZRefCusProcedureFilters.Category);

			var procedureCodeFilter = filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.ProcedureCode, RefCusProcedureSchema.ZZ6_ProcedureCode, Lookups.ProcedureCodeList);
			procedureCodeFilter.Visibility = FilterVisibility.AlwaysVisible;
			procedureCodeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|ProcedureCode", Constants.ZZRefCusProcedureFilters.ProcedureCode);

			var previousCodeFilter = filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.PreviousProcedureCode, RefCusProcedureSchema.ZZ6_PreviousProcedureCode, Lookups.PreviousProcedureCodeList);
			previousCodeFilter.Visibility = FilterVisibility.AlwaysVisible;
			previousCodeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|PreviousProcedureCode", Constants.ZZRefCusProcedureFilters.PreviousProcedureCode);

			var concessionFilter = filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.Concession, RefCusProcedureSchema.ZZ6_Concession, Lookups.ConcessionCodeList);
			concessionFilter.Visibility = FilterVisibility.AlwaysVisible;
			concessionFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|Concession", Constants.ZZRefCusProcedureFilters.Concession);

			filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.Description, RefCusProcedureSchema.ZZ6_Description).MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|Description", Constants.ZZRefCusProcedureFilters.Description);
			filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.Group, RefCusProcedureSchema.ZZ6_Group, Lookups.GroupList).MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|Group", Constants.ZZRefCusProcedureFilters.Group);
			filters.AddTextFilter(Constants.ZZRefCusProcedureFilters.ShipmentType, RefCusProcedureSchema.ZZ6_ShipmentType, Lookups.ShipmentTypeList).MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|ShipmentType", Constants.ZZRefCusProcedureFilters.ShipmentType);

			var effectiveDateFilter = filters.AddSingleDateFilter(Constants.ZZRefCusProcedureFilters.EffectiveDate, delegate(ZDateTime date)
			{
				ZQuery dateQuery = new ZQuery();
				if (date.IsValid)
				{
					dateQuery.AddToFilter(RefCusProcedureSchema.ZZ6_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
					dateQuery.AddToFilter(RefCusProcedureSchema.ZZ6_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				}
				return dateQuery;
			});
			effectiveDateFilter.Property1 = ZDateTime.Today;
			effectiveDateFilter.Category = FilterCategories.Dates;
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|EffectiveDate", Constants.ZZRefCusProcedureFilters.EffectiveDate);

			filters.AddDateFilter(Constants.ZZRefCusProcedureFilters.StartDate, RefCusProcedureSchema.ZZ6_StartDate).MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|StartDate", Constants.ZZRefCusProcedureFilters.StartDate);
			filters.AddDateFilter(Constants.ZZRefCusProcedureFilters.EndDate, RefCusProcedureSchema.ZZ6_EndDate).MultilingualDescription = ResString.GetMultilingualString("ZZRefCusProcedureFilter|EndDate", Constants.ZZRefCusProcedureFilters.EndDate);

			return filters;
		}

		ZQuery GetCPCQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				if (value.Length < 7)
				{
					value = value + "%";
				}

				var cpcQuery = new ZDBOnlyQuery(typeof(RefCusProcedure));
				cpcQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, SQLComparisonOperator.Like, value.SubstringSafe(0, 2));
				if (value.Length > 2)
				{
					cpcQuery.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, SQLComparisonOperator.Like, value.SubstringSafe(2, 2));
					if (value.Length > 4)
					{
						cpcQuery.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, SQLComparisonOperator.Like, value.SubstringSafe(4, 3));
					}
				}

				result.AddToFilter(cpcQuery);
			}

			return result;
		}

		protected override void ApplyInitialCode(ZString code, string propertyName)
		{
			if (propertyName == "FullCodeCurrentPlusPreviousPlusConcession")
			{
				GetCPCQuery(SQLComparisonOperator.StartsWith, code);
				ModuleTextFilter cpcFilter = (ModuleTextFilter)ModuleFilters[Constants.ZZRefCusProcedureFilters.CPC];
				if (cpcFilter != null)
				{
					cpcFilter.Property = code;
				}
			}
			else
			{
				base.ApplyInitialCode(code, propertyName);
			}
		}

		#endregion
	}
}
