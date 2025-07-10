using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class FoodConstituentWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFoodConstituentWrapper()
		{
			IFoodConstituent foodConstituentWrapper = new FoodConstituentWrapper("name", 1234m);
			NUnit.Framework.Assert.That(foodConstituentWrapper.ElementName, NUnit.Framework.Is.EqualTo("name").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(foodConstituentWrapper.ElementPercentNumeric, NUnit.Framework.Is.EqualTo(1234m).Using(CustomComparers.TypeComparison));
		}
	}
}
