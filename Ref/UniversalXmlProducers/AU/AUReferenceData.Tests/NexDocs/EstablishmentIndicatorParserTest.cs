using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class EstablishmentIndicatorParserTest : NexDocParserTests<RefCusCodeList, CsvToEstablishmentIndicatorSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "Catcher Vessel", "2000-06-15T04:51:29.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Establishment Indicator due to empty Code, Description or an invalid Start Date.
Establishment Indicator Details:
Position: 0
Code: 
Description: CATCHER VESSEL
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("CT", string.Empty, "2000-06-15T04:51:29.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Establishment Indicator due to empty Code, Description or an invalid Start Date.
Establishment Indicator Details:
Position: 0
Code: CT
Description: 
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("CT", "Catcher Vessel", "2000-45-15T04:51:29.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Establishment Indicator due to empty Code, Description or an invalid Start Date.
Establishment Indicator Details:
Position: 0
Code: CT
Description: CATCHER VESSEL
Start Date: 2000-45-15T04:51:29.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("CT", "Catcher Vessel", "2000-06-15T04:51:29.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Establishment Indicator exists in downloaded List.
Establishment Indicator Details:
Position: 0
Code: CT
Description: CATCHER VESSEL
Start Date: 2000-06-15T04:51:29.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_ESTABLISHMENT_INDICATOR;

		protected override string WebServiceMockResultFileName => "EstablishmentIndicatorTestList.xml";

		protected override int WebServiceMockResultCount => 14;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new EstablishmentIndicatorParser(listCodeSet);
	}
}
