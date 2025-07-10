using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class PackTypeParserTests : NexDocParserTests<RefCusCodeList, CsvToPackTypeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "BAG IN A BOX", "2018-09-14T12:14:37.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Pack Type due to empty Code, Description or an invalid Start Date.
Pack Type Details:
Position: 0
Code: 
Description: BAG IN A BOX
Start Date: 2018-09-14T12:14:37.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("BB", string.Empty, "2018-09-14T12:14:37.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Pack Type due to empty Code, Description or an invalid Start Date.
Pack Type Details:
Position: 0
Code: BB
Description: 
Start Date: 2018-09-14T12:14:37.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("BB", "BAG IN A BOX", "2018-18-14T12:14:37.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Pack Type due to empty Code, Description or an invalid Start Date.
Pack Type Details:
Position: 0
Code: BB
Description: BAG IN A BOX
Start Date: 2018-18-14T12:14:37.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("BB", "BAG IN A BOX", "2018-09-14T12:14:37.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Pack Type exists in downloaded List.
Pack Type Details:
Position: 0
Code: BB
Description: BAG IN A BOX
Start Date: 2018-09-14T12:14:37.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PACK_TYPE;

		protected override string WebServiceMockResultFileName => "PackTypeTestList.xml";

		protected override int WebServiceMockResultCount => 58;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new PackTypeParser(listCodeSet);
	}
}
