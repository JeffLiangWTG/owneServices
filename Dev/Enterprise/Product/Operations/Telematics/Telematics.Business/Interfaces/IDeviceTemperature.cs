using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceTemperature
	{
		ZDateTime GDT_MeasurementTimeUtc { get; set; }
		ZDecimal GDT_TemperatureC { get; set; }
	}
}
