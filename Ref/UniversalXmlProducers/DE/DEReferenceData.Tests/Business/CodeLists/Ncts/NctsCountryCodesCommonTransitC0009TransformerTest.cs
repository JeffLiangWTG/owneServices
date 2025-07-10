using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts.Testing
{
	sealed class NctsCountryCodesCommonTransitC0009TransformerTest : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Ncts";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_C0009_COUNTRY_CODES_COMMON_TRANSIT;

		protected override string CodeType => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_C0009_COUNTRY_CODES_COMMON_TRANSIT);

		protected override string DownloadUrl => CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_1);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new NctsCountryCodesCommonTransitC0009Transformer(downloadLinks);

		protected override string InputFileName =>
			nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_C0009_COUNTRY_CODES_COMMON_TRANSIT);

		public string ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Andorra
StartDate: 2007-06-14T00:00:00
EndDate: 
";

		public string ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: AT
Description: 
StartDate: 
EndDate: 
";

		public string ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: AX
Description: ÅLAND ISLANDS
StartDate: 2005-12-07T00:00:00
EndDate: 2010-09-09T23:59:69
";
	}
}
