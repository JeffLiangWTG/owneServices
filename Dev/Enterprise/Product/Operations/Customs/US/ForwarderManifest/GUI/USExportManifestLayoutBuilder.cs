using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class USExportManifestLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonManifestControlBag> where T : AsycudaManifestHeader
	{
		public override CommonManifestControlBag CommonBag => CommonManifestControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
