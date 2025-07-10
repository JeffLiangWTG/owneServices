using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWManifestLayoutBuilder))]
	sealed class TWManifestLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TWManifestLayoutBuilder, AsycudaManifestHeader, CommonManifestControlBag>
	{
		protected override TWManifestLayoutBuilder GetColumnLayoutBuilderForTesting() => new TWManifestLayoutBuilder();

		protected override int ExpectedMaxColumns => 3;
	}
}
