using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Generic.Testing
{
	sealed class GenericOriginCountryCodesCO15TransformerTests : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Generic";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY,
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1314_EU_MEMBER_STATES
		};

		protected override string CodeType => nameof(CodeListsConstants.Generic.CodeTypes.CO15_ORIGIN_COUNTRY_LIST);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[0], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY)),
			(CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_0), "Import", nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1314_EU_MEMBER_STATES))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => downLoadLinks => new GenericOriginCountryCodesCO15Transformer(downLoadLinks);

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: ÅLAND ISLANDS
StartDate: 2005-12-07T00:00:00
EndDate: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Österreich
StartDate: 
EndDate: "
		};

		string[] ITestManyEmptyDescriptionErrorMessages.ExpectedEmptyDescriptionErrorMessages => new[]
		{
		$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: BG
Description: 
StartDate: 2007-01-01T00:00:00
EndDate: ",
		$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: BE
Description: 
StartDate: 
EndDate: "
		};

		string[] ITestManyInvalidDateErrorMessages.ExpectedInvalidDateErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: CY
Description: Zypern
StartDate: 2004-19-01T00:00:00
EndDate: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: BE
Description: 
StartDate: 
EndDate: "
		};
	}
}
