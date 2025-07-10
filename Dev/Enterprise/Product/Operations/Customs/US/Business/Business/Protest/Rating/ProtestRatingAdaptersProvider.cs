using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Protest
{
	class ProtestRatingAdaptersProvider : RatingAdaptersProvider<Protest>
	{
		public ProtestRatingAdaptersProvider(Protest parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(Protest parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new ProtestRatingAdapter(parent) };
		}
	}
}
