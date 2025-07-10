using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyProcessTaskNullParentControllerID : DummyProcessTask
	{
		public DummyProcessTaskNullParentControllerID(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			SetParentControllerID(null);
		}
	}
}
