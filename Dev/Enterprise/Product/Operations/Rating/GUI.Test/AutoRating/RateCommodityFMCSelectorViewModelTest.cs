using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Rating.Business.RateCommodityFMCPairProvider;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(RateCommodityFMCSelectorViewModel))]
	public class RateCommodityFMCSelectorViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
			=> new RateCommodityFMCSelectorViewModel();
	}

	[TestedType(typeof(RateCommodityFMCViewModel))]
	public class CompanyTariffRateViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
			=> new RateCommodityFMCViewModel("GEN", "1234", "0000", "Potato", RateSources.Code.CompanyTariff);
	}

	[TestedType(typeof(RateCommodityFMCViewModels))]
	public class RateCommodityFMCViewModelsTest : NonPersistentBusinessObjectCollectionTestCase<RateCommodityFMCViewModels>
	{
		protected override RateCommodityFMCViewModels GetCollectionToTest()
		{
			return new RateCommodityFMCViewModels();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RateCommodityFMCViewModel("GEN", "1234", "0000", "Potato", RateSources.Code.CompanyTariff);
		}
	}
}
