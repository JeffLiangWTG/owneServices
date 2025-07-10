using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationRatingAdaptersProvider : RatingAdaptersProvider<JobDeclaration>
	{
		public JobDeclarationRatingAdaptersProvider(JobDeclaration parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(JobDeclaration parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new JobDeclarationRatingAdapter<JobDeclaration>(parent) };
		}
	}
}
