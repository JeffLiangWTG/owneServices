using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CompanyTariffsFilterBusinessObject))]
	public class CompanyTariffsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFMCTariffIDFilter()
		{
			var tariff1 = Factory.NewWithValidTestData<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.TI_FMCTariffID = "ABC";

			var tariff2 = Factory.NewWithValidTestData<CompanyTariff>();
			var entry2 = tariff2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry2.TI_FMCTariffID = "";

			Factory.Save();

			var filter = new CompanyTariffsFilterBusinessObject();
			var tariffs = new CompanyTariffCollection(Factory);
			tariffs.Load(filter.Filter);

			var actualFirstLoad = tariffs.Select(s => s.PK).ToArray();
			var expectedFirstLoad = new[] { tariff1.PK, tariff2.PK };
			AssertContainsExactElementsInAnyOrder(
				"If property is empty, we will load all tariffs.",
				expectedFirstLoad,
				actualFirstLoad
			);

			((ModuleTextFilter)filter["FMC Tariff ID"]).IsActive = true;
			((ModuleTextFilter)filter["FMC Tariff ID"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			tariffs.Load(filter.Filter);
			var actualSecondLoad = tariffs.Select(s => s.PK).ToArray();
			var expectedSecondLoad = new[] { tariff2.PK };
			AssertContainsExactElementsInAnyOrder(
				"When comparison operator is blank, only load tariff2.",
				expectedSecondLoad,
				actualSecondLoad
			);

			((ModuleTextFilter)filter["FMC Tariff ID"]).Property = "A";
			((ModuleTextFilter)filter["FMC Tariff ID"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			tariffs.Load(filter.Filter);
			var actualThirdLoad = tariffs.Select(s => s.PK).ToArray();
			var expectedThirdLoad = new[] { tariff1.PK };
			AssertContainsExactElementsInAnyOrder(
				"Only tariff1 has an entry with FMC Tariff ID starting with 'A'.",
				expectedThirdLoad,
				actualThirdLoad
			);

			AssertEquals("FMCTariffID MaxLength", 4, ((ModuleTextFilter)filter["FMC Tariff ID"]).MaxLength);
		}

		public void TestCommodityCodeFilter()
		{
			var commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";

			var tariff1 = Factory.NewWithValidTestData<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.TI_RH_NKCommodityCode = commodityCode1.RH_Code;
			AssertEquals("Pre-condition", "Base Company Tariff", tariff1.TH_GlobalRateDescription);

			var tariff2 = Factory.NewWithValidTestData<CompanyTariff>();
			var entry2 = tariff2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry2.TI_RH_NKCommodityCode = "";
			AssertEquals("Pre-condition", "Company Tariff Level 2", tariff2.TH_GlobalRateDescription);

			Factory.Save();

			var filter = new CompanyTariffsFilterBusinessObject();
			var tariffs = new CompanyTariffCollection(Factory);
			tariffs.Load(filter.Filter);
			var descriptions = tariffs.Select(s => s.TH_GlobalRateDescription).ToArray();
			AssertContainsExactElementsInAnyOrder(
				"If property is empty, we will load all tariffs.",
				new ZString[] { "Base Company Tariff", "Company Tariff Level 2" },
				descriptions
			);

			((ModuleNkFilter)filter["Commodity Code"]).IsActive = true;
			((ModuleNkFilter)filter["Commodity Code"]).Property = commodityCode1.RH_Code;
			tariffs.Load(filter.Filter);
			descriptions = tariffs.Select(s => s.TH_GlobalRateDescription).ToArray();
			AssertContainsExactElementsInAnyOrder(
				"Only tariff1 has an entry with commodityCode1.",
				new ZString[] { "Base Company Tariff" },
				descriptions
			);
		}

		#region TestContractNumberFilter

		public void TestContractNumberFilter()
		{
			var tariff1 = Factory.NewWithValidTestData<CompanyTariff>();
			RateEntry entry1 = tariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.TI_ContractNumber = "ABC123";

			var tariff2 = Factory.NewWithValidTestData<CompanyTariff>();
			RateEntry entry2 = tariff2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry2.TI_ContractNumber = "";

			Factory.Save();

			CompanyTariffsFilterBusinessObject filter = new CompanyTariffsFilterBusinessObject();
			CompanyTariffCollection tariffs = new CompanyTariffCollection(Factory);
			tariffs.Load(filter.Filter);
			AssertEquals(2, tariffs.Count);

			((ModuleTextFilter)filter["Client Contract Number"]).IsActive = true;
			((ModuleTextFilter)filter["Client Contract Number"]).Property = "ABC123";

			tariffs.Load(filter.Filter);
			AssertEquals(1, tariffs.Count);
		}

		#endregion

		public void TestGlobalRateTypeFilter()
		{
			var localTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var globalTariff = Factory.NewWithValidTestData<GlobalTariff>();

			Factory.Save();

			var filterBizo = (CompanyTariffsFilterBusinessObject)GetNewFilterStripBusinessObject();
			var tariffs = new CompanyTariffCollection(Factory);
			tariffs.Load(filterBizo.Filter);
			AssertEquals("Should have both tariffs", 2, tariffs.Count);

			((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).IsActive = true;
			((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property0 = true;
			((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property1 = false;
			tariffs.Load(filterBizo.Filter);

			Assert("Should be only one global tariff", tariffs.Count == 1 && tariffs[0].Company == null);

			((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property0 = true;
			((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property1 = true;
			tariffs.Load(filterBizo.Filter);

			Assert("Should be both global and local tariff", tariffs.Count == 2);

			((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property0 = false;
			((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property1 = true;

			tariffs.Load(filterBizo.Filter);
			Assert("Should be only one local tariff", tariffs.Count == 1 && tariffs[0].Company != null);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CompanyTariffsFilterBusinessObject();
		}

		#endregion
	}
}
