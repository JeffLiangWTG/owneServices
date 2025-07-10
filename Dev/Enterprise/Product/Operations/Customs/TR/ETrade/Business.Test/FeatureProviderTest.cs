using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsAsycudaPacks()
			=> AssertEquals("Should have Pack tab. ", true, new FeatureProvider().SupportsAsycudaPacks);
	}
}
