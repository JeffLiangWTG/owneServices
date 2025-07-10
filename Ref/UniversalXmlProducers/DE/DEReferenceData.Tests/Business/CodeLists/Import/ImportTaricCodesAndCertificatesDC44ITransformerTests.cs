using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import.Testing
{
	sealed class ImportTaricCodesAndCertificatesDC44ITransformerTests : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Import";

		protected override string[] CustomsCodeListIdentifiers =>
		[
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0200_TARIC_CODES_AND_CERTIFICATES,
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0255_ZELOS_DOCUMENTS
		];

		protected override string CodeType => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_DC44I_TARIC_CODES_AND_CERTIFICATES);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls =>
		[
			(CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifiers[0], TestConstants.AtlasVersion10_1), "Import", nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0200_TARIC_CODES_AND_CERTIFICATES)),
			(CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifiers[1], TestConstants.AtlasVersion10_1), "Import", nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0255_ZELOS_DOCUMENTS))
		];

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => (downloadLinks) => new ImportTaricCodesAndCertificatesDC44ITransformer(downloadLinks);

		public string[] ExpectedEmptyCodeErrorMessages =>
		[
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Echtheitsbescheinigung für Fleisch von Hausrindern, gefroren, als Crops and Blades und Brisket bezeichnete Teilstücke
StartDate: 1971-01-01T00:00:00
EndDate: 2065-04-30T23:59:59
Division: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
DocumentCode: 
Description: Echtheitsbescheinigung für Fleisch von Hausrindern, gefroren, als Crops and Blades und Brisket bezeichnete Teilstücke
StartDate: 1971-01-01T00:00:00
EndDate: 2065-04-30T23:59:59
Division: 1"
		];

		public string[] ExpectedEmptyDescriptionErrorMessages =>
		[
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: ACCB
Description: 
StartDate: 1971-01-01T00:00:00
EndDate: 2065-04-30T23:59:59
Division: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
DocumentCode: ACCB
Description: 
StartDate: 1971-01-01T00:00:00
EndDate: 2065-04-30T23:59:59
Division: 1"
		];

		public string[] ExpectedInvalidDateErrorMessages =>
		[
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: ACCC
Description: VO (EWG) Nr. 2782/75 über die Erzeugung von und den Verkehr mit Bruteiern und Küken von Hausgeflügel insbesondere Vorlage des Begleitpapiers gemäß Art. 13 beachten
StartDate: 1971-15-01T00:00:00
EndDate: 2008-04-30T23:59:59
Division: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
DocumentCode: ACCC
Description: VO (EWG) Nr. 2782/75 über die Erzeugung von und den Verkehr mit Bruteiern und Küken von Hausgeflügel insbesondere Vorlage des Begleitpapiers gemäß Art. 13 beachten
StartDate: 1971-15-01T00:00:00
EndDate: 2008-04-30T23:59:59
Division: 1"
		];
	}
}
