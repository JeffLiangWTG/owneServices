using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import.Testing
{
	sealed class ImportEconomicConditionsA2055TransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage
	{
		protected override string System => "Import";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A2055_ECONOMIC_CONDITIONS;

		protected override string CodeType => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_A2055_ECONOMIC_CONDITIONS);

		protected override string DownloadUrl => CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new ImportEconomicConditionsA2055Transformer(downloadLinks);

		protected override string InputFileName =>
			nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A2055_ECONOMIC_CONDITIONS);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Veredelung von Waren, die nicht in Anhang 71-02 der Delegierten Verordnung (EU) 2015/2446 aufgeführt sind,
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 02
Description: 
";
	}
}
