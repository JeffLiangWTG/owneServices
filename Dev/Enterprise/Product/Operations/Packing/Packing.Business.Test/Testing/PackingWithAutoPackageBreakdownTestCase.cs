#if DEBUG

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Packing.Business.Testing
{
	public abstract class PackingWithAutoPackageBreakdownTestCase : PackingTestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithAutoPackageBreakdown);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}
	}
}

#endif
