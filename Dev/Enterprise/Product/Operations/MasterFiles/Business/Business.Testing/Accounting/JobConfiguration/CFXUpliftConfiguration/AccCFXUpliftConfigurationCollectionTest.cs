using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCFXUpliftConfigurationCollection))]
	sealed class AccCFXUpliftConfigurationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCompanyLevelCollectionReturnsCorrectSets()
		{
			var configsPk = PrepTestData();

			var companyLevelCollection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);

			AssertEquals(AccCFXConfigurationLevelEnum.Company, companyLevelCollection.Level);

			companyLevelCollection.Load();

			Assert(companyLevelCollection.IsLoaded);
			AssertEquals(1, companyLevelCollection.Count);

			var cfxConfig = companyLevelCollection.Cast<AccCFXUpliftConfiguration>().FirstOrDefault();

			AssertNotNull(cfxConfig);
			AssertEquals(AccCFXConfigurationLevelEnum.Company, cfxConfig.Level);
			AssertEquals(configsPk.cmpLevelPk, cfxConfig.PK);

			TestAddNew(companyLevelCollection, ZGuid.Empty);
		}

		public void TestBranchLevelCollectionReturnsCorrectSets()
		{
			var configsPk = PrepTestData();

			var branchLevelCollection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK);
			AssertEquals(AccCFXConfigurationLevelEnum.Branch, branchLevelCollection.Level);

			branchLevelCollection.Load();

			Assert(branchLevelCollection.IsLoaded);
			AssertEquals(2, branchLevelCollection.Count);

			var cfxCmpConfig = branchLevelCollection.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(c => c.Level == AccCFXConfigurationLevelEnum.Company);

			AssertNotNull(cfxCmpConfig);
			AssertEquals(configsPk.cmpLevelPk, cfxCmpConfig.PK);

			var cfxBrnConfig = branchLevelCollection.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(c => c.Level == AccCFXConfigurationLevelEnum.Branch);

			AssertNotNull(cfxBrnConfig);
			AssertEquals(configsPk.brnLevelPk, cfxBrnConfig.PK);

			TestAddNew(branchLevelCollection, Env.CurrentBranchPK);
		}

		public void TestOrgLevelCollectionReturnsCorrectSets()
		{
			var configsPk = PrepTestData();

			var orgLevelCollection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentCompany.OrganisationPK);
			AssertEquals(AccCFXConfigurationLevelEnum.Organisation, orgLevelCollection.Level);

			orgLevelCollection.Load();

			Assert(orgLevelCollection.IsLoaded);
			AssertEquals(3, orgLevelCollection.Count);

			var cfxCmpConfig = orgLevelCollection.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(c => c.Level == AccCFXConfigurationLevelEnum.Company);

			AssertNotNull(cfxCmpConfig);
			AssertEquals(configsPk.cmpLevelPk, cfxCmpConfig.PK);

			var cfxBrnConfig = orgLevelCollection.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(c => c.Level == AccCFXConfigurationLevelEnum.Branch);

			AssertNotNull(cfxBrnConfig);
			AssertEquals(configsPk.brnLevelPk, cfxBrnConfig.PK);

			var cfxOrgConfig = orgLevelCollection.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(c => c.Level == AccCFXConfigurationLevelEnum.Organisation);

			AssertNotNull(cfxOrgConfig);
			AssertEquals(configsPk.orgLevelPk, cfxOrgConfig.PK);

			TestAddNew(orgLevelCollection, Env.CurrentCompany.OrganisationPK);
		}

		public void TestSecurityRightsAreAppliedCorrectly()
		{
			var companyLevelCollection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
			var branchLevelCollection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK);
			var orgLevelCollection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentCompany.OrganisationPK);

			Assert(!companyLevelCollection.ReadOnly);
			Assert(!branchLevelCollection.ReadOnly);
			Assert(!orgLevelCollection.ReadOnly);

			Env.Security.CompaniesModifyCurrencyCFXUplift.IsAllowed = false;

			Assert(companyLevelCollection.ReadOnly);
			Assert(!branchLevelCollection.ReadOnly);
			Assert(!orgLevelCollection.ReadOnly);

			Env.Security.CompaniesModifyCurrencyCFXUplift.IsAllowed = true;
			Env.Security.BranchModifyCurrencyCFXUplift.IsAllowed = false;

			Assert(!companyLevelCollection.ReadOnly);
			Assert(branchLevelCollection.ReadOnly);
			Assert(!orgLevelCollection.ReadOnly);

			Env.Security.BranchModifyCurrencyCFXUplift.IsAllowed = true;
			Env.Security.OrgReceivablesModifyCurrencyUplift.IsAllowed = false;

			Assert(!companyLevelCollection.ReadOnly);
			Assert(!branchLevelCollection.ReadOnly);
			Assert(orgLevelCollection.ReadOnly);
		}

		public void TestGetRecordAndSetUplifts()
		{
			TestCaseHelper.ClearTable(AccCFXUpliftConfigurationViewSchema.Constants.TableName);

			var fallbackRecord = CreateCFXUplift(AccCFXConfigurationLevelEnum.Company, ZGuid.Empty, "ALL", "ALL", "", "", "ALL", 1, 2);
			var fallbackRecordWithDates = CreateCFXUplift(AccCFXConfigurationLevelEnum.Company, ZGuid.Empty, "ALL", "ALL", "", "", "ALL", 1, 2, startDate: new ZDate(2025, 1, 4), expiryDate: new ZDate(2025, 1, 4));
			var companyRecords = ConfigureCFXUpliftForLevel(AccCFXConfigurationLevelEnum.Company,      ZGuid.Empty,                       "FR", "HU", "ROA", "EUR");
			var branchRecords =  ConfigureCFXUpliftForLevel(AccCFXConfigurationLevelEnum.Branch,       Env.CurrentBranchPK,               "US", "CA", "AIR", "USD");
			var orgRecords =     ConfigureCFXUpliftForLevel(AccCFXConfigurationLevelEnum.Organisation, Env.CurrentCompany.OrganisationPK, "AU", "NZ", "SEA", "AUD");

			Factory.Save();

			var collection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentCompany.OrganisationPK);
			collection.Load();
			AssertEquals(AccCFXConfigurationLevelEnum.Organisation, collection.Level);

			AssertRecordsForLevel(orgRecords,     "AU", "NZ", "SEA", "AUD");
			AssertRecordsForLevel(branchRecords,  "US", "CA", "AIR", "USD");
			AssertRecordsForLevel(companyRecords, "FR", "HU", "ROA", "EUR");
			AssertEquals(fallbackRecord, collection.GetRecord("SHP", "DOM", "RAI", "JP", "JP", "JPY").PK);
			AssertEquals(fallbackRecordWithDates, collection.GetRecord("SHP", "DOM", "RAI", "JP", "JP", "JPY", new ZDate(2025, 1, 4)).PK);
			AssertEquals(134, collection.Count);

			AssertRecordValues(10m, 11m, collection.GetRecord("SHP", "DOM", "SEA", currencyCode: "AUD"));
			collection.SetUplifts("SHP", "ALL", "SEA", "AUD", 30, 31);
			AssertEquals("No new records added", 134, collection.Count);
			AssertRecordValues(30m, 31m, collection.GetRecord("SHP", "DOM", "SEA", currencyCode: "AUD"));

			collection.SetUplifts("SHP", "DOM", "SEA", "AUD", 32, 33);
			AssertEquals("A new record added", 135, collection.Count);
			AssertRecordValues(32m, 33m, collection.GetRecord("SHP", "DOM", "SEA", currencyCode: "AUD"));

			AssertRecordValues(8m, 9m, collection.GetRecord("SHP", "DOM", "SEA"));
			collection.SetUplifts("SHP", "ALL", "SEA", 34, 35);
			AssertEquals("No new records added", 135, collection.Count);
			AssertRecordValues(34m, 35m, collection.GetRecord("SHP", "DOM", "SEA"));

			collection.SetUplifts("SHP", "DOM", "SEA", 36, 37);
			AssertEquals("A new record added", 136, collection.Count);
			AssertRecordValues(36m, 37m, collection.GetRecord("SHP", "DOM", "SEA"));

			Dictionary<string, ZGuid> ConfigureCFXUpliftForLevel(AccCFXConfigurationLevelEnum level, ZGuid parentPk, ZString origin, ZString destination, ZString transportMode, string currency)
			{
				var date = new ZDate(2025, 1, 4);
				return new Dictionary<string, ZGuid>
				{
					{ "01_Mode",                         CreateCFXUplift(level, parentPk, "ALL", "ALL", "",     "",          transportMode, 00, 01) },
					{ "02_Mode_Date",                    CreateCFXUplift(level, parentPk, "ALL", "ALL", "",     "",          transportMode, 01, 02, startDate: date, expiryDate: date) },
					{ "03_Mode_Curr",                    CreateCFXUplift(level, parentPk, "ALL", "ALL", "",     "",          transportMode, 02, 03, currency) },
					{ "04_Mode_Curr_Date",               CreateCFXUplift(level, parentPk, "ALL", "ALL", "",     "",          transportMode, 03, 04, currency, startDate: date, expiryDate: date) },
					{ "05_EXP_Mode",                     CreateCFXUplift(level, parentPk, "ALL", "EXP", "",     "",          transportMode, 04, 05) },
					{ "06_EXP_Mode_Date",                CreateCFXUplift(level, parentPk, "ALL", "EXP", "",     "",          transportMode, 05, 06, startDate: date, expiryDate: date) },
					{ "07_EXP_Mode_Curr",                CreateCFXUplift(level, parentPk, "ALL", "EXP", "",     "",          transportMode, 06, 07, currency) },
					{ "08_EXP_Mode_Curr_Date",           CreateCFXUplift(level, parentPk, "ALL", "EXP", "",     "",          transportMode, 07, 08, currency, startDate: date, expiryDate: date) },
					{ "09_SHP_Mode",                     CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     "",          transportMode, 08, 09) },
					{ "10_SHP_Mode_Date",                CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     "",          transportMode, 09, 10, startDate: date, expiryDate: date) },
					{ "11_SHP_Mode_Curr",                CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     "",          transportMode, 10, 11, currency) },
					{ "12_SHP_Mode_Curr_Date",           CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     "",          transportMode, 11, 12, currency, startDate: date, expiryDate: date) },
					{ "13_SHP_EXP_Mode",                 CreateCFXUplift(level, parentPk, "SHP", "EXP", "",     "",          transportMode, 12, 13) },
					{ "14_SHP_EXP_Mode_Date",            CreateCFXUplift(level, parentPk, "SHP", "EXP", "",     "",          transportMode, 13, 14, startDate: date, expiryDate: date) },
					{ "15_SHP_EXP_Mode_Curr",            CreateCFXUplift(level, parentPk, "SHP", "EXP", "",     "",          transportMode, 14, 15, currency) },
					{ "16_SHP_EXP_Mode_Curr_Date",       CreateCFXUplift(level, parentPk, "SHP", "EXP", "",     "",          transportMode, 15, 16, currency, startDate: date, expiryDate: date) },
					{ "17_SHP_Orig",                     CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          "ALL",         16, 17) },
					{ "18_SHP_Orig_Date",                CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          "ALL",         17, 18, startDate: date, expiryDate: date) },
					{ "19_SHP_Orig_Curr",                CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          "ALL",         18, 19, currency) },
					{ "20_SHP_Orig_Curr_Date",           CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          "ALL",         19, 20, currency, startDate: date, expiryDate: date) },
					{ "21_SHP_Orig_Mode",                CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          transportMode, 20, 21) },
					{ "22_SHP_Orig_Mode_Date",           CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          transportMode, 21, 22, startDate: date, expiryDate: date) },
					{ "23_SHP_Orig_Mode_Curr",           CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          transportMode, 22, 23, currency) },
					{ "24_SHP_Orig_Mode_Curr_Date",      CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, "",          transportMode, 23, 24, currency, startDate: date, expiryDate: date) },
					{ "25_SHP_Dest",                     CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, "ALL",         24, 25) },
					{ "26_SHP_Dest_Date",                CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, "ALL",         25, 26, startDate: date, expiryDate: date) },
					{ "27_SHP_Dest_Curr",                CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, "ALL",         26, 27, currency) },
					{ "28_SHP_Dest_Curr_Date",           CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, "ALL",         27, 28, currency, startDate: date, expiryDate: date) },
					{ "29_SHP_Dest_Mode",                CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, transportMode, 28, 29) },
					{ "30_SHP_Dest_Mode_Date",           CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, transportMode, 29, 30, startDate: date, expiryDate: date) },
					{ "31_SHP_Dest_Mode_Curr",           CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, transportMode, 30, 31, currency) },
					{ "32_SHP_Dest_Mode_Curr_Date",      CreateCFXUplift(level, parentPk, "SHP", "ALL", "",     destination, transportMode, 31, 32, currency, startDate: date, expiryDate: date) },
					{ "33_SHP_Orig_Dest",                CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, "ALL",         32, 33) },
					{ "34_SHP_Orig_Dest_Date",           CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, "ALL",         33, 34, startDate: date, expiryDate: date) },
					{ "35_SHP_Orig_Dest_Curr",           CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, "ALL",         34, 35, currency) },
					{ "36_SHP_Orig_Dest_Curr_Date",      CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, "ALL",         35, 36, currency, startDate: date, expiryDate: date) },
					{ "37_SHP_Orig_Dest_Mode",           CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, transportMode, 36, 37) },
					{ "38_SHP_Orig_Dest_Mode_Date",      CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, transportMode, 37, 38, startDate: date, expiryDate: date) },
					{ "39_SHP_Orig_Dest_Mode_Curr",      CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, transportMode, 38, 39, currency) },
					{ "40_SHP_Orig_Dest_Mode_Curr_Date", CreateCFXUplift(level, parentPk, "SHP", "ALL", origin, destination, transportMode, 39, 40, currency, startDate: date, expiryDate: date) },
					{ "41_BRK_Orig_Mode",                CreateCFXUplift(level, parentPk, "BRK", "ALL", origin, "",          transportMode, 40, 41) },
					{ "42_BRK_Orig_Mode_Date",           CreateCFXUplift(level, parentPk, "BRK", "ALL", origin, "",          transportMode, 41, 42, startDate: date, expiryDate: date) },
					{ "43_BRK_Dest_Mode",                CreateCFXUplift(level, parentPk, "BRK", "ALL", "",     destination, transportMode, 42, 43) },
					{ "44_BRK_Dest_Mode_Date",           CreateCFXUplift(level, parentPk, "BRK", "ALL", "",     destination, transportMode, 43, 44, startDate: date, expiryDate: date) }
				};
			}

			void AssertRecordsForLevel(Dictionary<string, ZGuid> records, ZString origin, ZString destination, ZString transport, ZString currency)
			{
				var date = new ZDate(2025, 1, 4);
				AssertRecord(records, "01_Mode",                         "TRN", "IMP", transport, null,     null,   null,        null);
				AssertRecord(records, "02_Mode_Date",                    "TRN", "IMP", transport, null,     null,   null,        date);
				AssertRecord(records, "03_Mode_Curr",		             "TRN", "IMP", transport, currency, null,   null,        null);
				AssertRecord(records, "04_Mode_Curr_Date",		         "TRN", "IMP", transport, currency, null,   null,        date);
				AssertRecord(records, "05_EXP_Mode",		             "TRN", "EXP", transport, null,     null,   null,        null);
				AssertRecord(records, "06_EXP_Mode_Date",		         "TRN", "EXP", transport, null,     null,   null,        date);
				AssertRecord(records, "07_EXP_Mode_Curr",                "TRN", "EXP", transport, currency, null,   null,        null);
				AssertRecord(records, "08_EXP_Mode_Curr_Date",           "TRN", "EXP", transport, currency, null,   null,        date);
				AssertRecord(records, "09_SHP_Mode",			         "SHP", "IMP", transport, null,     null,   null,        null);
				AssertRecord(records, "10_SHP_Mode_Date",			     "SHP", "IMP", transport, null,     null,   null,        date);
				AssertRecord(records, "11_SHP_Mode_Curr",	             "SHP", "IMP", transport, currency, null,   null,        null);
				AssertRecord(records, "12_SHP_Mode_Curr_Date",	         "SHP", "IMP", transport, currency, null,   null,        date);
				AssertRecord(records, "13_SHP_EXP_Mode",                 "SHP", "EXP", transport, null,     null,   null,        null);
				AssertRecord(records, "14_SHP_EXP_Mode_Date",            "SHP", "EXP", transport, null,     null,   null,        date);
				AssertRecord(records, "15_SHP_EXP_Mode_Curr",            "SHP", "EXP", transport, currency, null,   null,        null);
				AssertRecord(records, "16_SHP_EXP_Mode_Curr_Date",       "SHP", "EXP", transport, currency, null,   null,        date);
				AssertRecord(records, "17_SHP_Orig",                     "SHP", "EXP", null,      null,     origin, null,        null);
				AssertRecord(records, "18_SHP_Orig_Date",                "SHP", "EXP", null,      null,     origin, null,        date);
				AssertRecord(records, "19_SHP_Orig_Curr",                "SHP", "EXP", null,      currency, origin, null,        null);
				AssertRecord(records, "20_SHP_Orig_Curr_Date",           "SHP", "EXP", null,      currency, origin, null,        date);
				AssertRecord(records, "21_SHP_Orig_Mode",                "SHP", "EXP", transport, null,     origin, null,        null);
				AssertRecord(records, "22_SHP_Orig_Mode_Date",           "SHP", "EXP", transport, null,     origin, null,        date);
				AssertRecord(records, "23_SHP_Orig_Mode_Curr",           "SHP", "EXP", transport, currency, origin, null,        null);
				AssertRecord(records, "24_SHP_Orig_Mode_Curr_Date",      "SHP", "EXP", transport, currency, origin, null,        date);
				AssertRecord(records, "25_SHP_Dest",                     "SHP", "EXP", null,      null,     null,   destination, null);
				AssertRecord(records, "26_SHP_Dest_Date",                "SHP", "EXP", null,      null,     null,   destination, date);
				AssertRecord(records, "27_SHP_Dest_Curr",                "SHP", "EXP", null,      currency, null,   destination, null);
				AssertRecord(records, "28_SHP_Dest_Curr_Date",           "SHP", "EXP", null,      currency, null,   destination, date);
				AssertRecord(records, "29_SHP_Dest_Mode",                "SHP", "EXP", transport, null,     null,   destination, null);
				AssertRecord(records, "30_SHP_Dest_Mode_Date",           "SHP", "EXP", transport, null,     null,   destination, date);
				AssertRecord(records, "31_SHP_Dest_Mode_Curr",           "SHP", "EXP", transport, currency, null,   destination, null);
				AssertRecord(records, "32_SHP_Dest_Mode_Curr_Date",      "SHP", "EXP", transport, currency, null,   destination, date);
				AssertRecord(records, "33_SHP_Orig_Dest",                "SHP", "EXP", null,      null,     origin, destination, null);
				AssertRecord(records, "34_SHP_Orig_Dest_Date",           "SHP", "EXP", null,      null,     origin, destination, date);
				AssertRecord(records, "35_SHP_Orig_Dest_Curr",           "SHP", "EXP", null,      currency, origin, destination, null);
				AssertRecord(records, "36_SHP_Orig_Dest_Curr_Date",      "SHP", "EXP", null,      currency, origin, destination, date);
				AssertRecord(records, "37_SHP_Orig_Dest_Mode",           "SHP", "EXP", transport, null,     origin, destination, null);
				AssertRecord(records, "38_SHP_Orig_Dest_Mode_Date",      "SHP", "EXP", transport, null,     origin, destination, date);
				AssertRecord(records, "39_SHP_Orig_Dest_Mode_Curr",      "SHP", "EXP", transport, currency, origin, destination, null);
				AssertRecord(records, "40_SHP_Orig_Dest_Mode_Curr_Date", "SHP", "EXP", transport, currency, origin, destination, date);
				AssertRecord(records, "41_BRK_Orig_Mode",                "BRK", "EXP", transport, null,     origin, null,        null);
				AssertRecord(records, "42_BRK_Orig_Mode_Date",           "BRK", "EXP", transport, null,     origin, null,        date);
				AssertRecord(records, "41_BRK_Orig_Mode",                "BRK", "EXP", transport, null,     origin, destination, null);
				AssertRecord(records, "42_BRK_Orig_Mode_Date",           "BRK", "EXP", transport, null,     origin, destination, date);
				AssertRecord(records, "43_BRK_Dest_Mode",                "BRK", "EXP", transport, null,     null,   destination, null);
				AssertRecord(records, "44_BRK_Dest_Mode_Date",           "BRK", "EXP", transport, null,     null,   destination, date);
			}

			void AssertRecord(Dictionary<string, ZGuid> records, string expectedRecordKey, string jobType, string serviceDirection, string transportMode, string currency, string origin, string destination, ZDate? date)
			{
				AssertEquals(expectedRecordKey
					, records[expectedRecordKey]
					, collection.GetRecord(jobType, serviceDirection, transportMode, origin, destination, currency, date).PK);
			}

			void AssertRecordValues(ZDecimal expectedPercent, ZDecimal expectedMin, AccCFXUpliftConfiguration record)
			{
				AssertEquals(expectedPercent, record.JCF_CFXPercentage);
				AssertEquals(expectedMin, record.JCF_CFXMinimum);
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccCFXUpliftConfigurationCollection(Factory, ZGuid.NewZGuid());
		}

		(ZGuid cmpLevelPk, ZGuid brnLevelPk, ZGuid orgLevelPk) PrepTestData()
		{
			TestCaseHelper.ClearTable(AccCFXUpliftConfigurationViewSchema.Constants.TableName);
			var cmpLevelPk = CreateCFXUplift(AccCFXConfigurationLevelEnum.Company, ZGuid.Empty, 5, 2);
			var brnLevelPk = CreateCFXUplift(AccCFXConfigurationLevelEnum.Branch, Env.CurrentBranchPK, 10, 3);
			var orgLevelPk = CreateCFXUplift(AccCFXConfigurationLevelEnum.Organisation, Env.CurrentCompany.OrganisationPK, 15, 4);

			Factory.Save();

			return (cmpLevelPk, brnLevelPk, orgLevelPk);
		}

		ZGuid CreateCFXUplift(AccCFXConfigurationLevelEnum level, ZGuid parentPk, ZDecimal percentage, ZDecimal minimum, string currencyCode = "")
		{
			return CreateCFXUplift(level, parentPk, "ALL", "IMP", "", "", "SEA", percentage, minimum, currencyCode);
		}

		ZGuid CreateCFXUplift(AccCFXConfigurationLevelEnum level, ZGuid parentPk, ZString jobType, ZString serviceDirection, ZString origin, ZString destination, ZString transportMode, ZDecimal percentage, ZDecimal minimum, string currencyCode = "", ZDate? startDate = null, ZDate? expiryDate = null)
		{
			var cfxUpliftConfig = Factory.New<AccCFXUpliftConfiguration>();
			cfxUpliftConfig.JCF_GC = Env.CurrentCompanyPK;
			cfxUpliftConfig.JCF_JobType = jobType;
			cfxUpliftConfig.JCF_ServiceDirection = serviceDirection;
			cfxUpliftConfig.JCF_TransportMode = transportMode;
			cfxUpliftConfig.JCF_ParentTableCode = level.ToTablePrefix();
			cfxUpliftConfig.JCF_ParentID = parentPk;
			cfxUpliftConfig.JCF_CFXPercentage = percentage;
			cfxUpliftConfig.JCF_CFXMinimum = minimum;
			cfxUpliftConfig.JCF_RX_NKCurrency = currencyCode;
			cfxUpliftConfig.JCF_RN_NKOriginCountry = origin;
			cfxUpliftConfig.JCF_RN_NKDestinationCountry = destination;
			cfxUpliftConfig.JCF_StartDate = startDate ?? ZDate.Empty;
			cfxUpliftConfig.JCF_ExpiryDate = expiryDate ?? ZDate.Empty;

			return cfxUpliftConfig.PK;
		}

		void TestAddNew(AccCFXUpliftConfigurationCollection col, ZGuid parentPk)
		{
			var bizo = col.AddNew();
			AssertEquals(Env.CurrentCompanyPK, bizo.JCF_GC);
			AssertEquals(col.Level, bizo.Level);
			AssertEquals(parentPk, bizo.JCF_ParentID);
		}
	}
}
