using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceLog
	{
		ZDateTime GDL_MeasurementTimeUtc { get; set; }
		ZString GDL_MessageString { get; set; }
	}
}
