using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CNCodeParserTests : NexDocParserTests<RefCusCodeList, CsvToCombinedNomenclatureSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate(string.Empty, "DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN", "2010-06-15T04:45:27.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import CN Code due to empty Code, Description or an invalid Start Date.
CN Code Details:
Position: 0
Code: 
Description: DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN
Start Date: 2010-06-15T04:45:27.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("401", string.Empty, "2022-01-01T00:00:00.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import CN Code due to empty Code, Description or an invalid Start Date.
CN Code Details:
Position: 0
Code: 401
Description: 
Start Date: 2022-01-01T00:00:00.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("401", "DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN", "2011-06-31T04:45:27.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import CN Code due to empty Code, Description or an invalid Start Date.
CN Code Details:
Position: 0
Code: 401
Description: DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN
Start Date: 2011-06-31T04:45:27.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("401", "DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN", "2010-06-15T04:45:27.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate CN Code exists in downloaded List.
CN Code Details:
Position: 0
Code: 401
Description: DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN
Start Date: 2010-06-15T04:45:27.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_CN_CODE;

		protected override string WebServiceMockResultFileName => "CNCodeTestList.xml";

		protected override string ExpectedTestFileName => "RefCusCodeListZZ_AU_ECM_CNCode.xml";

		protected override int WebServiceMockResultCount => 23;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new CombinedNomenclatureParser(listCodeSet);
	}
}
