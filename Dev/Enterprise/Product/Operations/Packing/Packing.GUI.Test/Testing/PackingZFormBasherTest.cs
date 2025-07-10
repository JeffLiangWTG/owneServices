#if DEBUG

using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Packing.GUI.Testing
{
	public abstract class PackingZFormBasherTest : ZFormBasherTest
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

		TestDataForPacking data;
	}
}

#endif
