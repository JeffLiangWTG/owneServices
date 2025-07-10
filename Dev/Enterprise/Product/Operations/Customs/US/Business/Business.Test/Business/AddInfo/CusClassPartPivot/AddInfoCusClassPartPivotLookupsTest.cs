using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoCusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductExclusionList()
		{
			var addInfo = new AddInfoCusClassPartPivot(Factory.New<CusClassPartPivot>().CI_AddInfoInfo);
			var lookups = addInfo.Lookups;
			var pivot = addInfo.Parent;
			void AssertProductExclusionList(string tariffNum, string[] expectedCodes)
			{
				pivot.CI_TariffNum = tariffNum;
				AssertContainsExactElementsInAnyOrder(expectedCodes, lookups.ProductExclusionList.GetAllCodes());
			}

			AssertProductExclusionList(string.Empty, new[] { AdditionalDeclarationTypeCodeList.Codes._02, AdditionalDeclarationTypeCodeList.Codes._03 });
			AssertProductExclusionList("72000", new[] { AdditionalDeclarationTypeCodeList.Codes._02 });
			AssertProductExclusionList("73000", new[] { AdditionalDeclarationTypeCodeList.Codes._02 });
			AssertProductExclusionList("76000", new[] { AdditionalDeclarationTypeCodeList.Codes._03 });
		}
	}
}
