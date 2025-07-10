using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobVoyage.Loader))]
	sealed class LoaderTest : LoaderTestCase
	{
		public void TestSea_CarrierIsUsedForMatching()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();

			var helper = new VoyageTestHelper(Factory);

			var voyage1 = helper.CreateSeaVoyage("Visund", "123", carrier1.PK);
			voyage1.JV_VoyageType = Constants.VoyageType.SlotVoyage;

			var voyage2 = helper.CreateSeaVoyage("Visund", "123", carrier2.PK);
			voyage2.JV_VoyageType = Constants.VoyageType.MainVoyage;

			var voyage3 = helper.CreateSeaVoyage("Visund", "123", ZGuid.Empty);
			voyage3.JV_VoyageType = Constants.VoyageType.SlotVoyage;

			var voyage4 = helper.CreateSeaVoyage("Visund", "123", ZGuid.Empty);
			voyage4.JV_AirSeaRoad = Constants.TransportModes.Air;

			var voyage5 = helper.CreateSeaVoyage("Asgard", "123", ZGuid.Empty);

			var loader = new JobVoyage.Loader(Factory);
			AssertEquals(voyage1, loader.Load(Constants.TransportModes.Sea, "Visund", "123", carrier1.PK));
			AssertEquals(voyage2, loader.Load(Constants.TransportModes.Sea, "Visund", "123", carrier2.PK));

			AssertEquals("Main voyage first when no carrier is specified", voyage2, loader.Load(Constants.TransportModes.Sea, "Visund", "123"));
			AssertEquals("Main voyage first when no carrier is specified", voyage2, loader.Load(Constants.TransportModes.Sea, "Visund", "123", ZGuid.Empty));
		}

		public void TestIsCharteredIsIncludedInQuery()
		{
			var helper = new VoyageTestHelper(Factory);

			var voyage1 = helper.CreateSeaVoyage("NEWVESSEL", "999", ZGuid.Empty);
			voyage1.JV_IsChartered = false;

			var voyage2 = helper.CreateSeaVoyage("NEWVESSEL", "999", ZGuid.Empty);
			voyage2.JV_IsChartered = true;

			AssertEquals(voyage1.PK, new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Sea, "NEWVESSEL", "999").PK);
		}

		public void TestIsCharteredNotIncludedInQueryForSea()
		{
			var helper = new VoyageTestHelper(Factory);

			var voyage = helper.CreateSeaVoyage("NEWVESSEL", "999", ZGuid.Empty);
			voyage.JV_IsChartered = true;
			AssertEquals(voyage.PK, new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Sea, "NEWVESSEL", "999", ZGuid.Empty, ZDateTime.Empty, false).PK);

			voyage.JV_IsChartered = false;
			AssertEquals(voyage.PK, new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Sea, "NEWVESSEL", "999", ZGuid.Empty, ZDateTime.Empty, true).PK);
		}

		public void TestAir_FlightDate()
		{
			ZDateTime now = ZDateTime.Now;

			JobVoyage decoyVoyage = Factory.New<JobVoyage>();
			decoyVoyage.JV_IsChartered = false;
			decoyVoyage.JV_VoyageFlight = "999";
			decoyVoyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			decoyVoyage.JV_FlightDate = now.AddDays(1);

			JobVoyage newVoyage = Factory.New<JobVoyage>();
			newVoyage.JV_IsChartered = false;
			newVoyage.JV_VoyageFlight = "999";
			newVoyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			newVoyage.JV_FlightDate = now;

			AssertNull(new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Air, "", "999", ZDateTime.Empty));
			AssertNull(new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Air, "", "999", ZDateTime.Invalid));

			JobVoyage loadedVoyage = new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Air, "", "999", now.AddHours(1));
			AssertNotNull(loadedVoyage);
			AssertEquals(newVoyage.PK, loadedVoyage.PK);
		}

		public void TestLoaderReturnsNullWithEmptyParameters()
		{
			JobVoyage loadedVoyage = new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Sea, "", "");
			AssertNull(loadedVoyage);
		}

		public void TestSyncDepartReferenceAndArrivalReference()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF001";

				var origin1 = voyage.Origins.AddNew();
				origin1.JA_RL_NKPortOfLoading = "AUSYD";
				origin1.JA_E_DEP = 5.DaysAgo();
				origin1.JA_DepartReference = "D1";

				var destination1 = voyage.Destinations.AddNew();
				destination1.JB_RL_NKPortOfDischarge = "SGSIN";
				destination1.JB_E_ARV = 1.DaysAgo();
				destination1.JB_ArrivalReference = "A1";

				var origin2 = voyage.Origins.AddNew();
				origin2.JA_RL_NKPortOfLoading = "SGSIN";
				origin2.JA_E_DEP = 3.DaysAgo();
				origin2.JA_DepartReference = "D2";

				var destination2 = voyage.Destinations.AddNew();
				destination2.JB_RL_NKPortOfDischarge = "HKHKG";
				destination2.JB_E_ARV = 1.DaysAgo();
				destination2.JB_ArrivalReference = "A2";

				voyage.GenerateSailings();

				AssertEquals("Precondition", 3, voyage.Sailings.Count);

				Factory.Save();

				var sailing1 = voyage.Sailings[0];
				var sailing2 = voyage.Sailings[1];
				var sailing3 = voyage.Sailings[2];

				AssertEquals("D1", sailing1.JX_DeparturePortRouteId);
				AssertEquals("A1", sailing1.JX_ArrivalPortRouteId);
				AssertEquals("D2", sailing2.JX_DeparturePortRouteId);
				AssertEquals("A2", sailing2.JX_ArrivalPortRouteId);
				AssertEquals("D1", sailing3.JX_DeparturePortRouteId);
				AssertEquals("A2", sailing3.JX_ArrivalPortRouteId);

				origin1.JA_DepartReference = "DD1";
				destination1.JB_ArrivalReference = "AA1";
				AssertEquals("DD1", sailing1.JX_DeparturePortRouteId);
				AssertEquals("AA1", sailing1.JX_ArrivalPortRouteId);
				AssertEquals("D2", sailing2.JX_DeparturePortRouteId);
				AssertEquals("A2", sailing2.JX_ArrivalPortRouteId);
				AssertEquals("DD1", sailing3.JX_DeparturePortRouteId);
				AssertEquals("A2", sailing3.JX_ArrivalPortRouteId);
			}
		}

		#region Implementation

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new JobVoyage.Loader(Factory);
		}

		#endregion
	}
}
