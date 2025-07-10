using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class FoodWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFoodWrapper()
		{
			List<FoodConstituentWrapper> constituents = new List<FoodConstituentWrapper> { new FoodConstituentWrapper("name1", 444m), new FoodConstituentWrapper("name2", 555m), };
			IFood foodWrapper = new FoodWrapper(123m, 1234m, constituents);
			NUnit.Framework.Assert.That(foodWrapper.PHValueNumeric, NUnit.Framework.Is.EqualTo(123m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(foodWrapper.SterilizationValueNumeric, NUnit.Framework.Is.EqualTo(1234m).Using(CustomComparers.TypeComparison));
			var foodWarpperConstituents = foodWrapper.Constituents;
			NUnit.Framework.Assert.That(foodWarpperConstituents.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(foodWarpperConstituents.First().ElementName, NUnit.Framework.Is.EqualTo("name1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(foodWarpperConstituents.First().ElementPercentNumeric, NUnit.Framework.Is.EqualTo(444m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(foodWarpperConstituents.ElementAt(1).ElementName, NUnit.Framework.Is.EqualTo("name2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(foodWarpperConstituents.ElementAt(1).ElementPercentNumeric, NUnit.Framework.Is.EqualTo(555m).Using(CustomComparers.TypeComparison));
		}
	}
}
