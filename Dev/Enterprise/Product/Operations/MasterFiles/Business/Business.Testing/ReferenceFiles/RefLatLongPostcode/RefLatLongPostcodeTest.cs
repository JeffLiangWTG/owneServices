using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefLatLongPostcode))]
	sealed class RefLatLongPostcodeTest : EnterpriseBusinessObjectTestCase
	{
		#region Load Postcode

		public void TestLoad()
		{
			AssertNotNull("Loaded WPH in AU", RefLatLongPostcode.Load(Factory, "2125", "AU"));
			AssertNull("WPH in US not found", RefLatLongPostcode.Load(Factory, "2125", "US"));

			AssertNull("WPH in AU in Castle Hill not found", RefLatLongPostcode.Load(Factory, "2125", "Castle Hill", "AU"));
			AssertNotNull("WPH in AU in Pennant Hills found", RefLatLongPostcode.Load(Factory, "2125", "Pennant", "AU"));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadEmptyCountrySpecified()
		{
			RefLatLongPostcode.Load(Factory, "2125", "");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadNullCountrySpecified()
		{
			RefLatLongPostcode.Load(Factory, "2125", (RefCountry)null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadNullPostcodeSpecified()
		{
			RefLatLongPostcode.Load(Factory, null, "AU");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadEmptyPostcodeSpecified()
		{
			RefLatLongPostcode.Load(Factory, "", "AU");
		}

		#endregion

		#region Distance Calculation

		public void TestDistanceCalculationWithIdenticalLocations()
		{
			AssertEquals("Distance between 72 O'Riordan St to 76 O'Riordan St.", 0.066, RefLatLongPostcode.CalculateDistance(-33.916324, 151.195233, -33.916833, 151.194863));
			AssertEquals("Distance between same latitude and longitude should be 0", 0.0, RefLatLongPostcode.CalculateDistance(-33.916324, 151.195233, -33.916324, 151.195233));
		}

		public void TestDistance()
		{
			AssertEquals("Distance between West Pennant Hills and Alexandria", 22.097, RefLatLongPostcode.CalculateDistance(WPH, Alexandria));
			AssertEquals("Distance between Sydney and Caringbah", 21.384, RefLatLongPostcode.CalculateDistance(Sydney, Caringbah));
			AssertEquals("Distance between WPH and Caringbah", 33.83, RefLatLongPostcode.CalculateDistance(WPH, Caringbah));

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUBNE";

			OrgAddress westPH = org.Addresses.AddNew();
			westPH.OA_RL_NKRelatedPortCode = "AUSYD";
			westPH.OA_PostCode = WPH.RJ_Postcode;

			OrgAddress alex = org.Addresses.AddNew();
			alex.OA_RL_NKRelatedPortCode = "AUSYD";
			alex.OA_PostCode = Alexandria.RJ_Postcode;

			OrgAddress syd = org.Addresses.AddNew();
			syd.OA_RL_NKRelatedPortCode = "AUSYD";
			syd.OA_PostCode = Sydney.RJ_Postcode;

			OrgAddress car = org.Addresses.AddNew();
			car.OA_PostCode = Caringbah.RJ_Postcode;

			OrgAddress noPostCode = org.Addresses.AddNew();

			AssertEquals("Distance between West Pennant Hills and Alexandria", 22.097, RefLatLongPostcode.CalculateDistance(westPH, alex));
			AssertEquals("Distance between Sydney and Caringbah", 21.384, RefLatLongPostcode.CalculateDistance(syd, car));
			AssertEquals("Distance between WPH and Caringbah", 33.83, RefLatLongPostcode.CalculateDistance(westPH, car));
			AssertEquals("Distance between WPH and Org Address with no post code should be 0", (double)0, RefLatLongPostcode.CalculateDistance(westPH, noPostCode));
		}

		public void TestDistanceWithDocAddress()
		{
			AssertEquals("Distance between West Pennant Hills and Alexandria", 22.097, RefLatLongPostcode.CalculateDistance(WPH, Alexandria));
			AssertEquals("Distance between Sydney and Caringbah", 21.384, RefLatLongPostcode.CalculateDistance(Sydney, Caringbah));
			AssertEquals("Distance between WPH and Caringbah", 33.83, RefLatLongPostcode.CalculateDistance(WPH, Caringbah));

			JobDocAddress westPH = Factory.New<JobDocAddress>();
			westPH.E2_RN_NKCountryCode = "AU";
			westPH.E2_Postcode = WPH.RJ_Postcode;

			JobDocAddress alex = Factory.New<JobDocAddress>();
			alex.E2_RN_NKCountryCode = "AU";
			alex.E2_Postcode = Alexandria.RJ_Postcode;

			JobDocAddress syd = Factory.New<JobDocAddress>();
			syd.E2_RN_NKCountryCode = "AU";
			syd.E2_Postcode = Sydney.RJ_Postcode;

			JobDocAddress car = Factory.New<JobDocAddress>();
			car.E2_RN_NKCountryCode = "AU";
			car.E2_Postcode = Caringbah.RJ_Postcode;

			JobDocAddress noPostCode = Factory.New<JobDocAddress>();

			AssertEquals("Distance between West Pennant Hills and Alexandria", 22.097, RefLatLongPostcode.CalculateDistance(westPH, alex));
			AssertEquals("Distance between Sydney and Caringbah", 21.384, RefLatLongPostcode.CalculateDistance(syd, car));
			AssertEquals("Distance between WPH and Caringbah", 33.83, RefLatLongPostcode.CalculateDistance(westPH, car));
			AssertEquals("Distance between WPH and Org Address with no post code should be 0", (double)0, RefLatLongPostcode.CalculateDistance(westPH, noPostCode));
		}

		public void TestCalculatingDistanceShouldFilterOutIfBothLatAndLongAre0()
		{
			Factory.Save();
			var alexandriaPostcodeQuery = new ZQuery(RefLatLongPostcodeSchema.RJ_Postcode, "2015");
			var alexandriaPostcodes = Factory.Load<RefLatLongPostcode>(alexandriaPostcodeQuery);
			AssertEquals("Precondition: 2015 already existed in the DB, but setup should have created a duplicate.", 2, alexandriaPostcodes.Length);

			// ensure both 2015 have same lat-long
			var postcode2015a = alexandriaPostcodes[0];
			var postcode2015b = alexandriaPostcodes[1];
			var existing2015Lat = postcode2015a.RJ_Lattitude;
			var existing2015Long = postcode2015a.RJ_Longitude;
			postcode2015b.RJ_Lattitude = existing2015Lat;
			postcode2015b.RJ_Longitude = existing2015Long;
			var postcode2015DistanceToWPH = RefLatLongPostcode.CalculateDistance(WPH, postcode2015a);
			AssertEquals("Validate distance.", 22.097, postcode2015DistanceToWPH);

			// setup addresses with postcodes
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var westPHAddress = org.Addresses.AddNew();
			westPHAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			westPHAddress.OA_PostCode = "2125";
			var alexandriaAddress = org.Addresses.AddNew();
			alexandriaAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			alexandriaAddress.OA_PostCode = "2015";

			// test a (it may have tried to be loaded as top 1, ensure it doesn't)
			postcode2015a.RJ_Lattitude = 0;
			postcode2015a.RJ_Longitude = 0;
			AssertEquals("Should get 'b' distance as 'a' should have been filtered out", 22.097, RefLatLongPostcode.CalculateDistance(westPHAddress, alexandriaAddress));

			postcode2015b.RJ_Lattitude = 0;
			AssertEquals("'b' is still valid, distance should be 3735.295", 3735.295, RefLatLongPostcode.CalculateDistance(westPHAddress, alexandriaAddress));

			postcode2015b.RJ_Lattitude = existing2015Lat;
			postcode2015b.RJ_Longitude = 0;
			AssertEquals("'b' is still valid, distance should be 11852.172", 11852.172, RefLatLongPostcode.CalculateDistance(westPHAddress, alexandriaAddress));

			// test b (it may have tried to be loaded as top 1, ensure it doesn't)
			postcode2015a.RJ_Lattitude = existing2015Lat; // reset
			postcode2015a.RJ_Longitude = existing2015Long; // reset
			postcode2015b.RJ_Lattitude = 0;
			postcode2015b.RJ_Longitude = 0;
			AssertEquals("Should get 'a' distance as 'b' should have been filtered out", 22.097, RefLatLongPostcode.CalculateDistance(westPHAddress, alexandriaAddress));

			postcode2015a.RJ_Lattitude = 0;
			AssertEquals("'a' is still valid, distance should be 3735.295", 3735.295, RefLatLongPostcode.CalculateDistance(westPHAddress, alexandriaAddress));

			postcode2015a.RJ_Lattitude = existing2015Lat;
			postcode2015a.RJ_Longitude = 0;
			AssertEquals("'a' is still valid, distance should be 11852.172", 11852.172, RefLatLongPostcode.CalculateDistance(westPHAddress, alexandriaAddress));
		}

		#endregion

		#region Degrees to Radians Conversion

		public void TestDegreeToRadianConversion()
		{
			AssertEquals("Degrees correctly converted to Radians", Math.PI, RefLatLongPostcode.ConvertDegreesToRadians(180m));
			AssertEquals("Degrees correctly converted to Radians", Math.PI / 2, RefLatLongPostcode.ConvertDegreesToRadians((double)90));
			AssertEquals("Degrees correctly converted to Radians", -2.269, Math.Round(RefLatLongPostcode.ConvertDegreesToRadians(-130m), 3));
		}

		#endregion

		#region Identical Location

		public void TestIdenticalLocation()
		{
			RefLatLongPostcode loc1 = Factory.New<RefLatLongPostcode>();
			loc1.RJ_Postcode = "2125";
			loc1.RJ_Longitude = 10m;
			loc1.RJ_Lattitude = 30m;

			RefLatLongPostcode loc2 = Factory.New<RefLatLongPostcode>();
			loc2.RJ_Postcode = "2126";
			loc2.RJ_Longitude = 10m;
			loc2.RJ_Lattitude = 30m;

			Assert("Locations are the same, irrelevant of postcode", loc1.IsIdenticalLocation(loc2));
			Assert("Locations are the same, irrelevant of postcode", loc2.IsIdenticalLocation(loc1));

			loc2.RJ_Longitude = 50m;
			Assert("Locations are not the same", !loc1.IsIdenticalLocation(loc2));
			Assert("Locations are not the same", !loc2.IsIdenticalLocation(loc1));
		}

		#endregion

		#region Implementation

		RefLatLongPostcode WPH;
		RefLatLongPostcode Alexandria;
		RefLatLongPostcode Sydney;
		RefLatLongPostcode Caringbah;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			RefLatLongPostcode latLong = factory.New<RefLatLongPostcode>();
			latLong.RJ_RN_NKCountry = "IN";
			latLong.RJ_Postcode = "9191";

			return latLong;
		}

		protected override void SetUp()
		{
			base.SetUp();

			WPH = Factory.New<RefLatLongPostcode>();
			WPH.RJ_Postcode = "2125";
			WPH.RJ_CitySuburb = "West Pennant Hills";
			WPH.RJ_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			WPH.RJ_Longitude = 151.032189191692m;
			WPH.RJ_Lattitude = -33.7511134062404m;

			Alexandria = Factory.New<RefLatLongPostcode>();
			Alexandria.RJ_Postcode = "2015";
			Alexandria.RJ_CitySuburb = "Alexandria";
			Alexandria.RJ_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			Alexandria.RJ_Longitude = 151.195499014747m;
			Alexandria.RJ_Lattitude = -33.8976004733696m;

			Sydney = Factory.New<RefLatLongPostcode>();
			Sydney.RJ_Postcode = "2000";
			Sydney.RJ_CitySuburb = "Sydney";
			Sydney.RJ_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			Sydney.RJ_Longitude = 151.210019996922m;
			Sydney.RJ_Lattitude = -33.869675244948m;

			Caringbah = Factory.New<RefLatLongPostcode>();
			Caringbah.RJ_Postcode = "2229";
			Caringbah.RJ_CitySuburb = "Caringbah";
			Caringbah.RJ_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			Caringbah.RJ_Longitude = 151.120162028202m;
			Caringbah.RJ_Lattitude = -34.04794659738m;
		}

		#endregion
	}
}
