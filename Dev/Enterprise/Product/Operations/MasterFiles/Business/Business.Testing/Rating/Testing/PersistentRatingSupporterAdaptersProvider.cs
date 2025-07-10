using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PersistentRatingSupporterAdaptersProvider : RatingOrCostingAdaptersProvider<PersistentRatingSupporter>
	{
		public PersistentRatingSupporterAdaptersProvider(PersistentRatingSupporter parent, JobInvoicingConsumerType consumerType)
			: base(parent, consumerType) { }

		protected override List<IAutoRating> GetAdapters(PersistentRatingSupporter parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return parent.AdaptersGetter == null ? base.GetAdapters(parent, uiInteractor, options) : parent.AdaptersGetter();
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(PersistentRatingSupporter parent)
		{
			return parent.AdditionalJobsGetter == null ? base.GetAdditionalJobs(parent) : parent.AdditionalJobsGetter();
		}
	}
}
