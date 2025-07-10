using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Module
{
	public class ClassificationFilterStripBusinessObject : Customs.Module.CusClassificationFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			fActiveFilter = result.AddFlagsFilter("Flags", new string[] { "Active" }, new SchemaBoolColumn[] { CusClassificationSchema.CC_IsActive });
			fActiveFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusClassificationFilter|Flags", "Flags");
			return result;
		}

		protected ModuleFlagsFilter fActiveFilter;
	}
}
