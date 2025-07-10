using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class FunctionalReferenceIDNumberFountainTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetNext()
		{
			var entryNumber = "TW123456789";
			NUnit.Framework.Assert.That(FunctionalReferenceIDNumberFountain.GetNext(Factory, entryNumber), NUnit.Framework.Is.EqualTo("TW12345678900001").Using(CustomComparers.TypeComparison));
		}
	}
}
