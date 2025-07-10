using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWSequenceformatterWithFirstCharLimitationForBTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFormattedInt()
		{
			var formatter = new TWSequenceformatterWithFirstCharLimitationForB();
			NUnit.Framework.Assert.That(formatter.FormatIntToString(1), NUnit.Framework.Is.EqualTo("0001"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(9999), NUnit.Framework.Is.EqualTo("9999"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(10000), NUnit.Framework.Is.EqualTo("A001"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(17991), NUnit.Framework.Is.EqualTo("H999"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(17992), NUnit.Framework.Is.EqualTo("J001"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(19989), NUnit.Framework.Is.EqualTo("K999"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(19990), NUnit.Framework.Is.EqualTo("M001"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(21987), NUnit.Framework.Is.EqualTo("N999"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(21988), NUnit.Framework.Is.EqualTo("P001"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(32976), NUnit.Framework.Is.EqualTo("Z999"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(32977), NUnit.Framework.Is.EqualTo("AA01"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(92178), NUnit.Framework.Is.EqualTo("ZZ99"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(92179), NUnit.Framework.Is.EqualTo("AAA1"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(232110), NUnit.Framework.Is.EqualTo("ZZZ9"));
			NUnit.Framework.Assert.That(formatter.FormatStringToInt("0001"), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(formatter.FormatStringToInt("9999"), NUnit.Framework.Is.EqualTo(9999));
			NUnit.Framework.Assert.That(formatter.FormatStringToInt("A001"), NUnit.Framework.Is.EqualTo(10000));
			NUnit.Framework.Assert.That(formatter.FormatStringToInt("K999"), NUnit.Framework.Is.EqualTo(19989));
			NUnit.Framework.Assert.That(formatter.FormatStringToInt("N999"), NUnit.Framework.Is.EqualTo(21987));
			NUnit.Framework.Assert.That(formatter.FormatStringToInt("ZZ99"), NUnit.Framework.Is.EqualTo(92178));
			NUnit.Framework.Assert.That(formatter.FormatStringToInt("ZZZ9"), NUnit.Framework.Is.EqualTo(232110));
		}

		[ExpectNoExceptions]
		public void TestDoesEntryNumberFallIntoThisCategory()
		{
			var formatter = new TWSequenceformatterWithFirstCharLimitationForB();
			NUnit.Framework.Assert.That(formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XAL001"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XAO001"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XAI001"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XA1001"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XAA001"), NUnit.Framework.Is.True);
		}
	}
}
