using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRegistrationNumberTypeListTest : TestCaseWithFactory
	{
		RefCountryCollection countries;
		OrgHeader organization;
		OrgRegistrationNumberTypeList orgNumberTypes;

		RefCountry AU
		{
			get { return Factory.Load<RefCountry>(Constants.CountryGuids.Australia); }
		}

		RefCountryCollection Countries
		{
			get
			{
				if (countries == null)
				{
					countries = new RefCountryCollection(Factory);
				}
				return countries;
			}
		}

		OrgHeader Organization
		{
			get { return organization ?? (organization = OrgRegistrationNumberTest.GetTestOrganization(Factory, SG.Code)); }
		}

		OrgRegistrationNumberTypeList OrgNumberTypes
		{
			get { return orgNumberTypes ?? (orgNumberTypes = new OrgRegistrationNumberTypeList(Organization.Country)); }
		}

		RefCountry SG
		{
			get { return Factory.Load<RefCountry>(Constants.CountryGuids.Singapore); }
		}

		string SGGstCode
		{
			get { return OrgRegistrationNumberTypeList.GetNumberTypeWithCountryPrefix(OrgCusCode.CodeTypes.GSTCode, Constants.CountryCodes.Singapore); }
		}

		void AssertContainsCodesForCountry(RefCountry country, bool hasPrefix, int startIndex)
		{
			string[] numberTypes = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(country);
			CodeDescriptionPairList customsCodes = new OrgCodeLists().CustomsCodes_List(country);
			OrgRegistrationNumberTypeList orgNumberTypes = new OrgRegistrationNumberTypeList(Organization.Country);
			for (int i = 0; i < numberTypes.Length; i++)
			{
				string numberType = numberTypes[i];
				string id = numberType + '/' + country.RN_Code;
				string expectedCode = hasPrefix ? OrgRegistrationNumberTypeList.GetNumberTypeWithCountryPrefix(numberType, country.RN_Code) : numberType;
				int expectedIndex = i + startIndex;
				string description = orgNumberTypes.GetDescriptionFromCode(expectedCode);
				AssertEquals(string.Format("{0} should be at position {1}.", id, expectedIndex), expectedCode, orgNumberTypes[expectedIndex].Code);
				AssertEquals("Country Code for " + id, country.Code, orgNumberTypes.GetCountryCode(expectedCode));
				AssertEquals("There should be a description for " + id + ".", false, string.IsNullOrEmpty(description));
				AssertEquals("Description for " + id, customsCodes.GetDescriptionFromCode(numberType), description);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = AU.Code;
		}

		public void TestLocalBusinessRegNumberTypeShouldBeInGetApplicableNumberTypesList()
		{
			RefCountryCollection countries = new RefCountryCollection(Factory);

			AssertNotEquals("Ref Country Collection should not be empty.", 0, countries.Count);

			foreach (var country in countries)
			{
				var numberTypes = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(country);

				var localBusinessRegNoCodeType = country.LocalBusinessRegNoCodeType;
				AssertCollectionContains("For country '" + country.RN_Code + "' Local business Reg No Type '" + localBusinessRegNoCodeType + "' should be in GetApplicableNumberTypes()", localBusinessRegNoCodeType, numberTypes);
			}
		}

		public void TestGetActualCode()
		{
			AssertEquals("Actual code for GST", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, OrgNumberTypes.GetActualCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber));
			AssertEquals("Actual code for GST/SG", OrgCusCode.CodeTypes.GSTCode, OrgNumberTypes.GetActualCode(SGGstCode));
			AssertEquals("Actual code for MED", OrgCusCode.CodeTypes.MedicareID, OrgNumberTypes.GetActualCode(OrgCusCode.CodeTypes.MedicareID));
			AssertEquals("Actual code for !@#", "", OrgNumberTypes.GetActualCode("!@#"));
		}

		public void TestGetCountryCode()
		{
			AssertEquals("Country PK for GST", AU.Code, OrgNumberTypes.GetCountryCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber));
			AssertEquals("Country PK for GST/SG", SG.Code, OrgNumberTypes.GetCountryCode(SGGstCode));
			AssertEquals("Country PK for MED", AU.Code, OrgNumberTypes.GetCountryCode(OrgCusCode.CodeTypes.MedicareID));
			AssertEquals("Country PK for !@#", ZString.Empty, OrgNumberTypes.GetCountryCode("!@#"));
		}

		public void TestGetDisplayCode()
		{
			AssertEquals("Display code for GST/AU", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, OrgNumberTypes.GetDisplayCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, AU));
			AssertEquals("Display code for GST/SG", SGGstCode, OrgNumberTypes.GetDisplayCode(OrgCusCode.CodeTypes.GSTCode, SG));
			AssertEquals("Display code for MED/AU", OrgCusCode.CodeTypes.MedicareID, OrgNumberTypes.GetDisplayCode(OrgCusCode.CodeTypes.MedicareID, AU));
			AssertEquals("Display code for MED/SG", "", OrgNumberTypes.GetDisplayCode(OrgCusCode.CodeTypes.MedicareID, SG));
			AssertEquals("Display code for !@#/AU", "", OrgNumberTypes.GetDisplayCode("!@#", AU));
		}

		public void TestGetNumberTypeWithCountryPrefix()
		{
			AssertEquals("GetNumberTypeWithCountryPrefix", "XY:ABC", OrgRegistrationNumberTypeList.GetNumberTypeWithCountryPrefix("ABC", "XY"));
		}

		public void TestNumberTypesForTurkeyAreValidWhenEnabledNewTurkeyARComplianceFeatures()
		{
			RefCountry country = Countries.FirstOrDefault(x => x.Code == Constants.CountryCodes.Turkey);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime()))
			{
				AssertNumberOfTypesAreValidForOneCountry(country);
			}
		}

		public void TestNumberTypesForAllCountriesAreValid()
		{
			foreach (RefCountry country in Countries)
			{
				AssertNumberOfTypesAreValidForOneCountry(country);
			}
		}

		void AssertNumberOfTypesAreValidForOneCountry(RefCountry country)
		{
			string[] numberTypes = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(country);
			AssertEquals("1st " + country.RN_Code + " number type", country.LocalBusinessRegNoCodeType, numberTypes[0]);

			int passportIndex = Array.IndexOf(numberTypes, OrgCusCode.CodeTypes.PassportID);
			Assert(country.RN_Code + " should have passport number as one of its applicable number types.", passportIndex != -1);

			int driverLicenceIndex = Array.IndexOf(numberTypes, OrgCusCode.CodeTypes.DriverLicenceID);
			Assert(country.RN_Code + " should have driver's license number as one of its applicable number types.", driverLicenceIndex != -1);
			Assert("The passport number for " + country.RN_Code + " should have higher priority over the driver's license number.", driverLicenceIndex > passportIndex);

			CodeDescriptionPairList customsCodes = new OrgCodeLists().CustomsCodes_List(country);
			for (int i = 0; i < numberTypes.Length; i++)
			{
				string numberType = numberTypes[i];
				string id = country.RN_Code + '/' + numberType;
				AssertEquals(id + " should be in OrgCodeLists.CustomsCodes_List.", true, customsCodes.ContainsCode(numberType));
				for (int j = i + 1; j < numberTypes.Length; j++)
				{
					AssertNotEquals(id + " has been duplicated.", numberType, numberTypes[j]);
				}
			}
		}

		public void TestSingleCountryList()
		{
			int numberTypesLength = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(AU).Length;

			Organization.ClosestPort.RL_RN_NKCountryCode = ZString.Empty;
			AssertContainsCodesForCountry(AU, false, 0);
			AssertEquals("Count", numberTypesLength, new OrgRegistrationNumberTypeList(Organization.Country).Count);

			Organization.ClosestPort.RL_RN_NKCountryCode = AU.Code;
			AssertContainsCodesForCountry(AU, false, 0);
			AssertEquals("Count", numberTypesLength, new OrgRegistrationNumberTypeList(Organization.Country).Count);
		}

		public void TestMultipleCountryList()
		{
			int auNumberTypesLength = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(AU).Length;
			int sgNumberTypesLength = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(SG).Length;

			Organization.ClosestPort.RL_RN_NKCountryCode = SG.Code;
			AssertEquals("Count", auNumberTypesLength + sgNumberTypesLength, new OrgRegistrationNumberTypeList(Organization.Country).Count);
			AssertContainsCodesForCountry(AU, false, 0);
			AssertContainsCodesForCountry(SG, true, auNumberTypesLength);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = SG.Code;
			Organization.ClosestPort.RL_RN_NKCountryCode = AU.Code;
			AssertEquals("Count", auNumberTypesLength + sgNumberTypesLength, new OrgRegistrationNumberTypeList(Organization.Country).Count);
			AssertContainsCodesForCountry(SG, false, 0);
			AssertContainsCodesForCountry(AU, true, sgNumberTypesLength);
		}
	}
}
