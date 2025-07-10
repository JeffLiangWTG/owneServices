using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public sealed class DummyWithWorkflowCollection : BusinessObjectCollection<DummyWithWorkflow>
	{
		public DummyWithWorkflowCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
