using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRateTariffLevelValidationTest : BusinessObjectValidationTestCase
	{
		void AssertHasListValidationError(ZPropertyInfo info)
		{
			AssertHasError(info, ListValidation.GetNotificationMessage(info).ToString());
		}

		public void TestValidate()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Base Company Tariff");
			var org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			var level = org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);

			AssertNoErrors(level.P7_TariffTypeInfo);
			level.P7_TariffType = "";
			AssertHasError(level.P7_TariffTypeInfo, "Please enter a " + level.P7_TariffTypeInfo.Description + ".");

			level.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			AssertNoErrors(level.P7_TariffTypeInfo);
			level.P7_TariffType = "###";
			AssertHasListValidationError(level.P7_TariffTypeInfo);

			AssertNoErrors(level.P7_DirectionInfo);
			level.P7_Direction = "###";
			AssertHasListValidationError(level.P7_DirectionInfo);

			AssertNoErrors(level.P7_ModeInfo);
			level.P7_Mode = "###";
			AssertHasListValidationError(level.P7_ModeInfo);

			level.TariffLevelAsString = "1";
			AssertNoErrors(level.TariffLevelAsStringInfo);
			level.TariffLevelAsString = "2";
			AssertHasListValidationError(level.TariffLevelAsStringInfo);
		}

		public void TestValidate_InactiveCompany()
		{
			var org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			var level = org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);

			GlbCompany.CurrentCompany.GC_IsActive = false;
			Factory.Save();
			level.Validation.ValidateP7_GC();
			AssertHasError(level.P7_GCInfo, "Changes to the Company Tariff and Group Rate Usage are invalid as the current logged in company is inactive.");
		}

		public void TestValidate_NonDuplicateLevels()
		{
			var org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			org.CompanyData.RateTariffLevels.SetLevel("FRT", "EXP", "LCL", ZDate.Today.AddDays(-1), ZDate.Today, 0);
			var level = org.CompanyData.RateTariffLevels.AddNew();

			TestCase("DEF", "EXP", "LCL", ZDate.Empty, ZDate.Empty);
			TestCase("FRT", "EXP", "ALL", ZDate.Empty, ZDate.Empty);
			TestCase("FRT", "ALL", "LCL", ZDate.Empty, ZDate.Empty);
			TestCase("FRT", "EXP", "LCL", ZDate.Today.AddDays(1), ZDate.Empty);
			TestCase("FRT", "EXP", "LCL", ZDate.Empty, ZDate.Today.AddDays(-2));

			void TestCase(string type, string direction, string mode, ZDate startDate, ZDate expiryDate)
			{
				level.P7_TariffType = type;
				level.P7_Mode = mode;
				level.P7_Direction = direction;
				level.P7_StartDate = startDate;
				level.P7_ExpiryDate = expiryDate;

				AssertNoRowErrors(level);
			}
		}

		public void TestValidate_DuplicateLevels()
		{
			var org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			org.CompanyData.RateTariffLevels.SetLevel("FRT", "EXP", "LCL", ZDate.Today.AddDays(-1), ZDate.Today, 0);
			var level = org.CompanyData.RateTariffLevels.AddNew();

			TestCase("FRT", "LCL", "EXP", ZDate.Today, ZDate.Today);
			TestCase("FRT", "LCL", "EXP", ZDate.Empty, ZDate.Today);
			TestCase("FRT", "LCL", "EXP", ZDate.Today, ZDate.Empty);
			TestCase("FRT", "LCL", "EXP", ZDate.Empty, ZDate.Empty);

			void TestCase(string type, string mode, string direction, ZDate startDate, ZDate expiryDate)
			{
				level.P7_TariffType = type;
				level.P7_Mode = mode;
				level.P7_Direction = direction;
				level.P7_StartDate = startDate;
				level.P7_ExpiryDate = expiryDate;

				AssertHasRowError(level, "Duplicate or overlapping entries could not be saved. Please enter unique Tariff Type, Transport Mode, Service Direction with no overlapping Start and Expiry Dates.");
			}
		}

		public void TestValidate_ExpiryDateBeforeStartDate()
		{
			var org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			var level = org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);

			level.P7_ExpiryDate = ZDate.Today.AddDays(1);
			AssertNoErrors(level.P7_StartDateInfo);
			AssertNoErrors(level.P7_ExpiryDateInfo);
			level.P7_StartDate = ZDate.Today.AddDays(3);
			AssertHasError(level.P7_StartDateInfo, "Start date must be before the expiry date.");
			level.P7_StartDate = ZDate.Today;
			AssertNoErrors(level.P7_StartDateInfo);

			level.P7_ExpiryDate = ZDate.Today.AddDays(-1);
			AssertHasError(level.P7_ExpiryDateInfo, "Expiry date must be after the start date.");
			level.P7_ExpiryDate = ZDate.Today.AddDays(1);
			AssertNoErrors(level.P7_ExpiryDateInfo);
		}
	}
}

