using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using Event = Enterprise.ZArchitecture.Business.Event;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(Transport))]
	sealed class TransportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPopulate()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Random Vesel";
			vessel.RV_LloydsNumber = "Lloy002";
			vessel.RV_VesselType = "CV";
			vessel.RV_RN_NKCountryOfReg = "BE";
			vessel.RV_RadioCallSign = "OX54F";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var shipment = Factory.New<Business.ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.FillWithValidTestData();
			transport.JW_JX = voyage.Sailings[0].PK;
			transport.JW_Vessel = "Random Vesel";
			transport.JW_VoyageFlight = "FREIGHT001";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.Sailing.JX_DepotReceivalCommences = new ZDateTime(2019, 8, 2);
			transport.Sailing.JX_DepotCutOff = new ZDateTime(2019, 8, 1);
			transport.JW_AdditionalTransportMode = Core.Constants.TransportModes.Road;

			var dataObject = Transport.Create(Context, transport);

			AssertEquals("Random Vesel", dataObject.Vessel.Name);
			AssertEquals("Lloy002", dataObject.Vessel.LloydsIMO);
			AssertEquals("OX54F", dataObject.Vessel.RadioCallSign);
			AssertEquals("CV", dataObject.Vessel.Type.Code);
			AssertEquals("BE", dataObject.Vessel.CountryOfRegistration.Code);
			AssertEquals("FREIGHT001", dataObject.VoyageFlightNumber);
			AssertEquals("TransportMode", "SEA", dataObject.Mode.Code);
			AssertEquals(new ZDateTime(2019, 8, 1), dataObject.LCLCutOff);
			AssertEquals(new ZDateTime(2019, 8, 2), dataObject.LCLReceivalCommences);
			AssertEquals("AdditionalTransportMode", "ROA", dataObject.AdditionalTransportMode.Code);
		}

		public void TestCreateFromUniversalTransportLeg()
		{
			var etd = ZDateTime.Now.AddDays(1);
			var eta = ZDateTime.Now.AddDays(2);
			var atd = ZDateTime.Now.AddDays(3);
			var ata = ZDateTime.Now.AddDays(4);
			var lclCutOff = ZDateTime.Now.AddDays(5);
			var lclReceivalCommences = ZDateTime.Now.AddDays(6);
			var vgmCutOff = ZDateTime.Now.AddDays(7);
			var documentCutOff = ZDateTime.Now.AddDays(8);
			var fclCutOff = ZDateTime.Now.AddDays(9);
			var transportLeg = new TransportLeg()
			{
				LegOrder = 69,
				TransportMode = TransportMode.Sea,
				LegType = LegType.LocalTransport,
				EstimatedDeparture = etd,
				EstimatedArrival = eta,
				ActualDeparture = atd,
				ActualArrival = ata,
				LCLCutOff = lclCutOff,
				LCLReceivalCommences = lclReceivalCommences,
				VGMCutOff = vgmCutOff,
				DocumentCutOff = documentCutOff,
				FCLCutOff = fclCutOff,
				DepartureReference = "squabble",
				ArrivalReference = "squibble",
				VoyageFlightNo = "6969",
				VesselName = "Ye Olde Vessel",
				VesselLloydsIMO = "69420",
				PortOfLoading = new UNLOCO()
				{
					Code = "AUXXX",
					Name = "AUXXX Port Name"
				},
				PortOfDischarge = new UNLOCO()
				{
					Code = "NZXXX",
					Name = "NZXXX Port Name"
				},
				Carrier = new OrganizationAddress()
				{
					CompanyName = "Carrier Company"
				},
				DepartureFrom = new OrganizationAddress()
				{
					CompanyName = "Departure From Company"
				},
				ArrivalAt = new OrganizationAddress()
				{
					CompanyName = "Arrival At Company"
				}
			};

			var transport = Transport.Create(Context, transportLeg);
			AssertEquals(69, transport.LegOrder);
			AssertEquals("SEA", transport.Mode.Code);
			AssertEquals(TransportTypes.Codes.LocalTransport, transport.Type.Code);
			AssertEquals(etd, transport.ETD);
			AssertEquals(eta, transport.ETA);
			AssertEquals(atd, transport.ATD);
			AssertEquals(ata, transport.ATA);
			AssertEquals(lclCutOff, transport.LCLCutOff);
			AssertEquals(lclReceivalCommences, transport.LCLReceivalCommences);
			AssertEquals(vgmCutOff, transport.VGMCutOff);
			AssertEquals(documentCutOff, transport.DocumentCutOff);
			AssertEquals(fclCutOff, transport.FCLCutOff);
			AssertEquals("squabble", transport.DepartureReference);
			AssertEquals("squibble", transport.ArrivalReference);
			AssertEquals("6969", transport.VoyageFlightNumber);
			AssertEquals("Ye Olde Vessel", transport.Vessel.Name);
			AssertEquals("69420", transport.Vessel.LloydsIMO);
			AssertEquals("AUXXX", transport.PortOfLoading.Code);
			AssertEquals("AUXXX Port Name", transport.PortOfLoading.Name);
			AssertEquals("AU", transport.PortOfLoading.Country.Code);
			AssertEquals("NZXXX", transport.PortOfDischarge.Code);
			AssertEquals("NZXXX Port Name", transport.PortOfDischarge.Name);
			AssertEquals("NZ", transport.PortOfDischarge.Country.Code);
			AssertEquals("Carrier Company", transport.Carrier.CompanyName);
			AssertEquals("Departure From Company", transport.DepartureFrom.CompanyName);
			AssertEquals("Arrival At Company", transport.ArrivalAt.CompanyName);
		}

		public void TestTransportOneStopEventHandler()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "UAIEV";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_IsLinked = true;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var dataList = new List<Tuple<Event, string, string, string, string>>
			{
				new Tuple<Event, string, string, string, string>(Events.CutOffDate, AutoJobConsolTransport.Schema.JW_TerminalCutOff, "UAIEV", "", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.CutOffDate, AutoJobConsolTransport.Schema.JW_VGMCutOff, "UAIEV", "VGM", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.CutOffDate, AutoJobConsolTransport.Schema.JW_ReeferCutOff, "UAIEV", "Reefer", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.CutOffDate, AutoJobConsolTransport.Schema.JW_DGCutOff, "UAIEV", "Haz", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.ReceiptCommenced, AutoJobConsolTransport.Schema.JW_TerminalReceivalCommences, "UAIEV", "", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.ReceiptCommenced, AutoJobConsolTransport.Schema.JW_ReeferReceivalCommences, "UAIEV", "Reefer", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.ReceiptCommenced, AutoJobConsolTransport.Schema.JW_DGReceivalCommences, "UAIEV", "Haz", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.CargoAvailable, AutoJobConsolTransport.Schema.JW_TerminalAvailabilityDate, "AUSYD", "", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.StorageCommenced, AutoJobConsolTransport.Schema.JW_TerminalStorageDate, "AUSYD", "", Facilities.Code.Terminal),
				new Tuple<Event, string, string, string, string>(Events.CargoAvailable, AutoJobConsolTransport.Schema.JW_DepotAvailabilityDate, "AUSYD", "", Facilities.Code.Depot)
			};

			foreach (var data in dataList)
			{
				AssertTransportEventsStopProviding(data);
			}

			void AssertTransportEventsStopProviding(Tuple<Event, string, string, string, string> data)
			{
				var eventFacitity = data.Item5;
				var eventDate = 2.DaysAgo();
				var expectedDate = 2.DaysAgo();

				transport.Logs.RemoveAndDeleteAll();
				transport.Logs.CreateOrRecreateEventLog(
					data.Item1,
					EstimateActual.Actual,
					eventDate.ToOffset(),
					"MCLAREN",
					EventReferenceParameters.Codes.Location.AsKeyFor(data.Item3),
					EventReferenceParameters.Codes.Facility.AsKeyFor(eventFacitity),
					EventReferenceParameters.Codes.Type.AsKeyFor(data.Item4));

				AssertEquals($"transport.{data.Item2} should be the same ", expectedDate, transport[data.Item2]);
			}
		}

		public void TestIsEventTypeAllowedForUpdatingDates()
		{
			var testEventType = "";
			AssertEquals(true, OneStopEventHandler.IsEventTypeAllowedForUpdatingDates(testEventType));

			testEventType = "Empty";
			AssertEquals("Empty event type is not allowed to update dates", false, OneStopEventHandler.IsEventTypeAllowedForUpdatingDates(testEventType));

			testEventType = "VGM";
			AssertEquals("VGM event type is not allowed to update dates", false, OneStopEventHandler.IsEventTypeAllowedForUpdatingDates(testEventType));

			testEventType = "Reefer";
			AssertEquals("Reefer event type is not allowed to update dates", false, OneStopEventHandler.IsEventTypeAllowedForUpdatingDates(testEventType));

			testEventType = "Haz";
			AssertEquals("Haz event type is not allowed to update dates", false, OneStopEventHandler.IsEventTypeAllowedForUpdatingDates(testEventType));
		}

		#region Implementation

		IContext Context => context ?? (context = new CommonContext(Factory));
		IContext context;

		protected override BusinessObject GetNewBusinessObject()
		{
			var transport = GetNewTransportUsingReflectionAsCtorIsIntentionallyPrivate();
			return transport;
		}

		public static Transport GetNewTransportUsingReflectionAsCtorIsIntentionallyPrivate()
		{
			var transportCtor = typeof(Transport).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null);
			return (Transport)transportCtor.Invoke(new object[] { null });
		}

		#endregion
	}
}
