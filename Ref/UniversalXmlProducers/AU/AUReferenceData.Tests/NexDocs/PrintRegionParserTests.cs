using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class PrintRegionParserTests : NexDocParserTests<RefCusCodeList, CsvToPrintRegionSetConverter>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage
	{
		[Test]
		public void TestDuplicatedCodesWithDifferentCommodityTypeCode()
		{
			var duplicatedCodes = new[]
			{
				new ListCodeSet()
				{
					listItemCodeSet = new []
					{
						TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, "C"),
						TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Description, CodeValueType.@string, "Sydney Office"),
						TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.CommodityTypeCode, CodeValueType.@string, "O"),
					}
				},

				new ListCodeSet()
				{
					listItemCodeSet = new []
					{
						TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, "C"),
						TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Description, CodeValueType.@string, "Sydney Office"),
						TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.CommodityTypeCode, CodeValueType.@string, "D"),
					}
				}
			};

			AssertCorrectErrorMessage(duplicatedCodes, string.Empty);
		}

		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescription(string.Empty, "Sydney Office");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import Print Region due to empty Code or Description.
Print Region Details:
Position: 0
Code: 
Description: SYDNEY OFFICE
Commodity Type: 
Updated Date: 
";

		ListCodeSet ITestEmptyDescriptionErrorMessage.EmptyDescriptionTestData => TestHelperClass.CreateListCodeSetForCodeDescription("C", string.Empty);

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => @"Unable to import Print Region due to empty Code or Description.
Print Region Details:
Position: 0
Code: C
Description: 
Commodity Type: 
Updated Date: 
";

		protected override ListCodeSet DuplicateCodeTestData => TestHelperClass.CreateListCodeSetForCodeDescription("C", "Sydney Office");

		protected override string DuplicateCodeErrorMessage => @"Duplicate Print Region exists in downloaded List.
Print Region Details:
Position: 0
Code: C
Description: SYDNEY OFFICE
Commodity Type: 
Updated Date: 
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_PRINT_REGION;

		protected override string WebServiceMockResultFileName => "PrintRegionTestList.xml";

		protected override int WebServiceMockResultCount => 3;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusCodeList>> ParserToRun => (codeList) => new PrintRegionParser(codeList);
	}
}
