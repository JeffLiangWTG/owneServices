using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	class DataGroupingRelatedFilterHelperTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Kazakhstan, "T3T");
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Kazakhstan, "T4T");
			Factory.Save();
			var filter = GetNewModuleFilter();
			filter.Property1 = Core.Constants.CountryCodes.Congo;
			AssertEquals("No refCusTariffTypeList and show HSN", "HSN", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			filter.Property1 = Core.Constants.CountryCodes.Kazakhstan;
			AssertEquals("RefCusTariffTypeList", "T3T, T4T", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			var filter2 = GetNewModuleFilter();
			filter2.Property1 = Core.Constants.CountryCodes.Kazakhstan;
			AssertSame("cached list", filter.Property2List, filter2.Property2List);
		}

		DataGroupingRelatedFilter GetNewModuleFilter()
		{
			return new DataGroupingRelatedFilter("moo", (val1, val2) => new ZQuery(),
				Factory,
				FieldType.TextDropEdit,
				100,
				DataGroupingRelatedFilterHelper.ResourceStringGetters.TariffTypeResourceString,
				DataGroupingRelatedFilterHelper.ListGetters.TariffTypeGetter);
		}
	}
}
