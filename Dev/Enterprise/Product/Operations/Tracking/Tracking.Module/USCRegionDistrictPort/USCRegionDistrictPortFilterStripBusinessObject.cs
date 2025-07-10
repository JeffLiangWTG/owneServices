using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Tracking.Module
{
	public class USCRegionDistrictPortFilterStripBusinessObject : FilterStripBusinessObject
	{
		public USCRegionDistrictPortFilterStripBusinessObject()
		{
			this.QueryObjectType = typeof(ZZRefCusCodeListCombined);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Code", ZZRefCusCodeListCombinedSchema.ZZD_Code).MultilingualDescription = ResString.GetMultilingualString("4460c1a5-af39-44b1-9edf-fa6d6b39b5c8", "Code");
			result.AddTextFilter("Name", ZZRefCusCodeListCombinedSchema.ZZD_Description).MultilingualDescription = ResString.GetMultilingualString("c1b71f61-1f72-495d-8473-83d181ac05bb", "Name");
			return result;
		}
	}
}
