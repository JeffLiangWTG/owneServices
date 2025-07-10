using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts.Testing
{
	sealed class NctsCountryCodesFullListNC008TransformerTest : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Ncts";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_C0008_COUNTRY_CODES_FULL_LIST;

		protected override string CodeType => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_NC008_COUNTRY_CODES_FULL_LIST);

		protected override string DownloadUrl => CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_1);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new NctsCountryCodesFullListNC008Transformer(downloadLinks);

		protected override string InputFileName =>
			nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_C0008_COUNTRY_CODES_FULL_LIST);

		public string ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Andorra
StartDate: 
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
StartDate: 2005-07-01T00:00:00
EndDate: 2010-09-09T23:59:69
";
	}
}
