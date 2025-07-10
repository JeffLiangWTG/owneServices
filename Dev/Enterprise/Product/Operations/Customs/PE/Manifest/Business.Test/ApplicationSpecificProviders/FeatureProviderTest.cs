using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsAsycudaPacks() => AssertEquals("Should have Pack tab. ", ZBool.True, new FeatureProvider().SupportsAsycudaPacks);
	}
}
