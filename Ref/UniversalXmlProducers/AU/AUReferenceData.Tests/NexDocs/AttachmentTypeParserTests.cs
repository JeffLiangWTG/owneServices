using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class AttachmentTypeParserTests : NexDocParserTests<RefCusCodeList, CsvToAttachmentTypeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "AUSTRALIAN FISHERIES MANAGEMENT AUT", "2010-06-15T04:45:27.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Attachment Type due to empty Code, Description or an invalid Start Date.
Attachment Type Details:
Position: 0
Code: 
Description: AUSTRALIAN FISHERIES MANAGEMENT AUT
Start Date: 2010-06-15T04:45:27.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("AFM", string.Empty, "2010-06-15T04:45:27.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Attachment Type due to empty Code, Description or an invalid Start Date.
Attachment Type Details:
Position: 0
Code: AFM
Description: 
Start Date: 2010-06-15T04:45:27.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("AFM", "AUSTRALIAN FISHERIES MANAGEMENT AUT", "2010-06-56T04:45:27.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Attachment Type due to empty Code, Description or an invalid Start Date.
Attachment Type Details:
Position: 0
Code: AFM
Description: AUSTRALIAN FISHERIES MANAGEMENT AUT
Start Date: 2010-06-56T04:45:27.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("AFM", "AUSTRALIAN FISHERIES MANAGEMENT AUT", "2010-06-15T04:45:27.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Attachment Type exists in downloaded List.
Attachment Type Details:
Position: 0
Code: AFM
Description: AUSTRALIAN FISHERIES MANAGEMENT AUT
Start Date: 2010-06-15T04:45:27.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_ATTACHMENT_TYPE;

		protected override string WebServiceMockResultFileName => "AttachmentTypeTestList.xml";

		protected override int WebServiceMockResultCount => 16;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new AttachmentTypeParser(listCodeSet);
	}
}
