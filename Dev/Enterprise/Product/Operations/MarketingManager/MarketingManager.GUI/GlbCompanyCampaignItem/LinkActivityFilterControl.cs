using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MarketingManager.GUI
{
	public partial class LinkActivityFilterControl : ZDateRangeControl
	{
		public LinkActivityFilterControl(ZFilterStrip parentStrip)
			: base(parentStrip)
		{
			InitializeComponent();

			ControlDpiScalingHelper.SetWidth(ContextURLDropEdit.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
		}
	}
}
