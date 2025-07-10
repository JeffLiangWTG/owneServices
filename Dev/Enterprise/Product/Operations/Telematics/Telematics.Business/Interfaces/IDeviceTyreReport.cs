using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceTyreReport
	{
		ZDateTime GDR_MeasurementTimeUtc { get; set; }
		ZDecimal GDR_PressureKPa { get; set; }
		ZDecimal GDR_TemperatureC { get; set; }
	}
}
