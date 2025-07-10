using System.Drawing;

namespace Enterprise.Freight.Forwarding.GUI
{
	class ConsolTemperatureControlConfiguration : ITemperatureControlConfiguration
	{
		Point ITemperatureControlConfiguration.MinTemperatureControlLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 25, true);
		Point ITemperatureControlConfiguration.SetMinTemperatureButtonLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 25, true);
		Point ITemperatureControlConfiguration.MaxTemperatureControlLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 50, true);
		Point ITemperatureControlConfiguration.SetMaxTemperatureButtonLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 50, true);
		Size ITemperatureControlConfiguration.TotalControlSize => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 70, true);
	}
}
