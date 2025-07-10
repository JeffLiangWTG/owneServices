#if DEBUG

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Packing.Business.Testing
{
	public abstract class PackingBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected PackingTestHelper Helper
		{
			get { return helper ?? (helper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper helper;

		protected TestDataForPacking Data
		{
			get { return data ?? (data = new TestDataForPacking(Factory)); }
		}

		protected NotifyForPacking Notify
		{
			get { return notify ?? (notify = new NotifyForPacking()); }
		}

		TestDataForPacking data;
		NotifyForPacking notify;
	}
}

#endif
