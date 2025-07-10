using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummySubClassWithServices : DummyWithServices
	{
		public DummySubClassWithServices(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
