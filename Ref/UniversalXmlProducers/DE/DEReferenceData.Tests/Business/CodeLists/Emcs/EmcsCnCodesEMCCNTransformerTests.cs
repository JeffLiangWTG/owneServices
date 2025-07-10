using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs.Testing
{
	class EmcsCnCodesEMCCNTransformerTests : CodeListsTSVTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Emcs";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0037_CN_CODES;

		protected override string CodeType => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCCN_CN_CODES);

		protected override string DownloadUrl => CodeListsTestHelper.EmcsDownloadUrl("37.zip?__blob=publicationFile&amp;v=2");

		protected override string InputFileName =>
			nameof(CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0037_CN_CODES);

		protected override Func<Dictionary<string, string>, CodeListsParserTSV<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new EmcsCnCodesEMCCNTransformer(downloadLinks);

		public string ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Qualifikator (Qualifier): 
Gültig von (Valid From): 20170513000000
Gültig bis (Valid To): 
Beschreibung (Description): Rohes Sojaöl auch entschleimt, zu technischen oder industriellen Zwecken, ausgenommen zum Herstellen von Lebensmitteln
";

		public string ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 15079010
Qualifikator (Qualifier): 
Gültig von (Valid From): 20170513000000
Gültig bis (Valid To): 
Beschreibung (Description): 
";

		public string ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 15081010
Qualifikator (Qualifier): 
Gültig von (Valid From): 20171513000000
Gültig bis (Valid To): 
Beschreibung (Description): Rohes Erdnussöl zu technischen oder industriellen Zwecken, ausgenommen zum Herstellen von Lebensmitteln.
";
	}
}
