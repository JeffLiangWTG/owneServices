using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class TreatmentTests : NexDocParserTests<RefCusCodeList, CsvToTreatmentSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "COLD DIPPED", "2018-07-20T10:53:11.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Treatment due to empty Code, Description or an invalid Start Date.
Treatment Details:
Position: 0
Code: 
Description: COLD DIPPED
Start Date: 2018-07-20T10:53:11.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("COLD", string.Empty, "2018-07-20T10:53:11.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Treatment due to empty Code, Description or an invalid Start Date.
Treatment Details:
Position: 0
Code: COLD
Description: 
Start Date: 2018-07-20T10:53:11.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("COLD", "COLD DIPPED", "2018-07-45T10:53:11.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Treatment due to empty Code, Description or an invalid Start Date.
Treatment Details:
Position: 0
Code: COLD
Description: COLD DIPPED
Start Date: 2018-07-45T10:53:11.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("COLD", "COLD DIPPED", "2018-07-20T10:53:11.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Treatment exists in downloaded List.
Treatment Details:
Position: 0
Code: COLD
Description: COLD DIPPED
Start Date: 2018-07-20T10:53:11.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_TREATMENT;

		protected override string WebServiceMockResultFileName => "TreatmentTestList.xml";

		protected override int WebServiceMockResultCount => 18;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new TreatmentParser(listCodeSet);
	}
}
