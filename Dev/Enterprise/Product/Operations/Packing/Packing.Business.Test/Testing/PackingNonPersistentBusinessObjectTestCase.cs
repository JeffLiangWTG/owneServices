#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Packing.Business.Testing
{
	public abstract class PackingNonPersistentBusinessObjectTestCase<TNPBusinessObject> : NonPersistentBusinessObjectTestCase
		where TNPBusinessObject : NonPersistentBusinessObject
	{
		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

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
