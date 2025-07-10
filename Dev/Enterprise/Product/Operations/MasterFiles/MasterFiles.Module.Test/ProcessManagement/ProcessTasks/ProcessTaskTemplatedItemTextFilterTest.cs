using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessTaskTemplatedItemTextFilter))]
	sealed class ProcessTaskTemplatedItemTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProcessTaskTemplatedItemTextFilter(new ProcessTaskFilterBusinessObject());
		}
	}
}
