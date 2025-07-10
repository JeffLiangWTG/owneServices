using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	class CFSContainerRatingAdaptersProvider : RatingAdaptersProvider<CFSContainer>
	{
		public CFSContainerRatingAdaptersProvider(CFSContainer parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(CFSContainer parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new CFSContainerRatingAdapter(parent) };
		}
	}
}
