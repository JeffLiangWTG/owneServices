using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1Test : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFormattedInt()
		{
			var formatter = new TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(formatter.FormatIntToString(1), NUnit.Framework.Is.EqualTo("E0001"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(139987), NUnit.Framework.Is.EqualTo("Z0001"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(149985), NUnit.Framework.Is.EqualTo("Z9999"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(149986), NUnit.Framework.Is.EqualTo("EA001"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(539595), NUnit.Framework.Is.EqualTo("ZZ999"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(539596), NUnit.Framework.Is.EqualTo("EAA01"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(1543455), NUnit.Framework.Is.EqualTo("ZZZ99"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(1543456), NUnit.Framework.Is.EqualTo("EAAA1"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(3916215), NUnit.Framework.Is.EqualTo("ZZZZ9"));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("E0001"), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("Z0001"), NUnit.Framework.Is.EqualTo(139987));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("Z9999"), NUnit.Framework.Is.EqualTo(149985));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("EA001"), NUnit.Framework.Is.EqualTo(149986));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("ZZ999"), NUnit.Framework.Is.EqualTo(539595));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("EAA01"), NUnit.Framework.Is.EqualTo(539596));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("ZZZ99"), NUnit.Framework.Is.EqualTo(1543455));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("EAAA1"), NUnit.Framework.Is.EqualTo(1543456));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("ZZZZ9"), NUnit.Framework.Is.EqualTo(3916215));
			});
		}

		[ExpectNoExceptions]
		public void TestDoesEntryNumberFallIntoThisCategory()
		{
			var formatter = new TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1();
			var firstAlphabets = new List<char> { 'E', 'F', 'G', 'H', 'J', 'M', 'P', 'Q', 'R', 'U', 'V', 'W', 'X', 'Y', 'Z' };
			foreach (var str in new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' })
			{
				NUnit.Framework.Assert.That(formatter.DoesEntryNumberFallIntoThisCategory("000000000" + str), NUnit.Framework.Is.EqualTo(!firstAlphabets.Contains(str)));
			}
		}

		[ExpectNoExceptions]
		public void TestAllowedFormatDescription()
		{
			var formatter = new TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1();
			NUnit.Framework.Assert.That(formatter.AllowedFormatDescription, NUnit.Framework.Is.EqualTo("When the Sea Office of Receipt transships to the Air Office of Lading, please enter the Entry number manually. The first alphabet cannot be A, B, C, D, I, K, L, N, O, S, T.").Using(CustomComparers.TypeComparison));
		}
	}
}
