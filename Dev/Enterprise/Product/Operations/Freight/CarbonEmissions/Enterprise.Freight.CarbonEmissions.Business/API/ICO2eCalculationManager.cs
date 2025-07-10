using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public interface ICO2eCalculationManager : IDisposable
	{
		CancellationToken Token { get; }

		event EventHandler Cancelled;

		event EventHandler Timeout;

		void Cancel();

		Task<EmissionResult> GetEmissionAsync(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter);

		EmissionResult GetEmission(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter);
	}
}
