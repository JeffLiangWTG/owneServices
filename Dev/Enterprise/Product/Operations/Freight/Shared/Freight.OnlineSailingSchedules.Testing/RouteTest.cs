using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules.ServiceModel;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(Route))]
	public class RouteTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Sailing Schedule", new Route(Factory).HumanReadableName);
		}

		public void TestRouteMustNotHaveWriteableProperties()
		{
			Func<Type, string[]> findAllPropertiesWithPublicSetters = (typeToReflect) =>
			{
				return typeToReflect
					.GetProperties()
					.Where(p => p.CanWrite && p.GetSetMethod() != null)
					.Select(p => p.Name)
					.ToArray();
			};

			var baseWriteableProperties = findAllPropertiesWithPublicSetters(typeof(NonPersistentBusinessObject));
			var allRouteWriteableProperties = findAllPropertiesWithPublicSetters(typeof(Route));

			var routeWriteableProperties = allRouteWriteableProperties
				.Where(p => !baseWriteableProperties.Contains(p));

			AssertEquals(string.Empty, string.Join(",", routeWriteableProperties));
		}

		public void TestSetValues()
		{
			var carrier = new Carrier
			{
				Name = "CMA CGM",
				Code = "CGMA"
			};

			var serviceLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUSYD" },
				DischargePort = new Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = new Voyage
				{
					Code = "754B",
					Vessel = new Vessel { VesselName = "Santa Maria", ImoNumber = "12345" },
					TradeLane = new TradeLane { Name = "AAA" },
					Operator = new Carrier
					{
						Name = "CMA CGM",
						Code = "CMAG"
					}
				},
				Co2eKgPerTeu = 1.0m,
				Co2eKgPerTonne = 2.0m
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { serviceLeg }
			};

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			AssertEquals("AUSYD", route.OriginPortUnloco);
			AssertEquals("AUBNE", route.DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), route.Departure);
			AssertEquals(new DateTime(2016, 8, 15), route.Arrival);
			AssertEquals("CGMA", route.CarrierSCAC);
			AssertEquals(1, route.LegsCount);
			AssertEquals(false, route.IsGateway);

			AssertEquals(1, route.Legs.Count);
			AssertEquals("AUSYD", route.Legs[0].OriginPortUnloco);
			AssertEquals("AUBNE", route.Legs[0].DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), route.Legs[0].Departure);
			AssertEquals(new DateTime(2016, 8, 15), route.Legs[0].Arrival);
			AssertEquals("12345", route.Legs[0].LloydsNumber);
			AssertEquals("Santa Maria", route.Legs[0].VesselName);
			AssertEquals("754B", route.Legs[0].VoyageCode);
			AssertEquals("AAA", route.Legs[0].TradeLaneName);
			AssertEquals("CMAG", route.Legs[0].CarrierSCAC);
			AssertEquals("CMA CGM", route.Legs[0].OperatorName);
			AssertEquals(1.0m, route.Legs[0].Co2eKgPerTeu);
			AssertEquals(2.0m, route.Legs[0].Co2eKgPerTonne);
		}

		public void TestSetValues_With3LegsAndNoEtaEtdForTranshipments()
		{
			#region Create Routes

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);
			var leg1 = RouteTestHelper.CreateServiceLeg("VYG12", "Santa M", "MSCU Carrier", "AUSYD", routeDepart, "SGSIN");
			var leg2 = RouteTestHelper.CreateServiceLeg(loadPort: "SGSIN", dischargePort: "ESVLC", legType: GssConstants.WaterLegType);
			var leg3 = RouteTestHelper.CreateServiceLeg(loadPort: "ESVLC", dischargePort: "USNYC", arrivalDate: routeArrive, legType: GssConstants.WaterLegType);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1, leg2, leg3);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			#endregion

			AssertEquals("AUSYD", route.OriginPortUnloco);
			AssertEquals("USNYC", route.DestinationPortUnloco);
			AssertEquals(routeDepart, route.Departure);
			AssertEquals(routeArrive, route.Arrival);
			AssertEquals("MSCU", route.CarrierSCAC);
			AssertEquals(3, route.LegsCount);
			AssertEquals(false, route.IsGateway);

			AssertEquals(3, route.Legs.Count);
			AssertLegInfo(leg1, route.Legs[0]);
			AssertLegInfo(leg2, route.Legs[1]);
			AssertLegInfo(leg3, route.Legs[2]);
		}

		public void TestSetValues_With2LegsAndNoEtdForFirstLeg()
		{
			#region Create Routes

			var routeArrive = new DateTime(2016, 9, 18);
			var leg1 = RouteTestHelper.CreateServiceLeg("VYG12", "Santa M", "MSCU Carrier", "AUSYD", null, "SGSIN");
			var leg2 = RouteTestHelper.CreateServiceLeg(loadPort: "SGSIN", dischargePort: "ESVLC", legType: GssConstants.WaterLegType);
			var leg3 = RouteTestHelper.CreateServiceLeg(loadPort: "ESVLC", dischargePort: "USNYC", arrivalDate: routeArrive, legType: GssConstants.WaterLegType);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1, leg2, leg3);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);
			route.Validation.ValidateAll();

			#endregion

			AssertEquals("AUSYD", route.OriginPortUnloco);
			AssertEquals("USNYC", route.DestinationPortUnloco);
			AssertEquals(ZDateTime.Empty, route.Departure);
			AssertHasError(route.DepartureInfo, "Departure date for first leg has not been set.");
			AssertEquals(routeArrive, route.Arrival);
			AssertEquals("MSCU", route.CarrierSCAC);
			AssertEquals(3, route.LegsCount);
			AssertEquals(false, route.IsGateway);

			AssertEquals(3, route.Legs.Count);
			AssertLegInfo(leg1, route.Legs[0]);
			AssertLegInfo(leg2, route.Legs[1]);
			AssertLegInfo(leg3, route.Legs[2]);
		}

		public void TestSetValues_With2LegsAndNoEtaForLastLeg()
		{
			#region Create Routes

			var routeDepart = new DateTime(2016, 8, 10);
			var leg1 = RouteTestHelper.CreateServiceLeg("VYG12", "Santa M", "MSCU Carrier", "AUSYD", routeDepart, "SGSIN");
			var leg2 = RouteTestHelper.CreateServiceLeg(loadPort: "SGSIN", dischargePort: "ESVLC", legType: GssConstants.WaterLegType);
			var leg3 = RouteTestHelper.CreateServiceLeg(loadPort: "ESVLC", dischargePort: "USNYC", arrivalDate: null, legType: GssConstants.WaterLegType);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1, leg2, leg3);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);
			route.Validation.ValidateAll();

			#endregion

			AssertEquals("AUSYD", route.OriginPortUnloco);
			AssertEquals("USNYC", route.DestinationPortUnloco);
			AssertEquals(routeDepart, route.Departure);
			AssertEquals(ZDateTime.Empty, route.Arrival);
			AssertHasError(route.ArrivalInfo, "Arrival date for last leg has not been set.");
			AssertEquals("MSCU", route.CarrierSCAC);
			AssertEquals(3, route.LegsCount);
			AssertEquals(false, route.IsGateway);

			AssertEquals(3, route.Legs.Count);
			AssertLegInfo(leg1, route.Legs[0]);
			AssertLegInfo(leg2, route.Legs[1]);
			AssertLegInfo(leg3, route.Legs[2]);
		}

		public void TestSetValues_Co2ePropertiesSumUpLegCo2eDataInLegs()
		{
			#region Create Routes

			var routeDepart = new DateTime(2016, 8, 10);
			var leg1 = RouteTestHelper.CreateServiceLeg(co2eKgPerTeu: 10.0m, co2eKgPerTonne: 20.0m);
			var leg2 = RouteTestHelper.CreateServiceLeg(co2eKgPerTeu: 20.0m, co2eKgPerTonne: 30.0m);
			var leg3 = RouteTestHelper.CreateServiceLeg(co2eKgPerTeu: 30.0m, co2eKgPerTonne: 40.0m);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1, leg2, leg3);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);
			route.Validation.ValidateAll();

			#endregion

			AssertEquals(60.0m, route.Co2eKgPerTeu);
			AssertEquals(90.0m, route.Co2eKgPerTonne);
		}

		public void TestSetValues_Co2ePropertySetToZeroIfOneLegCo2PropertyIsZero()
		{
			#region Create Routes

			var routeDepart = new DateTime(2016, 8, 10);
			var leg1 = RouteTestHelper.CreateServiceLeg(co2eKgPerTeu: 10.0m, co2eKgPerTonne: 0.0m);
			var leg2 = RouteTestHelper.CreateServiceLeg(co2eKgPerTeu: 20.0m, co2eKgPerTonne: 30.0m);
			var leg3 = RouteTestHelper.CreateServiceLeg(co2eKgPerTeu: 0.0m, co2eKgPerTonne: 40.0m);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1, leg2, leg3);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);
			route.Validation.ValidateAll();

			#endregion

			AssertEquals(0.0m, route.Co2eKgPerTeu);
			AssertEquals(0.0m, route.Co2eKgPerTonne);
		}

		public void TestClone()
		{
			#region Create Routes

			var carrier = new Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var voyage = new Voyage
			{
				Code = "754N",
				TradeLane = new TradeLane { Name = "AAA" },
				Operator = new Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new Vessel { VesselName = "Santa Maria", ImoNumber = "1234567" }
			};

			var leg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUSYD" },
				DischargePort = new Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage
			};

			var serviceRoute = new ServiceModel.Route { Carrier = carrier, Legs = new[] { leg } };

			#endregion

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var clonnedRoute = route.Clone(Factory);

			AssertEquals("AUSYD", clonnedRoute.OriginPortUnloco);
			AssertEquals("AUBNE", clonnedRoute.DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), clonnedRoute.Departure);
			AssertEquals(new DateTime(2016, 8, 15), clonnedRoute.Arrival);
			AssertEquals("CMAC", clonnedRoute.CarrierSCAC);
			AssertEquals(1, clonnedRoute.LegsCount);
			AssertEquals(false, clonnedRoute.IsGateway);

			AssertEquals(1, clonnedRoute.Legs.Count);
			AssertEquals("AUSYD", clonnedRoute.Legs[0].OriginPortUnloco);
			AssertEquals("AUBNE", clonnedRoute.Legs[0].DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), clonnedRoute.Legs[0].Departure);
			AssertEquals(new DateTime(2016, 8, 15), clonnedRoute.Legs[0].Arrival);
			AssertEquals("1234567", clonnedRoute.Legs[0].LloydsNumber);
			AssertEquals("Santa Maria", clonnedRoute.Legs[0].VesselName);
			AssertEquals("754N", clonnedRoute.Legs[0].VoyageCode);
			AssertEquals("AAA", clonnedRoute.Legs[0].TradeLaneName);
			AssertEquals("CMAC", clonnedRoute.Legs[0].CarrierSCAC);
			AssertEquals("CMA CGM", clonnedRoute.Legs[0].OperatorName);
		}

		public void TestCarrier_CarrierExists()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			var cusCodes = carrier.CustomsCodes.AddNew();
			cusCodes.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCodes.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCodes.OK_CustomsRegNo = "SCAC";

			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "SCAC Carrier");
			AssertEquals("SCAC", route.CarrierSCAC);
			AssertEquals(carrier.PK, route.Carrier.PK);
			AssertEquals("Org", route.CarrierCode);
		}

		public void TestCarrier_CarrierDoesNotExist()
		{
			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "SCAC Carrier");
			AssertNull(route.Carrier);
			AssertEquals(ZString.Empty, route.CarrierCode);
		}

		public void TestCarrier_TwoCarriersWithSameSCACExist()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "Org1";
			var cusCodes1 = carrier1.CustomsCodes.AddNew();
			cusCodes1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCodes1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCodes1.OK_CustomsRegNo = "SCAC";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "Org2";
			var cusCodes2 = carrier2.CustomsCodes.AddNew();
			cusCodes2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCodes2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCodes2.OK_CustomsRegNo = "SCAC";

			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "SCAC Carrier");

			AssertEquals("SCAC", route.CarrierSCAC);
			AssertNull(route.Carrier);
		}

		public void TestCarrier_ReturnsActiveCarrier_When_ThereAreTwoCarriers_And_OnlyOneIsActive()
		{
			var entCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier1.OH_IsActive = false;
			var cusCode1 = entCarrier1.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "SCAC";
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var entCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier2.OH_IsActive = true;
			var cusCode2 = entCarrier2.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = "SCAC";
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "SCAC Carrier");

			AssertEquals("SCAC", route.CarrierSCAC);
			AssertNotNull(route.Carrier);
			AssertEquals(entCarrier2.OH_Code, route.Carrier.OH_Code);
		}

		public void TestLinkTransports_SingleLeg()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "SHMAERSK";
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			Factory.Save();

			var today = DateTime.Today;

			var route = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"SHMAERSK",
				"AUSYD", today,
				"NZAKL", today.AddDays(10));

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "OLGA MAERSK";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "100";
			voyage.JV_OH_Line = carrierOrganisation.PK;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			AssertNoExceptionThrown("Null checks are in place", () => route.MergeWithTransports(null, null));

			route.MergeWithTransports(transport, null);

			CombineAssertions(() =>
			{
				var matchedSailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");

				AssertEquals(true, transport.JW_IsLinked);
				AssertEquals(matchedSailing.PK, transport.JW_JX);
			});

			transport.JW_JX = ZGuid.Empty;

			route.MergeWithTransports(null, consol.Transports);

			CombineAssertions(() =>
			{
				var matchedSailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");

				var newlyAddedTransport = consol.Transports.Cast<Transport>().FirstOrDefault(t => t.JW_JX == matchedSailing.PK);

				AssertEquals("New transport was added", true, newlyAddedTransport.PK != transport.PK);
				AssertEquals(true, newlyAddedTransport.JW_IsLinked);
			});
		}

		public void TestLinkTransports_LinkRoute()
		{
			var (voyage, _, vessel1, _) = CreateVoyages();
			var sailing = voyage.Sailings[0];

			var contract = Factory.New<IRatingContract>();
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = sailing.PK;

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);

			var leg1 = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, vessel1.RV_LloydsNumber);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var mockSelectorProvider = new Mock<IMultiAllocationRouteSelectorProvider>();
			Factory.SetValue(() => mockSelectorProvider.Object);
			var mockDialogProvider = new Mock<IOverrideAllocationRouteDialogProvider>();
			mockDialogProvider.Setup(m => m.PromptUserForConfirmingOverride(It.IsAny<IRatingContractAllocationLine>(), It.IsAny<IAllocationRouteAssignable>())).Returns(true);
			Factory.SetValue(() => mockDialogProvider.Object);

			var consol = Factory.New<IForwardingConsol>();
			var transport = consol.Transports_AddNew();
			route.MergeWithTransports(transport as Transport, null);
			AssertEquals("Allocation Route Defaulted", consol.GetPropertyValue(JobConsolSchema.Constants.JK_RCA_AllocationLine), allocationRoute.PK);
			AssertEquals("Carrier Contract Number Defaulted", consol.GetPropertyValue(JobConsolSchema.Constants.JK_CarrierContractNumber), allocationRoute.Contract.RCT_ContractNumber);
		}

		public void TestMergeWithTransports_ShouldSetTransportCreditor_SEA() => TestMergeWithTransports_ShouldSetTransportCreditor(GssConstants.SeaLegType);

		public void TestMergeWithTransports_ShouldSetTransportCreditor_RAIL() => TestMergeWithTransports_ShouldSetTransportCreditor(GssConstants.RailLegType);

		public void TestMergeWithTransports_ShouldSetTransportCreditor_ROAD() => TestMergeWithTransports_ShouldSetTransportCreditor(GssConstants.RoadLegType);

		public void TestMergeWithTransports_ShouldSetTransportCreditor_WATER() => TestMergeWithTransports_ShouldSetTransportCreditor(GssConstants.WaterLegType);

		public void TestMergeWithTransports_ShouldSetTransportCreditor_FEEDER() => TestMergeWithTransports_ShouldSetTransportCreditor(GssConstants.FeederLegType);

		void TestMergeWithTransports_ShouldSetTransportCreditor(string legType)
		{
			var (voyage, _, vessel, _) = CreateVoyages();
			var sailing = voyage.Sailings[0];

			var contract = Factory.New<IRatingContract>();
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = sailing.PK;

			var leg = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", new DateTime(2016, 8, 10), "NZAKL", new DateTime(2016, 9, 18), vessel.RV_LloydsNumber, legType);

			voyage.Carrier.OH_IsCreditor = true;

			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			Factory.SetValue(() => new Mock<IMultiAllocationRouteSelectorProvider>().Object);
			var overrideAllocationRouteDialogMock = new Mock<IOverrideAllocationRouteDialogProvider>();
			overrideAllocationRouteDialogMock
				.Setup(m => m.PromptUserForConfirmingOverride(It.IsAny<IRatingContractAllocationLine>(), It.IsAny<IAllocationRouteAssignable>()))
				.Returns(true);
			Factory.SetValue(() => overrideAllocationRouteDialogMock.Object);

			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			var transport = consol.Transports.AddNew();
			route.MergeWithTransports(transport, null);
			AssertEquals("Allocation Route Defaulted", consol.JK_RCA_AllocationLine, allocationRoute.PK);

			AssertEquals("transport.JW_OA_CreditorAddress", voyage.Carrier.MainAddress.PK, transport.JW_OA_CreditorAddress);
		}

		public void TestLinkTransports_SingleLeg_VesselRenamed()
		{
			var (_, _, vessel1, _) = CreateVoyages();

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);
			var leg1 = RouteTestHelper.CreateServiceLeg("100", "NOT OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, vessel1.RV_LloydsNumber);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			route.MergeWithTransports(transport, null);
			CombineAssertions(() =>
			{
				AssertEquals(false, transport.JW_IsLinked);
				AssertEquals("NOT OLGA MAERSK", transport.JW_Vessel);
			});
		}

		public void TestLinkTransports_MultipleLegs()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "SHMAERSK";
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "OLGA MAERSK";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "HELIGA MAERSK";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "100";
			voyage1.JV_OH_Line = carrierOrganisation.PK;

			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage1.GenerateSailings();

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_VoyageFlight = "200";
			voyage2.JV_OH_Line = carrierOrganisation.PK;

			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage2.GenerateSailings();

			Factory.Save();

			var today = DateTime.Today;

			#region Create Route

			var carrier = new Carrier
			{
				Name = "SHMAERSK",
				Code = "SHMA"
			};

			var firstLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUSYD" },
				DischargePort = new Port { Unloco = "NZAKL" },
				Etd = today,
				Eta = today.AddDays(5),
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "OLGA MAERSK", ImoNumber = "1024" },
					Operator = carrier,
				}
			};

			var secondLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "NZAKL" },
				DischargePort = new Port { Unloco = "SGSIN" },
				Etd = today.AddDays(10),
				Eta = today.AddDays(20),
				Voyage = new Voyage
				{
					Code = "200",
					Vessel = new Vessel { VesselName = "HELIGA MAERSK", ImoNumber = "2048" },
					Operator = carrier,
				}
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { firstLeg, secondLeg }
			};

			#endregion

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			var transport = consol.Transports.AddNew();

			AssertNoExceptionThrown("Null checks are in place", () => route.MergeWithTransports(null, null));
			AssertNoExceptionThrown("Null checks are in place", () => route.MergeWithTransports(transport, null));

			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				var matchedSailing1 = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");

				AssertEquals(true, transport.JW_IsLinked);
				AssertEquals(matchedSailing1.PK, transport.JW_JX);

				AssertEquals("New transport added", 2, consol.Transports.Count);

				var extraTransport = consol.Transports[1];
				var matchedSailing2 = voyage2.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "SGSIN");

				AssertEquals(true, extraTransport.JW_IsLinked);
				AssertEquals(matchedSailing2.PK, extraTransport.JW_JX);
			});
		}

		public void TestLinkTransports_MultipleLegs_MissingEtaEtdForTranshipments()
		{
			var (voyage1, _, vessel1, _) = CreateVoyages();

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);
			var leg1 = RouteTestHelper.CreateServiceLeg("100", vessel1.RV_Name, "SHMAERSK", "AUSYD", routeDepart, "NZAKL", imoNumber: vessel1.RV_LloydsNumber);
			var leg2 = RouteTestHelper.CreateServiceLeg(loadPort: "NZAKL", dischargePort: "SGSIN", legType: GssConstants.WaterLegType);
			var leg3 = RouteTestHelper.CreateServiceLeg(loadPort: "SGSIN", dischargePort: "USNYC", arrivalDate: routeArrive, legType: GssConstants.WaterLegType);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1, leg2, leg3);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				var matchedSailing1 = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");

				AssertEquals(true, transport.JW_IsLinked);
				AssertEquals(matchedSailing1.PK, transport.JW_JX);

				AssertEquals("New transport added", 3, consol.Transports.Count);

				AssertEquals(false, consol.Transports[1].JW_IsLinked);
				AssertEquals(false, consol.Transports[2].JW_IsLinked);
			});
		}

		public void TestLinkTransports_MultipleLegs_VesselRenamed()
		{
			var (_, _, vessel1, vessel2) = CreateVoyages();

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);
			var leg1 = RouteTestHelper.CreateServiceLeg("100", "NOT " + vessel1.RV_Name, "SHMAERSK", "AUSYD", routeDepart, "NZAKL", imoNumber: vessel1.RV_LloydsNumber);
			var leg2 = RouteTestHelper.CreateServiceLeg(vesselName: "NOT " + vessel2.RV_Name, imoNumber: vessel2.RV_LloydsNumber, loadPort: "NZAKL", dischargePort: "SGSIN", legType: GssConstants.WaterLegType);
			var leg3 = RouteTestHelper.CreateServiceLeg(loadPort: "SGSIN", dischargePort: "USNYC", arrivalDate: routeArrive, legType: GssConstants.WaterLegType);
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1, leg2, leg3);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);
			CombineAssertions(() =>
			{
				AssertEquals(false, transport.JW_IsLinked);
				AssertEquals("NOT " + vessel1.RV_Name, transport.JW_Vessel);

				AssertEquals(false, consol.Transports[1].JW_IsLinked);
				AssertEquals("NOT " + vessel2.RV_Name, consol.Transports[1].JW_Vessel);
				AssertEquals(false, consol.Transports[2].JW_IsLinked);
				AssertEquals(ZString.Empty, consol.Transports[2].JW_Vessel);
			});
		}

		public void TestLinkTransport_SetConsolShippingAddress_When_AgentRouteCarrierIsNvo()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(false, true, null, null, CreateTestServiceRoute(), "ROT1_CC", null);

		public void TestLinkTransport_SetConsolShippingAddress_When_AgentRouteCarrierIsNotNvo()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(false, false, null, null, CreateTestServiceRoute(), "ROT1_CC", null);

		public void TestLinkTransport_SetConsolShippingAddressAndCreditor_When_CoLoadRouteCarrierIsNvo()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(true, true, null, null, CreateTestServiceRoute(), "LEG1_CC", "ROT1_CC");

		public void TestLinkTransport_SetConsolShippingAddress_When_CoLoadRouteCarrierIsNotNvo()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(true, false, null, null, CreateTestServiceRoute(), "ROT1_CC", null);

		public void TestLinkTransport_SetConsolShippingAddressAndCreditor_When_CoLoadAndRouteCarrierIsNvoAndFirstLegIsWater()
		{
			var serviceRoute = CreateTestServiceRoute();
			serviceRoute.Legs[0].LegType = GssConstants.WaterLegType;

			TestLinkTransports_SetConsolShippingAddressAndCreditor(true, true, null, null, serviceRoute, "LEG2_CC", "ROT1_CC");
		}

		public void TestLinkTransport_SetConsolShippingAddress_When_AgentRouteCarrierIsNotNvoAndExistingShippingAddressSetToDifferentCarrier()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(false, false, "OTHE_RR", null, CreateTestServiceRoute(), "ROT1_CC", null);

		public void TestLinkTransport_SetConsolShippingAddressAndCreditor_When_CoLoadAndRouteCarrierIsNvoAndExistingShippingAddressSetToDifferentCarrier()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(true, true, "OTHER_RR", "BLAB_RR", CreateTestServiceRoute(), "LEG1_CC", "ROT1_CC");

		public void TestLinkTransport_DoesNotSetConsolShippingAddress_When_AgentRouteCarrierIsNotNvoAndExistingShippingAddressSetToSameCarrier()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(false, false, "ROT1_RR", null, CreateTestServiceRoute(), "ROT1_RR", null);

		public void TestLinkTransport_DoesNotSetConsolShippingAddressAndCreditor_When_CoLoadAndRouteCarrierIsNvoAndExistingShippingAddressSetToSameCarrier()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(true, true, "LEG1_RR", "ROT1_RR", CreateTestServiceRoute(), "LEG1_RR", "ROT1_RR");

		public void TestLinkTransport_StaysSameDefaultConsolShippingAddressAndCreditor_When_CoLoadAndRouteCarrierIsNvoAndExistingShippingAddressSetToDefalt()
			=> TestLinkTransports_SetConsolShippingAddressAndCreditor(true, true, "LEG1_CC", "ROT1_CC", CreateTestServiceRoute(), "LEG1_CC", "ROT1_CC");

		void TestLinkTransports_SetConsolShippingAddressAndCreditor(bool isCoLoad, bool isNvo, string initialShippingAddress, string initialCreditorAddress, ServiceModel.Route serviceRoute, string expectedShippingAddress, string expectedCreditor)
		{
			var carriers = serviceRoute.Legs
				.Select(leg => leg.Voyage.Operator)
				.Append(serviceRoute.Carrier)
				.GroupBy(c => c.Code)
				.Select(g => g.First());

			foreach (var carrier in carriers)
			{
				CreateCarrierOrgHeader(carrier.Code + "_CC", carrier.Name, carrier.Code, isNvo);
			}

			var (voyage1, voyage2, _, _) = CreateVoyages();

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = isCoLoad ? Constants.AgentType.CoLoad : Constants.AgentType.Agent;
			consol.Transports.RemoveAndDeleteAll();

			if (initialShippingAddress != null)
			{
				var carrierOrganisation = CreateCarrierOrgHeader(initialShippingAddress, initialShippingAddress, initialShippingAddress.Substring(0, 4), false);
				consol.SetDefaultShippingLineAddress(carrierOrganisation);
			}

			if (initialCreditorAddress != null)
			{
				var carrierOrganisation = CreateCarrierOrgHeader(initialCreditorAddress, initialCreditorAddress, initialCreditorAddress.Substring(0, 4), true);
				consol.CreditorPK = carrierOrganisation.PK;
			}

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				AssertEquals(expectedShippingAddress, consol.ShippingLineAddress.Header.OH_Code);
				AssertEquals(expectedCreditor, consol.Creditor?.OH_Code);
			});
		}

		OrgHeader CreateCarrierOrgHeader(string code, string name, string scac, bool isNvo)
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = code;
			carrierOrganisation.OH_FullName = name;
			carrierOrganisation.OH_IsSeaWholesaler = isNvo;
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, scac, Constants.CountryCodes.UnitedStates);
			return carrierOrganisation;
		}

		public void TestLinkTransports_Imports2SeaLegsAnd1NonSeaLegIsDiscarded_When_DiscardedNonSeaLegHasSameOriginAndDestinationPort()
		{
			var (voyage1, voyage2, _, _) = CreateVoyages();

			var today = DateTime.Today;

			#region Create Route

			var carrier = new Carrier
			{
				Name = "SHMAERSK",
				Code = "SHMA"
			};

			var firstLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUSYD" },
				DischargePort = new Port { Unloco = "NZAKL" },
				Etd = today,
				Eta = today.AddDays(5),
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "OLGA MAERSK", ImoNumber = "1024" },
					Operator = carrier,
				}
			};

			var secondLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "NZAKL" },
				DischargePort = new Port { Unloco = "SGSIN" },
				Etd = today.AddDays(10),
				Eta = today.AddDays(20),
				Voyage = new Voyage
				{
					Code = "200",
					Vessel = new Vessel { VesselName = "HELIGA MAERSK", ImoNumber = "2048" },
					Operator = carrier,
				}
			};

			var thirdLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "SGSIN" },
				DischargePort = new Port { Unloco = "MYKLA" },
				Etd = today.AddDays(10),
				Eta = today.AddDays(12),
				LegType = GssConstants.RoadLegType
			};

			var fourthLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "MYKLA" },
				DischargePort = new Port { Unloco = "MYKLA" },
				Etd = today.AddDays(12),
				Eta = today.AddDays(12),
				LegType = GssConstants.RoadLegType
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { firstLeg, secondLeg, thirdLeg, fourthLeg }
			};

			#endregion

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				var matchedSailing1 = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");

				AssertEquals(true, transport.JW_IsLinked);
				AssertEquals(matchedSailing1.PK, transport.JW_JX);

				AssertEquals("New transport added", 3, consol.Transports.Count);

				var secondTransport = consol.Transports[1];
				var matchedSailing2 = voyage2.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "SGSIN");

				AssertEquals(true, secondTransport.JW_IsLinked);
				AssertEquals(matchedSailing2.PK, secondTransport.JW_JX);

				var thirdTransport = consol.Transports[2];
				AssertEquals(thirdTransport.JW_TransportMode, Constants.TransportModes.Road);
			});
		}

		(JobVoyage voyage1, JobVoyage voyage2, RefVessel vessel1, RefVessel vessel2) CreateVoyages()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "SHMAERSK";
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA",
				Core.Constants.CountryCodes.UnitedStates);

			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "OLGA MAERSK";
			vessel1.RV_LloydsNumber = "1234567";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "100";
			voyage1.JV_OH_Line = carrierOrganisation.PK;

			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage1.GenerateSailings();

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "HELIGA MAERSK";
			vessel2.RV_LloydsNumber = "4563455";

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_VoyageFlight = "200";
			voyage2.JV_OH_Line = carrierOrganisation.PK;

			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage2.GenerateSailings();

			Factory.Save();
			return (voyage1, voyage2, vessel1, vessel2);
		}

		static ServiceModel.Route CreateTestServiceRoute()
		{
			var today = DateTime.Today;
			var serviceRoute = RouteTestHelper.CreateServiceRoute(
				"ROT1",
				RouteTestHelper.CreateServiceLeg(carrierName: "LEG1", loadPort: "AUSYD", dischargePort: "NZAKL", departureDate: today, voyageNumber: "voy1"),
				RouteTestHelper.CreateServiceLeg(carrierName: "LEG2", loadPort: "NZAKL", dischargePort: "SGSIN", departureDate: today, voyageNumber: "voy2")
			);
			return serviceRoute;
		}

		public void TestMergeWithTransports_TransportModeAreProperlyMapped_When_AllTransportTypesKnown()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "SHMAERSK";
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "OLGA MAERSK";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			voyage1.JV_VoyageFlight = "100";
			voyage1.JV_OH_Line = carrierOrganisation.PK;

			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUNTL";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZTRG";
			voyage1.GenerateSailings();

			Factory.Save();

			var today = DateTime.Today;

			#region Create Route

			var carrier = new Carrier
			{
				Name = "SHMAERSK",
				Code = "SHMA"
			};

			var firstLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUHBA" },
				DischargePort = new Port { Unloco = "AUMEL" },
				Etd = today.AddDays(10),
				Eta = today.AddDays(14),
				LegType = GssConstants.WaterLegType,
				Voyage = new Voyage
				{
					Operator = carrier
				}
			};

			var secondLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUMEL" },
				DischargePort = new Port { Unloco = "AUNTL" },
				Etd = today.AddDays(14),
				Eta = today.AddDays(18),
				LegType = GssConstants.RailLegType
			};

			var thirdLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUNTL" },
				DischargePort = new Port { Unloco = "NZTRG" },
				Etd = today.AddDays(19),
				Eta = today.AddDays(25),
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "OLGA MAERSK", ImoNumber = "1024" },
					Operator = carrier,
				}
			};

			var fourthLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "NZTRG" },
				DischargePort = new Port { Unloco = "NZAKL" },
				Etd = today.AddDays(25),
				Eta = today.AddDays(27),
				LegType = GssConstants.RoadLegType
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { firstLeg, secondLeg, thirdLeg, fourthLeg }
			};

			#endregion

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				AssertEquals("New transport added", 4, consol.Transports.Count);

				var firstTransport = consol.Transports[0];
				AssertEquals(false, firstTransport.JW_IsLinked);
				AssertEquals(Constants.TransportModes.Sea, firstTransport.JW_TransportMode);
				AssertEquals(carrierOrganisation.OH_Code, firstTransport.Carrier?.OH_Code);

				var secondTransport = consol.Transports[1];
				AssertEquals(Constants.TransportModes.Rail, secondTransport.JW_TransportMode);

				var matchedSailing1 = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUNTL", "NZTRG");

				var thirdTransport = consol.Transports[2];
				AssertEquals(true, thirdTransport.JW_IsLinked);
				AssertEquals(matchedSailing1.PK, thirdTransport.JW_JX);

				var fourthTransport = consol.Transports[3];
				AssertEquals(fourthTransport.JW_TransportMode, Constants.TransportModes.Road);
			});
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameIsTba()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "TBA" },
				Constants.TransportModes.Sea);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameIsWater()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "WATER" },
				Constants.TransportModes.InlandWaterwayTransport);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameContainsWater()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "BLUE WATER" },
				Constants.TransportModes.Sea);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameIsBarge()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "BARGE" },
				Constants.TransportModes.InlandWaterwayTransport);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameContainsWordBarge()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "RIVER BARGE" },
				Constants.TransportModes.InlandWaterwayTransport);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameContainsBargeAsPartOfAnotherWord()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "BARGED IN" },
				Constants.TransportModes.Sea);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameIsInlandWaterway()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "INLAND WATERWAY" },
				Constants.TransportModes.InlandWaterwayTransport);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameIsCombinedWaterway()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = "COMBINED WATERWAY" },
				Constants.TransportModes.InlandWaterwayTransport);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselNameIsNull()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				new Vessel { VesselName = null },
				Constants.TransportModes.Sea);
		}

		public void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWaterAndVesselIsNull()
		{
			TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(
				null,
				Constants.TransportModes.Sea);
		}

		void TestMergeWithTransports_TransportModeIsProperlyMapped_When_GssTransportTypesIsWater(Vessel gssVessel, string expectedTransportType)
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "SHMAERSK";
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			Factory.Save();

			var today = DateTime.Today;

			#region Create Route

			var carrier = new Carrier
			{
				Name = "SHMAERSK",
				Code = "SHMA"
			};

			var leg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUHBA" },
				DischargePort = new Port { Unloco = "AUMEL" },
				Etd = today.AddDays(10),
				Eta = today.AddDays(11),
				LegType = GssConstants.WaterLegType,
				Voyage = new Voyage
				{
					Operator = carrier,
					Vessel = gssVessel
				}
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { leg }
			};

			#endregion

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				AssertEquals("New transport added", 1, consol.Transports.Count);

				var firstTransport = consol.Transports[0];
				AssertEquals(false, firstTransport.JW_IsLinked);
				AssertEquals(expectedTransportType, firstTransport.JW_TransportMode);
				AssertEquals(carrierOrganisation.OH_Code, firstTransport.Carrier?.OH_Code);
			});
		}

		public void TestMergeWithTransports_MapsTransportModeAsItIs_When_TransportModeUnknown()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "SHMAERSK";
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "OLGA MAERSK";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			voyage1.JV_VoyageFlight = "100";
			voyage1.JV_OH_Line = carrierOrganisation.PK;

			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUNTL";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZTRG";
			voyage1.GenerateSailings();

			Factory.Save();

			var today = DateTime.Today;

			#region Create Route

			var carrier = new Carrier
			{
				Name = "SHMAERSK",
				Code = "SHMA"
			};

			var firstLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUNTL" },
				DischargePort = new Port { Unloco = "NZTRG" },
				Etd = today.AddDays(19),
				Eta = today.AddDays(25),
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "OLGA MAERSK", ImoNumber = "1024" },
					Operator = carrier,
				}
			};

			var secondLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "NZTRG" },
				DischargePort = new Port { Unloco = "NZAKL" },
				Etd = today.AddDays(25),
				Eta = today.AddDays(27),
				LegType = "Bla"
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { firstLeg, secondLeg, }
			};

			#endregion

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				AssertEquals("New transport added", 2, consol.Transports.Count);

				var matchedSailing1 = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUNTL", "NZTRG");

				var firstTransport = consol.Transports[0];
				AssertEquals(true, firstTransport.JW_IsLinked);
				AssertEquals(matchedSailing1.PK, firstTransport.JW_JX);

				var secondTransport = consol.Transports[1];
				AssertEquals(secondTransport.JW_TransportMode, "Bla");
			});
		}

		public void TestMergeWithTransports_UnlinksTransport_When_LegBelongsToFeederAndLinked()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "SHMAERSK";
			carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "OLGA MAERSK";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			voyage1.JV_VoyageFlight = "100";
			voyage1.JV_OH_Line = carrierOrganisation.PK;

			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "DEHAM";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USORF";
			voyage1.GenerateSailings();

			Factory.Save();

			var today = DateTime.Today;

			#region Create Route

			const string secondLegTradeLaneName = "Trade Lane Inc. 2";

			var carrier = new Carrier
			{
				Name = "SHMAERSK",
				Code = "SHMA"
			};

			var firstLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "DEBRV" },
				DischargePort = new Port { Unloco = "DEHAM" },
				Etd = today.AddDays(19),
				Eta = today.AddDays(25),
				LegType = GssConstants.WaterLegType,
				Voyage = new Voyage
				{
					Operator = carrier,
					TradeLane = new TradeLane { Name = "Trade Lane Inc." }
				},
			};

			var secondLeg = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "DEHAM" },
				DischargePort = new Port { Unloco = "USORF" },
				Etd = today.AddDays(25),
				Eta = today.AddDays(27),
				LegType = GssConstants.SeaLegType,
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "OLGA MAERSK", ImoNumber = "1024" },
					Operator = carrier,
					TradeLane = new TradeLane { Name = secondLegTradeLaneName }
				}
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { firstLeg, secondLeg, }
			};

			#endregion

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals("Prerequisite: transport collection would re-create one transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];

			voyage1.Sailings[0].JX_ServiceString = secondLegTradeLaneName; // would be done by CreateEnterpriseVoyage
			route.MergeWithTransports(transport, consol.Transports);

			CombineAssertions(() =>
			{
				AssertEquals("New transport added", 2, consol.Transports.Count);

				var firstTransport = consol.Transports[0];
				AssertEquals(Constants.TransportModes.Sea, firstTransport.JW_TransportMode);
				AssertEquals(true, firstTransport.IsFeeder);
				AssertEquals(false, firstTransport.JW_IsLinked);
				AssertEquals(carrierOrganisation.OH_Code, firstTransport.Carrier?.OH_Code);
				AssertEquals("Trade Lane Inc.", firstTransport.JW_ServiceString);

				var matchedSailing = voyage1.Sailings.GetSailingFromLoadAndDischarge("DEHAM", "USORF");
				var secondTransport = consol.Transports[1];
				AssertEquals(Constants.TransportModes.Sea, secondTransport.JW_TransportMode);
				AssertEquals(true, secondTransport.JW_IsLinked);
				AssertEquals(matchedSailing.PK, secondTransport.JW_JX);
				AssertEquals(carrierOrganisation.OH_Code, secondTransport.Carrier?.OH_Code);
				AssertEquals("Trade Lane Inc. 2", secondTransport.JW_ServiceString);
			});
		}

		public void TestTranShipment()
		{
			var today = DateTime.Today;

			#region Create legs

			var carrier1 = new Carrier
			{
				Name = "SHMAERSK",
				Code = "SHMA"
			};

			var carrier2 = new Carrier
			{
				Name = "SECONDCA",
				Code = "SECA"
			};

			var leg1 = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "AUSYD" },
				DischargePort = new Port { Unloco = "NZAKL" },
				Etd = today,
				Eta = today.AddDays(5),
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "OLGA MAERSK", ImoNumber = "1024" },
					Operator = carrier1,
				}
			};

			var leg2 = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "NZAKL" },
				DischargePort = new Port { Unloco = "SGSIN" },
				Etd = today.AddDays(10),
				Eta = today.AddDays(20),
				Voyage = new Voyage
				{
					Code = "200",
					Vessel = new Vessel { VesselName = "HELIGA MAERSK", ImoNumber = "2048" },
					Operator = carrier2,
				}
			};

			var leg3 = new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = "NZAKL" },
				DischargePort = new Port { Unloco = "SGSIN" },
				Etd = today.AddDays(10),
				Eta = today.AddDays(20),
				Voyage = new Voyage
				{
					Code = "200",
					Vessel = new Vessel { VesselName = "HELIGA MAERSK", ImoNumber = "2048" },
					Operator = carrier1,
				}
			};

			#endregion

			var serviceRoute1 = new ServiceModel.Route
			{
				Carrier = carrier1,
				Legs = new[] { leg1, leg2 }
			};

			var route1 = new Route(Factory);
			route1.SetValues(serviceRoute1);
			Assert("Two carriers, TranShipment is required", route1.IsGateway);

			var serviceRoute3 = new ServiceModel.Route
			{
				Carrier = carrier1,
				Legs = new[] { leg1, leg3 }
			};

			var route3 = new Route(Factory);
			route3.SetValues(serviceRoute3);
			Assert("Only one carrier, TranShipment is not required", !route3.IsGateway);
		}

		void AssertLegInfo(ServiceModel.Leg expected, Leg actual)
		{
			AssertEquals(expected.LoadPort.Unloco, actual.OriginPortUnloco);
			AssertEquals(expected.DischargePort.Unloco, actual.DestinationPortUnloco);
			AssertDateTimeEquals(expected.Etd, actual.Departure);
			AssertDateTimeEquals(expected.Eta, actual.Arrival);
			AssertEquals(expected.Voyage.Vessel.ImoNumber, actual.LloydsNumber);
			AssertEquals(expected.Voyage.Vessel.VesselName, actual.VesselName);
			AssertEquals(expected.Voyage.Code, actual.VoyageCode);
			AssertEquals(expected.Voyage.TradeLane.Name, actual.TradeLaneName);
			AssertEquals(expected.Voyage.Operator.Code, actual.CarrierSCAC);
			AssertEquals(expected.Voyage.Operator.Name, actual.OperatorName);
		}

		void AssertDateTimeEquals(DateTime? expected, ZDateTime actual)
		{
			if (expected == null)
			{
				AssertEquals(ZDateTime.Empty, actual);
			}
			else
			{
				AssertEquals(expected, actual);
			}
		}
	}

	#region RouteTestHelper class

	public class RouteTestHelper
	{
		public RouteTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		public readonly BusinessObjectFactory Factory;

		/// <summary>
		/// Populate ServiceModel.Route with provided data and then use it to call SetValues() on new Route
		/// For simplicity of the setup:
		///     Carrier.SCAC => first 4 letters from Carrier.Name
		///     TradeLane => "Tradeline Inc."
		/// </summary>
		public Route CreateRoute(
			string voyageNumber = "",
			string vesselName = "",
			string carrierName = "",
			string loadPort = "",
			DateTime? departureDate = null,
			string dischargePort = "",
			DateTime? arrivalDate = null,
			string imoNumber = "1234567")
		{
			var carrier = new Carrier
			{
				Name = carrierName,
				Code = new ZString(carrierName.Replace(" ", "")).SubstringSafe(0, 4)
			};

			var serviceLeg = new ServiceModel.Leg
			{
				Voyage = new Voyage
				{
					Code = voyageNumber,
					Vessel = new Vessel
					{
						VesselName = vesselName,
						ImoNumber = imoNumber,
					},
					TradeLane = new TradeLane { Name = "Tradeline Inc." },
					Operator = carrier
				},

				LoadPort = new Port { Unloco = loadPort },
				DischargePort = new Port { Unloco = dischargePort },

				Etd = departureDate ?? DateTime.Today,
				Eta = arrivalDate ?? DateTime.Today,
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { serviceLeg }
			};

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			return route;
		}

		public static Carrier CreateCarrier(string carrierName)
			=> new Carrier
			{
				Name = carrierName,
				Code = new ZString(carrierName.Replace(" ", "")).SubstringSafe(0, 4)
			};

		public static ServiceModel.Route CreateServiceRoute(params ServiceModel.Leg[] legs) =>
			CreateServiceRoute(legs[0].Voyage.Operator.Name, legs);

		public static ServiceModel.Route CreateServiceRoute(string carrierName, params ServiceModel.Leg[] legs)
		{
			if (legs == null || legs.Length == 0)
			{
				legs = new[] { CreateServiceLeg() };
			}

			return new ServiceModel.Route { Carrier = CreateCarrier(carrierName), Legs = legs };
		}

		public static ServiceModel.Leg CreateServiceLeg(
			string voyageNumber = "",
			string vesselName = "",
			string carrierName = "",
			string loadPort = "",
			DateTime? departureDate = null,
			string dischargePort = "",
			DateTime? arrivalDate = null,
			string imoNumber = "1234567",
			string legType = GssConstants.SeaLegType,
			decimal co2eKgPerTeu = 0.0m,
			decimal co2eKgPerTonne = 0.0m)
		{
			return new ServiceModel.Leg
			{
				Voyage = new Voyage
				{
					Code = voyageNumber,
					Vessel = new Vessel
					{
						VesselName = vesselName,
						ImoNumber = imoNumber,
					},
					TradeLane = new TradeLane { Name = "Tradeline Inc." },
					Operator = CreateCarrier(carrierName)
				},

				LoadPort = new Port { Unloco = loadPort },
				DischargePort = new Port { Unloco = dischargePort },

				Etd = departureDate,
				Eta = arrivalDate,

				LegType = legType,
				Co2eKgPerTeu = co2eKgPerTeu,
				Co2eKgPerTonne = co2eKgPerTonne,
			};
		}
	}

	#endregion
}
