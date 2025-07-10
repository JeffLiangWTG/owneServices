using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbGroupForPluginWrapperProviderTest : TestCaseWithFactory
	{
		public void TestGetWrapperWithMutex()
		{
			var group = Factory.New<GlbGroup>();
			var provider = new GlbGroupForPluginWrapperProviderForTesting();
			var wrapper = provider.GetWrapperWithMutex(group);
			var provider2 = new GlbGroupForPluginWrapperProviderForTesting();
			var wrapper2 = provider2.GetWrapperWithMutex(group);

			AssertEquals(wrapper2, wrapper);
		}
	}
}
