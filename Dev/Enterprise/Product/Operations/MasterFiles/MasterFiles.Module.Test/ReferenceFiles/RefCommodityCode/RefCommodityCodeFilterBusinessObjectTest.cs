using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCommodityCodeFilterBusinessObject))]
	sealed class RefCommodityCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestIsSystemDefinedFilter()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.RefCommodityCode))
			{
				RefCommodityCode commodityType1 = Factory.NewWithValidTestData<RefCommodityCode>();
				RefCommodityCode commodityType2 = Factory.NewWithValidTestData<RefCommodityCode>();
				commodityType1.RH_IsSystem = true;
				commodityType2.RH_IsSystem = false;

				var filter = module.FilterBusinessObject;
				AssertEquals("Filter is null", !((ModuleTextFilter)filter["Is System Defined"]).IsNull, true);
				((ModuleTextFilter)filter["Is System Defined"]).IsActive = true;
				((ModuleTextFilter)filter["Is System Defined"]).Property = "System";
				RefCommodityCodeCollection collection = new RefCommodityCodeCollection(Factory, filter.Filter);

				AssertCollectionContains(commodityType1, collection);
				AssertCollectionNotContains(commodityType2, collection);

				((ModuleTextFilter)filter["Is System Defined"]).Property = "Not System";

				RefCommodityCodeCollection collection1 = new RefCommodityCodeCollection(Factory, filter.Filter);

				AssertCollectionNotContains(commodityType1, collection1);
				AssertCollectionContains(commodityType2, collection1);
			}
		}

		public void TestCommodityTypeFilter()
		{
			RefCommodityCode commodityType1 = Factory.NewWithValidTestData<RefCommodityCode>();
			RefCommodityCode commodityType2 = Factory.NewWithValidTestData<RefCommodityCode>();
			RefCommodityCode commodityType3 = Factory.NewWithValidTestData<RefCommodityCode>();
			RefCommodityCode commodityType4 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityType1.RH_IsForwarding = true;
			commodityType2.RH_IsForwarding = false;
			commodityType2.RH_IsShipping = true;
			commodityType3.RH_IsForwarding = false;
			commodityType3.RH_IsLandTransport = true;
			commodityType4.RH_IsForwarding = true;
			commodityType4.RH_IsShipping = true;

			Factory.Save();

			RefCommodityCodeFilterBusinessObject filter = new RefCommodityCodeFilterBusinessObject();
			var commodityType = filter[RefCommodityCodeFilterBusinessObject.FilterCodes.CommodityType];
			Assert(((ModuleFlagsFilter)commodityType).ShowAddOrRadioBox);
			((ModuleFlagsFilter)commodityType).IsActive = true;
			((ModuleFlagsFilter)commodityType).Property0 = true; // IsForwarding
			((ModuleFlagsFilter)commodityType).Property1 = false; // IsLandTransport
			((ModuleFlagsFilter)commodityType).Property2 = false; // IsShipping
			RefCommodityCodeCollection commodityCodes = new RefCommodityCodeCollection(Factory, filter.Filter);
			AssertCollectionContains(commodityType1, commodityCodes);
			AssertCollectionContains(commodityType4, commodityCodes);
			AssertCollectionNotContains(commodityType2, commodityCodes);
			AssertCollectionNotContains(commodityType3, commodityCodes);

			((ModuleFlagsFilter)commodityType).Property0 = false;
			((ModuleFlagsFilter)commodityType).Property1 = true;
			((ModuleFlagsFilter)commodityType).Property2 = false;
			commodityCodes = new RefCommodityCodeCollection(Factory, filter.Filter);
			AssertCollectionContains(commodityType2, commodityCodes);
			AssertCollectionContains(commodityType4, commodityCodes);
			AssertCollectionNotContains(commodityType1, commodityCodes);
			AssertCollectionNotContains(commodityType3, commodityCodes);

			((ModuleFlagsFilter)commodityType).Property0 = false;
			((ModuleFlagsFilter)commodityType).Property1 = false;
			((ModuleFlagsFilter)commodityType).Property2 = true;
			commodityCodes = new RefCommodityCodeCollection(Factory, filter.Filter);
			AssertCollectionContains(commodityType3, commodityCodes);
			AssertCollectionNotContains(commodityType1, commodityCodes);
			AssertCollectionNotContains(commodityType2, commodityCodes);
			AssertCollectionNotContains(commodityType4, commodityCodes);

			((ModuleFlagsFilter)commodityType).Property0 = true;
			((ModuleFlagsFilter)commodityType).Property1 = true;
			((ModuleFlagsFilter)commodityType).Property2 = false;
			commodityCodes = new RefCommodityCodeCollection(Factory, filter.Filter);
			AssertCollectionContains(commodityType4, commodityCodes);
			AssertCollectionNotContains(commodityType1, commodityCodes);
			AssertCollectionNotContains(commodityType2, commodityCodes);
			AssertCollectionNotContains(commodityType3, commodityCodes);
		}

		public void TestCommodityTypeFilter_ShowPersonalEffects()
		{
			using (ReferenceFilesDataRegistry.Instance.ShowPersonalEffects.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var commodityType1 = Factory.NewWithValidTestData<RefCommodityCode>();
				var commodityType2 = Factory.NewWithValidTestData<RefCommodityCode>();
				commodityType1.RH_IsForwarding = true;
				commodityType1.RH_IsPersonalEffects = false;
				commodityType2.RH_IsLandTransport = true;
				commodityType2.RH_IsPersonalEffects = true;

				Factory.Save();

				var filter = new RefCommodityCodeFilterBusinessObject();
				var commodityType = filter[RefCommodityCodeFilterBusinessObject.FilterCodes.CommodityType];

				((ModuleFlagsFilter)commodityType).IsActive = true;
				((ModuleFlagsFilter)commodityType).Property0 = true; // IsForwarding
				((ModuleFlagsFilter)commodityType).Property1 = false; // LandTransport
				((ModuleFlagsFilter)commodityType).Property2 = false; // IsShipping
				((ModuleFlagsFilter)commodityType).Property3 = false; // IsPersonalEffects
				var commodityCodes = new RefCommodityCodeCollection(Factory, filter.Filter);
				AssertCollectionContains(commodityType1, commodityCodes);
				AssertCollectionNotContains(commodityType2, commodityCodes);

				((ModuleFlagsFilter)commodityType).Property0 = false;
				((ModuleFlagsFilter)commodityType).Property1 = false;
				((ModuleFlagsFilter)commodityType).Property2 = false;
				((ModuleFlagsFilter)commodityType).Property3 = true;

				commodityCodes = new RefCommodityCodeCollection(Factory, filter.Filter);
				AssertCollectionContains(commodityType2, commodityCodes);
				AssertCollectionNotContains(commodityType1, commodityCodes);

				((ModuleFlagsFilter)commodityType).Property0 = false;
				((ModuleFlagsFilter)commodityType).Property1 = false;
				((ModuleFlagsFilter)commodityType).Property2 = true;
				((ModuleFlagsFilter)commodityType).Property3 = true;

				commodityCodes = new RefCommodityCodeCollection(Factory, filter.Filter);
				AssertCollectionContains(commodityType2, commodityCodes);
				AssertCollectionNotContains(commodityType1, commodityCodes);
			}
		}

		public void TestCommodityCodeCodeOrDescriptionFilter()
		{
			RefCommodityCode commodityCodeOrDescription1 = Factory.NewWithValidTestData<RefCommodityCode>();
			RefCommodityCode commodityCodeOrDescription2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCodeOrDescription1.RH_Description = "Indescriptive";
			commodityCodeOrDescription1.RH_Code = "Any";
			commodityCodeOrDescription2.RH_Description = "AnyDescription";
			commodityCodeOrDescription2.RH_Code = "Some";

			Factory.Save();

			RefCommodityCodeFilterBusinessObject filter = new RefCommodityCodeFilterBusinessObject();
			((ModuleTextFilter)filter["Code OR Description"]).Property = "Inde";
			((ModuleTextFilter)filter["Code OR Description"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Code OR Description"]).IsActive = true;

			RefCommodityCodeCollection commodityCodes = new RefCommodityCodeCollection(Factory);
			commodityCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(commodityCodeOrDescription1, commodityCodes);
			AssertCollectionNotContains(commodityCodeOrDescription2, commodityCodes);

			((ModuleTextFilter)filter["Code OR Description"]).Property = "Som";
			((ModuleTextFilter)filter["Code OR Description"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Code OR Description"]).IsActive = true;

			commodityCodes.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains(commodityCodeOrDescription1, commodityCodes);
			AssertCollectionContains(commodityCodeOrDescription2, commodityCodes);

			((ModuleTextFilter)filter["Code OR Description"]).Property = "Any";
			((ModuleTextFilter)filter["Code OR Description"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Code OR Description"]).IsActive = true;

			commodityCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(commodityCodeOrDescription1, commodityCodes);
			AssertCollectionContains(commodityCodeOrDescription2, commodityCodes);

			((ModuleTextFilter)filter["Code OR Description"]).Property = "Any";
			((ModuleTextFilter)filter["Code OR Description"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filter["Code OR Description"]).IsActive = true;

			commodityCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(commodityCodeOrDescription1, commodityCodes);
			AssertCollectionNotContains(commodityCodeOrDescription2, commodityCodes);

			((ModuleTextFilter)filter["Code OR Description"]).Property = "Inde";
			((ModuleTextFilter)filter["Code OR Description"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Code OR Description"]).IsActive = true;

			commodityCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(commodityCodeOrDescription1, commodityCodes);
			AssertCollectionNotContains(commodityCodeOrDescription2, commodityCodes);

			((ModuleTextFilter)filter["Code OR Description"]).Property = "om";
			((ModuleTextFilter)filter["Code OR Description"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((ModuleTextFilter)filter["Code OR Description"]).IsActive = true;

			commodityCodes.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains(commodityCodeOrDescription1, commodityCodes);
			AssertCollectionContains(commodityCodeOrDescription2, commodityCodes);

			((ModuleTextFilter)filter["Code OR Description"]).Property = "LUBRICANT";
			((ModuleTextFilter)filter["Code OR Description"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((ModuleTextFilter)filter["Code OR Description"]).IsActive = true;

			commodityCodes.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains(commodityCodeOrDescription1, commodityCodes);
			AssertCollectionNotContains(commodityCodeOrDescription2, commodityCodes);
		}

		public void TestAllFiltersUseMultilingualDescriptionsInNonEnglish()
		{
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.French))
			{
				TestAllFiltersUseMultilingualDescriptions();
			}
		}

		public void TestIATACommodityCodeFilter()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var iataCommodityCode = filterStrip[RefCommodityCodeFilterBusinessObject.FilterCodes.IATACommodityCode];
			var filter = (ModuleNkFilter)iataCommodityCode;

			AssertEquals("IATA Commodity Code should use comparison operators", true, filter.HasComparisonOperator);

			var expectedOperators = new string[6];
			expectedOperators[5] = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			expectedOperators[4] = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			expectedOperators[3] = ModuleTextFilter.ComparisonConstants.IsBlank;
			expectedOperators[2] = ModuleTextFilter.ComparisonConstants.NotEqual;
			expectedOperators[1] = ModuleTextFilter.ComparisonConstants.Exact;
			expectedOperators[0] = string.Empty;
			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);

			var airlineCommodityCode1 = Factory.New<RefAirlineCommodityCode>();
			airlineCommodityCode1.RAC_Code = "1234";
			airlineCommodityCode1.RAC_Description = "Testing1";

			var airlineCommodityCode2 = Factory.New<RefAirlineCommodityCode>();
			airlineCommodityCode2.RAC_Code = "1236";
			airlineCommodityCode2.RAC_Description = "Testing2";

			var refcommodityCode1 = Factory.NewWithValidTestData<RefCommodityCode>();
			refcommodityCode1.RH_Description = "Test1";
			refcommodityCode1.RH_Code = "T1";
			refcommodityCode1.RH_IATACommodityItem = "1234";

			var refcommodityCode2 = Factory.NewWithValidTestData<RefCommodityCode>();
			refcommodityCode2.RH_Description = "Test2";
			refcommodityCode2.RH_Code = "T2";
			refcommodityCode2.RH_IATACommodityItem = "1236";

			var refcommodityCode3 = Factory.NewWithValidTestData<RefCommodityCode>();
			refcommodityCode3.RH_Description = "Test3";
			refcommodityCode3.RH_Code = "T3";
			refcommodityCode3.RH_IATACommodityItem = ZString.Empty;

			Factory.Save();

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			filter.Property = "1234";
			var commodityCodes = new RefCommodityCodeCollection(Factory);
			commodityCodes.AdditionalFilter = filterStrip.Filter;

			AssertCollectionContains(refcommodityCode1, commodityCodes);
			AssertCollectionNotContains(refcommodityCode2, commodityCodes);
			AssertCollectionNotContains(refcommodityCode3, commodityCodes);

			((ModuleNkFilter)iataCommodityCode).Property = "1234";
			((ModuleNkFilter)iataCommodityCode).ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
			((ModuleNkFilter)iataCommodityCode).IsActive = true;
			commodityCodes.AdditionalFilter = filterStrip.Filter;

			AssertCollectionNotContains(refcommodityCode1, commodityCodes);
			AssertCollectionContains(refcommodityCode2, commodityCodes);
			AssertCollectionContains(refcommodityCode3, commodityCodes);

			((ModuleNkFilter)iataCommodityCode).Property = ZString.Empty;
			((ModuleNkFilter)iataCommodityCode).ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsNotBlank;
			((ModuleNkFilter)iataCommodityCode).IsActive = true;
			commodityCodes.AdditionalFilter = filterStrip.Filter;

			AssertCollectionContains(refcommodityCode1, commodityCodes);
			AssertCollectionContains(refcommodityCode2, commodityCodes);
			AssertCollectionNotContains(refcommodityCode3, commodityCodes);

			((ModuleNkFilter)iataCommodityCode).Property = ZString.Empty;
			((ModuleNkFilter)iataCommodityCode).ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;
			((ModuleNkFilter)iataCommodityCode).IsActive = true;
			commodityCodes.AdditionalFilter = filterStrip.Filter;

			AssertCollectionNotContains(refcommodityCode1, commodityCodes);
			AssertCollectionNotContains(refcommodityCode2, commodityCodes);
			AssertCollectionContains(refcommodityCode3, commodityCodes);

			((ModuleNkFilter)iataCommodityCode).Property = ZString.Empty;
			((ModuleNkFilter)iataCommodityCode).ComparisonOperator = ModuleNkFilter.ComparisonConstants.FiltersMatch;
			((ModuleNkFilter)iataCommodityCode).IsActive = true;
			((ModuleNkFilter)iataCommodityCode).SelectedFilters.AddTextFilterStrip("Code", "12");
			commodityCodes.AdditionalFilter = filterStrip.Filter;

			AssertCollectionContains(refcommodityCode1, commodityCodes);
			AssertCollectionContains(refcommodityCode2, commodityCodes);
			AssertCollectionNotContains(refcommodityCode3, commodityCodes);
		}

		public void TestCommodityRatingCodeFilter()
		{
			var a = CreateCommodityWithRateCodes("A");
			var b = CreateCommodityWithRateCodes("B");
			var c = CreateCommodityWithRateCodes("C", new[] { "A" });
			var d = CreateCommodityWithRateCodes("D", new[] { "A", "B" });
			var e = CreateCommodityWithRateCodes("E", new[] { "D" });
			var f = CreateCommodityWithRateCodes("F", new[] { "E" });
			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterStrip["Rating Code"];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;

			filter.Property = "A";
			var commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { c, d }, commodityCodes);

			filter.Property = "B";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { d }, commodityCodes);

			filter.Property = "E";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { f }, commodityCodes);
		}

		public void TestCommodityRatingCodeDescriptionFilter()
		{
			var a = CreateCommodityWithRateCodes("A");
			var b = CreateCommodityWithRateCodes("B");
			var c = CreateCommodityWithRateCodes("C", new[] { "A" });
			var d = CreateCommodityWithRateCodes("D", new[] { "A", "B" });
			var e = CreateCommodityWithRateCodes("E", new[] { "D" });
			var f = CreateCommodityWithRateCodes("F", new[] { "E" });
			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Rating Code Description"];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			filter.Property = "Description of";
			var commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { c, d, e, f }, commodityCodes);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Description of A";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { c, d }, commodityCodes);
		}

		public void TestCommodityRatingLocalCodeFilter()
		{
			var a = CreateCommodityWithRateCodes("A");
			var b = CreateCommodityWithRateCodes("B");
			var c = CreateCommodityWithRateCodes("C", new[] { "A" });
			var d = CreateCommodityWithRateCodes("D", new[] { "A", "B" });
			var e = CreateCommodityWithRateCodes("E", new[] { "D" });
			var f = CreateCommodityWithRateCodes("F", new[] { "E" });
			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Rating Local Code"];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.StartsWith;
			filter.Property = "TEST_LOCAL_";
			var commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { c, d, e, f }, commodityCodes);

			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			filter.Property = "TEST_LOCAL_A";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { c, d }, commodityCodes);

			filter.Property = "TEST_LOCAL_E";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { f }, commodityCodes);
		}

		RefCommodityCode CreateCommodityWithRateCodes(ZString code, string[] children = null)
		{
			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = code;
			commodityCode.RH_Description = $"Description of {code}";

			var localCode = commodityCode.RefCommodityCodeMaps.AddNew();
			localCode.LC_LocalCode = $"TEST_LOCAL_{code}";
			localCode.LC_LocalCodeProvider = GlobalCommodityCodeProviderList.Codes.Rating;
			localCode.LC_RH_NKCommodityCode = code;

			if (children != null)
			{
				foreach (var child in children)
				{
					var ratingCode = commodityCode.RefCommodityRatingCodeMaps.AddNew();
					ratingCode.RI_RH_NKCommodityChild = child;
				}
			}

			return commodityCode;
		}

		public void TestCommodityLocalCodeFilter()
		{
			var (commodityCodeA, commodityCodeB) = GetTestCommodityLocalCodes();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Local Code"];
			filter.IsActive = true;

			filter.Property = "TEST_LOCAL_1";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			var commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { commodityCodeA }, commodityCodes);

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { commodityCodeA, commodityCodeB }, commodityCodes);
		}

		public void TestCommodityLocalCodeCountryFilter()
		{
			var (commodityCodeA, commodityCodeB) = GetTestCommodityLocalCodes();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[RefCommodityCodeFilterBusinessObject.FilterCodes.LocalCodeCountry];
			filter.IsActive = true;

			filter.Property = "AU";
			var commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(Array.Empty<RefCommodityCodeMap>(), commodityCodes);

			filter.Property = "DE";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { commodityCodeB }, commodityCodes);
		}

		public void TestCommodityLocalCodeUsageFilter()
		{
			var (commodityCodeA, commodityCodeB) = GetTestCommodityLocalCodes();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[RefCommodityCodeFilterBusinessObject.FilterCodes.LocalCodeUsage];
			filter.IsActive = true;

			filter.Property = "RAT";
			var commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { commodityCodeA }, commodityCodes);

			filter.Property = "DBH";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			AssertContainsExactElementsInAnyOrder(new[] { commodityCodeB }, commodityCodes);
		}

		public void TestCommodityTypeFilterForNonHazardous()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[RefCommodityCodeFilterBusinessObject.FilterCodes.IsHazardous];
			filter.IsActive = true;

			filter.Property = "STD";
			var commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			var allIsHazFalse = commodityCodes.All(c => !c.RH_IsHazardous);
			Assert(allIsHazFalse);

			filter.Property = "HAZ";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			var allIsHazTrue = commodityCodes.All(c => c.RH_IsHazardous);
			Assert(allIsHazTrue);
			
			filter.Property = "ALL";
			commodityCodes = new RefCommodityCodeCollection(Factory) { AdditionalFilter = filterStrip.Filter };
			var someCommoditiesTrue = commodityCodes.Any(c => c.RH_IsHazardous);
			var someCommoditiesFalse = commodityCodes.Any(c => !c.RH_IsHazardous);
			Assert(someCommoditiesFalse);
			Assert(someCommoditiesTrue);
		}

		(RefCommodityCode, RefCommodityCode) GetTestCommodityLocalCodes()
		{
			var commodityCodeA = Factory.New<RefCommodityCode>();
			commodityCodeA.RH_Code = "A";
			var localCode1 = commodityCodeA.RefCommodityCodeMaps.AddNew();
			localCode1.LC_RN_NKCountry = "";
			localCode1.LC_LocalCode = "TEST_LOCAL_1";
			localCode1.LC_LocalCodeProvider = "RAT";

			var commodityCodeB = Factory.New<RefCommodityCode>();
			commodityCodeB.RH_Code = "B";
			var localCode2 = commodityCodeB.RefCommodityCodeMaps.AddNew();
			localCode2.LC_RN_NKCountry = "DE";
			localCode2.LC_LocalCode = "TEST_LOCAL_2";
			localCode2.LC_LocalCodeProvider = "DBH";
			Factory.Save();

			return (commodityCodeA, commodityCodeB);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCommodityCodeFilterBusinessObject();
		}

		#endregion
	}
}
