using System.Windows.Forms;

namespace Enterprise.MarketingManager.GUI
{
	internal interface IImageDialog
	{
		ImageElementWithMacro Element { get; set; }

		bool IsLocalResourceSelectionDisabled { get; set; }

		DialogResult ShowDialog();
	}
}
