using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class RefCommodityRatingCodeMapValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRI_RH_NKCommodityChild_ContainsValue()
		{
			var ratingCodeMap = Factory.New<RefCommodityRatingCodeMap>();

			ratingCodeMap.RI_RH_NKCommodityChild = ZString.Empty;
			AssertHasError(ratingCodeMap.RI_RH_NKCommodityChildInfo, "Please enter a value.");
			ratingCodeMap.RI_RH_NKCommodityChild = "C";
			AssertHasError(ratingCodeMap.RI_RH_NKCommodityChildInfo, "Enter a valid selection.");
			ratingCodeMap.RI_RH_NKCommodityChild = "CHEM";
			AssertNoErrors(ratingCodeMap.RI_RH_NKCommodityChildInfo);
		}

		public void TestValidateRI_RH_NKCommodityChild_NoDuplicates()
		{
			var commodityCode = Factory.NewWithValidTestData<RefCommodityCode>();

			var ratingCodeMap1 = commodityCode.RefCommodityRatingCodeMaps.AddNew();
			ratingCodeMap1.RI_RH_NKCommodityChild = "CHEM";
			AssertNoErrors(ratingCodeMap1.RI_RH_NKCommodityChildInfo);

			var ratingCodeMap2 = commodityCode.RefCommodityRatingCodeMaps.AddNew();
			ratingCodeMap2.RI_RH_NKCommodityChild = "CHEM";
			AssertHasError(ratingCodeMap2.RI_RH_NKCommodityChildInfo, "The Commodity Child has been duplicated and must be unique.");
		}

		public void TestValidateRI_RH_NKCommodityChild_NotParent()
		{
			var commodityCode = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode.RH_Code = "TEST";
			var ratingCodeMap1 = commodityCode.RefCommodityRatingCodeMaps.AddNew();
			ratingCodeMap1.RI_RH_NKCommodityChild = "TEST";
			AssertHasError(ratingCodeMap1.RI_RH_NKCommodityChildInfo, "Selected commodity code cannot be the same code as parent.");
		}
	}
}
