using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	sealed class WorkflowRoutingSupportHelperTest : TestCaseWithFactory
	{
		public void TestIsTransportLinkedEvent()
		{
			AssertEquals(true, WorkflowRoutingSupportHelper.IsTransportLinkedEvent(""));
			AssertEquals(true, WorkflowRoutingSupportHelper.IsTransportLinkedEvent(Events.Departure.Code));
			AssertEquals(true, WorkflowRoutingSupportHelper.IsTransportLinkedEvent(Events.Arrival.Code));
			AssertEquals(true, WorkflowRoutingSupportHelper.IsTransportLinkedEvent(Events.GateOut.Code));
			AssertEquals(true, WorkflowRoutingSupportHelper.IsTransportLinkedEvent(Events.GateIn.Code));
			AssertEquals(false, WorkflowRoutingSupportHelper.IsTransportLinkedEvent(Events.Authorised.Code));
			AssertEquals(false, WorkflowRoutingSupportHelper.IsTransportLinkedEvent(Events.AddedARecordToTheSystem.Code));
		}

		[ExpectNoExceptions]
		public void TestAddTransportSubQuery()
		{
			ZDBOnlySubQuery query = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_ParentID);
			WorkflowRoutingSupportHelper.AddTransportSubQuery(query, "AUSYD", "USLAX");
		}

		public void TestRunQuery()
		{
			JobVoyage voyage = CreateVoyage();
			JobSailing sailing1 = GetOrCreateSailing(voyage, "AUSYD", "NZAKL");
			JobSailing sailing2 = GetOrCreateSailing(voyage, "AUMEL", "NZAKL");
			JobSailing sailing3 = GetOrCreateSailing(voyage, "UAIEV", "AUSYD");

			Factory.Save();

			CommonConsol[] bOs1 = CreateBusinessObjects(sailing1);
			CommonConsol[] bOs2 = CreateBusinessObjects(sailing2);
			CommonConsol[] bOs3 = CreateBusinessObjects(sailing3);

			Factory.Save();

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CommonConsol));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_ParentID);
			WorkflowRoutingSupportHelper.AddTransportSubQuery(subQuery, "AUSYD", "NZAKL");
			query.AddSubQuery(subQuery, JoinCondition.And);
			CommonConsol[] found = Factory.Load<CommonConsol>(query);

			AssertCollectionContains(bOs1[0], found);
			AssertCollectionContains(bOs1[1], found);
			AssertCollectionNotContains(bOs2[0], found);
			AssertCollectionNotContains(bOs2[1], found);
			AssertCollectionNotContains(bOs3[0], found);
			AssertCollectionNotContains(bOs3[1], found);

			query = new ZDBOnlyQuery(typeof(CommonConsol));
			subQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_ParentID);
			WorkflowRoutingSupportHelper.AddTransportSubQuery(subQuery, "", "NZAKL");
			query.AddSubQuery(subQuery, JoinCondition.And);
			found = Factory.Load<CommonConsol>(query);

			AssertCollectionContains(bOs1[0], found);
			AssertCollectionContains(bOs1[1], found);
			AssertCollectionContains(bOs2[0], found);
			AssertCollectionContains(bOs2[1], found);
			AssertCollectionNotContains(bOs3[0], found);
			AssertCollectionNotContains(bOs3[1], found);
		}

		CommonConsol[] CreateBusinessObjects(JobSailing sailing)
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol1.Transports[0].JW_JX = sailing.PK;
			consol1.Transports[0].JW_IsLinked = true;
			ProcessTask task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = consol1.PK;
			task1.P9_ParentTableCode = consol1.TablePrefix;
			task1.P9_ReferencedID = consol1.Transports[0].PK;
			task1.P9_ReferencedTableCode = consol1.Transports[0].TablePrefix;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol2.Transports[0].JW_IsLinked = false;
			consol2.Transports[0].JW_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			consol2.Transports[0].JW_RL_NKDiscPort = sailing.JX_JB_RL_NKPortOfDischarge;
			ProcessTask task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = consol2.PK;
			task2.P9_ParentTableCode = consol2.TablePrefix;
			task2.P9_ReferencedID = consol2.Transports[0].PK;
			task2.P9_ReferencedTableCode = consol2.Transports[0].TablePrefix;

			return new CommonConsol[] { consol1, consol2 };
		}

		JobVoyage CreateVoyage()
		{
			var vessel = RefVessel.LookupVesselByName("APL EMERALD", Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_LloydsNumber = "8610033";
				vessel.RV_Name = "APL EMERALD";
			}

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "2345";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			return voyage;
		}

		JobSailing GetOrCreateSailing(JobVoyage voyage, ZString load, ZString disc)
		{
			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(load, disc);

			if (sailing == null)
			{
				VoyageOrigin origin = voyage.Origins.GetOriginFromLoading(load);
				if (origin == null)
				{
					origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = load;
				}

				VoyageDestination destination = voyage.Destinations.GetDestinationFromDischarge(disc);
				if (destination == null)
				{
					destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = disc;
				}

				sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(load, disc);
			}

			return sailing;
		}
	}
}
