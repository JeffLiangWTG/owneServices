using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceBattery
	{
		ZDateTime GDB_MeasurementTimeUtc { get; set; }
		ZBool GDB_IsCharging { get; set; }
		ZDecimal GDB_Voltage { get; set; }
		ZDecimal GDB_CurrentA { get; set; }
		ZShort GDB_TemperatureC { get; set; }
		ZDecimal GDB_ChargeRemaining { get; set; }
	}
}
