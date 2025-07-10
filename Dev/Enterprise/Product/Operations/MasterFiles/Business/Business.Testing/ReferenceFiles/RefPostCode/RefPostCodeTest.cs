using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefPostCode))]
	sealed class RefPostCodeTest : EnterpriseBusinessObjectTestCase
	{
		#region System Defined Postcodes

		public void TestRKCityTownPostCodeReadOnly()
		{
			RefPostCode postcode = Factory.New<RefPostCode>();
			postcode.RK_IsSystem = false;
			postcode.OnLoaded();
			Assert("RK_CityTownPostCode should not be read only", !postcode.RK_CityTownPostCodeInfo.ReadOnly);
			postcode.RK_IsSystem = true;
			postcode.OnLoaded();
			Assert("RK_CityTownPostCode should be read only", postcode.RK_CityTownPostCodeInfo.ReadOnly);
		}

		public void TestRKLattitudeReadOnly()
		{
			RefPostCode postcode = Factory.New<RefPostCode>();
			postcode.RK_IsSystem = false;
			postcode.OnLoaded();
			Assert("RK_Lattitude should not be read only", !postcode.RK_LattitudeInfo.ReadOnly);
			postcode.RK_IsSystem = true;
			postcode.OnLoaded();
			Assert("RK_Lattitude should be read only", postcode.RK_LattitudeInfo.ReadOnly);
		}

		public void TestRKLongitudeReadOnly()
		{
			RefPostCode postcode = Factory.New<RefPostCode>();
			postcode.RK_IsSystem = false;
			postcode.OnLoaded();
			Assert("RK_Longitude should not be read only", !postcode.RK_LongitudeInfo.ReadOnly);
			postcode.RK_IsSystem = true;
			postcode.OnLoaded();
			Assert("RK_Longitude should be read only", postcode.RK_LongitudeInfo.ReadOnly);
		}

		public void TestRKRNNKCountryReadOnly()
		{
			RefPostCode postcode = Factory.New<RefPostCode>();
			postcode.RK_IsSystem = false;
			postcode.OnLoaded();
			Assert("RK_RN_NKCountry should not be read only", !postcode.RK_RN_NKCountryInfo.ReadOnly);
			postcode.RK_IsSystem = true;
			postcode.OnLoaded();
			Assert("RK_RN_NKCountry should be read only", postcode.RK_RN_NKCountryInfo.ReadOnly);
		}

		#endregion

		public void TestLoad()
		{
			var postcode = Factory.NewWithValidTestData<RefPostCode>();
			postcode.RK_CityTownPostCode = "PostCode";
			postcode.RK_RN_NKCountry = "CN";
			postcode.RK_IsActive = true;
			Factory.Save();

			AssertNotNull(RefPostCode.Load(Factory, "PostCode", "CN"));
			AssertNull(RefPostCode.Load(Factory, "PostCodeNonExist", "CC"));

			postcode.RK_IsActive = false;
			Factory.Save();
			AssertNull(RefPostCode.Load(Factory, "PostCode", "CN"));

			AssertExceptionThrown<ArgumentNullException>(() => { RefPostCode.Load(Factory, "", "CC"); });
			AssertExceptionThrown<ArgumentNullException>(() => { RefPostCode.Load(Factory, "Bla", ""); });
		}
	}
}
