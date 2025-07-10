using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class EntryNumberGeneratorCatCTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEntryNumberShouldBeGeneratedCategoryC()
		{
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatCForTest(entryHeader);
			entryInstruction.CEI_CustomsOffice = ZString.Empty;
			entryInstruction.CEI_Style = ZString.Empty;
			entryInstruction.CEI_BoxNumber = ZString.Empty;
			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstruction.CEI_CustomsOffice = "AA";
			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstruction.CEI_Style = "D2";
			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstruction.CEI_DateForDuty = ZDateTime.Now;
			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstruction.CEI_BoxNumber = "456";
			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.Not.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = ZString.Empty;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_BoxNumber = "456";
			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.Not.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = ZString.Empty;
			entryInstruction.CEI_Style = ZString.Empty;
			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberCategoryC()
		{
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			entryInstruction.CEI_CustomsOffice = "AA";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 16);
			entryInstruction.CEI_BoxNumber = "456";
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatCForTest(entryHeader);
			var generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(0, 2), NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(2, 2), NUnit.Framework.Is.EqualTo("D2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(4, 2), NUnit.Framework.Is.EqualTo("08").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(6, 3), NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(9, 5), NUnit.Framework.Is.EqualTo("00001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestKeyFormatting()
		{
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatCForTest(entryHeader);
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
		}

		[ExpectNoExceptions]
		public void TestAllowedGeneratorDescription()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var entryNumberGeneratorForTest = new EntryNumberGeneratorCatC(entryHeader);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.RunPreSaveValidation();
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(@"Office of Receipt:You have not entered an Office of Receipt.
Box Number:You have not entered a Box Number.").Using(CustomComparers.TypeComparison));
		}

		#region Implementation
		class EntryNumberGeneratorCatCForTest : EntryNumberGeneratorCatC
		{
			public EntryNumberGeneratorCatCForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public string FormatNumberToKeyExposed(int number) => FormatIntToString(number);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		EntryNumberGeneratorCatCForTest entryNumberGeneratorForTest;
		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CusEntryInstruction;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}
		#endregion
	}
}
