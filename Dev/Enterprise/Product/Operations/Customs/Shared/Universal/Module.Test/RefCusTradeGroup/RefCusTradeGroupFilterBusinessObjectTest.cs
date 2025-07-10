using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using FilterName = Enterprise.Customs.Universal.RefCusTradeGroupCollection.FilterName;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefCusTradeGroupFilterBusinessObject))]
	sealed class RefCusTradeGroupFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterByCode()
		{
			var filter = (ModuleTextFilter)filterBO[FilterName.Code];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "1030";
			filter.IsActive = true;

			CheckFilter(filter, FilterCategories.AttributeSearch, new[] { eun_item1030_1, eun_item1030_2 });
		}

		public void TestFilterByName()
		{
			var filter = (ModuleTextFilter)filterBO[FilterName.Name];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "QWERTY";
			filter.IsActive = true;

			CheckFilter(filter, FilterCategories.AttributeSearch, new[] { eun_item1030_2, eun_itemPL });
		}

		public void TestFilterByEconomicGroup()
		{
			var filter = (ModuleTextFilter)filterBO[FilterName.EconomicGroup];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "EUN";
			filter.IsActive = true;

			CheckFilter(filter, FilterCategories.Organisations, new[] { eun_item1030_1, eun_item1030_2, eun_itemPL });
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new RefCusTradeGroupFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();

			eunGrouping = Factory.NewWithValidTestData<RefDataGrouping>();
			eunGrouping.ZZZ_DataGrouping = "EUN";
			eunGrouping.ZZZ_Description = eunGrouping.ZZZ_DataGrouping;

			othGrouping = Factory.NewWithValidTestData<RefDataGrouping>();
			othGrouping.ZZZ_DataGrouping = "OTH";
			othGrouping.ZZZ_Description = othGrouping.ZZZ_DataGrouping;

			eun_item1030_1 = CreateRefCusTradeGroup("1030", "ABC", eunGrouping);
			eun_item1030_2 = CreateRefCusTradeGroup("10305", "QWERTY", eunGrouping);
			eun_itemPL = CreateRefCusTradeGroup("PL", "QWERTY", eunGrouping);
			oth_itemOther = CreateRefCusTradeGroup("10", "TTT", othGrouping);

			allTradeGroups = new[] { eun_item1030_1, eun_item1030_2, eun_itemPL, oth_itemOther };

			filterBO = new RefCusTradeGroupFilterBusinessObject { QueryObjectType = typeof(RefCusTradeGroup) };
		}

		RefDataGrouping eunGrouping, othGrouping;
		RefCusTradeGroup eun_item1030_1, eun_item1030_2, eun_itemPL, oth_itemOther;
		RefCusTradeGroup[] allTradeGroups;
		RefCusTradeGroupFilterBusinessObject filterBO;

		RefCusTradeGroup CreateRefCusTradeGroup(ZString code, ZString description, RefDataGrouping grouping)
		{
			var result = Factory.New<RefCusTradeGroup>();
			result.ZZA_TradeGroup = code;
			result.ZZA_Description = description;
			result.ZZA_ZZZ_NKDataGrouping = grouping.ZZZ_DataGrouping;
			result.ZZA_StartDate = new ZDateTime(1900, 01, 01);
			result.ZZA_EndDate = new ZDateTime(2050, 01, 01);
			return result;
		}

		void CheckFilter(ModuleTextFilter filter, FilterCategory expectedFilterCategory, RefCusTradeGroup[] expectedFilteredItems)
			=> CombineAssertions(() =>
			{
				AssertEquals("Category", expectedFilterCategory, filter.Category);
				AssertContainsExactElementsInAnyOrder("Filtered Items", expectedFilteredItems, allTradeGroups.Where(x => x.MatchesFilter(filterBO.Filter)));
			});
	}
}
