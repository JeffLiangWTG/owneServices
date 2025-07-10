using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import.Testing
{
	sealed class ImportCountryListIM15TransformerTest : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Import";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0300_COUNTRY_LIST,
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES
		};

		protected override string CodeType => nameof(CodeListsConstants.Generic.CodeTypes.IM15_ORIGIN_COUNTRY_LIST);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifiers[0], TestConstants.AtlasVersion10_1), System, nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0300_COUNTRY_LIST)),
			(CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_0), System, nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => downLoadLinks => new ImportCountryListIM15Transformer(downLoadLinks);

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Andorra
StartDate: 1991-07-01T00:00:00
EndDate: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Schweiz
StartDate: 
EndDate: "
		};

		string[] ITestManyEmptyDescriptionErrorMessages.ExpectedEmptyDescriptionErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: AE
Description: 
StartDate: 1984-01-01T00:00:00
EndDate: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: LI
Description: 
StartDate: 
EndDate: "
		};

		string[] ITestManyInvalidDateErrorMessages.ExpectedInvalidDateErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: AI
Description: Anguilla
StartDate: 1984-13-01T00:00:00
EndDate: "
		};
	}
}
