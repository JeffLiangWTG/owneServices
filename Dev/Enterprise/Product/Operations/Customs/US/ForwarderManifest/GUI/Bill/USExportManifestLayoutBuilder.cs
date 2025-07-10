using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class USExportManifestBillLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonBillControlBag> where T : AsycudaBill
	{
		public override CommonBillControlBag CommonBag => CommonBillControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
