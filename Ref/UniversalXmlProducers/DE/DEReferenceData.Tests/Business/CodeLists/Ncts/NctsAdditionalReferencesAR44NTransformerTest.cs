using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts.Testing
{
	sealed class NctsAdditionalReferencesAR44NTransformerTest : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages
	{
		protected override string System => "Ncts";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0913_ADDITIONAL_REFERENCES,
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0915_ADDITIONAL_REFERENCES,
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0916_ADDITIONAL_REFERENCES
		};

		protected override string CodeType => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AR44N_ADDITIONAL_REFERENCES);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[0], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0913_ADDITIONAL_REFERENCES)),
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0915_ADDITIONAL_REFERENCES)),
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[2], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0916_ADDITIONAL_REFERENCES))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => downLoadLinks => new NctsAdditionalReferencesAR44NTransformer(downLoadLinks);

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
Value: N
",
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
Value: N
",
			$@"Unable to import record from {CustomsCodeListIdentifiers[2]} due to empty Code, Description or invalid Date.
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
Value: N
"
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
Value: N
",
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
Value: N
",
			$@"Unable to import record from {CustomsCodeListIdentifiers[2]} due to empty Code, Description or invalid Date.
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
Value: N
"
		};
	}
}
