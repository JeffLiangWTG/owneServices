using System;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IJobCO2e
	{
		ZString Status { get; set; }
		ZDecimal TotalCO2e { get; set; }
		ZDecimal CO2ePerTonneInKg { get; set; }
		ZDecimal CO2ePerTEUInKg { get; set; }
		ZDecimal DistanceInKM { get; set; }

		event EventHandler StatusChanged;
		event EventHandler JobCO2eOnSaving;
	}
}
