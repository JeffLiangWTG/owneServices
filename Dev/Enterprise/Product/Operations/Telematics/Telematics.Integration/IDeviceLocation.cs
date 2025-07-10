using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceLocation
	{
		ZDateTime MeasurementTimeUtc { get; }
		ZGeography Location { get; }
		ZDecimal Speedkmh { get; }
		ZDecimal CompassHeadingDegrees { get; }
		ZDecimal SpeedLimitKmh { get; }
		ZString SpeedLimitState { get; }
	}
}
