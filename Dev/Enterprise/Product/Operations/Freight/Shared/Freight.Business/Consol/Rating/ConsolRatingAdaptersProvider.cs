using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class ConsolRatingAdaptersProvider<T> : RatingOrCostingAdaptersProvider<T>
		where T : CommonConsol
	{
		public ConsolRatingAdaptersProvider(T parent) : base(parent, parent.ConsumerType) { }

		protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var ratingRoutes = parent.GetRatingRoutes(options.AutoratingProcess);
			if (ratingRoutes.Any())
			{
				return ratingRoutes.Select(x => new ConsolRatingAdapter<T>(x)).Cast<IAutoRating>().ToList();
			}

			return new List<IAutoRating> { parent.RatingAdapter };
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(T parent)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(parent));
			result.AddRange(parent.Shipments.Cast<IJobInvoicingPlugIn>());
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}
	}
}
