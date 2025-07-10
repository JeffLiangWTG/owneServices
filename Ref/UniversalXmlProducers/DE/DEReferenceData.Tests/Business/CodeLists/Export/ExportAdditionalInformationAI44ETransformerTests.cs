using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportAdditionalInformationAI44ETransformerTests : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages
	{
		protected override string System => "Export";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0901_ADDITIONAL_INFORMATION,
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0902_ADDITIONAL_INFORMATION
		};

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AI44E_ADDITIONAL_INFORMATION);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[0], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0901_ADDITIONAL_INFORMATION)),
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[1], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0902_ADDITIONAL_INFORMATION))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => downLoadLinks => new ExportAdditionalInformationAI44ETransformer(downLoadLinks);

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Diplomatengut
StartDate: 
EndDate: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Erledigung der aktiven Veredelung
StartDate: 
EndDate: "
		};

		string[] ITestManyEmptyDescriptionErrorMessages.ExpectedEmptyDescriptionErrorMessages => new[]
		{
		$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: X0000
Description: 
StartDate: 
EndDate: ",
		$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 00800
Description: 
StartDate: 
EndDate: "
		};
	}
}
