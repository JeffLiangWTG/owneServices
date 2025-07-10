using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USFeatureProvider))]
	sealed class USFeatureProviderTest : FeatureProviderAbstractTest<USFeatureProvider>
	{
		public void TestSupportsUSExportAsycudaPacks() => AssertEquals("Should have Pack tab. ", true, new USFeatureProvider().SupportsAsycudaPacks);
	}
}
