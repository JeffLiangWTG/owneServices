using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	internal class WhsAdHocServiceJobRatingAdaptersProvider : RatingAdaptersProvider<WhsAdHocServiceJob>
	{
		public WhsAdHocServiceJobRatingAdaptersProvider(WhsAdHocServiceJob adHocServiceJob)
			: base(adHocServiceJob)
		{
		}

		protected override List<IAutoRating> GetAdapters(WhsAdHocServiceJob parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new WhsAdHocServiceJobRatingAdapter(parent) };
		}
	}
}
