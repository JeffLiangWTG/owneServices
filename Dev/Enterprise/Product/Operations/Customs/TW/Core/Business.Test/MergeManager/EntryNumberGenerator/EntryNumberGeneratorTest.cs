using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class EntryNumberGeneratorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEntryNumberClassTypeBasedOnCategory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BT", "Nanjing Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var customsOffice = TWRefCusCodeListLoader.GetCustomsOffice(Factory, "BT", ZDateTime.Now);
			customsOffice.ZZD_IsSea = true;
			customsOffice.ZZD_IsAir = true;
			Factory.Save();
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			var entryNumberGenerator = EntryNumberGenerator.New(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGenerator, NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatA)));
				NUnit.Framework.Assert.That(EntryNumberGenerator.New(declaration), NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatA)));
			});

			declaration.JE_CustomsOffice = "BT";
			entryInstruction.CEI_CustomsOffice = "BT";
			NUnit.Framework.Assert.That(declaration.CustomsOffice.ZZD_IsAir && entryInstruction.CustomsOffice.ZZD_IsSea, NUnit.Framework.Is.True);
			entryNumberGenerator = EntryNumberGenerator.New(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGenerator, NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatA1)));
				NUnit.Framework.Assert.That(EntryNumberGenerator.New(entryHeader), NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatA1)));
			});

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			entryNumberGenerator = EntryNumberGenerator.New(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGenerator, NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatB)));
				NUnit.Framework.Assert.That(EntryNumberGenerator.New(declaration), NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatB)));
			});

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			entryNumberGenerator = EntryNumberGenerator.New(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGenerator, NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatC)));
				NUnit.Framework.Assert.That(EntryNumberGenerator.New(declaration), NUnit.Framework.Is.TypeOf(typeof(EntryNumberGeneratorCatC)));
			});
		}

		[TestDate(2019, 01, 01)]
		[ExpectNoExceptions]
		public void TestPart3Value()
		{
			var entryNumberGenerator = new EntryNumberGeneratorCatAForTest(entryHeader);
			NUnit.Framework.Assert.That(entryNumberGenerator.Part3Exposed, NUnit.Framework.Is.EqualTo("08"));
			entryInstruction.CEI_DateForDuty = new ZDateTime(2018, 01, 01);
			NUnit.Framework.Assert.That(entryNumberGenerator.Part3Exposed, NUnit.Framework.Is.EqualTo("07"));
		}

		[ExpectNoExceptions]
		public void TestPart4Caption()
		{
			var entryNumberGenerator = new EntryNumberGeneratorCatAForTest(entryHeader);
			NUnit.Framework.Assert.That(entryNumberGenerator.Part4Caption, NUnit.Framework.Is.EqualTo(entryInstruction.CEI_BoxNumberInfo.HumanReadableName));
		}

		[ExpectNoExceptions]
		public void TestPart4_Length()
		{
			var entryNumberGenerator = new EntryNumberGeneratorCatAForTest(entryHeader);
			NUnit.Framework.Assert.That(entryNumberGenerator.Part4_Length, NUnit.Framework.Is.EqualTo(3));
		}

		#region Implementation
		class EntryNumberGeneratorCatAForTest : EntryNumberGeneratorCatA
		{
			public EntryNumberGeneratorCatAForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public string Part3Exposed => base.Part3;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CusEntryInstruction;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}

		[ExpectNoExceptions]
		public void TestKeyFormattingWhenHasFirstAlphabets()
		{
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			var generatorCatForTest = new EntryNumberGeneratorCatA1ForTest(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(1), NUnit.Framework.Is.EqualTo("E0001"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(139987), NUnit.Framework.Is.EqualTo("Z0001"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(149985), NUnit.Framework.Is.EqualTo("Z9999"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(149986), NUnit.Framework.Is.EqualTo("EA001"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(539595), NUnit.Framework.Is.EqualTo("ZZ999"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(539596), NUnit.Framework.Is.EqualTo("EAA01"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(1543455), NUnit.Framework.Is.EqualTo("ZZZ99"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(1543456), NUnit.Framework.Is.EqualTo("EAAA1"));
				NUnit.Framework.Assert.That(generatorCatForTest.FormatNumberToKeyExposed(3916215), NUnit.Framework.Is.EqualTo("ZZZZ9"));
			});
		}

		class EntryNumberGeneratorCatA1ForTest : EntryNumberGeneratorCatA1
		{
			public EntryNumberGeneratorCatA1ForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public string FormatNumberToKeyExposed(int number) => FormatIntToString(number);
		}

		class EntryNumberGeneratorCatBForTest : EntryNumberGeneratorCatB
		{
			public EntryNumberGeneratorCatBForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public string FormatNumberToKeyExposed(int number) => FormatIntToString(number);
		}
		class EntryNumberGeneratorCatCForTest : EntryNumberGeneratorCatC
		{
			public EntryNumberGeneratorCatCForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}
		}

		[ExpectNoExceptions]
		public void TestCheck10ThCharacterNotInFirstAlphabets()
		{
			var generatorCatA1ForTest = new EntryNumberGeneratorCatA1ForTest(entryHeader);
			var generatorCatAForTest = new EntryNumberGeneratorCatAForTest(entryHeader);
			var generatorCatBForTest = new EntryNumberGeneratorCatBForTest(entryHeader);
			var generatorCatCForTest = new EntryNumberGeneratorCatCForTest(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(generatorCatA1ForTest.DoesEntryNumberFallIntoThisCategory("AAF50812X00010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!generatorCatAForTest.DoesEntryNumberFallIntoThisCategory("AAF50812X00010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!generatorCatBForTest.DoesEntryNumberFallIntoThisCategory("AAF50812XA1001"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!generatorCatCForTest.DoesEntryNumberFallIntoThisCategory("AAF50812X00010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!generatorCatA1ForTest.DoesEntryNumberFallIntoThisCategory("AAF50812XE0010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!generatorCatAForTest.DoesEntryNumberFallIntoThisCategory("AAF50812XE0010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(generatorCatBForTest.DoesEntryNumberFallIntoThisCategory("AAF50812XAI001"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!generatorCatCForTest.DoesEntryNumberFallIntoThisCategory("AAF50812XE0010"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!generatorCatCForTest.DoesEntryNumberFallIntoThisCategory("AAF50812XA0010"), NUnit.Framework.Is.True);
			});
		}
		#endregion
	}
}
