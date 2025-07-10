using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public interface ICO2eApiClient : IDisposable
	{
		Task<EmissionResult> GetEmissionAsync(BusinessObject bizo, CancellationToken cancellationToken, ICO2eCalculationSupporter hostSupporter);

		EmissionResult GetEmission(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter);
	}
}
