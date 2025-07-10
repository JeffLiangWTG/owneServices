using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Generic.Testing
{
	class GenericOriginCountryCodesEftaEU15TransformerTests : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Generic";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY,
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES
		};

		protected override string CodeType => nameof(CodeListsConstants.Generic.CodeTypes.EU15_ORIGIN_COUNTRY_LIST);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[0], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY)),
			(CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_0), "Import", nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => downLoadLinks => new GenericOriginCountryCodesEftaEU15Transformer(downLoadLinks);

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: ÅLAND ISLANDS
StartDate: 2005-12-07T00:00:00
EndDate: 2010-04-08T23:59:59
",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Schweiz
StartDate: 
EndDate: 
"
		};

		string[] ITestManyEmptyDescriptionErrorMessages.ExpectedEmptyDescriptionErrorMessages => new[]
		{
		$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: BG
Description: 
StartDate: 2007-01-01T00:00:00
EndDate: 
",
		$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: LI
Description: 
StartDate: 
EndDate: 
"
		};

		string[] ITestManyInvalidDateErrorMessages.ExpectedInvalidDateErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: CY
Description: Zypern
StartDate: 2004-19-01T00:00:00
EndDate: 
"
		};
	}
}
