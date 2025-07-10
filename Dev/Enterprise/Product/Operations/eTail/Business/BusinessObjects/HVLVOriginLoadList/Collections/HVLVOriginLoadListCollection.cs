using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Business
{
	[ModuleID(ModuleId.HVLVOriginLoadList)]
	public class HVLVOriginLoadListCollection : ActiveBusinessObjectCollection<HVLVOriginLoadList>
	{
		public HVLVOriginLoadListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public HVLVOriginLoadListCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
