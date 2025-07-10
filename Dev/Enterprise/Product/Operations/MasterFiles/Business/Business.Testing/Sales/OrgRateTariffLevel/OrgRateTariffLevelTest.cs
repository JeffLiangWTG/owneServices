using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRateTariffLevel))]
	sealed class OrgRateTariffLevelTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsDefault()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tariffLevels = org.CompanyData.RateTariffLevels;
			var tariffLevel = tariffLevels.AddNew();
			tariffLevel.P7_OH = org.PK;
			tariffLevel.P7_GC = org.CompanyData.OB_GC;
			tariffLevel.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			tariffLevel.P7_TariffLevel = 1;
			tariffLevel.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;

			AssertEquals(true, tariffLevel.IsDefault);

			tariffLevel.P7_TariffType = "FRT";
			AssertEquals(false, tariffLevel.IsDefault);

			tariffLevel.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			AssertEquals(true, tariffLevel.IsDefault);

			tariffLevel.P7_Mode = "FCL";
			AssertEquals(false, tariffLevel.IsDefault);

			tariffLevel.P7_Mode = OrgRateTariffLevel.ALL;
			AssertEquals(true, tariffLevel.IsDefault);

			tariffLevel.P7_Direction = "EXP";
			AssertEquals(false, tariffLevel.IsDefault);

			tariffLevel.P7_Direction = nameof(OrgRateTariffLevel.Directions.ALL);
			AssertEquals(true, tariffLevel.IsDefault);

			tariffLevel.P7_StartDate = ZDate.Today;
			AssertEquals(false, tariffLevel.IsDefault);

			tariffLevel.P7_StartDate = ZDate.Empty;
			AssertEquals(true, tariffLevel.IsDefault);

			tariffLevel.P7_ExpiryDate = ZDate.Today;
			AssertEquals(false, tariffLevel.IsDefault);

			tariffLevel.P7_ExpiryDate = ZDate.Empty;
			AssertEquals(true, tariffLevel.IsDefault);
		}

		public void TestTariffTypeDefaultsTransportMode()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Base Company Tariff");
			OrgCompanyDataLookupsTest.InsertCompanyTariff(2, "Company Tariff #2");

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevelCollection tariffLevels = org.CompanyData.RateTariffLevels;

			tariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.ALL), "LCL", 2);
			AssertEquals("FRT", tariffLevels[0].P7_TariffType);
			AssertEquals("Freight", tariffLevels[0].TariffDescription);
			AssertEquals("LCL", tariffLevels[0].P7_Mode);
			AssertEquals("ALL", tariffLevels[0].P7_Direction);
			AssertEquals((ZByte)2, tariffLevels[0].P7_TariffLevel);

			tariffLevels[0].P7_TariffType = "CFS";
			AssertEquals("LCL", tariffLevels[0].P7_Mode);

			tariffLevels[0].P7_TariffType = "TRN";
			AssertEquals("ALL", tariffLevels[0].P7_Mode);

			tariffLevels[0].P7_Mode = "LRO";
			tariffLevels[0].P7_TariffType = "TRN";
			AssertEquals("LRO", tariffLevels[0].P7_Mode);
		}

		public void TestNewProperties()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Base Company Tariff");
			OrgCompanyDataLookupsTest.InsertCompanyTariff(2, "Company Tariff #2");

			OrgHeader org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevelCollection tariffLevels = org.CompanyData.RateTariffLevels;
			AssertEquals(0, tariffLevels.Count);

			tariffLevels.SetLevel("FRT", 0);
			AssertEquals("FRT", tariffLevels[0].P7_TariffType);
			AssertEquals("Freight", tariffLevels[0].TariffDescription);
			AssertEquals(3, tariffLevels[0].Lookups.TariffLevels.Count);
			tariffLevels.SetLevel("WHS", 0);
			AssertEquals("WHS", tariffLevels[1].P7_TariffType);
			AssertEquals("Product Warehouse", tariffLevels[1].TariffDescription);
			AssertEquals(3, tariffLevels[1].Lookups.TariffLevels.Count);

			OrgRateTariffLevel fRTTariffLevel = tariffLevels[0];
			AssertEquals("0", fRTTariffLevel.TariffLevelAsString);

			fRTTariffLevel.TariffLevelAsString = "1";
			AssertEquals((byte)1, fRTTariffLevel.P7_TariffLevel);
			AssertNoErrors(fRTTariffLevel.TariffLevelAsStringInfo);

			fRTTariffLevel.TariffLevelAsString = "##";
			AssertEquals((byte)0, fRTTariffLevel.P7_TariffLevel);
			AssertHasError(fRTTariffLevel.TariffLevelAsStringInfo, "Enter a valid selection.");

			fRTTariffLevel.TariffLevelAsString = "2";
			AssertEquals((byte)2, fRTTariffLevel.P7_TariffLevel);
			AssertNoErrors(fRTTariffLevel.TariffLevelAsStringInfo);

			fRTTariffLevel.TariffLevelAsString = "123";
			AssertEquals((byte)123, fRTTariffLevel.P7_TariffLevel);
			AssertHasError(fRTTariffLevel.TariffLevelAsStringInfo, "Enter a valid selection.");
		}

		public void TestIsExpired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tariffLevels = org.CompanyData.RateTariffLevels;
			var tariffLevel = tariffLevels.AddNew();

			AssertEquals(false, tariffLevel.IsExpired);

			tariffLevel.P7_ExpiryDate = ZDate.Today.AddDays(-1);
			AssertEquals(true, tariffLevel.IsExpired);

			tariffLevel.P7_ExpiryDate = ZDate.Today;
			AssertEquals(false, tariffLevel.IsExpired);
		}

		#region Change logging

		public void TestCreateLevelLogs()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "KATTESSYD";
			OrgRateTariffLevelCollection tariffLevels = org.CompanyData.RateTariffLevels;
			OrgRateTariffLevel level = tariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			level.P7_StartDate = new ZDate(2024, 7, 29);

			Factory.Save();

			AssertHasLog(org.GetLogs(), "ADD", "Added Organisation KATTESSYD Company Tariff and Group Rate Usage config for 'DEF, ALL, ALL, 1, False, 29-Jul-24, Blank'.");
		}

		public void TestUpdateLevelLogs()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "KATTESSYD";
			OrgRateTariffLevelCollection tariffLevels = org.CompanyData.RateTariffLevels;
			OrgRateTariffLevel level = tariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			level.P7_OH = org.PK;
			level.P7_GC = org.CompanyData.OB_GC;
			level.P7_StartDate = new ZDate(2024, 7, 29);
			Factory.Save();

			level.P7_ExpiryDate = new ZDate(2024, 7, 31);
			level.P7_ApplyGroupRate = true;
			Factory.Save();

			AssertHasLog(org.GetLogs(), "EDT", "Changed Organisation KATTESSYD Company Tariff and Group Rate Usage config for 'DEF, ALL, ALL, 1, False, 29-Jul-24, Blank' to 'DEF, ALL, ALL, 1, True, 29-Jul-24, 31-Jul-24'.");
		}

		public void TestDeleteLevelLogs()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "KATTESSYD";
			OrgRateTariffLevelCollection tariffLevels = org.CompanyData.RateTariffLevels;
			OrgRateTariffLevel level = tariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			level.P7_StartDate = new ZDate(2024, 7, 29);
			Factory.Save();

			level.Delete();
			Factory.Save();

			AssertHasLog(org.GetLogs(), "DEL", "Deleted Organisation KATTESSYD Company Tariff and Group Rate Usage config for 'DEF, ALL, ALL, 1, False, 29-Jul-24, Blank'.");
		}

		void AssertHasLog(Logs logs, string expectedType, string expectedReference)
		{
			var filteredLogs = logs.Find(u => u.Event.SE_Code == expectedType).ToList();
			CombineAssertions(() =>
			{
				AssertEquals(1, filteredLogs.Count);
				AssertEquals(expectedReference, filteredLogs.First().DisplayEventReference);
			});
		}

		#endregion

		public void TestMultilingualDescription()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Base Company Tariff");

			OrgHeader org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevelCollection tariffLevels = org.CompanyData.RateTariffLevels;
			AssertEquals(0, tariffLevels.Count);

			using (Res.TemporarilySwitchLanguage("DE-DE"))
			using (var mockRes = Res.UseMockData())
			{
				var companyTariffs = new OrgCodeLists().GetCompanyRatingHeaders(Factory, Env.CurrentCompanyPK).Cast<ICompanyTariff>();
				var baseCompanyTariff = companyTariffs.Single();

				var multilingualDescription = "Basis Unternehmenstarif";
				var resKey = baseCompanyTariff.TH_GlobalRateDescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(baseCompanyTariff.TH_GlobalRateDescriptionInfo, "Base Company Tariff").ResourceKey;
				mockRes.Put(resKey, new ResourceStringData(resKey, multilingualDescription));

				tariffLevels.SetLevel("FRT", 0);
				var baseCompanyTariffLevel = tariffLevels[0];
				AssertEquals("FRT", baseCompanyTariffLevel.P7_TariffType);
				AssertEquals("Freight", baseCompanyTariffLevel.TariffDescription);
				AssertEquals(2, baseCompanyTariffLevel.Lookups.TariffLevels.Count);

				AssertEquals(multilingualDescription, baseCompanyTariffLevel.Lookups.TariffLevels[1].Description);
			}
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldRateTariffLevelValue = Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed;

			try
			{
				OrgRateTariffLevel testLevel = OrgInDB.CompanyData.RateTariffLevels.AddNew();

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testLevel.P7_TariffLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLevel.P7_TariffTypeInfo.ReadOnly);

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testLevel.P7_TariffLevelInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLevel.P7_TariffTypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = oldRateTariffLevelValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion
	}
}
