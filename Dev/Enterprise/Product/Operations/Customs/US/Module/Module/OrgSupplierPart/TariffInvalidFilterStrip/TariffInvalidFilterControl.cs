using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public partial class TariffInvalidFilterControl : ZUserControl
	{
		public TariffInvalidFilterControl()
		{
			InitializeComponent();
		}

		public static TariffInvalidFilterControl New()
		{
			var result = new TariffInvalidFilterControl();
			ControlDpiScalingHelper.SetHeight(ref result, result.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			return result;
		}
	}
}
