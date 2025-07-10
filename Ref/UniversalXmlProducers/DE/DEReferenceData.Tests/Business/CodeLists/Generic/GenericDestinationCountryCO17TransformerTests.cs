using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Generic.Testing
{
	sealed class GenericDestinationCountryCO17TransformerTests : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Generic";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0806_COUNTRY_LIST_CO,
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1314_EU_MEMBER_STATES
		};

		protected override string CodeType => nameof(CodeListsConstants.Generic.CodeTypes.CO17_DESTINATION_COUNTRY_LIST);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[0], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0806_COUNTRY_LIST_CO)),
			(CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_0), "Import", nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1314_EU_MEMBER_STATES))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => downLoadLinks => new GenericDestinationCountryCO17Transformer(downLoadLinks);

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Zypern (Nordzypern)
StartDate: 2017-10-08T00:00:00
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
Code: QQ
Description: 
StartDate: 2009-06-27T00:00:00
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
Code: QR
Description: Schiffs- u. Luftfahrzeugbedarf (EU)
StartDate: 2009-16-27T00:00:00
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
