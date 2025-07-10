using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class PermitTypeParserTests : NexDocParserTests<RefCusCodeList, CsvToPermitTypeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "AUSTRALIAN FISHERIES MANAGEMENT AUT", "2019-04-23T18:43:49.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Permit Type due to empty Code, Description or an invalid Start Date.
Permit Type Details:
Position: 0
Code: 
Description: AUSTRALIAN FISHERIES MANAGEMENT AUT
Start Date: 2019-04-23T18:43:49.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("AFM", string.Empty, "2019-04-23T18:43:49.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Permit Type due to empty Code, Description or an invalid Start Date.
Permit Type Details:
Position: 0
Code: AFM
Description: 
Start Date: 2019-04-23T18:43:49.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("AFM", "AUSTRALIAN FISHERIES MANAGEMENT AUT", "2019-00-23T18:43:49.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Permit Type due to empty Code, Description or an invalid Start Date.
Permit Type Details:
Position: 0
Code: AFM
Description: AUSTRALIAN FISHERIES MANAGEMENT AUT
Start Date: 2019-00-23T18:43:49.000
End Date: 
Updated Date: 
";
		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("AFM", "AUSTRALIAN FISHERIES MANAGEMENT AUT", "2019-04-23T18:43:49.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Permit Type exists in downloaded List.
Permit Type Details:
Position: 0
Code: AFM
Description: AUSTRALIAN FISHERIES MANAGEMENT AUT
Start Date: 2019-04-23T18:43:49.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PERMIT_TYPE;

		protected override string WebServiceMockResultFileName => "PermitTypeTestList.xml";

		protected override int WebServiceMockResultCount => 13;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (codeList) => new PermitTypeParser(codeList);
	}
}
