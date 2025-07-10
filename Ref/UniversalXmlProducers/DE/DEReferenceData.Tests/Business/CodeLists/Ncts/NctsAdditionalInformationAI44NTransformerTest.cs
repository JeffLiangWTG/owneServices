using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts.Testing
{
	sealed class NctsAdditionalInformationAI44NTransformerTest : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages
	{
		protected override string System => "Ncts";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0903_ADDITIONAL_INFORMATION,
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0905_ADDITIONAL_INFORMATION,
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0906_ADDITIONAL_INFORMATION
		};

		protected override string CodeType => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AI44N_ADDITIONAL_INFORMATION);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[0], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0903_ADDITIONAL_INFORMATION)),
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0905_ADDITIONAL_INFORMATION)),
			(CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifiers[2], TestConstants.AtlasVersion10_1), "Ncts", nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0906_ADDITIONAL_INFORMATION))
		};

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => downLoadLinks => new NctsAdditionalInformationAI44NTransformer(downLoadLinks);

		public string[] ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Mitteilung an die Abgangszollstelle",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Mitteilung an die Abgangszollstelle",
			$@"Unable to import record from {CustomsCodeListIdentifiers[2]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Mitteilung an die Abgangszollstelle"
		};

		public string[] ExpectedEmptyDescriptionErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 30600
Description: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 30600
Description: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[2]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 30600
Description: "
		};
	}
}
