using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefNMFC)]
	public class RefNMFCCollection : ActiveBusinessObjectCollection<RefNMFC>, Integration.IRefNMFCCollection
	{
		public RefNMFCCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefNMFCCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
