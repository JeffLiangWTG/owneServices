namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;

	public interface IMAFMeasurement
	{
		ZString MeasurementUQ { get; } // M
		ZDecimal MeasurementValue { get; } // M
	}
}


