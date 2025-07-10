using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WhsCartonGroupFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string AttachedOrganisations = "AttachedOrganisations"; // Filter description
			public const string Code = "Code"; // Filter description
			public const string Description = "Description"; // Filter description
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddAttachedOrganisationFilters(result);
			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Schema.Code, WhsCartonGroupSchema.WCG_Code).MultilingualDescription = ResString.GetMultilingualString("WhsCartonGroupFilterBusinessObject|Code", "Code");
			filters.AddTextFilter(Schema.Description, WhsCartonGroupSchema.WCG_Description).MultilingualDescription = ResString.GetMultilingualString("WhsCartonGroupFilterBusinessObject|Description", "Description");
		}

		#endregion

		#region AddAttachedOrganisationFilters

		void AddAttachedOrganisationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(Schema.AttachedOrganisations, ModuleIDs.Organisation, GetAttachedOrganisationFilter, new OrgHeaderCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("ef6a5582-c827-43ef-bc27-018e4b56d894", "Attached Organizations");
			filter.SupportsFiltersMatchComparisonOperator = false;
		}

		ZQuery GetAttachedOrganisationFilter(SQLComparisonOperator comparisonOperator, object value)
		{
			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var isNotBlank = comparisonOperator == SpecialComparisonOperator.IsNotBlank;

			var miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_WCG_CartonGroup, notIn || isBlank);

			// When Comparison Operator has the value 'IsNotBlank', the value passed in is null.
			// All we need to check is that the Carton Group has an Attached Org which is achieved above.
			if (!isBlank && !isNotBlank)
			{
				miscServSubQuery.AddToFilter(OrgMiscServSchema.OM_OH, comparisonOperator, value);
			}

			var cartonGroupQuery = new ZDBOnlyQuery(typeof(WhsCartonGroup));
			cartonGroupQuery.AddSubQuery(miscServSubQuery, JoinCondition.And);

			return cartonGroupQuery;
		}

		#endregion

		#endregion
	}
}
