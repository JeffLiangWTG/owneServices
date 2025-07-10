using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccSurchargeConfigurationCollection))]
	sealed class AccSurchargeConfigurationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccSurchargeConfigurationCollection(Factory, ZGuid.NewZGuid());
		}

		public void TestLogForAccSurchargeConfiguration()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();

			var accSurchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			accSurchargeConfiguration.ASC_GC_Company = company.PK;
			accSurchargeConfiguration.ASC_Code = "T01";

			company.AccSurchargeConfigurations.Add(accSurchargeConfiguration);
			Factory.Save();

			var expectLogForAdd = "Surcharge Added: Code: T01, Surcharge Type: PER, Percentage: 0%, Base Type: ALL.";
			var logs = company.Logs.GetAllLogs();
			AssertEquals("Percondition 1", true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForAdd));

			accSurchargeConfiguration.ASC_Code = "T02";
			Factory.Save();

			var expectLogForModified1 = "Surcharge Deleted: Code: T01, Surcharge Type: PER, Percentage: 0%, Base Type: ALL.";
			var expectLogForModified2 = "Surcharge Added: Code: T02, Surcharge Type: PER, Percentage: 0%, Base Type: ALL.";
			logs = company.Logs.GetAllLogs();
			AssertEquals("Percondition 2", true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForModified1));
			AssertEquals("Percondition 3", true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForModified2));

			company.AccSurchargeConfigurations.RemoveAndDeleteAll();
			Factory.Save();

			var expectLogForDelete = "Surcharge Deleted: Code: T02, Surcharge Type: PER, Percentage: 0%, Base Type: ALL.";
			logs = company.Logs.GetAllLogs();
			AssertEquals("Percondition 4", true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForDelete));
		}

		public void TestSetDefaultsForNewChild()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();

			var surchargeConfig = company.AccSurchargeConfigurations.AddNew();

			AssertEquals("ASC_GC_Company is set to surchargeConfig.PK", surchargeConfig.ASC_GC_Company, company.PK);
			AssertEquals("ASC_Type is set to PER", surchargeConfig.ASC_Type, "PER");
		}

		public void TestForApplicableOnly()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();

			var accSurchargeConfiguration1 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			accSurchargeConfiguration1.ASC_GC_Company = company.PK;
			accSurchargeConfiguration1.ASC_Code = "T01";

			var accSurchargeConfiguration2 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			accSurchargeConfiguration2.ASC_GC_Company = company.PK;
			accSurchargeConfiguration2.ASC_Code = "T02";

			var accSurchargeConfiguration3 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			accSurchargeConfiguration3.ASC_GC_Company = company.PK;
			accSurchargeConfiguration3.ASC_Code = "T03";

			company.AccSurchargeConfigurations.Add(accSurchargeConfiguration1);
			company.AccSurchargeConfigurations.Add(accSurchargeConfiguration2);
			company.AccSurchargeConfigurations.Add(accSurchargeConfiguration3);
			Factory.Save();

			var collection1 = new AccSurchargeConfigurationCollection(Factory, company.PK);
			collection1.Load();
			Assert(collection1.AllowNew);
			Assert(collection1.AllowRemove);
			AssertEquals(3, collection1.Count);
			AssertEquals("NON", collection1.ApplicableSurchargesAsString);

			collection1[0].IsApplicable = true;
			AssertEquals("T01", collection1.ApplicableSurchargesAsString);

			collection1[1].IsApplicable = true;
			AssertEquals("T01, T02", collection1.ApplicableSurchargesAsString);

			collection1[2].IsApplicable = true;
			AssertEquals("ALL", collection1.ApplicableSurchargesAsString);

			var collection2 = new AccSurchargeConfigurationCollection(Factory, company.PK, true);
			collection2.Load();
			Assert(!collection2.AllowNew);
			Assert(!collection2.AllowRemove);
			AssertEquals(3, collection2.Count);

			foreach (AccSurchargeConfiguration configuration in collection2)
			{
				configuration.IsApplicable = false;
			}

			AssertEquals("NON", collection2.ApplicableSurchargesAsString);

			collection2[0].IsApplicable = true;
			AssertEquals("T01", collection2.ApplicableSurchargesAsString);

			collection2[1].IsApplicable = true;
			AssertEquals("T01, T02", collection2.ApplicableSurchargesAsString);

			collection2[2].IsApplicable = true;
			AssertEquals("ALL", collection2.ApplicableSurchargesAsString);
		}
	}
}
