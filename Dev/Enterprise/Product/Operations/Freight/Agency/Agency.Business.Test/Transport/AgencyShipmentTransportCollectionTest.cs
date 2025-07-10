using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(AgencyShipmentTransportCollection))]
	internal class AgencyShipmentTransportCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMainSailingForExistingShipment()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			Factory.Save();
			AssertEquals(1, shipment.Transports.Count);
			shipment.Transports[0].Delete();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedShipment = newFactory.Load<AgencyShipment>(shipment.PK);
			AssertEquals(0, loadedShipment.Transports.Count);
		}

		public void TestMainSailingSailingRemoved()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			AssertEquals(1, shipment.Transports.Count);
			shipment.JS_JX = ZGuid.Empty;
			AssertEquals(0, shipment.Transports.Count);
		}

		public void TestDeletedMainSailingDoesNotThrowException()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sailing1 = voyage.Sailings[0];
			var sailing2 = voyage.Sailings[1];
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing1.PK;
			AssertEquals(1, shipment.Transports.Count);
			AssertEquals(sailing1.PK, shipment.Transports[0].JW_JX);
			shipment.Transports[0].Delete();
			shipment.JS_JX = sailing2.PK;
			AssertEquals(1, shipment.Transports.Count);
			AssertEquals(sailing2.PK, shipment.Transports[0].JW_JX);
		}

		public void TestIgnoreSettingSailingOnDataImport()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sailing1 = voyage.Sailings[0];
			var sailing2 = voyage.Sailings[1];
			var shipment = Factory.New<AgencyShipment>();
			AssertEquals("prerequisite", 0, shipment.Transports.Count);
			((ISupportDataImporting)shipment).IsImportingData = true;
			shipment.JS_JX = sailing1.PK;
			AssertEquals(0, shipment.Transports.Count);
			((ISupportDataImporting)shipment).IsImportingData = false;
			shipment.JS_JX = sailing2.PK;
			AssertEquals(1, shipment.Transports.Count);
			AssertEquals(sailing2.PK, shipment.Transports[0].JW_JX);
		}

		public void TestMainSailingForNewSailing()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			JobSailing sailing1 = voyage1.Sailings[0];
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUCNS";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			JobSailing sailing2 = voyage2.Sailings[0];
			JobVoyage transportVoyage = Factory.New<JobVoyage>();
			transportVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			transportVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			Factory.Save();
			JobSailing transportSailing = transportVoyage.Sailings[0];
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing1.PK;
			AssertSailings("sailings1", shipment, sailing1);
			AssertEquals(1, shipment.Transports.Count);
			Transport transport = shipment.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = transportSailing.PK;
			AssertSailings("sailing1 + transport sailing", shipment, sailing1, transportSailing);
			shipment.JS_JX = sailing2.PK;
			AssertSailings("sailing2 + transport sailing", shipment, sailing2, transportSailing);
			shipment.JS_JX = ZGuid.Empty;
			AssertSailings("transport sailing", shipment, transportSailing);
			transport.Delete();
			AssertSailings("no sailings", shipment);
			shipment.JS_JX = sailing1.PK;
			AssertSailings("back to sailing1", shipment, sailing1);
		}

		public void TestSuspendSettingHasChangesOnLoad()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			var sailing = voyage.Sailings[0];
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			Factory.Save();
			var transportCollection = new AgencyShipmentTransportCollection(shipment);
			transportCollection.Load();
			AssertEquals("shipment has main transport", 1, transportCollection.Count);
			AssertEquals("main transport does not HasChanges", false, transportCollection[0].HasChanges);
		}

		public void TestSuspendCountChangedOnLoad()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			var sailing = voyage.Sailings[0];
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			for (var index = 0; index < 10; index++)
			{
				shipment.Transports.AddNew();
			}
			Factory.Save();
			var countChanged = 0;
			var transportCollection = new AgencyShipmentTransportCollection(shipment);
			transportCollection.CountChanged += (sender, e) => countChanged++;
			transportCollection.Load();
			AssertEquals("shipment has main transport and 10 manually created transports", 11, transportCollection.Count);
			AssertEquals("CountChanged event should only be triggered once after the collection is loaded", 1, countChanged);
		}

		public void TestDoNotSuspendSettingHasChangesOnJS_JXChanged()
		{
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			var sailing1 = voyage1.Sailings[0];
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			var sailing2 = voyage2.Sailings[0];
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing1.PK;
			Factory.Save();
			var transportCollection = new AgencyShipmentTransportCollection(shipment);
			transportCollection.Load();
			AssertEquals("shipment has main transport", 1, transportCollection.Count);
			shipment.JS_JX = sailing2.PK;
			AssertEquals("main transport HasChanges", true, transportCollection[0].HasChanges);
		}

		public void TestAgencyShipmentShouldNotHasChangesAfterLoadingWhenNoActualChangesWerePerformed()
		{
			var transportVoyage = Factory.New<JobVoyage>();
			var origin = transportVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_E_DEP = ZDateTime.Today;

			var destination = transportVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(1);

			Factory.Save();
			var transportSailing = transportVoyage.Sailings[0];
			var shipment = Factory.New<AgencyShipment>();
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Road;
			transport1.JW_TransportType = Constants.TransportPlanningType.Other;
			shipment.JS_JX = transportSailing.PK;
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(-2);
			shipment.JS_E_ARV = ZDateTime.Today.AddDays(-1);

			Factory.Save();
			var shipmentInAnotherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<AgencyShipment>(shipment.PK);
			Assert("Shipment is not be changed before loading Transports.", !shipmentInAnotherFactory.HasChanges);
			var originalJS_JX = shipmentInAnotherFactory.JS_JX;
			var collectionInAnotherFactory = new AgencyShipmentTransportCollection(shipmentInAnotherFactory);
			collectionInAnotherFactory.Load();
			CombineAssertions(() =>
			{
				AssertEquals(@"This assertion is a pre-condition of next assertion.
When this one is failed it means some NECESSARY defaulting logic has changed shipment's JS_JX and we cannot ignore it.
Then this test should be modified to provide valid sailing data so this assertion passes again.", originalJS_JX, shipmentInAnotherFactory.JS_JX);
				Assert("Shipment should not be changed right after loading Transports.", !shipmentInAnotherFactory.HasChanges);
			});
		}

		#region Implementation
		void AssertSailings(string message, AgencyShipment shipment, params JobSailing[] sailings)
		{
			AssertContainsExactElementsInAnyOrder(message, (s) => s == null ? null : string.Format("{0} -> {1}", s.JX_JA_RL_NKPortOfLoading, s.JX_JB_RL_NKPortOfDischarge), sailings, Array.ConvertAll(shipment.Transports.ToArray<Transport>(), (t) => t.Sailing));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AgencyShipmentTransportCollection(Factory.New<AgencyShipment>());
		}
		#endregion
	}
}
