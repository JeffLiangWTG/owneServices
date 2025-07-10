using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Other.Surcharge
{
	[TestedType(typeof(AccSurchargeApplicationCollection))]
	sealed class AccSurchargeApplicationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccSurchargeApplicationCollection(Factory, ZGuid.NewZGuid());
		}

		public void TestSetDefaultsForNewChild()
		{
			var surchargeApplicationCollection = new AccSurchargeApplicationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var surchargeApplication = surchargeApplicationCollection.AddNew();
			AssertEquals(GlbCompany.CurrentCompany.PK, surchargeApplication.ASP_GC_Company);
		}

		public void TestLogForAccSurchargeApplication()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();

			var accSurchargeApplication = Factory.NewWithValidTestData<AccSurchargeApplication>();
			accSurchargeApplication.ASP_GC_Company = company.PK;
			accSurchargeApplication.ASP_JobType = "SHP";

			company.AccSurchargeApplications.Add(accSurchargeApplication);
			Factory.Save();

			var expectLogForAdd = "Surcharge Application Added: Job Type:SHP, Supply Type:, Organization Country or Zone:, Organization Category:ALL, Surcharge Code:";
			var logs = company.Logs.GetAllLogs();
			Assert("Log for add Surcharge Application", logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForAdd));

			accSurchargeApplication.ASP_JobType = "ALL";
			Factory.Save();

			var expectLogForModified1 = "Surcharge Application Deleted: Job Type:SHP, Supply Type:, Organization Country or Zone:, Organization Category:ALL, Surcharge Code:";
			var expectLogForModified2 = "Surcharge Application Added: Job Type:ALL, Supply Type:, Organization Country or Zone:, Organization Category:ALL, Surcharge Code:";
			logs = company.Logs.GetAllLogs();
			AssertEquals("Log for modify Surcharge Application(deleted log)", true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForModified1));
			AssertEquals("Log for modify Surcharge Application(added log)", true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForModified2));

			company.AccSurchargeApplications.RemoveAndDeleteAll();
			Factory.Save();

			var expectLogForDelete = "Surcharge Application Deleted: Job Type:ALL, Supply Type:, Organization Country or Zone:, Organization Category:ALL, Surcharge Code:";
			logs = company.Logs.GetAllLogs();
			AssertEquals("Log for delete Surcharge Application", true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == expectLogForDelete));
		}
	}
}
