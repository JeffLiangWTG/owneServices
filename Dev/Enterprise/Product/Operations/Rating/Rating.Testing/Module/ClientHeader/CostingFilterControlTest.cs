using Enterprise.Rating.Business;
using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Testing.Module
{
	sealed class CostingFilterControlTest : BaseRatingHeaderFilterControlTest
	{
		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var rateCollection = new RateCollection(Factory);
			var filterBO = new CostingFilterBusinessObject();
			return new CostingFilterControl(rateCollection, filterBO);
		}
	}
}
