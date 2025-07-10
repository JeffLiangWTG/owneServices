using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCForeignAndRegionPortFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Port Code", USCForeignAndRegionPortSchema.US_PortCode);
			result.AddTextFilter("Port Name", USCForeignAndRegionPortSchema.US_PortName);
			result.AddTextFilter("Port Type", USCForeignAndRegionPortSchema.US_PortType);
			return result;
		}
	}
}
