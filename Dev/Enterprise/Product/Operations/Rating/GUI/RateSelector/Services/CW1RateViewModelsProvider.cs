using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.GUI.RateSelector.Services
{
	#region SuppressResourceStringsCheckRegion

	public class CW1RateViewModelsProvider : IRateViewModelsProvider
	{
		public CW1RateViewModelsProvider(IRatingContext ratingContext, MemoryLogger logger = null)
		{
			RatingContext = ratingContext;
			Logger = RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.Value ? logger : null;
		}

		public RateProviderType ProviderType => RateProviderType.CW1;

		public Task<RateViewModelsProviderResult> GetRatesAsync(RateSelectorFilterStripBusinessObject filter, CancellationToken cancellationToken = default)
		{
			var stopwatch = Stopwatch.StartNew();

			try
			{
				// The slow work happens in find and calculate/merge/combine rates. 
				var (rates, effectiveDateDisposer) = new CW1RateProviderForRSL(RatingContext, Logger).FindRates(filter);

				try
				{
					var factory = new ReadOnlyBusinessObjectFactory();
					var context = new RateSelectorContext
					{
						Factory = factory,
						Filters = filter,
						Logger = new MemoryLogger(),
						CurrencyConverter = new RefCurrenciesCurrencyConverter(factory),
					};

					var ratesViewModel = rates
						.Where(result => result.Lines.Any())
						.Select(result => new CW1RateViewModel(context, result.Criteria, result.ContainerTypePk,
							result.CommodityCode, result.Lines, effectiveDateDisposer))
						.Cast<RateViewModel>()
						.ToList();

					var result = new RateViewModelsProviderResult
					{
						Rates = ratesViewModel,
						ProviderType = ProviderType,
						ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
					};

					return Task.FromResult(result);
				}
				catch
				{
					// We Could not get effectiveDateDisposer outside if an exception occurs, so call disposer here
					effectiveDateDisposer?.Dispose();
					throw;
				}
			}
			finally
			{
				stopwatch.Stop();
			}
		}

		public bool IsApplicable(RateSelectorFilterStripBusinessObject filter)
		{
			// When CG reference is set, the user expects only to see CG results.
			// So CW1 will return no rates
			return filter.CGReferences.IsNullOrEmpty();
		}

		IRatingContext RatingContext { get; }
		MemoryLogger Logger { get; }
	}

	#endregion
}
