using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskExtraResourceCollection))]
	sealed class ProcessTaskExtraResourceCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			return new ProcessTaskExtraResourceCollection(task);
		}

		public void TestAllowNewIsDrivenByTaskReadOnlyTrue()
		{
			AssertAllowNewIsDrivenByTaskReadOnly(true);
		}

		public void TestAllowNewIsDrivenByTaskReadOnlyFalse()
		{
			AssertAllowNewIsDrivenByTaskReadOnly(false);
		}

		void AssertAllowNewIsDrivenByTaskReadOnly(bool readOnly)
		{
			var task = Factory.New<ProcessTask>();
			task.ReadOnly = readOnly;

			var collection = new ProcessTaskExtraResourceCollection(task);
			AssertEquals(!readOnly, collection.AllowNew);
		}
	}
}
