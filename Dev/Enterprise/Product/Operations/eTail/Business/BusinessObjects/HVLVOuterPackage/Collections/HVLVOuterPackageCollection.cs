using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Business
{
	[ModuleID(ModuleId.HVLVOuterPackage)]
	public class HVLVOuterPackageCollection : ActiveBusinessObjectCollection<HVLVOuterPackage>, IHVLVOuterPackageCollection
	{
		public HVLVOuterPackageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		IHVLVOuterPackage IHVLVOuterPackageCollection.this[int i] => this[i];

		public HVLVOuterPackageCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
