using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BaseTWSequenceformatter))]
	sealed class BaseTWSequenceformatterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFormattedInt()
		{
			var formatter = new BaseTWSequenceformatter(129, 2, new List<char> { 'A', 'B', 'C' });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(formatter.FormatIntToString(1), NUnit.Framework.Is.EqualTo("01"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(99), NUnit.Framework.Is.EqualTo("99"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(100), NUnit.Framework.Is.EqualTo("A1"));
				NUnit.Framework.Assert.That(formatter.FormatIntToString(126), NUnit.Framework.Is.EqualTo("C9"));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("01"), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("99"), NUnit.Framework.Is.EqualTo(99));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("A1"), NUnit.Framework.Is.EqualTo(100));
				NUnit.Framework.Assert.That(formatter.FormatStringToInt("C9"), NUnit.Framework.Is.EqualTo(126));
			});
		}

		[ExpectNoExceptions]
		public void TestDoesEntryNumberFallIntoThisCategory()
		{
			var formatter = new BaseTWSequenceformatter(129, 2, new List<char> { 'A', 'B', 'C' });
			NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("A1"), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsSequenceNumberMatchEntryNumber()
		{
			var formatter = new BaseTWSequenceformatter(129, 5, new List<char> { 'A', 'B', 'C' });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(formatter.IsSequenceNumberMatchEntryNumber("BF  13223ABCDE", "ABCDE"), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(formatter.IsSequenceNumberMatchEntryNumber("BF  13223ABCDE", ""), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(formatter.IsSequenceNumberMatchEntryNumber("BF  13223ABCDE", "ABCD"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(formatter.IsSequenceNumberMatchEntryNumber("BF  132230BCDE", "BCDE"), NUnit.Framework.Is.EqualTo(true));
			});
		}

		[ExpectNoExceptions]
		public void TestIntToKeyFormatting()
		{
			var totalSize = 5;
			var alphabet = new char[] { 'E', 'F', 'G', 'H', 'J', 'M', 'P', 'Q', 'R', 'U', 'V', 'W', 'X', 'Y', 'Z' };
			var maximumValue = 1264509;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(100000, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("E0001"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(100001, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("E0002"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(109998, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("E9999"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(109999, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("F0001"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(119997, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("F9999"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(119998, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("G0001"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(1264509, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("ZZZZ9"));
			});

			totalSize = 4;
			alphabet = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
			maximumValue = 32976;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(10000, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("A001"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(10001, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("A002"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(10999, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("B001"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(11000, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("B002"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(11996, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("B998"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(11997, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("B999"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(11998, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("C001"));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatNumberToKey(32976, totalSize, alphabet, maximumValue), NUnit.Framework.Is.EqualTo("Z999"));
			});
		}

		[ExpectNoExceptions]
		public void TestNew()
		{
			var sequenceformatterA = BaseTWSequenceformatter.New(RangeTypeList.Codes.A);
			var sequenceformatterB = BaseTWSequenceformatter.New(RangeTypeList.Codes.B);
			var sequenceformatterC = BaseTWSequenceformatter.New(RangeTypeList.Codes.C);
			var sequenceformatterT = BaseTWSequenceformatter.New(RangeTypeList.Codes.T);
			var sequenceformatterA1 = BaseTWSequenceformatter.New(Constants.RangeTypes.A1);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sequenceformatterA, NUnit.Framework.Is.TypeOf<BaseTWSequenceformatter>());
				NUnit.Framework.Assert.That(sequenceformatterB, NUnit.Framework.Is.TypeOf<TWSequenceformatterWithFirstCharLimitationForB>());
				NUnit.Framework.Assert.That(sequenceformatterC, NUnit.Framework.Is.TypeOf<BaseTWSequenceformatter>());
				NUnit.Framework.Assert.That(sequenceformatterT, NUnit.Framework.Is.TypeOf<BaseTWSequenceformatter>());
				NUnit.Framework.Assert.That(sequenceformatterA1, NUnit.Framework.Is.TypeOf<TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1>());
				NUnit.Framework.Assert.That(sequenceformatterA.MaximumValue, NUnit.Framework.Is.EqualTo(6888105));
				NUnit.Framework.Assert.That(sequenceformatterC.MaximumValue, NUnit.Framework.Is.EqualTo(6888105));
				NUnit.Framework.Assert.That(sequenceformatterT.MaximumValue, NUnit.Framework.Is.EqualTo(6888105));
				NUnit.Framework.Assert.That(sequenceformatterA1.MaximumValue, NUnit.Framework.Is.EqualTo(3916215));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.New("X"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.ITWSequenceformatter)));
				NUnit.Framework.Assert.That(sequenceformatterA.AllowedFormatDescription, NUnit.Framework.Is.EqualTo("The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sequenceformatterB.AllowedFormatDescription, NUnit.Framework.Is.EqualTo("The first character only accept digits and English Characters A/B/C/D/E/F/G/H/J/K/M/N/P/Q/R/S/T/U/V/W/X/Y/Z, the other character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, and last character must is digits, and the length is 4.").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sequenceformatterC.AllowedFormatDescription, NUnit.Framework.Is.EqualTo("The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sequenceformatterT.AllowedFormatDescription, NUnit.Framework.Is.EqualTo("The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sequenceformatterA1.AllowedFormatDescription, NUnit.Framework.Is.EqualTo("When the Sea Office of Receipt transships to the Air Office of Lading, please enter the Entry number manually. The first alphabet cannot be A, B, C, D, I, K, L, N, O, S, T.").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestFormatStringToNumber()
		{
			var totalSize = 5;
			var alphabet = new char[] { 'E', 'F', 'G', 'H', 'J', 'M', 'P', 'Q', 'R', 'U', 'V', 'W', 'X', 'Y', 'Z' };
			var maximumValue = 1264509;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatStringToNumber("E0001", totalSize, null, alphabet, maximumValue), NUnit.Framework.Is.EqualTo(100000));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatStringToNumber("ZZZZ9", totalSize, null, alphabet, maximumValue), NUnit.Framework.Is.EqualTo(1264509));
			});

			totalSize = 4;
			alphabet = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
			maximumValue = 32976;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatStringToNumber("A001", totalSize, null, alphabet, maximumValue), NUnit.Framework.Is.EqualTo(10000));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatStringToNumber("Z999", totalSize, null, alphabet, maximumValue), NUnit.Framework.Is.EqualTo(32976));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatStringToNumber("A001", totalSize, null, alphabet, maximumValue, 1), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatStringToNumber("AZ99", totalSize, new char[] { 'A', 'B', 'C' }, alphabet, maximumValue), NUnit.Framework.Is.EqualTo(15273));
				NUnit.Framework.Assert.That(BaseTWSequenceformatter.FormatStringToNumber("AZ99", totalSize, new char[] { 'A', 'B', 'C' }, alphabet, maximumValue, 1), NUnit.Framework.Is.EqualTo(5274));
			});
		}

		[ExpectNoExceptions]
		public void TestFormatSequenceNumber()
		{
			var formatter = new BaseTWSequenceformatter(129, 5, new List<char> { 'A', 'B', 'C' });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(formatter.FormatSequenceNumber("A"), NUnit.Framework.Is.EqualTo("0000A").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(formatter.FormatSequenceNumber("ABC12"), NUnit.Framework.Is.EqualTo("ABC12").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(formatter.FormatSequenceNumber("ABC123"), NUnit.Framework.Is.EqualTo("BC123").Using(CustomComparers.TypeComparison));
			});
		}
	}
}
