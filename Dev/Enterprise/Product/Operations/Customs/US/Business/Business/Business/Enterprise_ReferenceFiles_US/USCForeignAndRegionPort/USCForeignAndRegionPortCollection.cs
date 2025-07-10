using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.USCForeignAndRegionPort)]
	public class USCForeignAndRegionPortCollection : BusinessObjectCollection<USCForeignAndRegionPort>
	{
		public USCForeignAndRegionPortCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
