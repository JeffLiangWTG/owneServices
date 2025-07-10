using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class PackageTypeParserTests : NexDocParserTests<RefCusCodeList, CsvToPackageTypeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "BUNDLES", "2018-09-14T12:14:37.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Package Type due to empty Code, Description or an invalid Start Date.
Package Type Details:
Position: 0
Code: 
Description: BUNDLES
Start Date: 2018-09-14T12:14:37.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("BE", string.Empty, "2018-09-14T12:14:37.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Package Type due to empty Code, Description or an invalid Start Date.
Package Type Details:
Position: 0
Code: BE
Description: 
Start Date: 2018-09-14T12:14:37.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("BE", "BUNDLES", "2018-21-14T12:14:37.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Package Type due to empty Code, Description or an invalid Start Date.
Package Type Details:
Position: 0
Code: BE
Description: BUNDLES
Start Date: 2018-21-14T12:14:37.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("BE", "BUNDLES", "2018-09-14T12:14:37.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Package Type exists in downloaded List.
Package Type Details:
Position: 0
Code: BE
Description: BUNDLES
Start Date: 2018-09-14T12:14:37.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PACKAGE_TYPE;

		protected override string WebServiceMockResultFileName => "PackageTypeTestList.xml";

		protected override int WebServiceMockResultCount => 51;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new PackageTypeParser(listCodeSet);
	}
}
