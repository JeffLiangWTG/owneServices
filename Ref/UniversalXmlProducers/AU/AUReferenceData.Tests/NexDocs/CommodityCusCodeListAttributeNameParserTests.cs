using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CommodityCusCodeListAttributeNameParserTests : NexDocParserTests<RefCusCodeListAttributeName, CsvToCommodityTypeSetConverter>
	{
		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescriptionStartDate("D", "Dairy", "2018-08-31T00:00:00.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Commodity Type exists in downloaded List.
Commodity Type Details:
Position: 0
Code: D
Description: DAIRY
Start Date: 2018-08-31T00:00:00.000
End Date: 
Updated Date: 
";
		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_COMMODITY_TYPE;

		protected override string WebServiceMockResultFileName => "CommodityTypeTestList.xml";

		protected override int WebServiceMockResultCount => 2;

		protected override string ExpectedTestFileName => "RefCusCodeTypeZZ_AU_COMMODITY_REFCUSCODELISTATTRIBUTENAME.xml";

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeListAttributeName>> ParserToRun => (listCodeSet) => new CommodityCusCodeListAttributeNameParser(listCodeSet);
	}
}
