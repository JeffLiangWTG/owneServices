using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	internal class WhsReceiveRatingAdaptersProvider : RatingAdaptersProvider<WhsReceive>
	{
		public WhsReceiveRatingAdaptersProvider(WhsReceive order) : base(order) { }

		protected override List<IAutoRating> GetAdapters(WhsReceive parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new WhsReceiveRatingAdapter(parent) };
		}
	}
}
