using System.Drawing;

namespace Enterprise.Freight.Forwarding.GUI
{
	class PacklineTemperatureControlConfiguration : ITemperatureControlConfiguration
	{
		Point ITemperatureControlConfiguration.MinTemperatureControlLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 25, true);
		Point ITemperatureControlConfiguration.SetMinTemperatureButtonLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 50, true);
		Point ITemperatureControlConfiguration.MaxTemperatureControlLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 25, true);
		Point ITemperatureControlConfiguration.SetMaxTemperatureButtonLocation => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 50, true);
		Size ITemperatureControlConfiguration.TotalControlSize => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 70, true);
	}
}
