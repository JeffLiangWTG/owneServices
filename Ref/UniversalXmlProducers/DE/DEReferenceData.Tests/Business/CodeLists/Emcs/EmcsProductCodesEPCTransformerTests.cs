using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs.Testing
{
	class EmcsProductCodesEPCTransformerTests : CodeListsTSVTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Emcs";

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0036_PRODUCT_CODES;

		protected override string CodeType => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EPC_PRODUCT_CODES);

		protected override string DownloadUrl => CodeListsTestHelper.EmcsDownloadUrl("36.zip?__blob=publicationFile&amp;v=2");

		protected override string InputFileName =>
			nameof(CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0036_PRODUCT_CODES);

		protected override Func<Dictionary<string, string>, CodeListsParserTSV<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => (downloadLinks) => new EmcsProductCodesEPCTransformer(downloadLinks);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Qualifikator (Qualifier): 
Gültig von (Valid From): 20161015000000
Gültig bis (Valid To): 
Beschreibung (Description): Bier
ExciseProductsCategoryCode: B
UnitOfMeasureCode: 3
AlcoholicStrengthApplicabilityFlag: 1
DegreePlatoApplicabilityFlag: 1
DensityApplicabilityFlag: 0
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: E200
Qualifikator (Qualifier): 
Gültig von (Valid From): 20181220000000
Gültig bis (Valid To): 
Beschreibung (Description): 
ExciseProductsCategoryCode: E
UnitOfMeasureCode: 2
AlcoholicStrengthApplicabilityFlag: 0
DegreePlatoApplicabilityFlag: 0
DensityApplicabilityFlag: 1
";

		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: E300
Qualifikator (Qualifier): 
Gültig von (Valid From): 20182112000000
Gültig bis (Valid To): 
Beschreibung (Description): Aromaten der KN-C. 2707 10, 2707 20, 2707 30 und 2707 50
ExciseProductsCategoryCode: E
UnitOfMeasureCode: 2
AlcoholicStrengthApplicabilityFlag: 0
DegreePlatoApplicabilityFlag: 0
DensityApplicabilityFlag: 1
";
	}
}
