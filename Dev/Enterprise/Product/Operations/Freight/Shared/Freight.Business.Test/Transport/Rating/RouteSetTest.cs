using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RouteSetTest : TestCaseWithFactory
	{
		public void TestVia_NoOriginSet()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "";
			transport1.JW_RL_NKDiscPort = "AUMEL";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			var legs = new Transport[] { transport1, transport2 };

			var routeSet = new RouteSet(Factory, 1, legs);

			AssertNoExceptionThrown("should be able to handle empty load ports", () => { var getVia = routeSet.Via; });
		}

		public void TestLocationsMakesNoExtraFactories()
		{
			int GetCountOfFactoriesCreated(Action action)
			{
				var factoriesBefore = BusinessObjectFactory._NextInstance;
				action();
				var factoriesAfter = BusinessObjectFactory._NextInstance;
				return (int)(factoriesAfter - factoriesBefore);
			}

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var legs = new Transport[] { transport };
			var routeSet = new RouteSet(Factory, 1, legs);

			var createdFactories = GetCountOfFactoriesCreated(() =>
			{
				var origin = routeSet.Origin;
				var destination = routeSet.Destination;

				AssertEquals("AUMEL", origin.UNLOCO.Code);
				AssertEquals("NZAKL", destination.UNLOCO.Code);
			});

			AssertEquals("there should be no factories created when getting the origin or destination", 0, createdFactories);
		}

		public void TestVia_NoDestinationSet()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "";

			var legs = new Transport[] { transport1, transport2 };

			var routeSet = new RouteSet(Factory, 1, legs);

			AssertNoExceptionThrown("should be able to handle empty discharge ports", () => { var getVia = routeSet.Via; });
		}

		public void TestRouteSetsIsCleared_WhenTransportLegsAreDeleted()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "AUMEL";

			AssertEquals(1, ((IRoutingSupport)consol).TransportsIncludingRelated.RouteSets.Count);

			consol.Transports.RemoveAndDeleteAll();

			AssertEquals(0, ((IRoutingSupport)consol).TransportsIncludingRelated.RouteSets.Count);
		}

		public void TestRecursiveRecalculateRouteSets()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = shipment.Consols.AddNew();
			var transports = consol.Transports;

			var transport1 = transports[0];
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "NZAHU";
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			Factory.Save();

			var shipmentInOtherFactory = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			var consolInOtherFactory = new BusinessObjectFactory().Load<CommonConsol>(consol.PK);
			AssertEquals(1, shipmentInOtherFactory.TransportsIncludingRelated.RouteSets.Count);
			AssertEquals(1, ((IRoutingSupport)consolInOtherFactory).Transports.Count);
			AssertEquals("add SubscribeToChildrenChanges handler by accessing RouteSet collection", 1, ((IRoutingSupport)consolInOtherFactory).TransportsIncludingRelated.RouteSets.Count);
			AssertNoExceptionThrown("this runs fine", () => shipmentInOtherFactory.TransportsIncludingRelated.RouteSets.GetRouteSetNumberForTransport(shipment.TransportsIncludingRelated[0]));

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Random Vesel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAHU";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_DepartReference = "AOEUI1234";
			sailing.Destination.JB_ArrivalReference = "QWERTY123";

			var transport2 = transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NZAHU";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;

			Factory.Save();
			AssertNoExceptionThrown("this fails", () => shipmentInOtherFactory.TransportsIncludingRelated.RouteSets.GetRouteSetNumberForTransport(shipment.TransportsIncludingRelated[1]));
		}
	}
}
