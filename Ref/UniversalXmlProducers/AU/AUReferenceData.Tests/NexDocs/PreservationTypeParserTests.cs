using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class PreservationTypeParserTests : NexDocParserTests<RefCusCodeList, CsvToPreservationTypeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "chilled", "2018-07-20T10:53:11.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Preservation Type due to empty Code, Description or an invalid Start Date.
Preservation Type Details:
Position: 0
Code: 
Description: CHILLED
Start Date: 2018-07-20T10:53:11.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("C", string.Empty, "2018-07-20T10:53:11.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Preservation Type due to empty Code, Description or an invalid Start Date.
Preservation Type Details:
Position: 0
Code: C
Description: 
Start Date: 2018-07-20T10:53:11.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("C", "chilled", "null");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Preservation Type due to empty Code, Description or an invalid Start Date.
Preservation Type Details:
Position: 0
Code: C
Description: CHILLED
Start Date: null
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("C", "chilled", "2018-07-20T10:53:11.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Preservation Type exists in downloaded List.
Preservation Type Details:
Position: 0
Code: C
Description: CHILLED
Start Date: 2018-07-20T10:53:11.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PRESERVATION_TYPE;

		protected override string WebServiceMockResultFileName => "PreservationTypeTestList.xml";

		protected override int WebServiceMockResultCount => 19;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new PreservationTypeParser(listCodeSet);
	}
}
