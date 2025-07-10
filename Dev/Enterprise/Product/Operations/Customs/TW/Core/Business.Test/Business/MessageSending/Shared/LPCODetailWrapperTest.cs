using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LPCODetailWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLPCODetailWrapper()
		{
			var doc = new LPCODetailWrapper("5555", "6666", "7777");
			NUnit.Framework.Assert.That(doc.LPCOExemptionCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(doc.LPCOID, NUnit.Framework.Is.EqualTo("5555").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(doc.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("6666").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(doc.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("7777").Using(CustomComparers.TypeComparison));
		}
	}
}
