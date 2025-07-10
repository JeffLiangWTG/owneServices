using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ApprovalDocumentWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestApprovalDocument()
		{
			var doc = new ApprovalDocumentWrapper("1", "5555", "6666", "7777");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(doc.LPCOExemptionCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(doc.LPCOID, NUnit.Framework.Is.EqualTo("5555").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(doc.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("6666").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(doc.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("7777").Using(CustomComparers.TypeComparison));
			});

			doc = new ApprovalDocumentWrapper("2", "3333", "9898", "53");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(doc.LPCOExemptionCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(doc.LPCOID, NUnit.Framework.Is.EqualTo("3333").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(doc.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("NO9898").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(doc.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			});
		}
	}
}
