using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class UNDGCommonDataFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter("Type", UNDGCommonDataSchema.DC_Type, new UNDGCommonDataLookups(null).Types).MultilingualDescription = ResString.GetMultilingualString("b652b2b3-42af-47ba-bc93-991c60a53984", "Type");
			filters.AddTextFilter("Language", UNDGCommonDataSchema.DC_Language, new CodeDescriptionPairList(OLookUpEditType.Language)).MultilingualDescription = ResString.GetMultilingualString("fa81d60d-2272-46b5-8a1a-9da6c6846452", "Language");
			filters.AddTextFilter("Index", UNDGCommonDataSchema.DC_Index).MultilingualDescription = ResString.GetMultilingualString("eb0637da-babf-4bf9-bb55-1166a5a675fc", "Index");
			return filters;
		}

		#endregion
	}
}
