using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocumentWrapperHelper))]
	sealed class DocumentWrapperHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestReplaceLineBreakWithSpace()
		{
			ZString testString1 = "\"SC-RC5-0001-0\rRC5 TX_SINGLE LINK-ETHERNET HDBaseT\r400-1139 REV. A\"\r";
			ZString testString2 = "\"PSC-RC5-0002-0\nRC5 RX_SINGLE LINK-ETHERNET HDBaseT\n400-1140 REV. A\"\n";
			ZString testString3 = "\"PSC-RC5-0003-0\r\nRC5 RX_SINGLE LINK-ETHERNET HDBaseT\r\n400-1141 REV. A\"\r\n";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testString1.ReplaceLineBreakWithSpace(), NUnit.Framework.Is.EqualTo("\"SC-RC5-0001-0 RC5 TX_SINGLE LINK-ETHERNET HDBaseT 400-1139 REV. A\" ").Using(CustomComparers.TypeComparison), "testString1");
				NUnit.Framework.Assert.That(testString2.ReplaceLineBreakWithSpace(), NUnit.Framework.Is.EqualTo("\"PSC-RC5-0002-0 RC5 RX_SINGLE LINK-ETHERNET HDBaseT 400-1140 REV. A\" ").Using(CustomComparers.TypeComparison), "testString2");
				NUnit.Framework.Assert.That(testString3.ReplaceLineBreakWithSpace(), NUnit.Framework.Is.EqualTo("\"PSC-RC5-0003-0 RC5 RX_SINGLE LINK-ETHERNET HDBaseT 400-1141 REV. A\" ").Using(CustomComparers.TypeComparison), "testString3");
			});
		}

		[ExpectNoExceptions]
		public void TestGetGovernmentAgencyImage()
		{
			TestGetGovernmentAgencyImageForN5110();
			TestGetGovernmentAgencyImageForN5111();
		}

		[ExpectNoExceptions]
		void TestGetGovernmentAgencyImageForN5110()
		{
			var governmentAgencyIDPrefix = "R99";
			ZString governmentAgencyID;
			for (int id = 1; id <= 100; id++)
			{
				governmentAgencyID = governmentAgencyIDPrefix + id.ToString().PadLeft(2, '0');
				var messageString = $"The government gency id is {governmentAgencyID}";
				var resourcePath = DocumentWrapperHelper.ResourcePath.N5110;
				var actualImage = DocumentWrapperHelper.GetGovernmentAgencyImage(governmentAgencyID, resourcePath);
				if (id >= 1 && id <= 99)
				{
					NUnit.Framework.Assert.That(ConvertImageToString(actualImage), NUnit.Framework.Is.EqualTo(ConvertImageToString(GetImage(governmentAgencyID, resourcePath))), messageString);
				}
				else if (id >= 100)
				{
					NUnit.Framework.Assert.That(actualImage, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)), "messageString - should be [null]");
				}
			}
		}

		[ExpectNoExceptions]
		void TestGetGovernmentAgencyImageForN5111()
		{
			var governmentAgencyIDPrefix = "R99";
			ZString governmentAgencyID;
			for (int id = 1; id <= 21; id++)
			{
				governmentAgencyID = governmentAgencyIDPrefix + id.ToString().PadLeft(2, '0');
				var messageString = $"The government gency id is {governmentAgencyID}";
				var resourcePath = DocumentWrapperHelper.ResourcePath.N5111;
				var actualImage = DocumentWrapperHelper.GetGovernmentAgencyImage(governmentAgencyID, resourcePath);
				if (id >= 1 && id <= 20)
				{
					NUnit.Framework.Assert.That(ConvertImageToString(actualImage), NUnit.Framework.Is.EqualTo(ConvertImageToString(GetImage(governmentAgencyID, resourcePath))), messageString);
				}
				else if (id >= 21)
				{
					NUnit.Framework.Assert.That(actualImage, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)), "messageString - should be [null]");
				}
			}
		}

		string ConvertImageToString(Image image)
		{
			var result = ZString.Empty;
			using (var ms = new MemoryStream())
			{
				image.Save(ms, image.RawFormat);
				result = System.Text.Encoding.UTF8.GetString(ms.ToArray());
			}

			return result;
		}

		Image GetImage(ZString governmentAgencyID, ZString resourcePath)
		{
			Image image = null;
			var path = ZString.Empty;
			if (resourcePath == DocumentWrapperHelper.ResourcePath.N5110)
			{
				if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9901AndR9920.png";
				}
				else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9921AndR9950(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9921AndR9950.png";
				}
				else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9951AndR9980(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9951AndR9980.png";
				}
				else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9981AndR9999(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9981AndR9999.png";
				}
			}
			else if (resourcePath == DocumentWrapperHelper.ResourcePath.N5111)
			{
				if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID))
				{
					path = "GovernmentAgencyImage.png";
				}
			}

			if (!path.IsEmpty)
			{
				path = resourcePath + path;
				using (var stream = typeof(DocumentWrapperHelper).Assembly.GetManifestResourceStream(path))
				{
					if (stream != null)
					{
						image = Image.FromStream(stream);
					}
				}
			}

			return image;
		}

		[ExpectNoExceptions]
		public void TestCheckDigit()
		{
			var barcode1 = "0712186AX";
			var barcode2 = "R9902ABI11170535417";
			var barcode3WithoutCheckDigit = "611R0002219832";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetCheckDigit(barcode1 + barcode2 + barcode3WithoutCheckDigit), NUnit.Framework.Is.EqualTo("Z").Using(CustomComparers.TypeComparison));
			barcode3WithoutCheckDigit = "611R0002219808";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetCheckDigit(barcode1 + barcode2 + barcode3WithoutCheckDigit), NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			barcode3WithoutCheckDigit = ZString.Empty;
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetCheckDigit(barcode1 + barcode2 + barcode3WithoutCheckDigit), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			barcode3WithoutCheckDigit = "611R000221980a";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetCheckDigit(barcode1 + barcode2 + barcode3WithoutCheckDigit), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestStringAsDateTime()
		{
			var dateStringValue = "2019-01-03";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.StringAsDateTime(dateStringValue), NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 3)));
			dateStringValue = "aa";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.StringAsDateTime(dateStringValue), NUnit.Framework.Is.EqualTo(ZDateTime.Invalid));
			dateStringValue = ZString.Empty;
			NUnit.Framework.Assert.That(DocumentWrapperHelper.StringAsDateTime(dateStringValue), NUnit.Framework.Is.EqualTo(ZDateTime.Invalid));
		}

		[ExpectNoExceptions]
		public void TestGetDeclarationIDFormat()
		{
			var declarationID = "AB  07094AD515";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDeclarationIDFormat(declarationID), NUnit.Framework.Is.EqualTo("AB/  /07/094/AD515").Using(CustomComparers.TypeComparison), "when length of DeclarationID is more than 13");
			declarationID = "AB  07094AD51";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDeclarationIDFormat(declarationID), NUnit.Framework.Is.EqualTo(ZString.Empty), "when length of DeclarationID is 13");
			declarationID = "AB  07094AD5";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDeclarationIDFormat(declarationID), NUnit.Framework.Is.EqualTo(ZString.Empty), "when length of DeclarationID less than 13");
		}

		[ExpectNoExceptions]
		public void TestSplitTextByWord()
		{
			var veryLongWord = new string('A', 35);
			var testText = veryLongWord + "   This    is test description, this is test description.   ";
			var result = DocumentWrapperHelper.SplitTextByWord(testText, 15);
			var fifteenAs = new string('A', 15);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(7));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo(fifteenAs).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo(fifteenAs).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("AAAAA   This").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[3], NUnit.Framework.Is.EqualTo("is test").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[4], NUnit.Framework.Is.EqualTo("description,").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[5], NUnit.Framework.Is.EqualTo("this is test").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[6], NUnit.Framework.Is.EqualTo("description.").Using(CustomComparers.TypeComparison));
			testText = "   This    is test description,   " + veryLongWord + "   this is test description.   ";
			result = DocumentWrapperHelper.SplitTextByWord(testText, 15);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(7));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("This    is test").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("description,").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo(fifteenAs).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[3], NUnit.Framework.Is.EqualTo(fifteenAs).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[4], NUnit.Framework.Is.EqualTo("AAAAA   this is").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[5], NUnit.Framework.Is.EqualTo("test").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[6], NUnit.Framework.Is.EqualTo("description.").Using(CustomComparers.TypeComparison));
			testText = "  apple 为什么客户？要输入很？多中文 and English  ";
			result = DocumentWrapperHelper.SplitTextByWord(testText, 11);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("apple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("为什么客户？要输入很？").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("多中文 and").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[3], NUnit.Framework.Is.EqualTo("English").Using(CustomComparers.TypeComparison));
			result = DocumentWrapperHelper.SplitTextByWord(testText, 11, true);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(5));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("apple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("为什么客户").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("？要输入很").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[3], NUnit.Framework.Is.EqualTo("？多中文").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[4], NUnit.Framework.Is.EqualTo("and English").Using(CustomComparers.TypeComparison));
			testText = "  apple 为什么客户?要输入很?多中文 and English  ";
			result = DocumentWrapperHelper.SplitTextByWord(testText, 11, true);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(5));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("apple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("为什么客户").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("?要输入很").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[3], NUnit.Framework.Is.EqualTo("?多中文 and").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[4], NUnit.Framework.Is.EqualTo("English").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSplitTextByWordIncludingEmpty()
		{
			var testText = "P/O:30247328 P/N:  EM-36.0188";
			var result = DocumentWrapperHelper.SplitTextByWord(testText, 28);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("P/O:30247328 P/N:").Using(CustomComparers.TypeComparison), "Line 1");
				NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("EM-36.0188").Using(CustomComparers.TypeComparison), "Line 2");
			});
		}

		[ExpectNoExceptions]
		public void TestSplitTextByWord_ResetLineValue()
		{
			var testText = @"CHIP COMMON MODE FILTER                                               ACT45B-101-2P-TL003";
			var result = DocumentWrapperHelper.SplitTextByWord(testText, 35, true);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("CHIP COMMON MODE FILTER").Using(CustomComparers.TypeComparison), "Line 1");
				NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("ACT45B-101-2P-TL003").Using(CustomComparers.TypeComparison), "Line 2");
			});
		}

		[ExpectNoExceptions]
		public void TestSplitTextByLineBreak()
		{
			var testLine = "  This is test description, this is test description two.  \r\n  This is another description.  ";
			var result = DocumentWrapperHelper.SplitTextByLineBreak(testLine, 25);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(5));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("This is test description,").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("this is test description").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("two.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[3], NUnit.Framework.Is.EqualTo("This is another").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[4], NUnit.Framework.Is.EqualTo("description.").Using(CustomComparers.TypeComparison));
			testLine = "  apple 中文？？ and  \r\n  other Line  ";
			result = DocumentWrapperHelper.SplitTextByLineBreak(testLine, 11);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("apple 中文？？").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("and").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("other Line").Using(CustomComparers.TypeComparison));
			result = DocumentWrapperHelper.SplitTextByLineBreak(testLine, 11, true);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("apple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("中文？？").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("and").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[3], NUnit.Framework.Is.EqualTo("other Line").Using(CustomComparers.TypeComparison));
			testLine = "  apple 中文?？ and  \r\n  other Line  ";
			result = DocumentWrapperHelper.SplitTextByLineBreak(testLine, 11, true);
			NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo("apple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[1], NUnit.Framework.Is.EqualTo("中文?？ and").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result[2], NUnit.Framework.Is.EqualTo("other Line").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatCurrencyMacro()
		{
			var result = DocumentWrapperHelper.FormatCurrencyMacro(1.23m, "CAD");
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("<Currency(1.23,CAD)>").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatFormatNumberMacro()
		{
			var result = DocumentWrapperHelper.FormatFormatNumberMacro(1.233m, 2);
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("<FormatNumber(1.233,2)>").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatTariffNumber()
		{
			var result = DocumentWrapperHelper.FormatTariffNumber("12345678901");
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("1234.56.78.90-1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatQuantityNumber()
		{
			var result = DocumentWrapperHelper.FormatQuantityNumber(new ZDecimal(1.00m));
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			result = DocumentWrapperHelper.FormatQuantityNumber(new ZDecimal(1.01m));
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("1.01").Using(CustomComparers.TypeComparison));
			result = DocumentWrapperHelper.FormatQuantityNumber(new ZDecimal(1.000011m));
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("1.00001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatWeightNumber()
		{
			var result = DocumentWrapperHelper.FormatWeightNumber(new ZDecimal(1.00m));
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			result = DocumentWrapperHelper.FormatWeightNumber(new ZDecimal(1.01m));
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("1.01").Using(CustomComparers.TypeComparison));
			result = DocumentWrapperHelper.FormatWeightNumber(new ZDecimal(1.0000011m));
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo("1.000001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatNumber()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(1111m), 2), NUnit.Framework.Is.EqualTo("1,111.00").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(1111m), 3), NUnit.Framework.Is.EqualTo("1,111.000").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(1111m), 0), NUnit.Framework.Is.EqualTo("1,111").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(1111m), -1), NUnit.Framework.Is.EqualTo("1,111").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(5555555555.5555m), 2), NUnit.Framework.Is.EqualTo("5,555,555,555.56").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(5555555555.5555m), 4), NUnit.Framework.Is.EqualTo("5,555,555,555.5555").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(5555555555.55m), 5), NUnit.Framework.Is.EqualTo("5,555,555,555.55000").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatNumber(new ZDecimal(5555555555.5555m)), NUnit.Framework.Is.EqualTo("5,555,555,555.5555").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCombineTextLines()
		{
			var lines = new List<ZString>()
			{ "1", "2", "3" };
			NUnit.Framework.Assert.That(DocumentWrapperHelper.CombineTextLines(lines, 0), NUnit.Framework.Is.EqualTo(@"1
2
3").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.CombineTextLines(lines, 1), NUnit.Framework.Is.EqualTo(@"2
3").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.CombineTextLines(lines, 4), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.CombineTextLines(lines, 0, 2), NUnit.Framework.Is.EqualTo(@"1
2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.CombineTextLines(lines, 1, 2), NUnit.Framework.Is.EqualTo(@"2
3").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.CombineTextLines(lines, 1, 10), NUnit.Framework.Is.EqualTo(@"2
3").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.CombineTextLines(lines, 3, 10), NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestGetSayTotalDescription()
		{
			var twdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "TWD");
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetSayTotalDescription(twdCurrency, 1.01), NUnit.Framework.Is.EqualTo("SAY TOTAL NEW TAIWAN DOLLAR ONE AND CENT ONE ONLY.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetSayTotalDescription(twdCurrency, 1.02), NUnit.Framework.Is.EqualTo("SAY TOTAL NEW TAIWAN DOLLAR ONE AND CENTS TWO ONLY.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetSayTotalDescription(twdCurrency, 34567.02), NUnit.Framework.Is.EqualTo("SAY TOTAL NEW TAIWAN DOLLARS THIRTY FOUR THOUSAND FIVE HUNDRED AND SIXTY SEVEN AND CENTS TWO ONLY.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetSayTotalDescription(usdCurrency, 34567.02), NUnit.Framework.Is.EqualTo("SAY TOTAL UNITED STATES DOLLARS THIRTY FOUR THOUSAND FIVE HUNDRED AND SIXTY SEVEN AND CENTS TWO ONLY.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetSayTotalDescription(usdCurrency, 34567), NUnit.Framework.Is.EqualTo("SAY TOTAL UNITED STATES DOLLARS THIRTY FOUR THOUSAND FIVE HUNDRED AND SIXTY SEVEN ONLY.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetOriginProperNameWithCountry()
		{
			RefUNLOCO originUNLOCO = Factory.New<RefUNLOCO>();
			originUNLOCO.RL_Code = "TWABC";
			originUNLOCO.Description = "origin name";
			originUNLOCO.RL_NameWithDiacriticals = "origin proper name";
			originUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TWABC";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetOriginProperNameWithCountry(declaration, " - "), NUnit.Framework.Is.EqualTo("Taiwan - origin proper name").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKOrigin = "TWZZZ";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetOriginProperNameWithCountry(declaration, " - "), NUnit.Framework.Is.EqualTo("Taiwan - ").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKOrigin = "TWZ99";
			declaration.JE_Z99PortOfOrigin = "Z99 origin name";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetOriginProperNameWithCountry(declaration, " - "), NUnit.Framework.Is.EqualTo("Taiwan - Z99 origin name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetFinalDestinationProperNameWithCountry()
		{
			RefUNLOCO destinationUNLOCO = Factory.New<RefUNLOCO>();
			destinationUNLOCO.RL_Code = "TWABC";
			destinationUNLOCO.Description = "destination name";
			destinationUNLOCO.RL_NameWithDiacriticals = "destination proper name";
			destinationUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "TWABC";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetFinalDestinationProperNameWithCountry(declaration, " - "), NUnit.Framework.Is.EqualTo("Taiwan - destination proper name").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKFinalDestination = "TWZZZ";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetFinalDestinationProperNameWithCountry(declaration, " - "), NUnit.Framework.Is.EqualTo("Taiwan - ").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			declaration.JE_Z99FinalDestination = "Z99 destination name";
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetFinalDestinationProperNameWithCountry(declaration, " - "), NUnit.Framework.Is.EqualTo("Taiwan - Z99 destination name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetFormatAdditionalDocument()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetFormatAdditionalDocument(null).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetFormatAdditionalDocument(new AdditionalDocumentWrapper("CI999999999", ZInt.Zero)), NUnit.Framework.Is.EqualTo("CI999999999").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetFormatAdditionalDocument(new AdditionalDocumentWrapper("CI999999999", 1)), NUnit.Framework.Is.EqualTo("CI999999999-1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatAdditionalDocument()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatAdditionalDocument("CI999999999", ZInt.Zero), NUnit.Framework.Is.EqualTo("CI999999999").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatAdditionalDocument("CI999999999", 1), NUnit.Framework.Is.EqualTo("CI999999999-1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFormatBarCode()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatBarCode("CA 99999"), NUnit.Framework.Is.EqualTo("*CA 99999*").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.FormatBarCode(ZString.Empty), NUnit.Framework.Is.EqualTo("**").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsTextExceedsMaxChar()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(DocumentWrapperHelper.IsTextExceedsMaxChar("原進倉報單號碼/項次:Test1234567890-2111", 39), NUnit.Framework.Is.EqualTo(false), "Test1234567890/2111");
				NUnit.Framework.Assert.That(DocumentWrapperHelper.IsTextExceedsMaxChar("原進倉報單號碼/項次:Test1234567890-21111", 39), NUnit.Framework.Is.EqualTo(true), "Test1234567890/21111");
				NUnit.Framework.Assert.That(DocumentWrapperHelper.IsTextExceedsMaxChar("原進倉報單號碼/項次:Test1234567890", 34), NUnit.Framework.Is.EqualTo(false), "Test1234567890");
				NUnit.Framework.Assert.That(DocumentWrapperHelper.IsTextExceedsMaxChar("原進倉報單號碼/項次:Test1234567890-2", 34), NUnit.Framework.Is.EqualTo(true), "Test1234567890/2");
			});
		}

		[ExpectNoExceptions]
		public void TestSplitTextByWidthOnce()
		{
			var font = new Font("MingLiU-ExtB", 8);
			CombineAssertions("This test must be run in display scaling 100%", () =>
			{
				var result = DocumentWrapperHelper.SplitTextByWidthOnce("Testing line", 150, font);
				NUnit.Framework.Assert.That(result.FirstRowText, NUnit.Framework.Is.EqualTo("Testing line").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidthOnce("Testing long long line of text", 150, font);
				NUnit.Framework.Assert.That(result.FirstRowText, NUnit.Framework.Is.EqualTo("Testing long long line of").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo(" text").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidthOnce(" Leading Space", 150, font);
				NUnit.Framework.Assert.That(result.FirstRowText, NUnit.Framework.Is.EqualTo("Leading Space").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidthOnce("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFG", 150, font);
				NUnit.Framework.Assert.That(result.FirstRowText, NUnit.Framework.Is.EqualTo("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("ABCDEFG").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidthOnce("ABCDEFGHIJ加入一些中文字看看KLMNOPQRSTUVWXYZ", 150, font);
				NUnit.Framework.Assert.That(result.FirstRowText, NUnit.Framework.Is.EqualTo("ABCDEFGHIJ加入一些中文字看").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("看KLMNOPQRSTUVWXYZ").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidthOnce("Testing line \r\nof text with linebreak", 150, font);
				NUnit.Framework.Assert.That(result.FirstRowText, NUnit.Framework.Is.EqualTo("Testing line").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("of text with linebreak").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidthOnce("Testing longer line of text with \r\nlinebreak", 150, font);
				NUnit.Framework.Assert.That(result.FirstRowText, NUnit.Framework.Is.EqualTo("Testing longer line of").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo(" text with \r\nlinebreak").Using(CustomComparers.TypeComparison));
			}

			);
		}

		[ExpectNoExceptions]
		public void TestSplitTextByWidth()
		{
			var font = new Font("MingLiU-ExtB", 8);
			CombineAssertions("This test must be run in display scaling 100%", () =>
			{
				var result = DocumentWrapperHelper.SplitTextByWidth("Testing line", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("Testing line").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth("Testing long long line of text", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("Testing long long line of").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[1], NUnit.Framework.Is.EqualTo("text").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth(" Leading Space", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("Leading Space").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFG", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[1], NUnit.Framework.Is.EqualTo("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[2], NUnit.Framework.Is.EqualTo("ABCDEFG").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth("ABCDEFGHIJ加入一些中文字看看KLMNOPQRSTUVWXYZ", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("ABCDEFGHIJ加入一些中文字看").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[1], NUnit.Framework.Is.EqualTo("看KLMNOPQRSTUVWXYZ").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth("Testing \r\nline of text with linebreak", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("Testing").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[1], NUnit.Framework.Is.EqualTo("line of text with").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[2], NUnit.Framework.Is.EqualTo("linebreak").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth("Testing \nline of text with linebreak", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("Testing").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[1], NUnit.Framework.Is.EqualTo("line of text with").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[2], NUnit.Framework.Is.EqualTo("linebreak").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth("Testing longer line of text with \r\nlinebreak", 150, font);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("Testing longer line of").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[1], NUnit.Framework.Is.EqualTo("text with").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[2], NUnit.Framework.Is.EqualTo("linebreak").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				result = DocumentWrapperHelper.SplitTextByWidth("Testing very long long line of text with results needed set.", 150, font, 2);
				NUnit.Framework.Assert.That(result.SubRowTexts[0], NUnit.Framework.Is.EqualTo("Testing very long long").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.SubRowTexts[1], NUnit.Framework.Is.EqualTo("line of text with results").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(result.RemainingText, NUnit.Framework.Is.EqualTo(" needed set.").Using(CustomComparers.TypeComparison));
			}

			);
		}

		[ExpectNoExceptions]
		public void TestGetDetailInfo()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDetailInfo(0m, ZString.Empty, 0).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDetailInfo(1m, "PK", 0), NUnit.Framework.Is.EqualTo("1 PK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDetailInfo(1m, "PK", 1), NUnit.Framework.Is.EqualTo("1 PK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDetailInfo(2m, "PK", 2), NUnit.Framework.Is.EqualTo("@1 PK\r\n2 PK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDetailInfo(1.899m, "PK", 2), NUnit.Framework.Is.EqualTo("@0.95 PK\r\n1.899 PK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetDetailInfo(1m, "PK", 2), NUnit.Framework.Is.EqualTo("@0.5 PK\r\n1 PK").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBuildKeyValueStringFromDictionary()
		{
			var dictionary = new Dictionary<ZString, ZDecimal>();
			dictionary.Add("PLA", 0m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 0).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 1).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 2).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 3).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			dictionary.Add("DRN", 0m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 0).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 1).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 2).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 3).ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			dictionary.Add("PLT", 1m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary), NUnit.Framework.Is.EqualTo("1 PLT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 0), NUnit.Framework.Is.EqualTo("1 PLT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 1), NUnit.Framework.Is.EqualTo("1.0 PLT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 2), NUnit.Framework.Is.EqualTo("1.00 PLT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 3), NUnit.Framework.Is.EqualTo("1.000 PLT").Using(CustomComparers.TypeComparison));
			dictionary.Add("DRM", 2m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary), NUnit.Framework.Is.EqualTo("1 PLT\n2 DRM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 0), NUnit.Framework.Is.EqualTo("1 PLT\n2 DRM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 1), NUnit.Framework.Is.EqualTo("1.0 PLT\n2.0 DRM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 2), NUnit.Framework.Is.EqualTo("1.00 PLT\n2.00 DRM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionary(dictionary, 3), NUnit.Framework.Is.EqualTo("1.000 PLT\n2.000 DRM").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBuildKeyValueStringFromDictionaryForPrefix()
		{
			var dictionary = new Dictionary<ZString, ZDecimal>();
			dictionary.Add("PLA", 0m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionaryForPrefix(dictionary, "+").ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			dictionary.Add("DRN", 0m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionaryForPrefix(dictionary, "+").ToString(), NUnit.Framework.Is.Null.Or.Empty, "0 - should be [null] or [empty]");
			dictionary.Add("PLT", 1m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionaryForPrefix(dictionary, "+"), NUnit.Framework.Is.EqualTo("1PLT").Using(CustomComparers.TypeComparison));
			dictionary.Add("DRM", 2m);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionaryForPrefix(dictionary, "+"), NUnit.Framework.Is.EqualTo("1PLT\r\n+2DRM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.BuildKeyValueStringFromDictionaryForPrefix(dictionary, "-"), NUnit.Framework.Is.EqualTo("1PLT\r\n-2DRM").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddOrUpdateDictionary()
		{
			var dictionary = new Dictionary<ZString, ZDecimal>();
			NUnit.Framework.Assert.That(!dictionary.ContainsKey("PLT"), NUnit.Framework.Is.True);
			DocumentWrapperHelper.AddOrUpdateDictionary(dictionary, "PLT", 1m);
			NUnit.Framework.Assert.That(dictionary.ContainsKey("PLT"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dictionary["PLT"], NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			DocumentWrapperHelper.AddOrUpdateDictionary(dictionary, "PLT", 2m);
			NUnit.Framework.Assert.That(dictionary["PLT"], NUnit.Framework.Is.EqualTo(3m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetDimension()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageDimension(0m, 2m, 3m, "PLT"), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageDimension(1m, 0m, 3m, "PLT"), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageDimension(1m, 2m, 0m, "PLT"), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageDimension(1m, 2m, 3m, ZString.Empty), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageDimension(1m, 2m, 3m, "PLT"), NUnit.Framework.Is.EqualTo("1*2*3 PLT³"));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageDimension(1m, 2m, 3m, "M"), NUnit.Framework.Is.EqualTo("1*2*3 CBM"));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageDimension(1m, 2m, 3m, "CM"), NUnit.Framework.Is.EqualTo("1*2*3 CM³"));
		}

		[ExpectNoExceptions]
		public void TestGetVolumeInfo()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageVolumeInfo(ZString.Empty, 1m, "PLT", 3, "1*2*3 PLT³"), NUnit.Framework.Is.EqualTo("@0.333 PLT\r\n1 PLT\r\n1*2*3 PLT³"));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageVolumeInfo("1", 1m, "PLT", 3, "1*2*3 PLT³"), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageVolumeInfo(ZString.Empty, "@0.333 PLT\r\n1 PLT", "1*2*3 PLT³"), NUnit.Framework.Is.EqualTo("@0.333 PLT\r\n1 PLT\r\n1*2*3 PLT³"));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageVolumeInfo("1", "@0.333 PLT\r\n1 PLT", "1 *2*3 PLT³"), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestGetVolumeUQInfo()
		{
			foreach (var volumeCode in Core.Constants.Volume.Codes)
			{
				switch (volumeCode)
				{
					case Core.Constants.Volume.CubicMetres:
						NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageVolumeUQInfo(volumeCode), NUnit.Framework.Is.EqualTo("CBM"));
						break;
					case Core.Constants.Volume.CubicFeet:
						NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageVolumeUQInfo(volumeCode), NUnit.Framework.Is.EqualTo("Cuft"));
						break;
					default:
						NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackageVolumeUQInfo(volumeCode), NUnit.Framework.Is.EqualTo(volumeCode));
						break;
				}
			}
		}

		[ExpectNoExceptions]
		public void TestGetSummaryLineWithTailFlag()
		{
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag("Summary1", 1), NUnit.Framework.Is.EqualTo("Summary1\r\nv").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag("Summary8", 8), NUnit.Framework.Is.EqualTo("Summary8\r\nvvvvvvvv").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag("", 8), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag("    ", 8), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetMarksNumbersByAlignedFields()
		{
			AssertGetMarksNumbersByAlignedFields(ZString.Empty, ZString.Empty, true);
			AssertGetMarksNumbersByAlignedFields("S", "A", true);
			AssertGetMarksNumbersByAlignedFields("S\r\nS", "A\r\nB", false);
			AssertGetMarksNumbersByAlignedFields("S\r\nS\r\nS", "A\r\nB", false);
		}

		[ExpectNoExceptions]
		void AssertGetMarksNumbersByAlignedFields(ZString value, ZString expectedMarksNumbers, bool expectedHasNext)
		{
			var marksNumbers = new List<ZString> { "A", "B" }.GetEnumerator();
			var hasNext = marksNumbers.MoveNext();
			NUnit.Framework.Assert.That(marksNumbers.GetMarksNumbersByAlignedFields(value, ref hasNext), NUnit.Framework.Is.EqualTo(expectedMarksNumbers).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(hasNext, NUnit.Framework.Is.EqualTo(expectedHasNext));
		}
	}
}
