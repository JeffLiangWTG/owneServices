using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class SupplementaryCodeParserTests : NexDocParserTests<RefCusCodeList, CsvToSupplementaryCodeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "US BEEF FOR GRINDING", "2000-06-15T04:51:29.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Supplementary Code due to empty Code, Description or an invalid Start Date.
Supplementary Code Details:
Position: 0
Code: 
Description: US BEEF FOR GRINDING
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";
		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("GB", string.Empty, "2000-06-15T04:51:29.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Supplementary Code due to empty Code, Description or an invalid Start Date.
Supplementary Code Details:
Position: 0
Code: GB
Description: 
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("GB", "US BEEF FOR GRINDING", "2000-13-15T04:51:29.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Supplementary Code due to empty Code, Description or an invalid Start Date.
Supplementary Code Details:
Position: 0
Code: GB
Description: US BEEF FOR GRINDING
Start Date: 2000-13-15T04:51:29.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("E", "US BEEF FOR GRINDING", "2000-06-15T04:51:29.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Supplementary Code exists in downloaded List.
Supplementary Code Details:
Position: 0
Code: E
Description: US BEEF FOR GRINDING
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_SUPPLEMENTARY_CODE;

		protected override string WebServiceMockResultFileName => "SupplementaryCodeTestList.xml";

		protected override int WebServiceMockResultCount => 43;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new SupplementaryCodeParser(listCodeSet);
	}
}
