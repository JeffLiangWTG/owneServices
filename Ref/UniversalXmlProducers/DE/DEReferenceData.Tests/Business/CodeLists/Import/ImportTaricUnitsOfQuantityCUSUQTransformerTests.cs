using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import.Testing
{
	sealed class ImportTaricUnitsOfQuantityCUSUQTransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		[Test]
		public void TestDoNotMergeAttributeAdded()
		{
			Assert.Multiple(() =>
			{
				AssertAttributeWithValue("ASV", "NoMerge", "Y");
				AssertAttributeWithValue("ASVX", "NoMerge", "Y");
				AssertAttributeWithValue("062", "NoMerge", "Y");
				AssertAttributeWithValue("063", "NoMerge", "Y");
				AssertNoAttribute("CCT", "NoMerge");
			});
		}

		protected override string System => "Import";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0700_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY;

		protected override string CodeType => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_CUSUQ_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY);

		protected override string DownloadUrl => CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_1);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => (downloadLinks) => new ImportTaricUnitsOfQuantityCUSUQTransformer(downloadLinks);

		protected override string InputFileName =>
			nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0700_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Ladetonnen
StartDate: 2002-01-01T00:00:00
EndDate: 
Qualifier: 
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: CEN
Description: 
StartDate: 2002-01-01T00:00:00
EndDate: 
Qualifier: 
";

		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: DTN
Description: 100 Kilogramm Netto Abtropfgewicht
StartDate: 1994-21-21T00:00:00
EndDate: 
Qualifier: E
";
	}
}
