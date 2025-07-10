using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceExternalVoltage
	{
		ZDateTime GDV_MeasurementTimeUtc { get; set; }
		ZDecimal GDV_Voltage { get; set; }
	}
}
