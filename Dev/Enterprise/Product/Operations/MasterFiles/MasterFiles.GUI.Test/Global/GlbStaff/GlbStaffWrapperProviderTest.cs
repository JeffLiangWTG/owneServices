using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestsSubclassesOf(typeof(GlbStaffWrapperProvider))]
	public abstract class GlbStaffWrapperProviderTest<T> : TestCaseWithFactory
			where T : GlbStaffWrapperProvider, new()
	{
		public void TestGetWrapperIsCached()
		{
			var provider = new T();
			var staff = Factory.New<GlbStaff>();
			var wrapper1 = provider.GetWrapper(staff);
			var wrapper2 = provider.GetWrapper(staff);
			AssertEquals("Wrapper should be cached", true, object.ReferenceEquals(wrapper1, wrapper2));
		}
	}
}
