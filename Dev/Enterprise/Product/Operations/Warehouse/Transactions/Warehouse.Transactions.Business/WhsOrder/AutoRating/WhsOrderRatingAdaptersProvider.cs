using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	internal class WhsOrderRatingAdaptersProvider : RatingAdaptersProvider<WhsOrder>
	{
		public WhsOrderRatingAdaptersProvider(WhsOrder order) : base(order) { }

		protected override List<IAutoRating> GetAdapters(WhsOrder parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new WhsOrderRatingAdapter(parent) };
		}
	}
}
