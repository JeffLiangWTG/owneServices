using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class MessageStatusCodeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsValidCode()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(TWMessageStatusCodeList.IsValidCode("NOT"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "NOT is a valid code.");
				NUnit.Framework.Assert.That(TWMessageStatusCodeList.IsValidCode("UNK"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "UNK is a valid code.");
				NUnit.Framework.Assert.That(TWMessageStatusCodeList.IsValidCode("AWR"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "AWR is a valid code.");
				NUnit.Framework.Assert.That(TWMessageStatusCodeList.IsValidCode("ERR"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ERR is a valid code.");
				NUnit.Framework.Assert.That(TWMessageStatusCodeList.IsValidCode("SNT"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "SNT is a valid code.");
				NUnit.Framework.Assert.That(TWMessageStatusCodeList.IsValidCode("ACK"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ACK is a valid code.");
				NUnit.Framework.Assert.That(!TWMessageStatusCodeList.IsValidCode("AAA"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "AAA isn't a valid code.");
				NUnit.Framework.Assert.That(!TWMessageStatusCodeList.IsValidCode("BBB"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "BBB isn't a valid code.");
			});
		}
	}
}
