using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class PkgPackageDataObjectReaderTestCase : TestCaseWithFactoryAndMessagingHelpers
	{
		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory.BOFactory));
		TestDataForPacking data;

		protected NotifyForPacking Notify => notify ?? (notify = new NotifyForPacking());
		NotifyForPacking notify;

		protected PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory.BOFactory));
		PackingTestHelper helper;
	}
}
