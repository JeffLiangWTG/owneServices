using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	[TestedType(typeof(USExportManifestLayoutBuilder<USExportAsycudaManifestHeader>))]
	class USExportManifestLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<USExportManifestLayoutBuilder<USExportAsycudaManifestHeader>, USExportAsycudaManifestHeader, CommonManifestControlBag>
	{
		protected override USExportManifestLayoutBuilder<USExportAsycudaManifestHeader> GetColumnLayoutBuilderForTesting() => new USExportManifestLayoutBuilder<USExportAsycudaManifestHeader>();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
