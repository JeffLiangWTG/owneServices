using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class NatureOfCommodityParserTests : NexDocParserTests<RefCusCodeList, CsvToNatureOfCommoditySetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "TESTING NATURE OF COMMODITY", "2018-08-30T10:07:18.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Nature Of Commodity due to empty Code, Description or an invalid Start Date.
Nature Of Commodity Details:
Position: 0
Code: 
Description: TESTING NATURE OF COMMODITY
Start Date: 2018-08-30T10:07:18.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("TT", string.Empty, "2018-08-30T10:07:18.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Nature Of Commodity due to empty Code, Description or an invalid Start Date.
Nature Of Commodity Details:
Position: 0
Code: TT
Description: 
Start Date: 2018-08-30T10:07:18.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("TT", "TESTING NATURE OF COMMODITY", "2018-08-32T10:07:18.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Nature Of Commodity due to empty Code, Description or an invalid Start Date.
Nature Of Commodity Details:
Position: 0
Code: TT
Description: TESTING NATURE OF COMMODITY
Start Date: 2018-08-32T10:07:18.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("TT", "TESTING NATURE OF COMMODITY", "2018-08-30T10:07:18.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Nature Of Commodity exists in downloaded List.
Nature Of Commodity Details:
Position: 0
Code: TT
Description: TESTING NATURE OF COMMODITY
Start Date: 2018-08-30T10:07:18.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_NATURE_OF_COMMODITY;

		protected override string WebServiceMockResultFileName => "NatureOfCommodityTestList.xml";

		protected override int WebServiceMockResultCount => 1;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new NatureOfCommodityParser(listCodeSet);
	}
}
