using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class UNDGSubstanceFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("UN", UNDGSubstanceSchema.DG_UNNO).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|UN", "UN");
			filters.AddTextFilter("IMO Class", UNDGSubstanceSchema.DG_Class).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|IMOClass", "IMO Class");
			filters.AddTextFilter("Variant", UNDGSubstanceSchema.DG_Variant).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|Variant", "Variant");
			filters.AddTextFilter("Variation", UNDGSubstanceSchema.DG_Variation).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|Variation", "Variation");
			filters.AddTextFilter("Proper Shipping Name", UNDGSubstanceSchema.DG_PSN).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|ProperShippingName", "Proper Shipping Name");
			var filter = filters.AddTextFilter("Proper Shipping Name [Translations]", ViewUNDGAttributeSchema.DA_Descriptor);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|ProperShippingNameTranslated", "Proper Shipping Name [Translations]");
			filter.SubGroup = new ProperShippingNameTranslatedSubGroup();
			filters.AddTextFilter("US DOT Name", UNDGSubstanceSchema.DG_UsrUSDOTShippingName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|USDOTName", "US DOT Name");
			filters.AddTextFilter("Code", UNDGSubstanceSchema.DG_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|UNDGSubstanceFilter|Code", "Code");
			filters.AddTextFilter("Standard", UNDGSubstanceSchema.DG_Standard, new UNDGSubstanceLookups(null).StandardTypes).MultilingualDescription = ResString.GetMultilingualString("Masterfiles|UNDGSubstanceFilter|Standard", "Standard");
		}

		class ProperShippingNameTranslatedSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(UNDGSubstance));

				var subQuery = new ZDBOnlySubQuery(typeof(ViewUNDGAttribute), ViewUNDGAttributeSchema.DA_DG);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region IsSystemDefinedDefaultProperty

		protected override string IsSystemDefinedDefaultProperty => DefinedStatusAllCode;

		#endregion
	}
}
