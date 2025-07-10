using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.VN.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		/// <summary>
		/// Test supported Ports in the Customs Manifest Header object
		/// </summary>
		public void TestSupportsCustomsPorts()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("VN Manifests supports CustomPorts", true, new FeatureProvider().SupportsCustomsPorts(header));
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("VN Manifests supports CustomPorts", false,
				new FeatureProvider().SupportsCustomsPorts(header));
		}

		public void TestSupportAsycudaPacks()
		{
			Assert(new FeatureProvider().SupportsAsycudaPacks);
		}
	}
}
