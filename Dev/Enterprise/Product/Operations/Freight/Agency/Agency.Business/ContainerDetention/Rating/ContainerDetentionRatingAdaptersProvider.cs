using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	class ContainerDetentionRatingAdaptersProvider : RatingAdaptersProvider<ContainerDetention>
	{
		public ContainerDetentionRatingAdaptersProvider(ContainerDetention parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(ContainerDetention parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = new List<IAutoRating> { new ContainerDetentionRatingAdapter(parent) };
			result.AddRange(parent.Movements.Select(x => new ContainerDetentionMovementRatingAdapter(x)));
			return result;
		}
	}
}
