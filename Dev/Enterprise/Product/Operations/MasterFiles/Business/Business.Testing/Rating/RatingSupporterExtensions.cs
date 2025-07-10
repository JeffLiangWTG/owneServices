using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class RatingSupporterExtensions
	{
		public static bool CanExecuteAutoRating(this IRatingSupporter autoratable)
		{
			RatingAdaptersProvider provider;

			if (autoratable != null && (provider = autoratable.AdaptersProvider) != null)
			{
				using (provider.NewRatingSession())
				{
					return provider.CanExecuteAutoRating(null, AutoRateOptions.AutorateRevenue);
				}
			}

			return false;
		}

		public static IAutoRating GetFirstAdapter(this IRatingSupporter autoratable)
		{
			List<IAutoRating> adapters = GetRatingAdapters(autoratable);
			return adapters != null ? adapters.FirstOrDefault() : null;
		}

		public static List<IAutoRating> GetRatingAdapters(this IRatingSupporter autoratable)
		{
			RatingAdaptersProvider provider;

			if (autoratable != null && (provider = autoratable.AdaptersProvider) != null)
			{
				using (provider.NewRatingSession())
				{
					return provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);
				}
			}

			return null;
		}
	}
}
