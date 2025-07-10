using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Business.ServiceTasks.Test
{
	[TestedType(typeof(SailingScheduleRecyclingServiceTask))]
	sealed class SailingScheduleRecyclingServiceTaskTest : ServiceTaskTestCase<SailingScheduleRecyclingServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "SSR", hostedServiceAttribute.Code);
				AssertEquals("Description", "Sailing Schedule Recycling", hostedServiceAttribute.Description);
				AssertEquals("Category", "FRT", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1day", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestOverflowForSmallDatetime()
		{
			using (FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "6"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "TITANIC";

				var voyage1 = Factory.New<JobVoyage>();
				voyage1.JV_AirSeaRoad = "SEA";
				voyage1.JV_RV_NKVessel = vessel.RV_FK;
				voyage1.JV_VoyageFlight = "123";
				voyage1.JV_IsActive = true;
				voyage1.Origins.AddNew().JA_A_DEP = new ZDateTime(2079, 6, 6);

				Factory.Save();

				var task = new SailingScheduleRecyclingServiceTask();
				InitialiseTaskSchedule(task);
				AssertNoExceptionThrown(() => RunTaskSchedule(task));
			}
		}

		public void TestLogging()
		{
			using (FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "6"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "TITANIC";

				var voyage1 = Factory.New<JobVoyage>();
				voyage1.JV_AirSeaRoad = "SEA";
				voyage1.JV_RV_NKVessel = vessel.RV_FK;
				voyage1.JV_VoyageFlight = "123";
				voyage1.JV_IsActive = true;
				voyage1.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-9);

				var voyage2 = Factory.New<JobVoyage>();
				voyage2.JV_AirSeaRoad = "SEA";
				voyage2.JV_RV_NKVessel = vessel.RV_FK;
				voyage2.JV_VoyageFlight = "456";
				voyage2.JV_IsActive = false;
				voyage2.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-9);

				Factory.Save();

				var task = new SailingScheduleRecyclingServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				var expectedLog = string.Format(@"Information|Archived Sailing: {0}
TITANIC - 123
", voyage1.PK);
				AssertEquals(expectedLog, task.ServiceLogger.ToString());
			}
		}

		public void TestOnlyLatestDateMatters()
		{
			using (FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "6"))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_IsActive = true;
				voyage.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-7);
				voyage.Destinations.AddNew().JB_A_ARV = ZDateTime.Now.AddMonths(-5);

				Factory.Save();

				var task = new SailingScheduleRecyclingServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert("Voyage is from 7 months ago to 5 months ago, should not be archived.", voyage.JV_IsActive);

				voyage.Origins[0].JA_A_DEP = ZDateTime.Now.AddMonths(-9);
				voyage.Destinations[0].JB_A_ARV = ZDateTime.Now.AddMonths(-7);

				voyage.Factory.Save();

				RunTaskSchedule(task);

				newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert(string.Format("Voyage is from 9 months ago to 7 months ago, should be archived.\r\n{0}", GetDatesInfo(voyage)), !voyage.JV_IsActive);
			}
		}

		public void TestNoRecycling()
		{
			using (FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, VoyageRecyclingPeriodList.Codes.None))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_IsActive = true;
				voyage.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-6);

				Factory.Save();

				var task = new SailingScheduleRecyclingServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert("Recyling period is NON, voyage should not be archived.", voyage.JV_IsActive);
			}
		}

		public void TestOnlySeaVoyagesChanged()
		{
			using (FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "6"))
			{
				var voyage1 = Factory.New<JobVoyage>();
				voyage1.JV_AirSeaRoad = "SEA";
				voyage1.JV_IsActive = true;
				voyage1.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-9);

				var voyage2 = Factory.New<JobVoyage>();
				voyage2.JV_AirSeaRoad = "AIR";
				voyage2.JV_IsActive = true;
				voyage2.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-9);

				Factory.Save();

				var task = new SailingScheduleRecyclingServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory();
				voyage1 = newFactory.Load<JobVoyage>(voyage1.PK);
				voyage2 = newFactory.Load<JobVoyage>(voyage2.PK);

				Assert("Sea Voyage should be archived", !voyage1.JV_IsActive);
				Assert("Air Voyage should not be archived", voyage2.JV_IsActive);
			}
		}

		public void TestRecyclingPeriodFallback()
		{
			using (FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "6"))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_IsActive = true;
				voyage.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-9);

				var line = Factory.NewWithValidTestData<OrgHeader>();
				line.MiscServ.OM_CRVoyageRecyclingPeriodInMonths = 12;

				voyage.JV_OH_Line = line.PK;

				Factory.Save();

				var task = new SailingScheduleRecyclingServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert("MiscServ takes priority and job is not eligble for archive.", voyage.JV_IsActive);

				line.MiscServ.OM_CRVoyageRecyclingPeriodInMonths = 3;

				Factory.Save();

				RunTaskSchedule(task);

				newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert(string.Format("MiscServ takes priority and job is eligble for archive.\r\n{0}", GetDatesInfo(voyage)), !voyage.JV_IsActive);

				line.MiscServ.OM_CRVoyageRecyclingPeriodInMonths = VoyageRecyclingPeriodList.Default;

				Factory.Save();

				voyage.JV_IsActive = true;
				voyage.Origins[0].JA_A_DEP = ZDateTime.Now.AddMonths(-3);
				voyage.Factory.Save();

				RunTaskSchedule(task);

				newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert("MiscServ is default so fallback to registry and job is not eligble for archive.", voyage.JV_IsActive);

				voyage.Origins[0].JA_A_DEP = ZDateTime.Now.AddMonths(-9);
				voyage.Factory.Save();

				RunTaskSchedule(task);

				newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert("MiscServ is default so fallback to registry and job is eligble for archive.", !voyage.JV_IsActive);
			}
		}

		public void TestDateMatching()
		{
			using (FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "6"))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_IsActive = true;
				voyage.Origins.AddNew().JA_A_DEP = ZDateTime.Now.AddMonths(-6).AddDays(5);

				Factory.Save();

				var task = new SailingScheduleRecyclingServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert("One day short of recycling period. Should not be archived.", voyage.JV_IsActive);

				voyage.Origins[0].JA_A_DEP = ZDateTime.Now.AddMonths(-6).AddDays(-5);
				voyage.Factory.Save();

				RunTaskSchedule(task);

				newFactory = new BusinessObjectFactory();
				voyage = newFactory.Load<JobVoyage>(voyage.PK);

				Assert(string.Format("One day over recycling period. Should be archived.\r\n{0}", GetDatesInfo(voyage)), !voyage.JV_IsActive);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		string GetDatesInfo(JobVoyage voyage)
		{
			using (var command = Db.Connection.Command("SELECT GETDATE()"))
			{
				return string.Format(@"Debug information:
JA_A_DEP: {0}
JA_E_DEP: {1}
JB_A_ARV: {2}
JB_E_ARV: {3}
GETDATE(): {4}"
					, voyage.Origins.Any() ? voyage.Origins[0].JA_A_DEP.ToString() : string.Empty
					, voyage.Origins.Any() ? voyage.Origins[0].JA_E_DEP.ToString() : string.Empty
					, voyage.Destinations.Any() ? voyage.Destinations[0].JB_A_ARV.ToString() : string.Empty
					, voyage.Destinations.Any() ? voyage.Destinations[0].JB_E_ARV.ToString() : string.Empty
					, command.ExecuteScalar());
			}
		}
	}
}
