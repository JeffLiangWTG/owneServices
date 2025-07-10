using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class ProductCategoryParserTests : NexDocParserTests<RefCusCodeList, CsvToProductCategorySetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestEmptySecondaryCodeErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => CreateListCodeSetForCodeTypeDescriptionCategoryDetails("BEV", "X", "BEVERAGE", string.Empty, "POMEGRANATE BLUEBERRY BEVERAGE");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Product Category due to empty Category Code, Type or Category Description.
Product Category Details:
Position: 0
Code: BEV
Type: X
Description: BEVERAGE
Category Code: 
Category Description: POMEGRANATE BLUEBERRY BEVERAGE
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => CreateListCodeSetForCodeTypeDescriptionCategoryDetails("BEV", "X", "BEVERAGE", "CT0817", string.Empty);

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Product Category due to empty Category Code, Type or Category Description.
Product Category Details:
Position: 0
Code: BEV
Type: X
Description: BEVERAGE
Category Code: CT0817
Category Description: 
";

		ListCodeSet ITestEmptySecondaryCodeErrorMessage.InvalidSecondaryCodeTestData => CreateListCodeSetForCodeTypeDescriptionCategoryDetails("BEV", string.Empty, "BEVERAGE", "CT0817", "POMEGRANATE BLUEBERRY BEVERAGE");

		string ITestEmptySecondaryCodeErrorMessage.InvalidSecondaryCodeErrorMessage => @"Unable to import Product Category due to empty Category Code, Type or Category Description.
Product Category Details:
Position: 0
Code: BEV
Type: 
Description: BEVERAGE
Category Code: CT0817
Category Description: POMEGRANATE BLUEBERRY BEVERAGE
";

		protected override ListCodeSet DuplicateCodeTestData => CreateListCodeSetForCodeTypeDescriptionCategoryDetails("BEV", "X", "BEVERAGE", "CT0817", "POMEGRANATE BLUEBERRY BEVERAGE");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Product Category exists in downloaded List.
Product Category Details:
Position: 0
Code: BEV
Type: X
Description: BEVERAGE
Category Code: CT0817
Category Description: POMEGRANATE BLUEBERRY BEVERAGE
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PRODUCT_CATEGORY;

		protected override string WebServiceMockResultFileName => "ProductCategoryTestList.xml";

		protected override int WebServiceMockResultCount => 578;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new ProductCategoryParser(listCodeSet);

		ListCodeSet CreateListCodeSetForCodeTypeDescriptionCategoryDetails(string code, string type, string description, string categoryCode, string categoryDescription)
		{
			return new ListCodeSet
			{
				listItemCodeSet = new ItemCodeSet[]
				{
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, code),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.ProductType, CodeValueType.@string, type),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Description, CodeValueType.@string, description),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.CategoryCode, CodeValueType.@string, categoryCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.CategoryDescription, CodeValueType.@string, categoryDescription)
				}
			};
		}
	}
}
