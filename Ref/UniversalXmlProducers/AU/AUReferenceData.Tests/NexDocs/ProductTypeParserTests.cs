using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class ProductTypeParserTests : NexDocParserTests<RefCusCodeList, CsvToProductTypeSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidStartDateErrorMessage,
		ITestEmptySecondaryCodeErrorMessage
	{
		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => CreateListCodeSetForCodeSecondaryCodeDescriptionStartDate(string.Empty, NexDocConstants.ItemCodeSetKeys.Type, "BUT", "BUTTER", "2017-03-21T00:00:00.000");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Product Type due to empty Code, Type, Description or an invalid Start Date.
Product Type Details:
Position: 0
Code: 
Type: BUT
Description: BUTTER
Start Date: 2017-03-21T00:00:00.000
End Date: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => CreateListCodeSetForCodeSecondaryCodeDescriptionStartDate("D", NexDocConstants.ItemCodeSetKeys.Type, string.Empty, "BUTTER", "2017-03-21T00:00:00.000");

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Product Type due to empty Code, Type, Description or an invalid Start Date.
Product Type Details:
Position: 0
Code: D
Type: 
Description: BUTTER
Start Date: 2017-03-21T00:00:00.000
End Date: 
Updated Date: 
";
		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => CreateListCodeSetForCodeSecondaryCodeDescriptionStartDate("D", NexDocConstants.ItemCodeSetKeys.Type, "BUT", "BUTTER", "2017-03-32T00:00:00.000");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import Product Type due to empty Code, Type, Description or an invalid Start Date.
Product Type Details:
Position: 0
Code: D
Type: BUT
Description: BUTTER
Start Date: 2017-03-32T00:00:00.000
End Date: 
Updated Date: 
";
		ListCodeSet ITestEmptySecondaryCodeErrorMessage.InvalidSecondaryCodeTestData => CreateListCodeSetForCodeSecondaryCodeDescriptionStartDate("D", NexDocConstants.ItemCodeSetKeys.Type, string.Empty, "BUTTER", "2017-03-21T00:00:00.000");

		string ITestEmptySecondaryCodeErrorMessage.InvalidSecondaryCodeErrorMessage => @"Unable to import Product Type due to empty Code, Type, Description or an invalid Start Date.
Product Type Details:
Position: 0
Code: D
Type: 
Description: BUTTER
Start Date: 2017-03-21T00:00:00.000
End Date: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => CreateListCodeSetForCodeSecondaryCodeDescriptionStartDate("D", NexDocConstants.ItemCodeSetKeys.Type, "BUT", "BUTTER", "2017-03-21T00:00:00.000");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Product Type exists in downloaded List.
Product Type Details:
Position: 0
Code: D
Type: BUT
Description: BUTTER
Start Date: 2017-03-21T00:00:00.000
End Date: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PRODUCT_TYPE;

		protected override string WebServiceMockResultFileName => "ProductTypeTestList.xml";

		protected override int WebServiceMockResultCount => 62;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (listCodeSet) => new ProductTypeParser(listCodeSet);

		ListCodeSet CreateListCodeSetForCodeSecondaryCodeDescriptionStartDate(string code, string secondaryCodeKey, string secondaryCode, string description, string startDate)
		{
			return new ListCodeSet
			{
				listItemCodeSet = new ItemCodeSet[]
				{
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, code),
					TestHelperClass.CreateItemCodeSet(secondaryCodeKey, CodeValueType.@string, secondaryCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Description, CodeValueType.@string, description),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.StartDate, CodeValueType.dateTime, startDate)
				}
			};
		}
	}
}
