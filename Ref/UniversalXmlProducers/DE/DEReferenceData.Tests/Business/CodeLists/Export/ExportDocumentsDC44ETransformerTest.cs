using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportDocumentsDC44ETransformerTest : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Export";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0922_DOCUMENTS,
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0921_DOCUMENTS
		};

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC44E_DOCUMENTS);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[0], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0922_DOCUMENTS)),
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[1], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0921_DOCUMENTS))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => downLoadLinks => new ExportDocumentsDC44ETransformer(downLoadLinks);

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Ernährung (BLE) nach § 10 Abs.
StartDate: 2013-09-01T00:00:00
EndDate: 2015-08-19T23:59:59",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Endgültiger Kauf/Verkauf
StartDate: 2021-03-07T00:00:00
EndDate: 2025-10-07T23:59:59"
		};

		string[] ITestManyEmptyDescriptionErrorMessages.ExpectedEmptyDescriptionErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 3LLA
Description: 
StartDate: 2013-09-01T00:00:00
EndDate: 2015-08-19T23:59:59",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZY
Description: 
StartDate: 2021-03-07T00:00:00
EndDate: 2025-10-07T23:59:59"
		};

		string[] ITestManyInvalidDateErrorMessages.ExpectedInvalidDateErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 3LLA
Description: Ernährung (BLE) nach § 10 Abs.
StartDate: 2013-09-01T00:00:00
EndDate: 2015-08-32T23:59:59",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZZ
Description: Endgültiger Kauf/Verkauf
StartDate: 2021-13-07T00:00:00
EndDate: 2025-10-07T23:59:59"
		};
	}
}
