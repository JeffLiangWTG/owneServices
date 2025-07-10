using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[ModuleID(ModuleId.ZZRefCusRuling)]
	public class ZZRefCusRulingCombinedCollection : ActiveBusinessObjectCollection<ZZRefCusRulingCombined>
	{
		public ZZRefCusRulingCombinedCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(ZZRefCusRulingCombinedSchema.ZZX_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
		{
		}
	}
}
