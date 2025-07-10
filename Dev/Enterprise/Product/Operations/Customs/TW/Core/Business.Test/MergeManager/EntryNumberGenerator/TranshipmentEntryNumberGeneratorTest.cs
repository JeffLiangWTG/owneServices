using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TranshipmentEntryNumberGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2019, 08, 14)]
		[ExpectNoExceptions]
		public void TestGenerateEntryNumber()
		{
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "123";
			var generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(0, 2), NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(2, 2), NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(4, 2), NUnit.Framework.Is.EqualTo("08").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(6, 3), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(9, 5), NUnit.Framework.Is.EqualTo("00001").Using(CustomComparers.TypeComparison));
			TestDateAttribute.AddYears(5);
			header.UnladingOffice = "AA";
			generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(2, 2), NUnit.Framework.Is.EqualTo("  ").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(4, 2), NUnit.Framework.Is.EqualTo("13").Using(CustomComparers.TypeComparison));
			header.ReceiptOffice = "A";
			generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "B";
			generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestKeyFormatting()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(1), NUnit.Framework.Is.EqualTo("00001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(10000), NUnit.Framework.Is.EqualTo("10000"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(99999), NUnit.Framework.Is.EqualTo("99999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(100000), NUnit.Framework.Is.EqualTo("A0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(100001), NUnit.Framework.Is.EqualTo("A0002"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(109998), NUnit.Framework.Is.EqualTo("A9999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(109999), NUnit.Framework.Is.EqualTo("B0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(119997), NUnit.Framework.Is.EqualTo("B9999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(119998), NUnit.Framework.Is.EqualTo("C0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(6888105), NUnit.Framework.Is.EqualTo("ZZZZ9"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatStringToKeyExposed("00001"), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatStringToKeyExposed("10000"), NUnit.Framework.Is.EqualTo(10000));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatStringToKeyExposed("99999"), NUnit.Framework.Is.EqualTo(99999));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatStringToKeyExposed("A0001"), NUnit.Framework.Is.EqualTo(100000));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatStringToKeyExposed("C0001"), NUnit.Framework.Is.EqualTo(119998));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatStringToKeyExposed("ZZZZ9"), NUnit.Framework.Is.EqualTo(6888105));
			});
		}

		[ExpectNoExceptions]
		public void TestUnladingOfficeAndReceiptOffice()
		{
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.EntryNumberPart1, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.EntryNumberPart2, NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison));
			header.ReceiptOffice = "";
			header.UnladingOffice = "";
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.EntryNumberPart1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.EntryNumberPart2, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAllowedGeneratorDescription()
		{
			var entryNumberGeneratorForTest = new TranshipmentEntryNumberGenerator(header);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			header.RunPreSaveValidation();
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPart4Caption()
		{
			var entryNumberGeneratorForTest = new TranshipmentEntryNumberGenerator(header);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4Caption, NUnit.Framework.Is.EqualTo(header.TW_BoxNumberInfo.HumanReadableName));
		}

		[ExpectNoExceptions]
		public void TestPart4_Length()
		{
			var entryNumberGeneratorForTest = new TranshipmentEntryNumberGenerator(header);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4_Length, NUnit.Framework.Is.EqualTo(3));
		}

		#region Implementation
		class TranshipmentEntryNumberGeneratorForTest : TranshipmentEntryNumberGenerator
		{
			public TranshipmentEntryNumberGeneratorForTest(CusInBondHeader header) : base(header)
			{
			}

			public string FormatNumberToKeyExposed(int number) => FormatIntToString(number);
			public int FormatStringToKeyExposed(string number) => Sequenceformatter.FormatStringToInt(number);
		}

		CusInBondHeader header;
		TranshipmentEntryNumberGeneratorForTest entryNumberGeneratorForTest;
		protected override void SetUp()
		{
			header = Factory.New<CusInBondHeader>();
			entryNumberGeneratorForTest = new TranshipmentEntryNumberGeneratorForTest(header);
		}
		#endregion
	}
}
