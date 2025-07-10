using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsCustomsPorts()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("NO manifests supports CustomsPorts", true, new FeatureProvider().SupportsCustomsPorts(header));
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("NO manifests supports CustomsPorts", false, new FeatureProvider().SupportsCustomsPorts(header));
		}

		public void TestSupportsAsycudaPacksAsycudaBill() => AssertEquals("Should have Pack tab. ", true, new FeatureProvider().SupportsAsycudaPacks);
	}
}
