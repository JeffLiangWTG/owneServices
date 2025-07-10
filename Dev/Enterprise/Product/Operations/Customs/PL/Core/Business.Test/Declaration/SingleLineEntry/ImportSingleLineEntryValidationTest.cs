using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ImportSingleLineEntryValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckGoodsOrigin()
	{
		var message = "The code you have selected is not in the list.";
		var declaration = Factory.New<JobDeclaration>();
		var singleLineEntry = new ImportSingleLineEntry(declaration);

		CombineAssertions(() =>
		{
			singleLineEntry.Validation.ValidateGoodsOrigin();
			AssertNoMessageError("Empty", singleLineEntry.GoodsOriginInfo, message);
			singleLineEntry.GoodsOrigin = "12";
			AssertHasMessageError("Wrong value", singleLineEntry.GoodsOriginInfo, message);
			singleLineEntry.GoodsOrigin = "PL";
			AssertNoMessageError("Correct value", singleLineEntry.GoodsOriginInfo, message);
		});
	}

	public void TestCheckPreviousDocument()
	{
		var message = "The code you have selected is not in the list.";
		var declaration = Factory.New<JobDeclaration>();
		var singleLineEntry = new ImportSingleLineEntryForValidationTest(declaration);

		CombineAssertions(() =>
		{
			singleLineEntry.Validation.ValidateGoodsOrigin();
			AssertNoMessageError("Empty", singleLineEntry.PreviousDocumentInfo, message);
			singleLineEntry.PreviousDocument = "12";
			AssertHasMessageError("Wrong value", singleLineEntry.PreviousDocumentInfo, message);
			singleLineEntry.PreviousDocument = "PDC";
			AssertNoMessageError("Correct value", singleLineEntry.PreviousDocumentInfo, message);
		});
	}

	class ImportSingleLineEntryForValidationTest : ImportSingleLineEntry
	{
		public ImportSingleLineEntryForValidationTest(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override SingleLineEntryLookups GetNewLookups()
		{
			var lookupsMock = new Mock<ImportSingleLineEntryLookups>(this);
			var previousDocumentCodes = new CodeDescriptionPairList();
			previousDocumentCodes.AddPair("PDC");
			lookupsMock.Setup(_ => _.PreviousDocumentCodes).Returns(previousDocumentCodes);
			return lookupsMock.Object;
		}
	}
}
