using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import.Testing
{
	sealed class ImportDepartureAirportsEUIATTransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Import";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0600_DEPARTURE_AIRPORTS;

		protected override string CodeType => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_EUIAT_DEPARTURE_AIRPORTS);

		protected override string DownloadUrl => CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_1);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => (downloadLinks) => new ImportDepartureAirportsEUIATTransformer(downloadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0600_DEPARTURE_AIRPORTS);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Airport: 
Description: Arrabury (Qeensland), Australien
StartDate: 1996-11-05T00:00:00
EndDate: 
Percentage: 79
Zone: N
";

		public string ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Airport: AAE
Description: 
StartDate: 1996-11-05T00:00:00
EndDate: 
Percentage: 33
Zone: D
";

		public string ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Airport: AAG
Description: Arapoti, Brasilien
StartDate: 1996-21-05T00:00:00
EndDate: 
Percentage: 78
Zone: B
";
	}
}
