using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTradeGroupCollection))]
	sealed class RefCusTradeGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusTradeGroupCollection>
	{
		public void TestGrouping()
		{
			var eunGrouping = Factory.NewWithValidTestData<RefDataGrouping>();
			eunGrouping.ZZZ_DataGrouping = "EUN";
			eunGrouping.ZZZ_Description = "EUN";

			var otherGrouping = Factory.NewWithValidTestData<RefDataGrouping>();
			otherGrouping.ZZZ_DataGrouping = "OTH";
			otherGrouping.ZZZ_Description = "OTH";

			var refCusTradeGroup1 = Factory.NewWithValidTestData<RefCusTradeGroup>();
			refCusTradeGroup1.ZZA_TradeGroup = "TG1";
			refCusTradeGroup1.ZZA_ZZZ_NKDataGrouping = eunGrouping.ZZZ_DataGrouping;

			var refCusTradeGroup2 = Factory.NewWithValidTestData<RefCusTradeGroup>();
			refCusTradeGroup2.ZZA_TradeGroup = "TG2";
			refCusTradeGroup2.ZZA_ZZZ_NKDataGrouping = "OTH";

			var refCusTradeGroup3 = Factory.NewWithValidTestData<RefCusTradeGroup>();
			refCusTradeGroup3.ZZA_TradeGroup = "TG3";
			refCusTradeGroup3.ZZA_ZZZ_NKDataGrouping = eunGrouping.ZZZ_DataGrouping;

			Factory.Save();

			var euTradeGroupCollection = new RefCusTradeGroupCollection(Factory,
				new ZQuery(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, SQLComparisonOperator.Equal, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN));

			AssertContainsExactElementsInAnyOrder(new[] { refCusTradeGroup1, refCusTradeGroup3 }, euTradeGroupCollection);
		}

		protected override RefCusTradeGroupCollection GetCollectionToTest() => new RefCusTradeGroupCollection(Factory);
	}
}
