using CargoWise.EntityFramework;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportCustomsDeclarationDocumentWrapper))]
	sealed class ImportCustomsDeclarationDocumentWrapperTest : ImportCustomsDeclarationDocumentWrapperAbstractTest<ImportCustomsDeclarationDocumentWrapper>
	{
		protected override ImportCustomsDeclarationDocumentWrapper GetDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory)
		{
			return ImportCustomsDeclarationDocumentWrapper.New(entryHeader, factory);
		}

		[ExpectNoExceptions]
		public void TestImporterChineseNameParts()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var importerAddress = declaration.ImporterDocumentaryAddress;
			importerAddress.E2_AddressOverride = true;
			var importerLocalAddress = importerAddress.LocalAddress;
			importerLocalAddress.CompanyName = "一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十";

			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ImporterChineseNamePart1, NUnit.Framework.Is.EqualTo("一二三四五六七八九十一二三四五六七").Using(CustomComparers.TypeComparison), "Part1");
				NUnit.Framework.Assert.That(wrapper.ImporterChineseNamePart2, NUnit.Framework.Is.EqualTo("八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十").Using(CustomComparers.TypeComparison), "Part2");
			});

			importerLocalAddress.CompanyName = "一二三四五六七八九十一二三四五六七";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ImporterChineseNamePart1, NUnit.Framework.Is.EqualTo("一二三四五六七八九十一二三四五六七").Using(CustomComparers.TypeComparison), "Part1");
				NUnit.Framework.Assert.That(wrapper.ImporterChineseNamePart2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Part2 - should be [null] or [empty]");
			});
		}
	}
}
