using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLineCollectionCalculator.SubPackLineCollectionView))]
	sealed class SubPackLineCollectionViewTest : BusinessObjectCollectionViewTestCase<PackLineCollectionCalculator.SubPackLineCollectionView>
	{
		protected override PackLineCollectionCalculator.SubPackLineCollectionView GetCollectionToTest()
		{
			var line = new PackLineNonDependentCollection(Factory);
			return new PackLineCollectionCalculator.SubPackLineCollectionView(line);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<PackLine>();
		}
	}
}
