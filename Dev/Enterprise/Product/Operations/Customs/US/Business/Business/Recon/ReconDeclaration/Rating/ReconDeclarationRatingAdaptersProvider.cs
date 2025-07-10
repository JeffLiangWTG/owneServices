using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	class ReconDeclarationRatingAdaptersProvider : RatingAdaptersProvider<ReconDeclaration>
	{
		public ReconDeclarationRatingAdaptersProvider(ReconDeclaration parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(ReconDeclaration parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new ReconDeclarationRatingAdapter(parent) };
		}
	}
}
