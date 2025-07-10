using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceOdometer
	{
		ZDateTime GDO_MeasurementTimeUtc { get; set; }
		ZDecimal GDO_OdometerKM { get; set; }
	}
}
