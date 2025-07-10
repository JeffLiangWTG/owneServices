using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class SGFeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsCustomsPorts() => AssertEquals("SGAccess supports CustomsPorts", true, new FeatureProvider().SupportsCustomsPorts(Factory.New<AsycudaManifestHeader>()));

		public void TestSupportsAsycudaPacks() => AssertEquals("Should have Pack tab. ", true, new FeatureProvider().SupportsAsycudaPacks);
	}
}
