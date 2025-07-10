using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(OnlineSchedules))]
	public class OnlineSchedulesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadRoutes()
		{
			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = "2016-08-09"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			#region Create Route

			var carrier = new ServiceModel.Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var loadPort = new ServiceModel.Port { Unloco = "AUSYD" };
			var dischargePort = new ServiceModel.Port { Unloco = "AUBNE" };

			var voyage = new ServiceModel.Voyage
			{
				Code = "754N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = "Santa Maria", ImoNumber = "1234567" }
			};

			var leg = new ServiceModel.Leg
			{
				LoadPort = loadPort,
				DischargePort = dischargePort,
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage
			};

			var serviceRoute = new ServiceModel.Route { Carrier = carrier, Legs = new[] { leg } };

			#endregion

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			routesProvider.Setup(r => r.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object) { Request = filterRequest };
			onlineSchedules.LoadRoutes(new NotificationBuffer());

			AssertEquals(1, onlineSchedules.Routes.Count);
			AssertEquals("AUSYD", onlineSchedules.Routes[0].OriginPortUnloco);
			AssertEquals("AUBNE", onlineSchedules.Routes[0].DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), onlineSchedules.Routes[0].Departure);
			AssertEquals(new DateTime(2016, 8, 15), onlineSchedules.Routes[0].Arrival);

			AssertHasError(onlineSchedules.Routes[0].CarrierSCACInfo, "No organization found for SCAC CMAC.");
			AssertHasError(onlineSchedules.Routes[0].Legs[0].CarrierSCACInfo, "No organization found for SCAC CMAC.");
		}

		public void TestResortAfterLoadRoutes()
		{
			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = "2016-08-09"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			#region Create Route

			var loadPort = new ServiceModel.Port { Unloco = "AUSYD" };
			var dischargePort = new ServiceModel.Port { Unloco = "AUBNE" };

			var carrier1 = new ServiceModel.Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var voyage1 = new ServiceModel.Voyage
			{
				Code = "754N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = "Santa Maria", ImoNumber = "1234567" }
			};

			var leg1 = new ServiceModel.Leg
			{
				LoadPort = loadPort,
				DischargePort = dischargePort,
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage1
			};

			var serviceRoute1 = new ServiceModel.Route { Carrier = carrier1, Legs = new[] { leg1 } };

			var carrier2 = new ServiceModel.Carrier
			{
				Code = "HPAG",
				Name = "Hapag Lloyd"
			};

			var voyage2 = new ServiceModel.Voyage
			{
				Code = "758N",
				TradeLane = new ServiceModel.TradeLane { Name = "BBB" },
				Operator = new ServiceModel.Carrier { Code = "HPAG", Name = "Hapag Lloyd" },
				Vessel = new ServiceModel.Vessel { VesselName = "Admiral Freight", ImoNumber = "7654321" }
			};

			var leg2 = new ServiceModel.Leg
			{
				LoadPort = loadPort,
				DischargePort = dischargePort,
				Etd = new DateTime(2016, 8, 11),
				Eta = new DateTime(2016, 8, 16),
				Voyage = voyage2
			};

			var serviceRoute2 = new ServiceModel.Route { Carrier = carrier2, Legs = new[] { leg2 } };

			#endregion

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var route1 = new Route(Factory);
			route1.SetValues(serviceRoute1);
			var route2 = new Route(Factory);
			route2.SetValues(serviceRoute2);

			routesProvider.SetupSequence(r => r.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(Array.Empty<Route>())
				.Returns(new[] { route1, route2 })
				.CallBase();

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object) { Request = filterRequest };
			onlineSchedules.LoadRoutes(new NotificationBuffer());

			AssertEquals(0, onlineSchedules.Routes.Count);

			onlineSchedules.Routes.Sort("Departure", ListSortDirection.Descending);

			onlineSchedules.LoadRoutes(new NotificationBuffer());

			AssertEquals(2, onlineSchedules.Routes.Count);
			AssertEquals(new DateTime(2016, 8, 11), onlineSchedules.Routes[0].Departure);
			AssertEquals(new DateTime(2016, 8, 10), onlineSchedules.Routes[1].Departure);
		}

		public void TestCloneSelectedSchedules()
		{
			#region Create Routes

			var carrier1 = new ServiceModel.Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var voyage1 = new ServiceModel.Voyage
			{
				Code = "754N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = "Santa Maria", ImoNumber = "1234567" }
			};

			var leg1 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUSYD" },
				DischargePort = new ServiceModel.Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage1
			};

			var serviceRoute1 = new ServiceModel.Route { Carrier = carrier1, Legs = new[] { leg1 } };

			var carrier2 = new ServiceModel.Carrier
			{
				Code = "HPAG",
				Name = "Hapag Lloyd"
			};

			var voyage2 = new ServiceModel.Voyage
			{
				Code = "758N",
				TradeLane = new ServiceModel.TradeLane { Name = "BBB" },
				Operator = new ServiceModel.Carrier { Code = "HPAG", Name = "Hapag Lloyd" },
				Vessel = new ServiceModel.Vessel { VesselName = "Admiral Freight", ImoNumber = "7654321" }
			};

			var leg2 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUBNE" },
				DischargePort = new ServiceModel.Port { Unloco = "USLAX" },
				Etd = new DateTime(2016, 8, 11),
				Eta = new DateTime(2016, 8, 16),
				Voyage = voyage2
			};

			var serviceRoute2 = new ServiceModel.Route { Carrier = carrier2, Legs = new[] { leg2 } };

			#endregion

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var route1 = new Route(Factory);
			route1.SetValues(serviceRoute1);
			var route2 = new Route(Factory);
			route2.SetValues(serviceRoute2);
			var clonnedSchedules = onlineSchedules.CloneSelectedSchedules(new[] { route1, route2 });

			AssertEquals(2, clonnedSchedules.Routes.Count);

			#region Assert Route 1

			var clonnedRoute1 = clonnedSchedules.Routes[0];

			AssertEquals("AUSYD", clonnedRoute1.OriginPortUnloco);
			AssertEquals("AUBNE", clonnedRoute1.DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), clonnedRoute1.Departure);
			AssertEquals(new DateTime(2016, 8, 15), clonnedRoute1.Arrival);
			AssertEquals("CMAC", clonnedRoute1.CarrierSCAC);
			AssertEquals(1, clonnedRoute1.LegsCount);

			AssertEquals(1, clonnedRoute1.Legs.Count);
			AssertEquals("AUSYD", clonnedRoute1.Legs[0].OriginPortUnloco);
			AssertEquals("AUBNE", clonnedRoute1.Legs[0].DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), clonnedRoute1.Legs[0].Departure);
			AssertEquals(new DateTime(2016, 8, 15), clonnedRoute1.Legs[0].Arrival);
			AssertEquals("1234567", clonnedRoute1.Legs[0].LloydsNumber);
			AssertEquals("Santa Maria", clonnedRoute1.Legs[0].VesselName);
			AssertEquals("754N", clonnedRoute1.Legs[0].VoyageCode);
			AssertEquals("AAA", clonnedRoute1.Legs[0].TradeLaneName);
			AssertEquals("CMAC", clonnedRoute1.Legs[0].CarrierSCAC);
			AssertEquals("CMA CGM", clonnedRoute1.Legs[0].OperatorName);

			#endregion

			#region Assert Route 2

			var clonnedRoute2 = clonnedSchedules.Routes[1];

			AssertEquals("AUBNE", clonnedRoute2.OriginPortUnloco);
			AssertEquals("USLAX", clonnedRoute2.DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 11), clonnedRoute2.Departure);
			AssertEquals(new DateTime(2016, 8, 16), clonnedRoute2.Arrival);
			AssertEquals("HPAG", clonnedRoute2.CarrierSCAC);
			AssertEquals(1, clonnedRoute2.LegsCount);

			AssertEquals(1, clonnedRoute2.Legs.Count);
			AssertEquals("AUBNE", clonnedRoute2.Legs[0].OriginPortUnloco);
			AssertEquals("USLAX", clonnedRoute2.Legs[0].DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 11), clonnedRoute2.Legs[0].Departure);
			AssertEquals(new DateTime(2016, 8, 16), clonnedRoute2.Legs[0].Arrival);
			AssertEquals("7654321", clonnedRoute2.Legs[0].LloydsNumber);
			AssertEquals("Admiral Freight", clonnedRoute2.Legs[0].VesselName);
			AssertEquals("758N", clonnedRoute2.Legs[0].VoyageCode);
			AssertEquals("BBB", clonnedRoute2.Legs[0].TradeLaneName);
			AssertEquals("HPAG", clonnedRoute2.Legs[0].CarrierSCAC);
			AssertEquals("Hapag Lloyd", clonnedRoute2.Legs[0].OperatorName);

			#endregion
		}

		public void TestCloneSelectedSchedules_MustNotCreateNonSeaLegs()
		{
			#region Create Routes

			var carrier1 = new ServiceModel.Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var voyage1 = new ServiceModel.Voyage
			{
				Code = "754N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = "Santa Maria", ImoNumber = "1234567" }
			};

			var leg1 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUASP" },
				DischargePort = new ServiceModel.Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 2),
				Eta = new DateTime(2016, 8, 10),
				LegType = GssConstants.RailLegType
			};

			var leg2 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUBNE" },
				DischargePort = new ServiceModel.Port { Unloco = "SGSIN" },
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage1
			};

			var serviceRoute1 = new ServiceModel.Route { Carrier = carrier1, Legs = new[] { leg1, leg2 } };

			#endregion

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var route1 = new Route(Factory);
			route1.SetValues(serviceRoute1);
			var clonnedSchedules = onlineSchedules.CloneSelectedSchedules(new[] { route1 });

			AssertEquals(1, clonnedSchedules.Routes.Count);

			#region Assert Route

			var clonnedRoute1 = clonnedSchedules.Routes[0];

			AssertEquals("AUASP", clonnedRoute1.OriginPortUnloco);
			AssertEquals("SGSIN", clonnedRoute1.DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 2), clonnedRoute1.Departure);
			AssertEquals(new DateTime(2016, 8, 15), clonnedRoute1.Arrival);
			AssertEquals("CMAC", clonnedRoute1.CarrierSCAC);
			AssertEquals(2, clonnedRoute1.LegsCount);

			AssertEquals(1, clonnedRoute1.Legs.Count);
			AssertEquals("AUBNE", clonnedRoute1.Legs[0].OriginPortUnloco);
			AssertEquals("SGSIN", clonnedRoute1.Legs[0].DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), clonnedRoute1.Legs[0].Departure);
			AssertEquals(new DateTime(2016, 8, 15), clonnedRoute1.Legs[0].Arrival);
			AssertEquals("1234567", clonnedRoute1.Legs[0].LloydsNumber);
			AssertEquals("Santa Maria", clonnedRoute1.Legs[0].VesselName);
			AssertEquals("754N", clonnedRoute1.Legs[0].VoyageCode);
			AssertEquals("AAA", clonnedRoute1.Legs[0].TradeLaneName);
			AssertEquals("CMAC", clonnedRoute1.Legs[0].CarrierSCAC);
			AssertEquals("CMA CGM", clonnedRoute1.Legs[0].OperatorName);

			#endregion
		}

		public void TestCreateEnterpriseVoyages()
		{
			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "CMAC";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			#region Create Routes

			var carrier = new ServiceModel.Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var voyage1 = new ServiceModel.Voyage
			{
				Code = "754N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = "Santa Maria", ImoNumber = "9308390" }
			};

			var voyage2 = new ServiceModel.Voyage
			{
				Code = "755N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = "Admiral Grant", ImoNumber = "9463085" }
			};

			var leg1 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUSYD" },
				DischargePort = new ServiceModel.Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage1
			};

			var leg2 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUBNE" },
				DischargePort = new ServiceModel.Port { Unloco = "HKHKG" },
				Etd = new DateTime(2016, 8, 15),
				Eta = new DateTime(2016, 8, 29),
				Voyage = voyage2
			};

			var serviceRoute = new ServiceModel.Route { Carrier = carrier, Legs = new[] { leg1, leg2 } };

			#endregion

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);
			onlineSchedules.Routes.Add(route);

			onlineSchedules.CreateEnterpriseVoyages();

			Factory.Save();

			var voyages = GetVoyages(route.Carrier, route.Legs[0]);
			AssertEquals(1, voyages.Length);

			voyages = GetVoyages(route.Carrier, route.Legs[1]);
			AssertEquals(1, voyages.Length);
		}

		public void TestCreateEnterpriseVoyages_HasError()
		{
			#region Create Routes

			var carrier = new ServiceModel.Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var voyage = new ServiceModel.Voyage
			{
				Code = "754N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = "Santa Maria", ImoNumber = "1234567" }
			};

			var leg = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUSYD" },
				DischargePort = new ServiceModel.Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage
			};

			var serviceRoute = new ServiceModel.Route { Carrier = carrier, Legs = new[] { leg } };

			#endregion

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);
			onlineSchedules.Routes.Add(route);

			onlineSchedules.CreateEnterpriseVoyages();

			Factory.Save();

			var voyages = Factory.Load<JobVoyage>(new ZQuery());
			AssertEquals(0, voyages.Length);
		}

		#region Implementation

		JobVoyage[] GetVoyages(OrgHeader carrier, Leg leg)
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, leg.VoyageCode);
			query.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, leg.VesselName);
			query.AddToFilter(JobVoyageSchema.JV_OH_Line, carrier.PK);
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Sea);
			query.AddToFilter(JobVoyageSchema.JV_IsActive, true);

			return Factory.Load<JobVoyage>(query);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			return new OnlineSchedules(Factory, routesProvider.Object);
		}

		#endregion
	}
}
