using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceTyreAlert
	{
		ZDateTime GDA_MeasurementTimeUtc { get; set; }
		ZByte GDA_OperationMode { get; set; }
		ZBool GDA_IsADCOverflow { get; set; }
		ZBool GDA_IsLowBatteryVoltage { get; set; }
		ZByte GDA_SpecificHardwareFault { get; set; }
		ZDecimal GDA_PressureKPa { get; set; }
		ZDecimal GDA_TemperatureC { get; set; }
	}
}
