using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BaseEntryNumberGenerator))]
	sealed class BaseEntryNumberGeneratorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAllowedGeneratorDescription()
		{
			var entryNumberGeneratorForTest = new EntryNumberGeneratorForTest(Factory.New<EntryNumberGeneratorProviderForTest>());
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestGetFormattedInt()
		{
			var entryNumberGeneratorForTest = new EntryNumberGeneratorForTest(Factory.New<EntryNumberGeneratorProviderForTest>());
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(1), NUnit.Framework.Is.EqualTo("00001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(10000), NUnit.Framework.Is.EqualTo("10000"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(99999), NUnit.Framework.Is.EqualTo("99999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(100000), NUnit.Framework.Is.EqualTo("E0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(100001), NUnit.Framework.Is.EqualTo("E0002"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(109998), NUnit.Framework.Is.EqualTo("E9999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(109999), NUnit.Framework.Is.EqualTo("F0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(119997), NUnit.Framework.Is.EqualTo("F9999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(119998), NUnit.Framework.Is.EqualTo("G0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(1264509), NUnit.Framework.Is.EqualTo("ZZZZ9"));
			});
		}

		[ExpectNoExceptions]
		public void TestGenerateEntryNumber()
		{
			var entryNumberGeneratorProvider = Factory.New<EntryNumberGeneratorProviderForTest>();
			var entryNumberGeneratorForTest = new EntryNumberGeneratorForTest(entryNumberGeneratorProvider);
			var generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber, NUnit.Framework.Is.EqualTo("AABBCCDDDD00001").Using(CustomComparers.TypeComparison));

			entryNumberGeneratorProvider.SequenceNumber = "ABCDE";
			entryNumberGeneratorForTest = new EntryNumberGeneratorForTest(entryNumberGeneratorProvider);
			generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber, NUnit.Framework.Is.EqualTo("AABBCCDDDDABCDE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsBrokerageBoxNumber()
		{
			var entryNumberGeneratorForTest = new EntryNumberGeneratorForTest(Factory.New<EntryNumberGeneratorProviderForTest>());
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.CustomsBrokerageBoxNumber, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFountainName()
		{
			var entryNumberGeneratorForTest = new EntryNumberGeneratorForTest(Factory.New<EntryNumberGeneratorProviderForTest>());
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FountainName, NUnit.Framework.Is.EqualTo("TWEntryNum_OTH_A").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPart4Caption()
		{
			var entryNumberGeneratorForTest = new EntryNumberGeneratorForTest(Factory.New<EntryNumberGeneratorProviderForTest>());
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4Caption, NUnit.Framework.Is.EqualTo("Box Number").Using(CustomComparers.TypeComparison));
		}

		class EntryNumberGeneratorForTest : BaseEntryNumberGenerator
		{
			public EntryNumberGeneratorForTest(IEntryNumberGeneratorProvider provider) : base(provider)
			{
			}
			public string FormatNumberToKeyExposed(int number) => FormatIntToString(number);
			protected override BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
			BusinessObjectFactory factory;
			protected override ZString Category => "A";
			protected override ZString ShipmentType => "OTH";
			public override ZString Part1 => "AA";
			public override ZString Part2 => "BB";
			public override ZString Part3 => "CC";
			public override ZString Part4 => "DDDD";
			public override ZString Part4Caption => "Box Number";
			public override int Part4_Length => 4;
			public override ZString CustomsBrokerageBoxNumber => "123";
			public override ZString EntryNumberType => ZString.Empty;

			readonly List<char> alphabets = new List<char> { 'E', 'F', 'G', 'H', 'J', 'M', 'P', 'Q', 'R', 'U', 'V', 'W', 'X', 'Y', 'Z' };
			protected override ITWSequenceformatter GetSequenceformatter() => new BaseTWSequenceformatter(1264509, 5, alphabets);
			protected override GlbCompany Company => GlbCompany.CurrentCompany;
		}
	}
}
