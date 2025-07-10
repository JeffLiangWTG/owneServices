using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class AdditionalInfosUserControlLayoutBuilder : AdditionalInformationDetailsLayoutBuilder
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	protected override int MaxColumns => 2;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		var controlBag = AdditionalInformationDetailsControlBag.Instance;

		SetVisibility(controlBag.KindDropEdit, x => !IsImport(x));
		SetVisibility(controlBag.ReferenceTextBox, x => !IsImport(x));
	}

	static bool IsImport(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo x)
	{
		return x is AdditionalInfo additionalInfo && additionalInfo.IsImport();
	}
}
