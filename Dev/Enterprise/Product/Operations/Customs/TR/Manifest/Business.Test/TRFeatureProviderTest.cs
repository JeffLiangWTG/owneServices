using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class TRFeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsCustomsPorts()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TR manifests supports CustomsPorts", true, new FeatureProvider().SupportsCustomsPorts(header));
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TR manifests supports CustomsPorts", false, new FeatureProvider().SupportsCustomsPorts(header));
		}

		public void TestSupportsAsycudaPacks() => AssertEquals("Should have Pack tab. ", true, new FeatureProvider().SupportsAsycudaPacks);
	}
}
