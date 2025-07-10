using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ConstituentWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstituent()
		{
			IConstituent constituent = new ConstituentWrapper("element", "LevelID", "Thickness");
			NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("element").Using(CustomComparers.TypeComparison), "Constituent.ElementDescription should be");
			NUnit.Framework.Assert.That(constituent.LevelID, NUnit.Framework.Is.EqualTo("LevelID").Using(CustomComparers.TypeComparison), "Constituent.LevelID should be");
			NUnit.Framework.Assert.That(constituent.Thickness, NUnit.Framework.Is.EqualTo("Thickness").Using(CustomComparers.TypeComparison), "Constituent.Thickness should be");
		}
	}
}
