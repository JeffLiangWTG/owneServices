using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefFacilityFilterBusinessObject))]
	sealed class RefFacilityFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region CheckBoxFiltersTests

		public void TestIsSea()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_IsSea = true;
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_IsSea = false;
			Factory.Save();

			RefFacility[] result;
			var filter = new RefFacilityFilterBusinessObject();
			ModuleFlagsFilter strip = (ModuleFlagsFilter)filter["Is Sea"];

			strip.Property0 = ZBool.True;
			result = Factory.Load<RefFacility>(strip.Query);
			AssertCollectionContains("RefFacility1", refFacility1, result);
			AssertCollectionNotContains("RefFacility2", refFacility2, result);
		}

		public void TestIsRail()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_IsRail = true;
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_IsRail = false;
			Factory.Save();

			RefFacility[] result;
			var filter = new RefFacilityFilterBusinessObject();
			ModuleFlagsFilter strip = (ModuleFlagsFilter)filter["Is Rail"];

			strip.Property0 = ZBool.True;
			result = Factory.Load<RefFacility>(strip.Query);
			AssertCollectionContains("RefFacility1", refFacility1, result);
			AssertCollectionNotContains("RefFacility2", refFacility2, result);
		}

		public void TestIsRoad()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_IsRoad = true;
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_IsRoad = false;
			Factory.Save();

			RefFacility[] result;
			var filter = new RefFacilityFilterBusinessObject();
			ModuleFlagsFilter strip = (ModuleFlagsFilter)filter["Is Road"];

			strip.Property0 = ZBool.True;
			result = Factory.Load<RefFacility>(strip.Query);
			AssertCollectionContains("RefFacility1", refFacility1, result);
			AssertCollectionNotContains("RefFacility2", refFacility2, result);
		}

		public void TestIsAir()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_IsAir = true;
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_IsAir = false;
			Factory.Save();

			RefFacility[] result;
			var filter = new RefFacilityFilterBusinessObject();
			ModuleFlagsFilter strip = (ModuleFlagsFilter)filter["Is Air"];

			strip.Property0 = ZBool.True;
			result = Factory.Load<RefFacility>(strip.Query);
			AssertCollectionContains("RefFacility1", refFacility1, result);
			AssertCollectionNotContains("RefFacility2", refFacility2, result);
		}

		public void TestIsInlandWaterwayOrFerry()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_IsInlandWaterway = true;
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_IsInlandWaterway = false;
			Factory.Save();

			RefFacility[] result;
			var filter = new RefFacilityFilterBusinessObject();
			ModuleFlagsFilter strip = (ModuleFlagsFilter)filter["Is Inland Waterway/Ferry"];

			strip.Property0 = ZBool.True;
			result = Factory.Load<RefFacility>(strip.Query);
			AssertCollectionContains("RefFacility1", refFacility1, result);
			AssertCollectionNotContains("RefFacility2", refFacility2, result);
		}

		public void TestRFT_ActiveStatus()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_IsActive = true;
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_IsActive = false;
			Factory.Save();

			var filter = new RefFacilityFilterBusinessObject();

			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					((ModuleTextFilter)filter["Active Status"]).Property = FilterStripBusinessObject.StatusActive;
					((ModuleTextFilter)filter["Active Status"]).IsActive = true;

					var refFacilities = new RefFacilityCollection(Factory, filter.Filter);
					AssertCollectionContains(refFacility1, refFacilities);
					AssertCollectionNotContains(refFacility2, refFacilities);

					((ModuleTextFilter)filter["Active Status"]).Property = FilterStripBusinessObject.StatusInactive;
					((ModuleTextFilter)filter["Active Status"]).IsActive = true;

					refFacilities = new RefFacilityCollection(Factory, filter.Filter);
					AssertCollectionNotContains(refFacility1, refFacilities);
					AssertCollectionContains(refFacility2, refFacilities);

					((ModuleTextFilter)filter["Active Status"]).Property = FilterStripBusinessObject.StatusAll;
					((ModuleTextFilter)filter["Active Status"]).IsActive = true;

					refFacilities = new RefFacilityCollection(Factory, filter.Filter);
					AssertCollectionContains(refFacility1, refFacilities);
					AssertCollectionContains(refFacility2, refFacilities);
				}
			});
		}
		#endregion

		#region TextFiltersTests

		public void TestC1FCodeFilter()
		{
			var refFacilityCode1 = Factory.NewWithValidTestData<RefFacility>();
			var refFacilityCode2 = Factory.NewWithValidTestData<RefFacility>();
			refFacilityCode1.RFT_Code = "00000000001";
			refFacilityCode2.RFT_Code = "00000000002";

			Factory.Save();

			var filter = new RefFacilityFilterBusinessObject();
			((ModuleTextFilter)filter["C1F Code"]).Property = "00000000001";
			((ModuleTextFilter)filter["C1F Code"]).IsActive = true;

			var refFacilities = new RefFacilityCollection(Factory, filter.Filter);

			AssertCollectionContains(refFacilityCode1, refFacilities);
			AssertCollectionNotContains(refFacilityCode2, refFacilities);
		}

		public void TestBICCodeFilter()
		{
			var refFacilityCode1 = Factory.NewWithValidTestData<RefFacility>();
			var refFacilityCode2 = Factory.NewWithValidTestData<RefFacility>();
			refFacilityCode1.RFT_BICCode = "METWAU4A";
			refFacilityCode2.RFT_BICCode = "METWAU4B";

			Factory.Save();

			var filter = new RefFacilityFilterBusinessObject();
			((ModuleTextFilter)filter["BIC Code"]).Property = "METWAU4A";
			((ModuleTextFilter)filter["BIC Code"]).IsActive = true;

			var refFacilities = new RefFacilityCollection(Factory, filter.Filter);

			AssertCollectionContains(refFacilityCode1, refFacilities);
			AssertCollectionNotContains(refFacilityCode2, refFacilities);
		}

		public void TestSMDGCodeFilter()
		{
			var refFacilityCode1 = Factory.NewWithValidTestData<RefFacility>();
			var refFacilityCode2 = Factory.NewWithValidTestData<RefFacility>();
			refFacilityCode1.RFT_SMDGCode = "USSEA";
			refFacilityCode2.RFT_SMDGCode = "USOAK";

			Factory.Save();

			var filter = new RefFacilityFilterBusinessObject();
			((ModuleTextFilter)filter["SMDG Code"]).Property = "USSEA";
			((ModuleTextFilter)filter["SMDG Code"]).IsActive = true;

			var refFacilities = new RefFacilityCollection(Factory, filter.Filter);

			AssertCollectionContains(refFacilityCode1, refFacilities);
			AssertCollectionNotContains(refFacilityCode2, refFacilities);
		}

		public void TestFacilityNameFilter()
		{
			var refFacilityCode1 = Factory.NewWithValidTestData<RefFacility>();
			var refFacilityCode2 = Factory.NewWithValidTestData<RefFacility>();
			refFacilityCode1.RFT_Name = "Facility1";
			refFacilityCode2.RFT_Name = "Facility2";

			Factory.Save();

			var filter = new RefFacilityFilterBusinessObject();
			((ModuleTextFilter)filter["Facility Name"]).Property = "Facility1";
			((ModuleTextFilter)filter["Facility Name"]).IsActive = true;

			var refFacilities = new RefFacilityCollection(Factory, filter.Filter);

			AssertCollectionContains(refFacilityCode1, refFacilities);
			AssertCollectionNotContains(refFacilityCode2, refFacilities);
		}

		public void TestFacilityTypeFilter()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_FacilityType = Constants.FacilityType.Code.Terminal;
			refFacility2.RFT_FacilityType = Constants.FacilityType.Code.ContainerYard;
			Factory.Save();

			var filter = new RefFacilityFilterBusinessObject();
			((ModuleTextFilter)filter["Facility Type"]).Property = Constants.FacilityType.Code.Terminal;
			((ModuleTextFilter)filter["Facility Type"]).IsActive = true;

			var refFacilities = new RefFacilityCollection(Factory, filter.Filter);
			AssertCollectionContains(refFacility1, refFacilities);
			AssertCollectionNotContains(refFacility2, refFacilities);
		}

		#endregion

		public void TestRFT_UNLOCO()
		{
			var filter = (ModuleNkFilter)(GetNewFilterStripBusinessObject()["UNLOCO"]);

			var location1 = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);
			var location2 = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);

			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_RL_NKLocationCode = location1.RL_Code;
			refFacility2.RFT_RL_NKLocationCode = location2.RL_Code;

			refFacility1.RFT_RN_NKCountryCode = location1.RL_Code.Substring(0, 2);
			refFacility2.RFT_RN_NKCountryCode = location2.RL_Code.Substring(0, 2);

			Factory.Save();
			filter.Property = location1.RL_Code;
			filter.IsActive = true;
			var collection = new RefFacilityCollection(Factory);

			collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(refFacility1, collection);
			AssertCollectionNotContains(refFacility2, collection);

			filter.Property = location2.RL_Code;
			filter.IsActive = true;
			collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(refFacility1, collection);
			AssertCollectionContains(refFacility2, collection);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(refFacility1, collection);
			AssertCollectionContains(refFacility2, collection);

			var location3 = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);

			Factory.Save();
			filter.Property = location3.RL_Code;
			filter.IsActive = true;
			collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(refFacility1, collection);
			AssertCollectionNotContains(refFacility2, collection);
		}

		public void TestRFT_Country()
		{
			var filter = (ModuleNkFilter)(GetNewFilterStripBusinessObject()["Country"]);

			var country1 = Factory.NewWithValidTestData<RefCountry>(TestBusinessObjectKind.MinimumRequiredToSave);
			var country2 = Factory.NewWithValidTestData<RefCountry>(TestBusinessObjectKind.MinimumRequiredToSave);
			country1.RN_Code = "ZZ";
			country2.RN_Code = "DD";

			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_RN_NKCountryCode = country1.RN_Code;
			refFacility2.RFT_RN_NKCountryCode = country2.RN_Code;

			Factory.Save();

			filter.Property = country1.RN_Code;
			filter.IsActive = true;
			var collection = new RefFacilityCollection(Factory);

			collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(refFacility1, collection);
			AssertCollectionNotContains(refFacility2, collection);

			filter.Property = country2.RN_Code;
			filter.IsActive = true;
			collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(refFacility1, collection);
			AssertCollectionContains(refFacility2, collection);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(refFacility1, collection);
			AssertCollectionContains(refFacility2, collection);

			var country3 = Factory.NewWithValidTestData<RefCountry>(TestBusinessObjectKind.MinimumRequiredToSave);
			country3.RN_Code = "AA";

			Factory.Save();
			filter.Property = country3.RN_Code;
			filter.IsActive = true;
			collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(refFacility1, collection);
			AssertCollectionNotContains(refFacility2, collection);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefFacilityFilterBusinessObject();
		}

		#endregion
	}
}
