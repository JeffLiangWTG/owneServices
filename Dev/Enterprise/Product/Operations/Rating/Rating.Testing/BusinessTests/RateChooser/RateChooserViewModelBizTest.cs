using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Rating.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	[TestedType(typeof(RateChooserViewModel))]
	public class RateChooserViewModelBizTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var chooserHelper = new RateChooserTestHelper(Factory);
			var consol = chooserHelper.CreateConsol();
			chooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);
			chooserHelper.AddContainer(consol, "40GP", "GEN", Core.Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);
			return viewModel;
		}
	}
}
