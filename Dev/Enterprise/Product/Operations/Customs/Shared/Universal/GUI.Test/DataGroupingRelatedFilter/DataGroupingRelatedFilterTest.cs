using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(DataGroupingRelatedFilter))]
	class DataGroupingRelatedFilterTest : ModuleFilterTestCase<DataGroupingRelatedFilter>
	{
		public void TestUseProperty2ListGetterWhenProperty1IsEmpty()
		{
			var filter = GetNewModuleFilter();
			filter.UseProperty2ListGetterWhenProperty1IsEmpty = false;
			filter.Property1 = ZString.Empty;
			AssertEquals(0, filter.Property2List.Count);
			filter.UseProperty2ListGetterWhenProperty1IsEmpty = true;
			Assert(filter.Property2List.Count > 0);
		}

		public void TestIsEmpty()
		{
			var filter = GetNewModuleFilter();
			Assert(filter.IsEmpty);
			filter.Property1 = Core.Constants.CountryCodes.Singapore;
			Assert(filter.IsEmpty);
			filter.Property2 = "SNT";
			Assert(!filter.IsEmpty);
			filter.Property1 = string.Empty;
			Assert(!filter.IsEmpty);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override DataGroupingRelatedFilter GetNewModuleFilter()
			=> new DataGroupingRelatedFilter("moo", (val1, val2) => new ZQuery(), Factory, FieldType.TextDropEdit, 100, DataGroupingRelatedFilterHelper.ResourceStringGetters.TariffTypeResourceString, DataGroupingRelatedFilterHelper.ListGetters.TariffTypeGetter);

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;
	}
}
