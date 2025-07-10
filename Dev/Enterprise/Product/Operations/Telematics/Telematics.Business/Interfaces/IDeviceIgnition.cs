using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceIgnition
	{
		ZDateTime GDI_MeasurementTimeUtc { get; set; }
		ZBool GDI_State { get; set; }
	}
}
