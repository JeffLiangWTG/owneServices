using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class EntryNumberGeneratorCatA1Test : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsAutoGenerateEntryNumberAllowed()
		{
			entryNumberGeneratorForTesting = new EntryNumberGeneratorCatAForTest(entryHeaderForTesting);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.IsAutoGenerateEntryNumberAllowed, NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestCannotAutoGenerateEntryNumberMessage()
		{
			entryNumberGeneratorForTesting = new EntryNumberGeneratorCatAForTest(entryHeaderForTesting);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.CannotAutoGenerateEntryNumberMessage, NUnit.Framework.Is.EqualTo("When the Sea Office of Receipt transships to the Air Office of Lading, please enter the Entry number manually.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberShouldBeGeneratedCategoryA1()
		{
			entryInstructionForTesting.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstructionForTesting.CEI_DateForDuty = ZDateTime.Empty;
			entryNumberGeneratorForTesting = new EntryNumberGeneratorCatAForTest(entryHeaderForTesting);
			entryInstructionForTesting.CEI_CustomsOffice = ZString.Empty;
			declarationForTesting.JE_CustomsOffice = ZString.Empty;
			entryInstructionForTesting.CEI_BoxNumber = ZString.Empty;
			entryHeaderForTesting.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeaderForTesting.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstructionForTesting.CEI_CustomsOffice = "AA";
			entryHeaderForTesting.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeaderForTesting.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declarationForTesting.JE_CustomsOffice = "BB";
			entryHeaderForTesting.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeaderForTesting.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstructionForTesting.CEI_DateForDuty = ZDateTime.Now;
			entryHeaderForTesting.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeaderForTesting.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstructionForTesting.CEI_BoxNumber = "123";
			entryHeaderForTesting.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeaderForTesting.EntryNumber, NUnit.Framework.Is.Not.EqualTo(ZString.Empty));
			entryHeaderForTesting.EntryNumber = ZString.Empty;
			entryInstructionForTesting.CEI_CustomsOffice = "BB";
			entryInstructionForTesting.CEI_BoxNumber = "123";
			entryHeaderForTesting.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeaderForTesting.EntryNumber, NUnit.Framework.Is.Not.EqualTo(ZString.Empty));
			entryHeaderForTesting.EntryNumber = ZString.Empty;
			entryInstructionForTesting.CEI_Style = ZString.Empty;
			entryHeaderForTesting.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeaderForTesting.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberCategoryA1()
		{
			entryInstructionForTesting.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryNumberGeneratorForTesting = new EntryNumberGeneratorCatAForTest(entryHeaderForTesting);
			entryInstructionForTesting.CEI_CustomsOffice = "AA";
			declarationForTesting.JE_CustomsOffice = "BB";
			entryInstructionForTesting.CEI_BoxNumber = "123";
			entryInstructionForTesting.CEI_DateForDuty = new ZDateTime(2019, 05, 16);
			var generatedEntryNumber = entryNumberGeneratorForTesting.GenerateEntryNumber();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(generatedEntryNumber.Substring(0, 2), NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(generatedEntryNumber.Substring(2, 2), NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(generatedEntryNumber.Substring(4, 2), NUnit.Framework.Is.EqualTo("08").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(generatedEntryNumber.Substring(6, 3), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(generatedEntryNumber.Substring(9, 5), NUnit.Framework.Is.EqualTo("E0001").Using(CustomComparers.TypeComparison));
			});

			declarationForTesting.JE_CustomsOffice = "AA";
			generatedEntryNumber = entryNumberGeneratorForTesting.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(2, 2), NUnit.Framework.Is.EqualTo("  ").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestKeyFormatting()
		{
			entryNumberGeneratorForTesting = new EntryNumberGeneratorCatAForTest(entryHeaderForTesting);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BT", "Nanjing Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var customsOffice = TWRefCusCodeListLoader.GetCustomsOffice(Factory, "BT", ZDateTime.Now);
			customsOffice.ZZD_IsSea = true;
			customsOffice.ZZD_IsAir = true;
			Factory.Save();
			declarationForTesting.JE_CustomsOffice = "BT";
			entryInstructionForTesting.CEI_CustomsOffice = "BT";
			NUnit.Framework.Assert.That(declarationForTesting.CustomsOffice.ZZD_IsAir && entryInstructionForTesting.CustomsOffice.ZZD_IsSea, NUnit.Framework.Is.True);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(1), NUnit.Framework.Is.EqualTo("E0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(139987), NUnit.Framework.Is.EqualTo("Z0001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(149985), NUnit.Framework.Is.EqualTo("Z9999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(149986), NUnit.Framework.Is.EqualTo("EA001"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(539595), NUnit.Framework.Is.EqualTo("ZZ999"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(539596), NUnit.Framework.Is.EqualTo("EAA01"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(1543455), NUnit.Framework.Is.EqualTo("ZZZ99"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(1543456), NUnit.Framework.Is.EqualTo("EAAA1"));
				NUnit.Framework.Assert.That(entryNumberGeneratorForTesting.FormatNumberToKeyExposed(3916215), NUnit.Framework.Is.EqualTo("ZZZZ9"));
			});
		}

		[ExpectNoExceptions]
		public void TestAllowedGeneratorDescription()
		{
			var entryNumberGeneratorForTest = new EntryNumberGeneratorCatA1(entryHeaderForTesting);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declarationForTesting.RunPreSaveValidation();
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(@"Office of Receipt:You have not entered an Office of Receipt.
Box Number:You have not entered a Box Number.").Using(CustomComparers.TypeComparison));
		}

		#region Implementation
		class EntryNumberGeneratorCatAForTest : EntryNumberGeneratorCatA1
		{
			public EntryNumberGeneratorCatAForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public string FormatNumberToKeyExposed(int number) => FormatIntToString(number);
		}

		JobDeclaration declarationForTesting;
		CusEntryInstruction entryInstructionForTesting;
		CusEntryHeader entryHeaderForTesting;
		EntryNumberGeneratorCatAForTest entryNumberGeneratorForTesting;
		protected override void SetUp()
		{
			declarationForTesting = Factory.New<JobDeclaration>();
			entryInstructionForTesting = declarationForTesting.CusEntryInstruction;
			entryHeaderForTesting = declarationForTesting.CustomsEntryHeaders.AddNew();
			entryHeaderForTesting.CH_CEI_Instruction = entryInstructionForTesting.PK;
		}
		#endregion
	}
}
