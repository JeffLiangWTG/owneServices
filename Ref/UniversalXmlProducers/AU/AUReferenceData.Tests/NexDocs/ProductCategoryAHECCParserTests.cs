using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class ProductCategoryAHECCParserTests : NexDocParserTests<RefCusCodeList, CsvToProductCategoryAHECCSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptySecondaryCodeErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => CreateListCodeSetForCodeTypeDescriptionCategoryDetails(string.Empty, "04029900");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Product Category due to empty Category Code or AHECC Code.
Product Category AHECC Details:
Position: 0
Category Code: 
AHECC Code: 04029900
";

		ListCodeSet ITestEmptySecondaryCodeErrorMessage.InvalidSecondaryCodeTestData => CreateListCodeSetForCodeTypeDescriptionCategoryDetails("DC0699", string.Empty);

		string ITestEmptySecondaryCodeErrorMessage.InvalidSecondaryCodeErrorMessage => @"Unable to import Product Category due to empty Category Code or AHECC Code.
Product Category AHECC Details:
Position: 0
Category Code: DC0699
AHECC Code: 
";

		protected override ListCodeSet DuplicateCodeTestData => CreateListCodeSetForCodeTypeDescriptionCategoryDetails("DC0699", "04029900");

		protected override string DuplicateCodeErrorMessage => string.Empty;

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PRODUCT_CATEGORY_AHECC;

		protected override string WebServiceMockResultFileName => "ProductCategoryAHECCTestList.xml";

		protected override int WebServiceMockResultCount => 1701;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new ProductCategoryAHECCParser(listCodeSet);

		ListCodeSet CreateListCodeSetForCodeTypeDescriptionCategoryDetails(string categoryCode, string aheccCode)
		{
			return new ListCodeSet
			{
				listItemCodeSet = new ItemCodeSet[]
				{
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.CategoryCodeFull, CodeValueType.@string, categoryCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.AHECCCode, CodeValueType.@string, aheccCode),
				}
			};
		}
	}
}
