using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWManifestLayoutBuilder<AsycudaManifestHeader>))]
	sealed class TWManifestLayoutBuilderTest : ASYCUDA.GUI.Testing.ManifestLayoutBuilderAbstractTest<TWManifestLayoutBuilder<AsycudaManifestHeader>, AsycudaManifestHeader>
	{
		protected override TWManifestLayoutBuilder<AsycudaManifestHeader> GetColumnLayoutBuilderForTesting() => new TWManifestLayoutBuilder<AsycudaManifestHeader>();

		protected override int ExpectedMaxColumns => 3;
	}
}
