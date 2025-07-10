using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class DeclarationStatementsParserTests : NexDocParserTests<RefCusCodeList, CsvToDeclarationStatmentSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescription(string.Empty, "Milk products in this shipment meet Algerian product testing requirements.");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Declaration Statement due to empty Code or Description.
Declaration Statement Details:
Position: 0
Code: 
Description: MILK PRODUCTS IN THIS SHIPMENT MEET ALGERIAN PRODUCT TESTING REQUIREMENTS.
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescription("ALGDAI", string.Empty);

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Declaration Statement due to empty Code or Description.
Declaration Statement Details:
Position: 0
Code: ALGDAI
Description: 
Updated Date: 
";
		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescription("ALGDAI", "Milk products in this shipment meet Algerian product testing requirements.");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Declaration Statement exists in downloaded List.
Declaration Statement Details:
Position: 0
Code: ALGDAI
Description: MILK PRODUCTS IN THIS SHIPMENT MEET ALGERIAN PRODUCT TESTING REQUIREMENTS.
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_DECLARATION;

		protected override string WebServiceMockResultFileName => "DeclarationStatmentTestList.xml";

		protected override int WebServiceMockResultCount => 17;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new DeclarationStatementsParser(listCodeSet);
	}
}
