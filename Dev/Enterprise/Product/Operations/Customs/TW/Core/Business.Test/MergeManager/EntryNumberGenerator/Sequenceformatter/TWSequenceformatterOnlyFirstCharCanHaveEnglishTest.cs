using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWSequenceformatterOnlyFirstCharCanHaveEnglishTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFormattedInt()
		{
			var formatter = SequenceformatterOnlyFirstCharCanHaveEnglish;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(formatter.FormatIntToString(1), NUnit.Framework.Is.EqualTo("D01"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(297), NUnit.Framework.Is.EqualTo("F99"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(298), NUnit.Framework.Is.EqualTo("DA1"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(378), NUnit.Framework.Is.EqualTo("FC9"));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("D01"), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("F99"), NUnit.Framework.Is.EqualTo(297));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("DA1"), NUnit.Framework.Is.EqualTo(298));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("FC9"), NUnit.Framework.Is.EqualTo(378));
			});
		}

		[ExpectNoExceptions]
		public void TestDoesEntryNumberFallIntoThisCategory()
		{
			var formatter = SequenceformatterOnlyFirstCharCanHaveEnglish;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812X00010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XA0010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XD0010"), NUnit.Framework.Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestAllowedFormatDescription()
		{
			NUnit.Framework.Assert.That(SequenceformatterOnlyFirstCharCanHaveEnglish.AllowedFormatDescription, NUnit.Framework.Is.EqualTo("The first character only accept English Characters D/E/F, the other character only accept digits and English Characters A/B/C, and last character must is digits, and the length is 3.").Using(CustomComparers.TypeComparison));
		}

		TWSequenceformatterOnlyFirstCharCanHaveEnglish SequenceformatterOnlyFirstCharCanHaveEnglish => sequenceformatterOnlyFirstCharCanHaveEnglish ?? (sequenceformatterOnlyFirstCharCanHaveEnglish = new TWSequenceformatterOnlyFirstCharCanHaveEnglish(378, 3, new List<char> { 'A', 'B', 'C' }, new List<char> { 'D', 'E', 'F' }));
		TWSequenceformatterOnlyFirstCharCanHaveEnglish sequenceformatterOnlyFirstCharCanHaveEnglish;
	}
}
