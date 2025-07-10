using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportPreviousDocumentsDC40ETransformerTests : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Export";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0931_PREVIOUS_DOCUMENTS,
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0932_PREVIOUS_DOCUMENTS
		};

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC40E_PREVIOUS_DOCUMENTS);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[0], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0931_PREVIOUS_DOCUMENTS)),
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[1], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0932_PREVIOUS_DOCUMENTS))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => downLoadLinks => new ExportPreviousDocumentsDC40ETransformer(downLoadLinks);

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Referenz auf die Buchführung des Ausführers zur Feststellung der ausgeführten Waren / Referenz auf die MRN, mit der die Waren ausgeführt wurden
StartDate: 2021-03-07T00:00:00
EndDate: 
Reference: R
ItemNumber: N
Complement: N
Detail: N
Authority: N
IssuingDate: N
ValidityDate: N
MeasurementUnit: N
ComplementaryUnit: N
Value: N",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Zollanmeldung zur vorübergehenden Verwendung
StartDate: 2021-03-07T00:00:00
EndDate: 
Reference: R
ItemNumber: O
Complement: O
Detail: N
Authority: N
IssuingDate: N
ValidityDate: N
MeasurementUnit: N
ComplementaryUnit: N
Value: N"
		};

		string[] ITestManyEmptyDescriptionErrorMessages.ExpectedEmptyDescriptionErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZX
Description: 
StartDate: 2021-03-07T00:00:00
EndDate: 
Reference: O
ItemNumber: N
Complement: N
Detail: N
Authority: N
IssuingDate: N
ValidityDate: N
MeasurementUnit: N
ComplementaryUnit: N
Value: N",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9DFG
Description: 
StartDate: 2021-03-07T00:00:00
EndDate: 
Reference: R
ItemNumber: O
Complement: O
Detail: N
Authority: N
IssuingDate: N
ValidityDate: N
MeasurementUnit: N
ComplementaryUnit: N
Value: N"
		};

		string[] ITestManyInvalidDateErrorMessages.ExpectedInvalidDateErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZY
Description: Sonstige Unterlagen ZELOS (Kopie)
StartDate: 2021-13-07T00:00:00
EndDate: 
Reference: O
ItemNumber: N
Complement: N
Detail: N
Authority: N
IssuingDate: N
ValidityDate: N
MeasurementUnit: N
ComplementaryUnit: N
Value: N",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZX
Description: Sonstige Unterlagen ZELOS (Original)
StartDate: 2021-13-07T00:00:00
EndDate: 
Reference: O
ItemNumber: O
Complement: O
Detail: N
Authority: N
IssuingDate: N
ValidityDate: N
MeasurementUnit: N
ComplementaryUnit: N
Value: N"
		};
	}
}
