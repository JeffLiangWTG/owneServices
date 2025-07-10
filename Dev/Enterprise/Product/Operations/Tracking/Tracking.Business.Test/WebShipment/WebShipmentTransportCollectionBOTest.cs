using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class WebShipmentTransportCollectionBOTest : TestCaseWithFactory
	{
		public void TestWhenIsLinked()
		{
			VoyageOrigin testVoyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
			testVoyageOrigin.JA_E_DEP = ZDateTime.Now.AddDays(-3);
			testVoyageOrigin.JA_A_DEP = testVoyageOrigin.JA_E_DEP;
			VoyageDestination testVoyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
			testVoyageDestination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			testVoyageDestination.JB_A_ARV = testVoyageDestination.JB_E_ARV;

			JobSailing testSailing = Factory.NewWithValidTestData<JobSailing>();
			testSailing.JX_JA = testVoyageOrigin.PK;
			testSailing.JX_JB = testVoyageDestination.PK;

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_JX = testSailing.PK;

			TrackingConsol consol = shipment.Consols.AddNew();
			AssertEquals("Should be one consol", 1, shipment.Consols.Count);
			AssertEquals("Should be one transport on consol", 1, shipment.Consols[0].Transports.Count);

			Transport testTransport = shipment.Consols[0].Transports[0];
			testTransport.JW_VoyageFlight = "1";
			testTransport.JW_ETD = ZDateTime.Now.AddDays(-2);
			testTransport.JW_ATD = testTransport.JW_ETD;
			testTransport.JW_ETA = ZDateTime.Now.AddDays(2);
			testTransport.JW_ATA = testTransport.JW_ETA;
			testTransport.JW_JX = testSailing.PK;
			testTransport.JW_IsLinked = true;

			Transport testTransport2 = shipment.Consols[0].Transports.AddNew();
			testTransport2.JW_VoyageFlight = "2";
			testTransport2.JW_ETD = ZDateTime.Now.AddDays(-1);
			testTransport2.JW_ATD = testTransport2.JW_ETD;
			testTransport2.JW_ETA = ZDateTime.Now.AddDays(3);
			testTransport2.JW_ATA = testTransport2.JW_ETA;
			testTransport2.JW_IsLinked = false;

			Factory.Save();
			WebShipmentTransportCollection collection = new WebShipmentTransportCollection(Factory, shipment.PK);
			collection.Load();

			AssertEquals("IsLinked -- ETA should be taken from sailing", testVoyageDestination.JB_E_ARV.ToShortDateString(), collection[0].JW_ETA.ToShortDateString());
			AssertEquals("IsLinked -- ETD should be taken from sailing", testVoyageOrigin.JA_E_DEP.ToShortDateString(), collection[0].JW_ETD.ToShortDateString());
			AssertEquals("IsLinked -- ATA should be taken from sailing", testVoyageDestination.JB_A_ARV.ToShortDateString(), collection[0].JW_ATA.ToShortDateString());
			AssertEquals("IsLinked -- ATD should be taken from sailing", testVoyageOrigin.JA_E_DEP.ToShortDateString(), collection[0].JW_ATD.ToShortDateString());

			AssertEquals("Not linked -- ETA should be taken from the transport", testTransport2.JW_ETA.ToShortDateString(), collection[1].JW_ETA.ToShortDateString());
			AssertEquals("Not linked -- ETD should be taken from the transport", testTransport2.JW_ETD.ToShortDateString(), collection[1].JW_ETD.ToShortDateString());
			AssertEquals("Not linked -- ATA should be taken from the transport", testTransport2.JW_ATA.ToShortDateString(), collection[1].JW_ATA.ToShortDateString());
			AssertEquals("Not linked -- ATD should be taken from the transport", testTransport2.JW_ATD.ToShortDateString(), collection[1].JW_ATD.ToShortDateString());
		}

		public void TestLoad()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();

			TrackingConsol consol = shipment.Consols.AddNew();
			AssertEquals("Should be one consol", 1, shipment.Consols.Count);
			AssertEquals("Should be one transport on consol", 1, shipment.Consols[0].Transports.Count);

			Transport transport1 = shipment.Consols[0].Transports[0];
			transport1.JW_VoyageFlight = "1";
			transport1.JW_ETD = ZDateTime.Now.AddDays(-3);
			transport1.JW_ATD = transport1.JW_ETD;

			Transport transport2 = shipment.Consols[0].Transports.AddNew();
			transport2.JW_VoyageFlight = "2";
			transport2.JW_ETD = ZDateTime.Now.AddDays(-2);

			Transport transport3 = shipment.Consols[0].Transports.AddNew();
			transport3.JW_VoyageFlight = "3";
			transport3.JW_ETD = ZDateTime.Now.AddDays(-1);

			Transport transport4 = shipment.Transports.AddNew();
			transport4.JW_VoyageFlight = "4";
			transport4.JW_ETD = ZDateTime.Now.AddDays(-4);

			Transport transport5 = shipment.Transports.AddNew();
			transport5.JW_VoyageFlight = "5";

			JobVoyage testJobVoyage = Factory.NewWithValidTestData<JobVoyage>();
			testJobVoyage.JV_VoyageFlight = "6";
			VoyageOrigin testVoyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
			testVoyageOrigin.JA_E_DEP = ZDateTime.Now.AddDays(-5);
			testVoyageOrigin.JA_A_DEP = testVoyageOrigin.JA_E_DEP;
			testVoyageOrigin.JA_JV = testJobVoyage.PK;
			VoyageDestination testVoyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
			testVoyageDestination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			testVoyageDestination.JB_A_ARV = testVoyageDestination.JB_E_ARV;

			JobSailing testSailing = Factory.NewWithValidTestData<JobSailing>();
			testSailing.JX_JA = testVoyageOrigin.PK;
			testSailing.JX_JB = testVoyageDestination.PK;

			Transport transport6 = shipment.Transports.AddNew();
			transport6.JW_JX = testSailing.PK;
			transport6.JW_IsLinked = true;
			transport6.JW_VoyageFlight = "6";

			Factory.Save();

			WebShipmentTransportCollection collection = new WebShipmentTransportCollection(Factory, shipment.PK);
			AssertEquals("Should be no Transports", 0, collection.Count);

			collection.Load();
			AssertEquals("Should be six Transports", 6, collection.Count);
			AssertEquals("Default Sort", "6", collection[0].JV_VoyageFlight);
			AssertEquals("Default Sort", "4", collection[1].JV_VoyageFlight);
			AssertEquals("Default Sort", "1", collection[2].JV_VoyageFlight);
			AssertEquals("Default Sort", "2", collection[3].JV_VoyageFlight);
			AssertEquals("Default Sort", "3", collection[4].JV_VoyageFlight);
			AssertEquals("Default Sort", "5", collection[5].JV_VoyageFlight);
		}
	}
}
