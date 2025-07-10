using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs.Testing
{
	class EmcsMemberStatesEMCMSTransformerTest : CodeListsTSVTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Emcs";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0011_MEMBER_STATES;

		protected override string CodeType => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCMS_MEMBER_STATES);

		protected override string DownloadUrl => CodeListsTestHelper.EmcsDownloadUrl("11.zip?__blob=publicationFile&amp;v=1");

		protected override string InputFileName =>
			nameof(CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0011_MEMBER_STATES);

		protected override Func<Dictionary<string, string>, CodeListsParserTSV<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new EmcsMemberStatesEMCMSTransformer(downloadLinks);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Qualifikator (Qualifier): 
Gültig von (Valid From): 20130320000000
Gültig bis (Valid To): 
Beschreibung (Description): Belgien
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: LU
Qualifikator (Qualifier): 
Gültig von (Valid From): 20130320000000
Gültig bis (Valid To): 
Beschreibung (Description): 
";

		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: SE
Qualifikator (Qualifier): 
Gültig von (Valid From): 20131320000000
Gültig bis (Valid To): 
Beschreibung (Description): Schweden
";
	}
}
