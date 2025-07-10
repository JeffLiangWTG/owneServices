
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MarketingManager.Module
{
	[SuppressFormDesignerAnalysis]
	public partial class ValueAnalysisQuantityFilterControl : ZNumberRangeControl
	{
		public ValueAnalysisQuantityFilterControl(ZFilterStrip parentStrip)
			: base(parentStrip)
		{
			InitializeComponent();
			ControlDpiScalingHelper.SetLeft(ref Period, parentStrip.FilterControlsBox2Start(Period), true);
		}
	}
}
