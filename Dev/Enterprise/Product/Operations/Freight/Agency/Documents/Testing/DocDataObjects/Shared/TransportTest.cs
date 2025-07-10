using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(Transport))]
	class TransportTest : NonPersistentBusinessObjectTestCase
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

			var billOfLading = Factory.New<Business.BillOfLading>();
			var transport = billOfLading.Transports.AddNew();
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
