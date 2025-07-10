using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class DV1DetailsLayoutBuilder<T> : EU.GUI.DV1DetailsLayoutBuilder<T> where T : JobDeclaration
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override int MaxColumns => 3;
}
