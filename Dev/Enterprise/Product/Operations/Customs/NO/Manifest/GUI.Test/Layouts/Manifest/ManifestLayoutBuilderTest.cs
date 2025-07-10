using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(ManifestLayoutBuilder))]
sealed class ManifestLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ManifestLayoutBuilder, AsycudaManifestHeader, CommonManifestControlBag>
{
	protected override ManifestLayoutBuilder GetColumnLayoutBuilderForTesting() => new ();

	protected override int ExpectedMaxColumns => 3;
}
