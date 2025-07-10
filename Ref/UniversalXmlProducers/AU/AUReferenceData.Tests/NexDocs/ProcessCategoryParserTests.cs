using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class ProcessCategoryParserTests : NexDocParserTests<RefCusCodeList, CsvToProcessCategorySetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "[03B] RAW GROUND, COMMINUTED, OR OTHERWISE NON-INTACT BEEF: GROUND BEEF", "2000-06-15T04:51:29.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Process Category due to empty Code, Description or an invalid Start Date.
Process Category Details:
Position: 0
Code: 
Description: [03B] RAW GROUND, COMMINUTED, OR OTHERWISE NON-INTACT BEEF: GROUND BEEF
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("03B007001", string.Empty, "2000-06-15T04:51:29.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Process Category due to empty Code, Description or an invalid Start Date.
Process Category Details:
Position: 0
Code: 03B007001
Description: 
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("03B007001", "[03B] RAW GROUND, COMMINUTED, OR OTHERWISE NON-INTACT BEEF: GROUND BEEF", "2000-45-15T04:51:29.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Process Category due to empty Code, Description or an invalid Start Date.
Process Category Details:
Position: 0
Code: 03B007001
Description: [03B] RAW GROUND, COMMINUTED, OR OTHERWISE NON-INTACT BEEF: GROUND BEEF
Start Date: 2000-45-15T04:51:29.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("03B007001", "[03B] RAW GROUND, COMMINUTED, OR OTHERWISE NON-INTACT BEEF: GROUND BEEF", "2000-06-15T04:51:29.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Process Category exists in downloaded List.
Process Category Details:
Position: 0
Code: 03B007001
Description: [03B] RAW GROUND, COMMINUTED, OR OTHERWISE NON-INTACT BEEF: GROUND BEEF
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PROCESS_CATEGORY;

		protected override string WebServiceMockResultFileName => "ProcessCategoryTestList.xml";

		protected override int WebServiceMockResultCount => 295;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new ProcessCategoryParser(listCodeSet);
	}
}
