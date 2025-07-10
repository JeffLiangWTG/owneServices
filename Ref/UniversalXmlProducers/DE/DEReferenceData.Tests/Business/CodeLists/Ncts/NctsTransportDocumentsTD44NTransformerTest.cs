using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts.Testing
{
	sealed class NctsTransportDocumentsTD44NTransformerTest : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages
	{
		protected override string System => "Ncts";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0943_TRANSPORT_DOCUMENTS,
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0945_TRANSPORT_DOCUMENTS
		};

		protected override string CodeType => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_TD44N_TRANSPORT_DOCUMENTS);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[0], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0943_TRANSPORT_DOCUMENTS)),
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0945_TRANSPORT_DOCUMENTS))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => downLoadLinks => new NctsTransportDocumentsTD44NTransformer(downLoadLinks);

		public string[] ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Sonstige Unterlagen ZELOS (Original)
StartDate: 
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
Code: 
Description: Sonstige Unterlagen ZELOS (Original)
StartDate: 
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
Value: N"
		};

		public string[] ExpectedEmptyDescriptionErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZY
Description: 
StartDate: 
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
Code: 9ZZY
Description: 
StartDate: 
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
Value: N"
		};
	}
}
