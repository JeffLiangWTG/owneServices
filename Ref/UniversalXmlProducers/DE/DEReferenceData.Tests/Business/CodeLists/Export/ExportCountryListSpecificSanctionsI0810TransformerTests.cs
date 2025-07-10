using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportCountryListSpecificSanctionsI0810TransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Export";

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers
			.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS;

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS);

		protected override string DownloadUrl => CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifier, TestConstants.AESVersion3_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new ExportCountryListSpecificSanctionsI0810Transformer(downloadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Iran
StartDate: 
EndDate: 
";

		public string ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: KP
Description: 
StartDate: 2016-05-01T00:00:00
EndDate: 
";

		public string ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: MM
Description: Myanmar
StartDate: 2016-15-01T00:00:00
EndDate: 
";
	}
}
