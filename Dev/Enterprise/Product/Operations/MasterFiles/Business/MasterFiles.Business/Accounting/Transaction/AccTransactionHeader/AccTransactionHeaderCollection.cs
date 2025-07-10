using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.ARTransaction)]
	public class AccTransactionHeaderCollection : BusinessObjectCollection<AccTransactionHeader>
	{
		public AccTransactionHeaderCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}
		public AccTransactionHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
