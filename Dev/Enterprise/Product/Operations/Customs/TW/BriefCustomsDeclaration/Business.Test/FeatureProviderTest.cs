using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsAsycudaPacks()
		{
			AssertEquals("SupportsAsycudaPacks should be true", true, new FeatureProvider().SupportsAsycudaPacks);
		}

		public void TestSupportsCustomsPorts()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(true, new FeatureProvider().SupportsCustomsPorts(header));
		}
	}
}
