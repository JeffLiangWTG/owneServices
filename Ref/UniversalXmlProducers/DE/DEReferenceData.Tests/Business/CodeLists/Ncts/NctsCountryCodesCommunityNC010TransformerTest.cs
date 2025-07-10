using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts.Testing
{
	sealed class NctsCountryCodesCommunityNC010TransformerTest : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Ncts";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_C0010_COUNTRY_CODES_COMMUNITY;

		protected override string CodeType => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_NC010_COUNTRY_CODES_COMMUNITY);

		protected override string DownloadUrl => CodeListsTestHelper.NctsDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_1);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new NctsCountryCodesCommunityNC010Transformer(downloadLinks);

		protected override string InputFileName =>
			nameof(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_C0010_COUNTRY_CODES_COMMUNITY);

		public string ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Aland Inseln
StartDate: 2010-09-10T00:00:00
EndDate: 
";

		public string ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: BE
Description: 
StartDate: 
EndDate: 
";

		public string ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: CY
Description: Zypern
StartDate: 2004-25-01T00:00:00
EndDate: 
";
	}
}
