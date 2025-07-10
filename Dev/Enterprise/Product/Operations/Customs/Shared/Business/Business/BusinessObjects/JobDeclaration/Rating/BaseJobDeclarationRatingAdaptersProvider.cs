using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class BaseJobDeclarationRatingAdaptersProvider : RatingAdaptersProvider<BaseJobDeclaration>
	{
		public BaseJobDeclarationRatingAdaptersProvider(BaseJobDeclaration parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(BaseJobDeclaration parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new BaseJobDeclarationRatingAdapter<BaseJobDeclaration>(parent) };
		}
	}
}
