using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LicensingMessageFunctionalReferenceIDFountainTest : TestCaseWithFactory
	{
		[TestDate(2022, 12, 29)]
		[ExpectNoExceptions]
		public void TestGetNext()
		{
			var vatNumber = "96944490";
			NUnit.Framework.Assert.That(LicensingMessageFunctionalReferenceIDFountain.GetNext(Factory, vatNumber), NUnit.Framework.Is.EqualTo("96944490002212290001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(LicensingMessageFunctionalReferenceIDFountain.GetNext(Factory, vatNumber, true), NUnit.Framework.Is.EqualTo("96944490SW2212290001").Using(CustomComparers.TypeComparison));
		}
	}
}
