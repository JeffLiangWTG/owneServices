using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class UnitOfMeasurementParserTests : NexDocParserTests<RefCusCodeList, CsvToUnitOfMeasurementSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => CreateListCodeSetSymbolCodeDescriptionStartDate(string.Empty, "CENTIGRAM", "2000-02-13T00:00:00.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Unit Of Measurement due to empty Symbol Code, Description or an invalid Start Date.
Unit Of Measurement Details:
Position: 0
Code: 
Symbol Code: 
System Code: 
Description: CENTIGRAM
Start Date: 2000-02-13T00:00:00.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => CreateListCodeSetSymbolCodeDescriptionStartDate("CGM", string.Empty, "2000-02-13T00:00:00.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Unit Of Measurement due to empty Symbol Code, Description or an invalid Start Date.
Unit Of Measurement Details:
Position: 0
Code: 
Symbol Code: CGM
System Code: 
Description: 
Start Date: 2000-02-13T00:00:00.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => CreateListCodeSetSymbolCodeDescriptionStartDate("CGM", "CENTIGRAM", "2000-133-13T00:00:00.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Unit Of Measurement due to empty Symbol Code, Description or an invalid Start Date.
Unit Of Measurement Details:
Position: 0
Code: 
Symbol Code: CGM
System Code: 
Description: CENTIGRAM
Start Date: 2000-133-13T00:00:00.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => CreateListCodeSetSymbolCodeDescriptionStartDate("CGM", "CENTIGRAM", "2000-02-13T00:00:00.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Unit Of Measurement exists in downloaded List.
Unit Of Measurement Details:
Position: 0
Code: 
Symbol Code: CGM
System Code: 
Description: CENTIGRAM
Start Date: 2000-02-13T00:00:00.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_UNIT_OF_MEASUREMENT;

		protected override string WebServiceMockResultFileName => "UnitOfMeasurementTestList.xml";

		protected override int WebServiceMockResultCount => 73;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new UnitOfMeasurementParser(listCodeSet);

		public static ListCodeSet CreateListCodeSetSymbolCodeDescriptionStartDate(string symbolCode, string description, string startDate)
		{
			return new ListCodeSet
			{
				listItemCodeSet = new ItemCodeSet[]
				{
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.SymbolCode, CodeValueType.@string, symbolCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Description, CodeValueType.@string, description),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.StartDate, CodeValueType.dateTime, startDate)
				}
			};
		}
	}
}
