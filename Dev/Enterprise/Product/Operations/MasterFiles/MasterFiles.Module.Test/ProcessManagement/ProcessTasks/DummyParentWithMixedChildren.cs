using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public sealed class DummyParentWithMixedChildren : DummyBusinessObject
	{
		public DummyParentWithMixedChildren(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public DummyBusinessObjectCollection DummiesWithAndOrWithoutWorkflow => dummiesWithAndOrWithoutWorkflow ?? (dummiesWithAndOrWithoutWorkflow = new DummyBusinessObjectCollection(Factory));
		DummyBusinessObjectCollection dummiesWithAndOrWithoutWorkflow;
	}
}
