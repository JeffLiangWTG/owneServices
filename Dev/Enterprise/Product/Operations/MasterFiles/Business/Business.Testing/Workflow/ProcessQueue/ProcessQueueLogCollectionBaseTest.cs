using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessQueueLogCollectionBase))]
	sealed class ProcessQueueLogCollectionBaseTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ProcessQueue processQueue = Factory.New<ProcessQueue>();
			return new ProcessQueueLogCollectionBase(processQueue);
		}
	}
}
