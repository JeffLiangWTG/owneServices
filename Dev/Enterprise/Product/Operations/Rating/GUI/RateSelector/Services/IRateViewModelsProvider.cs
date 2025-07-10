using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.BrandManager;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.Services
{
	public enum RateProviderType
	{
		CW1,
		RatesService,
		Unknown
	}

	public class RateViewModelsProviderResult : IEnumerable<RateViewModel>
	{
		public IEnumerable<RateViewModel> Rates { get; set; }
		
		public long ElapsedMilliseconds { get; set; }

		public RateProviderType ProviderType { get; set; }

		public IEnumerator<RateViewModel> GetEnumerator() => Rates.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => Rates.GetEnumerator();
	}

	public interface IRateViewModelsProvider
	{
		/// <summary>
		/// Gets the rates for the rate view model provider.
		/// Each rate view model provider is free to execute on its own thread or the main thread, preferrably its own thread.
		///
		/// Whomever implements this function *must* ensure that returned RateViewModel object's factories
		/// have already given their ownership to the main thread. 
		///
		/// If this cannot be done, then consider changing the return type to be a Func that the
		/// RatesViewModel will execute, in the main thread which does:
		/// a) Assign a captured factory instance ownership to the main thread
		/// b) Return the captured RateViewModels
		/// </summary>
		Task<RateViewModelsProviderResult> GetRatesAsync(RateSelectorFilterStripBusinessObject filter, CancellationToken cancellationToken = default);

		bool IsApplicable(RateSelectorFilterStripBusinessObject filter);

		RateProviderType ProviderType { get; }
	}

	public static class IRateViewModelsProviderExtensions
	{
		public static string ProviderName(this IRateViewModelsProvider provider)
		{
			if (provider as CW1RateViewModelsProvider != null)
			{
				return BrandingFactory.Instance.ProductName;
			}
			else if (provider is RateServiceRateViewModelsProvider)
			{
				return Res.GetString("73a23597-813d-4a56-89a3-839248c9f128", "Rates Service");
			}
			else
			{
				return Res.GetString("6fbe0591-f863-4571-85ba-d3a071bda2b8", "unknown provider");
			}
		}
	}
}
