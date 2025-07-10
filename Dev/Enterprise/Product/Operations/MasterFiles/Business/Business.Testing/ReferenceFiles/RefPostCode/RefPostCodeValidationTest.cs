using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefPostCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDuplicatesPostcode_Add_WhenHasInactiveDuplicates()
		{
			var postcode = Factory.New<RefPostCode>();
			postcode.RK_CityTownPostCode = "ABC";
			postcode.RK_RN_NKCountry = "CN";
			postcode.RK_IsActive = false;
			Factory.Save();

			var newPostcode = Factory.New<RefPostCode>();
			newPostcode.RK_CityTownPostCode = "ABC";
			newPostcode.RK_RN_NKCountry = "CN";
			newPostcode.RK_IsActive = false;

			AssertHasErrorContaining(newPostcode.RK_IsActiveInfo, "(Postcode: ABC + Country/Region: CN + Active Status: Inactive) already exists, please retrieve and use it.");

			newPostcode.RK_IsActive = true;
			AssertHasErrorContaining(newPostcode.RK_IsActiveInfo, "(Postcode: ABC + Country/Region: CN + Active Status: Inactive) already exists, please retrieve and activate it.");
		}

		public void TestDuplicatesPostcode_Add_WhenHasActiveDuplicates()
		{
			var postcode = Factory.New<RefPostCode>();
			postcode.RK_CityTownPostCode = "ABC";
			postcode.RK_RN_NKCountry = "CN";
			postcode.RK_IsActive = true;
			Factory.Save();

			var newPostcode = Factory.New<RefPostCode>();
			newPostcode.RK_IsActive = true;
			newPostcode.RK_CityTownPostCode = "ABC";
			newPostcode.RK_RN_NKCountry = "CN";
			AssertHasErrorContaining(newPostcode.RK_RN_NKCountryInfo, "(Postcode: ABC + Country/Region: CN + Active Status: Active) already exists, please retrieve and use it.");

			newPostcode.RK_RN_NKCountry = "US";
			AssertNoNotifications(newPostcode.RK_RN_NKCountryInfo);

			Factory.Save();

			newPostcode.Reload();
			AssertEquals(true, newPostcode.IsInDatabase);
			AssertEquals("US", newPostcode.RK_RN_NKCountry);
		}

		public void TestDuplicatesPostcode_Edit_WhenHasActiveDuplicates()
		{
			var postcode = Factory.New<RefPostCode>();
			postcode.RK_CityTownPostCode = "ABC";
			postcode.RK_RN_NKCountry = "CN";
			postcode.RK_IsActive = true;

			var duplicatePostcode = Factory.New<RefPostCode>();
			duplicatePostcode.RK_IsActive = true;
			duplicatePostcode.RK_CityTownPostCode = "DEF";
			duplicatePostcode.RK_RN_NKCountry = "CN";
			Factory.Save();

			duplicatePostcode.RK_CityTownPostCode = "ABC";
			AssertHasErrorContaining(duplicatePostcode.RK_CityTownPostCodeInfo, "(Postcode: ABC + Country/Region: CN + Active Status: Active) already exists, please retrieve and use it.");

			duplicatePostcode.RK_CityTownPostCode = "GHI";
			AssertNoNotifications(duplicatePostcode.RK_CityTownPostCodeInfo);

			Factory.Save();

			duplicatePostcode.Reload();
			AssertEquals(true, duplicatePostcode.IsInDatabase);
			AssertEquals("GHI", duplicatePostcode.RK_CityTownPostCode);
		}

		public void TestDuplicatesPostcode_Edit_WhenHasInactiveDuplicates()
		{
			var postcode = Factory.New<RefPostCode>();
			postcode.RK_CityTownPostCode = "ABC";
			postcode.RK_RN_NKCountry = "CN";
			postcode.RK_IsActive = false;

			var duplicatePostcode = Factory.New<RefPostCode>();
			duplicatePostcode.RK_IsActive = true;
			duplicatePostcode.RK_CityTownPostCode = "DEF";
			duplicatePostcode.RK_RN_NKCountry = "CN";
			Factory.Save();

			duplicatePostcode.RK_CityTownPostCode = "ABC";
			AssertHasErrorContaining(duplicatePostcode.RK_CityTownPostCodeInfo, "(Postcode: ABC + Country/Region: CN + Active Status: Inactive) already exists, please retrieve and activate it.");

			duplicatePostcode.RK_CityTownPostCode = "GHI";
			AssertNoNotifications(duplicatePostcode.RK_CityTownPostCodeInfo);

			Factory.Save();

			duplicatePostcode.Reload();
			AssertEquals(true, duplicatePostcode.IsInDatabase);
			AssertEquals("GHI", duplicatePostcode.RK_CityTownPostCode);
		}
	}
}
