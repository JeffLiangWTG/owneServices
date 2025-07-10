using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCCountryFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Code", USCCountrySchema.UC_Code);
			result.AddTextFilter("Currency", USCCountrySchema.UC_CurrencyCode);
			result.AddTextFilter("Name", USCCountrySchema.UC_Name);
			return result;
		}
	}
}
