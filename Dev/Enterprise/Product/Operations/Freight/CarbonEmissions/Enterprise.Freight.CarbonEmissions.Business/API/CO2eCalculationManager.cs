using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class CO2eCalculationManager : ICO2eCalculationManager
	{
		readonly ICO2eApiClient client;

		CancellationTokenSource tokenSource;

		public virtual CancellationToken Token => tokenSource?.Token ?? CancellationToken.None;

		public event EventHandler Cancelled;
		public event EventHandler Timeout;

		public CO2eCalculationManager(ICO2eApiClient client)
		{
			this.client = client;
		}

		protected virtual void OnCancel()
		{
			Cancelled?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnTimeout()
		{
			Timeout?.Invoke(this, EventArgs.Empty);
		}

		public void Cancel()
		{
			tokenSource?.Cancel();
		}

		public async Task<EmissionResult> GetEmissionAsync(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter)
		{
			try
			{
				tokenSource = new CancellationTokenSource();
				return await client.GetEmissionAsync(bizo, Token, hostSupporter);
			}
			catch (TaskCanceledException) when (Token.IsCancellationRequested)
			{
				OnCancel();
				return new EmissionResult();
			}
			catch (TaskCanceledException)
			{
				OnTimeout();
				return new EmissionResult();
			}
			catch (Exception ex)
			{
				var er = new EmissionResult();
				er.Error = ex;
				return er;
			}
		}

		public EmissionResult GetEmission(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter)
		{
			try
			{
				return client.GetEmission(bizo, hostSupporter);
			}
			catch (TaskCanceledException)
			{
				OnTimeout();
				return new EmissionResult();
			}
			catch (Exception ex)
			{
				var er = new EmissionResult { Error = ex };
				return er;
			}
		}

		public void Dispose()
		{
			tokenSource?.Dispose();
			client?.Dispose();
		}
	}
}
