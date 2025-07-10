using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateChartCurrencyTranslationDependentCollection))]
	sealed class AccAlternateChartCurrencyTranslationDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccAlternateChartCurrencyTranslationDependentCollection(Factory.NewWithValidTestData<AccAlternateChart>());
		}
	}
}
