using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CommodityCusCodeTypeParserTests : NexDocParserTests<RefCusCodeType, CsvToCommodityTypeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "Dairy", "2018-08-31T00:00:00.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Commodity Type due to empty Code, Description or an invalid Start Date.
Commodity Type Details:
Position: 0
Code: 
Description: DAIRY
Start Date: 2018-08-31T00:00:00.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("D", string.Empty, "2018-08-31T00:00:00.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Commodity Type due to empty Code, Description or an invalid Start Date.
Commodity Type Details:
Position: 0
Code: D
Description: 
Start Date: 2018-08-31T00:00:00.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("D", "Dairy", "2018-14-31T00:00:00.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Commodity Type due to empty Code, Description or an invalid Start Date.
Commodity Type Details:
Position: 0
Code: D
Description: DAIRY
Start Date: 2018-14-31T00:00:00.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("D", "Dairy", "2018-08-31T00:00:00.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Commodity Type exists in downloaded List.
Commodity Type Details:
Position: 0
Code: D
Description: DAIRY
Start Date: 2018-08-31T00:00:00.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_COMMODITY_TYPE;

		protected override string WebServiceMockResultFileName => "CommodityTypeTestList.xml";

		protected override int WebServiceMockResultCount => 2;

		protected override string ExpectedTestFileName => "RefCusCodeTypeZZ_AU_COMMODITY_REFCUSCODETYPE.xml";

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeType>> ParserToRun => (listCodeSet) => new CommodityCusCodeTypeParser(listCodeSet);
	}
}
