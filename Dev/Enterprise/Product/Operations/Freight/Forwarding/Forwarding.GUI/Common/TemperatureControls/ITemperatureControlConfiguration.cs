using System.Drawing;
using CargoWise.Windows.UI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public interface ITemperatureControlConfiguration
	{
		[DpiState(DpiState.ScaledVariant)]
		Point MinTemperatureControlLocation { get; }
		[DpiState(DpiState.ScaledVariant)]
		Point SetMinTemperatureButtonLocation { get; }
		[DpiState(DpiState.ScaledVariant)]
		Point MaxTemperatureControlLocation { get; }
		[DpiState(DpiState.ScaledVariant)]
		Point SetMaxTemperatureButtonLocation { get; }
		[DpiState(DpiState.ScaledVariant)]
		Size TotalControlSize { get; }
	}
}
