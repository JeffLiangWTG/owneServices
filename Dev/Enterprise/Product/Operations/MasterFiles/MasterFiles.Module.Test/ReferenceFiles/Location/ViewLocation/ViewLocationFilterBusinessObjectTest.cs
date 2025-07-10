using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ViewLocationFilterBusinessObject))]
	sealed class ViewLocationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Text Filters Tests

		public void TestUnloco()
		{
			RefUNLOCO unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			RefCountry country = Factory.NewWithValidTestData<RefCountry>();
			RefUNLOCO unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			unloco1.RL_Code = "ABC";
			unloco1.RL_RN_NKCountryCode = country.Code;
			unloco1.RL_PortName = "Desc";

			unloco2.RL_Code = "XYZ";
			unloco2.RL_PortName = "Asc";
			unloco2.RL_RN_NKCountryCode = country.Code;

			Factory.Save();

			var filter = (ViewLocationFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Location Type"]).Property = ViewLocationTypeList.Codes.UNLOCO;
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			((ModuleTextFilter)filter["Code"]).Property = "ABC";
			((ModuleTextFilter)filter["Code"]).IsActive = true;

			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			var locations = new ViewLocationCollection(Factory);
			locations.AdditionalFilter = filter.Filter;
			AssertEquals(1, locations.Count);

			Assert(locations.Any(x => x.PK == unloco1.PK));
			Assert(!locations.Any(x => x.PK == unloco2.PK));
		}

		public void TestCountry()
		{
			RefCountry country1 = Factory.NewWithValidTestData<RefCountry>();
			RefCountry country2 = Factory.NewWithValidTestData<RefCountry>();

			country1.RN_Code = "AB";
			country1.RN_Desc = "Desc";

			country2.RN_Code = "XY";
			country2.RN_Desc = "ASC";

			Factory.Save();

			var filter = (ViewLocationFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Location Type"]).Property = ViewLocationTypeList.Codes.Country;
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			((ModuleTextFilter)filter["Code"]).Property = "AB";
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Code"]).IsActive = true;
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			var locations = new ViewLocationCollection(Factory);
			locations.AdditionalFilter = filter.Filter;
			AssertEquals(1, locations.Count);

			Assert(locations.Any(x => x.PK == country1.PK));
			Assert(!locations.Any(x => x.PK == country2.PK));
		}

		public void TestZone()
		{
			RefZoneHeader region1 = Factory.NewWithValidTestData<RefZoneHeader>();
			RefZoneHeader region2 = Factory.NewWithValidTestData<RefZoneHeader>();

			region1.FZ_Code = "AB";
			region1.FZ_Description = "Desc";

			region2.FZ_Code = "XY";
			region2.FZ_Description = "ASC";

			Factory.Save();

			var filter = (ViewLocationFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Location Type"]).Property = ViewLocationTypeList.Codes.InternationalZone;
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			((ModuleTextFilter)filter["Code"]).Property = "AB";
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Code"]).IsActive = true;
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			var locations = new ViewLocationCollection(Factory);
			locations.AdditionalFilter = filter.Filter;
			AssertEquals(1, locations.Count);

			Assert(locations.Any(x => x.PK == region1.PK));
			Assert(!locations.Any(x => x.PK == region2.PK));
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var collection = new ViewLocationCollection(Factory);
			return new ViewLocationFilterBusinessObject(collection);
		}

		#endregion
	}
}
