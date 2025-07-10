using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class NexDocCodesParserTests : NexDocParserTests<RefCusAUNexdocECMCode, CsvToCodesSetConverter>, ITestInvalidStartDateErrorMessage, ITestEmptyCodeErrorMessage
	{
		ListCodeSet ITestInvalidStartDateErrorMessage.InvalidStartDateTestData => CreateListCodeSetForNexdocECMCode("", "", "", "", "");

		string ITestInvalidStartDateErrorMessage.InvalidStartDataErrorMessage => @"Unable to import NexDoc Codes due to empty Code.
NexDoc Code Position: 0
";

		protected override ListCodeSet DuplicateCodeTestData => CreateListCodeSetForNexdocECMCode("D", "C", "AMF", "BB", "DM");

		protected override string DuplicateCodeErrorMessage => @"Duplicate NexDoc Codes exists in downloaded List.
NexDoc Code Details:
Position: 0
Code: D|C|AMF|BB|DM
";

		protected override string CodeSetName => NexDocConstants.CodeListTypes.ECM_CODES;

		protected override string WebServiceMockResultFileName => "CodesTestList.xml";

		protected override string ExpectedTestFileName => "RefCusAUNexdocECMCodeZZ_AU_ECM_CODES.xml";

		protected override int WebServiceMockResultCount => 5057;

		protected override Func<IListCodeSet[], BaseNexDocCodeParser<RefCusAUNexdocECMCode>> ParserToRun => (listCodeSet) => new NexDocCodesParser(listCodeSet);

		ListCodeSet ITestEmptyCodeErrorMessage.EmptyCodeTestData => CreateListCodeSetForNexdocECMCode("", "", "", "", "");

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => @"Unable to import NexDoc Codes due to empty Code.
NexDoc Code Position: 0
";

		ListCodeSet CreateListCodeSetForNexdocECMCode(string commodityCode, string preservationCode, string productTypeCode, string packTypeCode, string supplementaryCode)
		{
			return new ListCodeSet
			{
				listItemCodeSet = new ItemCodeSet[]
				{
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.CommodityCode, CodeValueType.@string, commodityCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.PreservationCode, CodeValueType.@string, preservationCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.ProductTypeCode, CodeValueType.@string, productTypeCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.PackTypeCode, CodeValueType.@string, packTypeCode),
					TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.SupplementaryCode, CodeValueType.@string, supplementaryCode)
				}
			};
		}
	}
}
