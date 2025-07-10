using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Module
{
	public class ValidationRuleFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var filter = new ModuleFountainFilter(UniversalValidationRuleSetSchema.Constants.VRS_Code, UniversalValidationRuleSetSchema.VRS_Code, "R");
			filter.MultilingualDescription = ResString.GetMultilingualString("ValidationRule|VRS_Code", "Code");
			filters.AddFilter(filter);

			filters.AddTextFilter(UniversalValidationRuleSetSchema.Constants.VRS_Name, UniversalValidationRuleSetSchema.VRS_Name)
				.MultilingualDescription = ResString.GetMultilingualString("ValidationRule|VRS_Name", "Name");

			filters.AddTextFilter(UniversalValidationRuleSetSchema.Constants.VRS_Description, UniversalValidationRuleSetSchema.VRS_Description)
				.MultilingualDescription = ResString.GetMultilingualString("ValidationRule|VRS_Description", "Description");

			filters.AddTextFilter(UniversalValidationRuleSetSchema.Constants.VRS_DataContext, UniversalValidationRuleSetSchema.VRS_DataContext)
				.MultilingualDescription = ResString.GetMultilingualString("ValidationRule|VRS_DataContext", "Data Context");

			filters.AddTextFilter(UniversalValidationRuleSetSchema.Constants.VRS_Criteria, UniversalValidationRuleSetSchema.VRS_Criteria)
				.MultilingualDescription = ResString.GetMultilingualString("ValidationRule|VRS_Criteria", "Criteria");

			return filters;
		}
	}
}
