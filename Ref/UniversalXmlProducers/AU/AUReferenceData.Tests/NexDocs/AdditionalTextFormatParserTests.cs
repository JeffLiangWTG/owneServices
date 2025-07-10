using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class AdditionalTextFormatParserTests : NexDocParserTests<RefCusCodeList, CsvToAdditionalTextSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescription(string.Empty, "Continuous (doesn’t contain carriage returns)");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Additional Text Format due to empty Code or Description.
Additional Text Format Details:
Position: 0
Code: 
Description: CONTINUOUS (DOESN’T CONTAIN CARRIAGE RETURNS)
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescription("C", string.Empty);

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Additional Text Format due to empty Code or Description.
Additional Text Format Details:
Position: 0
Code: C
Description: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescription("C", "Continuous (doesn’t contain carriage returns)");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Additional Text Format exists in downloaded List.
Additional Text Format Details:
Position: 0
Code: C
Description: CONTINUOUS (DOESN’T CONTAIN CARRIAGE RETURNS)
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_ADD_TEXT_FORMAT;

		protected override string WebServiceMockResultFileName => "AdditionalTextFormatTestList.xml";

		protected override int WebServiceMockResultCount => 3;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new AdditionalTextFormatParser(listCodeSet);
	}
}
