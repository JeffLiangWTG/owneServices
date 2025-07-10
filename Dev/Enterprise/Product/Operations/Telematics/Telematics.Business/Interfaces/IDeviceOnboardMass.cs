using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceOnboardMass
	{
		ZDateTime GDM_MeasurementTimeUtc { get; set; }
		ZDecimal GDM_PressureKPa { get; set; }
		ZDecimal GDM_DeviationPercent { get; set; }
	}
}
