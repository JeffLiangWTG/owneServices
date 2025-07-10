using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.CusTempStorage;

public partial class CusTempStorageDecUserControl : ZUserControl
{
	public CusTempStorageDecUserControl()
	{
		InitializeComponent();
		LabelCaptionRenderProvider.SetLabelCaptionVisible(GuaranteeDescriptionTextBox, false);
	}
}
