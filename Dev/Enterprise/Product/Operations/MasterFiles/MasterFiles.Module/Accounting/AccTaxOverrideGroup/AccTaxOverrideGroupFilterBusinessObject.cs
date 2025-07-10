using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccTaxOverrideGroupFilterBusinessObject : FilterStripBusinessObject
	{
		public AccTaxOverrideGroupFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddGuidFilters(filters);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", AccTaxOverrideGroupSchema.AX_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccTaxOverrideGroup|Code", "Code");
			filters.AddTextFilter("Description", AccTaxOverrideGroupSchema.AX_Description).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccTaxOverrideGroup|Description", "Description");
		}

		#endregion

		#region Guid Filter
		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var chargeCodeFilter = filters.AddGuidFilter("Linked Charge Code", ModuleIDs.AccChargeCode, GetChargeCodeFilterQuery, ChargeCodeCollection, AccTaxOverrideGroupSchema.PK);
			chargeCodeFilter.Category = FilterCategories.ChargeCode;
			chargeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("1DCB9C87-F805-4237-8F7E-A648FD8214FC", "Linked Charge Code");
			chargeCodeFilter.SupportsBlankComparisonOperators = false;
			chargeCodeFilter.SupportsFiltersMatchComparisonOperator = true;
		}

		ZQuery GetChargeCodeFilterQuery(ZDBOnlySubQuery filterQuery, SQLComparisonOperator comparisonOperator, object value)
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery selectAccTaxOverrideGroupQuery = new ZDBOnlyQuery(typeof(AccTaxOverrideGroup));
			ZDBOnlySubQuery selectAccChargeCodeQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK, AccTaxOverrideGroupChargeCodePivotSchema.ACP_AC_ChargeCode);
			if (value != null && comparisonOperator != SQLComparisonOperator.NotSpecified)
			{
				var notIn = comparisonOperator == SQLComparisonOperator.NotEqual;
				var selectAccTaxOverrideGroupChargeCodePivotQuery = new ZDBOnlySubQuery(typeof(AccTaxOverrideGroupChargeCodePivot), AccTaxOverrideGroupChargeCodePivotSchema.ACP_AX_TaxOverrideGroup, notIn);
				selectAccChargeCodeQuery.AddToFilter(AccChargeCodeSchema.PK, value);
				selectAccTaxOverrideGroupChargeCodePivotQuery.AddSubQuery(selectAccChargeCodeQuery, JoinCondition.And);

				selectAccTaxOverrideGroupQuery.AddSubQuery(selectAccTaxOverrideGroupChargeCodePivotQuery, JoinCondition.And);

				ZDBOnlySubQuery selectVATAccChargeCodeQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.AC_AX_TaxOverrideGroup, notIn);
				selectVATAccChargeCodeQuery.AddToFilter(AccChargeCodeSchema.PK, value);
				selectAccTaxOverrideGroupQuery.AddSubQuery(selectVATAccChargeCodeQuery, notIn ? JoinCondition.And : JoinCondition.Or);

				query.AddToFilter(selectAccTaxOverrideGroupQuery, JoinCondition.And);
			}
			else if (filterQuery != null)
			{
				var selectAccTaxOverrideGroupChargeCodePivotQuery = new ZDBOnlySubQuery(typeof(AccTaxOverrideGroupChargeCodePivot), AccTaxOverrideGroupChargeCodePivotSchema.ACP_AX_TaxOverrideGroup);
				selectAccChargeCodeQuery.AddSubQuery(AccChargeCodeSchema.PK, AccChargeCodeSchema.PK, filterQuery, JoinCondition.And);
				selectAccTaxOverrideGroupChargeCodePivotQuery.AddSubQuery(selectAccChargeCodeQuery, JoinCondition.And);
				selectAccTaxOverrideGroupQuery.AddSubQuery(selectAccTaxOverrideGroupChargeCodePivotQuery, JoinCondition.And);

				ZDBOnlySubQuery selectVATAccChargeCodeQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.AC_AX_TaxOverrideGroup, AccTaxOverrideGroupSchema.PK);
				selectVATAccChargeCodeQuery.AddSubQuery(AccChargeCodeSchema.PK, AccChargeCodeSchema.PK, filterQuery, JoinCondition.And);
				selectAccTaxOverrideGroupQuery.AddSubQuery(selectVATAccChargeCodeQuery, JoinCondition.Or);

				query.AddToFilter(selectAccTaxOverrideGroupQuery, JoinCondition.And);
			}
			return query;
		}

		AccChargeCodeCollection ChargeCodeCollection
		{
			get { return chargeCodeCollection ?? (chargeCodeCollection = new AccChargeCodeCollection(Factory)); }
		}
		AccChargeCodeCollection chargeCodeCollection;

		#endregion

		#endregion
	}
}
