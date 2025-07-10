using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class ImportAdditionalDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, ImportAdditionalDetailsControlBag> where T : BaseMessageSendingObject
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	public override ImportAdditionalDetailsControlBag CommonBag => ImportAdditionalDetailsControlBag.Instance;

	protected override int MaxColumns => 1;
}
