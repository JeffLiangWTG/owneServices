using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs.Testing
{
	class EmcsPackTypesEMCPKTransformerTests : CodeListsTSVTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage
	{
		protected override string System => "Emcs";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0017_PACK_TYPES;

		protected override string CodeType => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCPK_PACK_TYPES);

		protected override string DownloadUrl => CodeListsTestHelper.EmcsDownloadUrl("17.zip?__blob=publicationFile&amp;v=1");

		protected override string InputFileName =>
			nameof(CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0017_PACK_TYPES);

		protected override Func<Dictionary<string, string>, CodeListsParserTSV<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new EmcsPackTypesEMCPKTransformer(downloadLinks);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Qualifikator (Qualifier): 
Gültig von (Valid From): 20100313000000
Gültig bis (Valid To): 
Beschreibung (Description): Ballen, gepresst
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: CX
Qualifikator (Qualifier): 
Gültig von (Valid From): 20170513000000
Gültig bis (Valid To): 
Beschreibung (Description): 
";
	}
}
