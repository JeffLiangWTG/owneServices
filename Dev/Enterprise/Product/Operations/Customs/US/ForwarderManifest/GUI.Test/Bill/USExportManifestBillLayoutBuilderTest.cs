using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	[TestedType(typeof(USExportManifestBillLayoutBuilder<USExportAsycudaBill>))]
	internal class USExportManifestBillLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<USExportManifestBillLayoutBuilder<USExportAsycudaBill>, USExportAsycudaBill, CommonBillControlBag>
	{
		protected override USExportManifestBillLayoutBuilder<USExportAsycudaBill> GetColumnLayoutBuilderForTesting()
		{
			return new USExportManifestBillLayoutBuilder<USExportAsycudaBill>();
		}

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
