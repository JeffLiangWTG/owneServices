using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RoutingCollectionTest : BaseFreightTest
	{
		public void TestNonPersistentReadOnly()
		{
			TransportCollection mainCollection = Factory.New<CommonShipment>().Transports;
			Transport transport1 = mainCollection.AddNew();
			Transport transport2 = mainCollection.AddNew();
			transport2.MakeNonPersistent();

			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)mainCollection.Master);

			AssertEquals("Transport1", false, transport1.ReadOnly);
			AssertEquals("Transport2", true, transport2.ReadOnly);
		}

		public void TestImportExportArrivalDepartureTransport()
		{
			ZDateTime now = ZDateTime.Now;

			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol1 = shipment.Consols.AddNew();
			CommonConsol consol2 = shipment.Consols.AddNew();

			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = OverseasPort;
			transport1.JW_RL_NKDiscPort = OverseasPort2;
			transport1.JW_ETD = now.AddDays(1);

			Transport transport2 = consol1.Transports[0];
			transport2.JW_RL_NKLoadPort = OverseasPort2;
			transport2.JW_RL_NKDiscPort = HomePort;
			transport2.JW_ETD = now.AddDays(2);

			Transport transport3 = shipment.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = HomePort;
			transport3.JW_RL_NKDiscPort = AlternateHomePort;
			transport3.JW_ETA = now.AddDays(3);

			Transport transport4 = consol2.Transports[0];
			transport4.JW_RL_NKLoadPort = AlternateHomePort;
			transport4.JW_RL_NKDiscPort = OverseasPort3;
			transport4.JW_ETA = now.AddDays(4);

			Transport transport5 = shipment.Transports.AddNew();
			transport5.JW_RL_NKLoadPort = OverseasPort3;
			transport5.JW_RL_NKDiscPort = OverseasPort4;
			transport5.JW_ATD = now.AddDays(5);

			RoutingCollection collection = new ShipmentRoutingCollection(shipment);
			AssertEquals("DepartureTransport", transport1, collection.DepartureTransport);
			AssertEquals("ImportTransport", transport2, collection.ImportTransport);
			AssertEquals("ExportTransport", transport4, collection.ExportTransport);
			AssertEquals("ArrivalTransport", transport5, collection.ArrivalTransport);

			string local = HomePort.Left(2);
			AssertEquals(transport4, collection.LastLegMatching((t) => t.JW_RL_NKLoadPort.StartsWith(local)));
			AssertEquals(transport2, collection.FirstLegMatching((t) => t.JW_RL_NKDiscPort.StartsWith(local)));
			AssertEquals(transport1, collection.FirstLeg);
			AssertEquals(transport5, collection.LastLeg);
		}

		public void TestRunSetDefaultValuesFromMainCollection()
		{
			IRoutingSupport support = Factory.New<CommonConsol>();
			Transport transport = support.TransportsIncludingRelated.AddNew();

			AssertEquals("the set default values method on the consols tarnsports collection sets JW_LegOrder", (byte)2, transport.JW_LegOrder);
		}

		public void TestBelongsToMainCollection()
		{
			TransportCollection mainCollection = Factory.New<CommonShipment>().Transports;
			TransportCollection otherCollection = Factory.New<CommonShipment>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)mainCollection.Master);
			collection.MonitorCollection(otherCollection);

			Transport transport1 = mainCollection.AddNew();
			Transport transport2 = otherCollection.AddNew();

			AssertEquals("transport1 belongs to the main collection", true, collection.BelongsToMainCollection(transport1));
			AssertEquals("transport2 belongs to the other collection", false, collection.BelongsToMainCollection(transport2));
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestDeleteChecking_MainTransportCollection()
		{
			TransportCollection mainTransportCollection = Factory.New<CommonShipment>().Transports;
			TransportCollection otherTransportCollection = Factory.New<CommonShipment>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)mainTransportCollection.Master);
			collection.MonitorCollection(otherTransportCollection);

			Transport shipmentTransport = mainTransportCollection.AddNew();
			collection.RemoveAndDelete(shipmentTransport);
		}

		[NUnit.Framework.ExpectException(typeof(CannotDeleteException))]
		public void TestDeleteChecking_OtherTransportCollection()
		{
			TransportCollection mainTransportCollection = Factory.New<CommonShipment>().Transports;
			TransportCollection otherTransportCollection = Factory.New<CommonShipment>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)mainTransportCollection.Master);
			collection.MonitorCollection(otherTransportCollection);

			Transport shipmentTransport = otherTransportCollection.AddNew();
			collection.RemoveAndDelete(shipmentTransport);
		}

		public void TestTransportsNotFromTheMainTransportShouldBeReadOnly()
		{
			TransportCollection mainTransportCollection = Factory.New<CommonShipment>().Transports;
			TransportCollection otherTransportCollection = Factory.New<CommonShipment>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)mainTransportCollection.Master);
			collection.MonitorCollection(otherTransportCollection);

			AssertEquals("plugin behaviour should be disabled by default", false, collection.PluginBehaviourEnabled);

			Transport transport1 = mainTransportCollection.AddNew();
			Transport transport2 = otherTransportCollection.AddNew();

			AssertEquals(2, collection.Count);
			AssertEquals("transport1 should not be read only since plugin behaviour is not yet enabled", false, transport1.ReadOnly);
			AssertEquals("transport2 should not be read only since plugin behaviour is not yet enabled", false, transport2.ReadOnly);

			collection.PluginBehaviourEnabled = true;
			Transport transport3 = mainTransportCollection.AddNew();
			Transport transport4 = otherTransportCollection.AddNew();

			AssertEquals(4, collection.Count);
			AssertEquals("transport1 should not be read only since it belongs to mainTransportCollection", false, transport1.ReadOnly);
			AssertEquals("transport2 should be read only since it belongs to otherTransportCollection", true, transport2.ReadOnly);
			AssertEquals("transport3 should not be read only since it belongs to mainTransportCollection", false, transport3.ReadOnly);
			AssertEquals("transport4 should be read only since it belongs to otherTransportCollection", true, transport4.ReadOnly);
		}

		public void TestAllTransportsShouldBeReadOnlyWhenCollectionIsReadOnly()
		{
			TransportCollection mainTransportCollection = Factory.New<CommonShipment>().Transports;
			TransportCollection otherTransportCollection = Factory.New<CommonShipment>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)mainTransportCollection.Master);
			collection.MonitorCollection(otherTransportCollection);

			collection.SetReadOnlyIncludingChildren(true);
			AssertEquals("Precondition: collection is readonly", true, collection.ReadOnly);

			Transport transport1 = mainTransportCollection.AddNew();
			Transport transport2 = mainTransportCollection.AddNew();
			Transport transport3 = otherTransportCollection.AddNew();
			Transport transport4 = otherTransportCollection.AddNew();

			collection.PluginBehaviourEnabled = false;
			foreach (Transport transport in collection)
			{
				AssertEquals("Transport should be readonly due to collection state", true, transport.ReadOnly);
			}

			collection.PluginBehaviourEnabled = true;
			foreach (Transport transport in collection)
			{
				AssertEquals("Transport should be readonly due to collection state", true, transport.ReadOnly);
			}
		}

		public void TestAddNew()
		{
			TransportCollection transports = Factory.New<CommonConsol>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)transports.Master);
			IBindingList bindingList = collection;

			Transport transport = collection.AddNew();
			AssertEquals("Should be a ConsolTransport", transports.TypeOfElements, transport.GetType());
			AssertEquals("Parent Type", Core.Constants.TransportParentTypes.Consol, transport.JW_ParentType);
			AssertEquals("Parent", transports.Master.PK, transport.JW_ParentGUID);
			AssertEquals("Consol.Transports has transport", true, transports.Contains(transport));
		}

		public void TestBindingAddNew()
		{
			TransportCollection transports = Factory.New<CommonConsol>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)transports.Master);
			IBindingList bindingList = collection;

			Transport transport1 = (Transport)bindingList.AddNew();
			AssertEquals("Should be a ConsolTransport", transports.TypeOfElements, transport1.GetType());
			AssertEquals("Parent Type", Core.Constants.TransportParentTypes.Consol, transport1.JW_ParentType);
			AssertEquals("Parent", transports.Master.PK, transport1.JW_ParentGUID);

			((ICancelAddNew)collection).CancelNew(bindingList.Count - 1);
			AssertEquals("Consol.Transports should not have canceled transports", false, transports.Contains(transport1));

			Transport transport2 = (Transport)bindingList.AddNew();
			transport2.HasChanges = true;
			((ICancelAddNew)collection).EndNew(bindingList.Count - 1);
			AssertEquals("Consol.Transports should have accepted transports", true, transports.Contains(transport2));
		}

		public void TestCollectionMonitoring()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = Factory.New<CommonConsol>();

			AssertEquals("precondition: expecting CommonShipment to have no transports", 0, shipment.Transports.Count);
			AssertEquals("precondition: expecting consol to have 1 transport", 1, consol.Transports.Count);

			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)shipment.Transports.Master);

			AssertEquals("CommonShipment has no transports so collection be empty", 0, collection.Count);

			Transport shipmentTransport1 = shipment.Transports.AddNew();
			AssertEquals("count", 1, collection.Count);
			AssertEquals("CommonShipment now has a transport, collection should now also have the transport", true, collection.Contains(shipmentTransport1));

			Transport shipmentTransport2 = shipment.Transports.AddNew();
			AssertEquals("count", 2, collection.Count);
			AssertEquals("collection should have shipmentTransport2", true, collection.Contains(shipmentTransport2));

			shipment.Transports.Remove(shipmentTransport1);
			AssertEquals("count", 1, collection.Count);
			AssertEquals("collection should not have shipmentTransport1 anymore", false, collection.Contains(shipmentTransport1));

			Transport consolTransport1 = consol.Transports[0];
			collection.MonitorCollection(consol.Transports);
			AssertEquals("count", 2, collection.Count);
			AssertEquals("collection should now also have the consols transports", true, collection.Contains(consolTransport1));

			collection.UnMonitorCollection(shipment.Transports);
			AssertEquals("count", 1, collection.Count);
			AssertEquals("collection should not include CommonShipment transports anymore", false, collection.Contains(shipmentTransport2));
		}

		public void TestMonitoredCollectionsChangedEvent()
		{
			TransportCollection shipmentTransports = Factory.New<CommonShipment>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest(Factory.New<CommonShipment>());

			int eventCount = 0;
			MonitoredCollectionsChangedEventArgs lastArgs = null;
			object lastSender = null;

			collection.MonitoredCollectionsChanged += delegate(object sender, MonitoredCollectionsChangedEventArgs e)
			{
				eventCount++;
				lastArgs = e;
				lastSender = sender;
			};

			AssertEquals("should not have fired the event yet", 0, eventCount);

			collection.MonitorCollection(shipmentTransports);
			AssertEquals("should have fired the event once", 1, eventCount);
			AssertEquals("sender", collection, lastSender);
			AssertEquals("e.Collection", shipmentTransports, lastArgs.Collection);
			AssertEquals("e.CollectionAdded", true, lastArgs.CollectionAdded);
			AssertEquals("e.CollectionRemoved", false, lastArgs.CollectionRemoved);

			collection.UnMonitorCollection(shipmentTransports);
			AssertEquals("should have fired the event once more", 2, eventCount);
			AssertEquals("sender", collection, lastSender);
			AssertEquals("e.Collection", shipmentTransports, lastArgs.Collection);
			AssertEquals("e.CollectionAdded", false, lastArgs.CollectionAdded);
			AssertEquals("e.CollectionRemoved", true, lastArgs.CollectionRemoved);
		}

		public void TestIsAnyDischargeInCountry()
		{
			TransportCollection mainTransportCollection = Factory.New<CommonShipment>().Transports;
			TransportCollection otherTransportCollection = Factory.New<CommonShipment>().Transports;
			RoutingCollectionForTest collection = new RoutingCollectionForTest((ITransportParent)mainTransportCollection.Master);
			collection.MonitorCollection(otherTransportCollection);
			Transport transport1 = mainTransportCollection.AddNew();
			transport1.JW_RL_NKDiscPort = "CATOR";
			Transport transport2 = otherTransportCollection.AddNew();
			transport2.JW_RL_NKDiscPort = "USNYK";
			Transport transport3 = mainTransportCollection.AddNew();
			transport3.JW_RL_NKDiscPort = "AUSYD";
			Transport transport4 = otherTransportCollection.AddNew();
			transport4.JW_RL_NKDiscPort = "NZAKL";
			Assert(collection.IsAnyDischargeInCountry("CA"));
			Assert(collection.IsAnyDischargeInCountry("US"));
			Assert(collection.IsAnyDischargeInCountry("AU"));
			Assert(collection.IsAnyDischargeInCountry("NZ"));
			Assert(!collection.IsAnyDischargeInCountry("GB"));
		}

		public void TestHasIcs2ZoneDischarge()
		{
			SetupNorthernIrelandZone();

			List<string> ics2Zones = new List<string> { "GBBEL", "NOABE", "CHARF", "DEHAM" };
			foreach (var ics2Zone in ics2Zones)
			{
				var transportCollection = Factory.New<CommonShipment>().Transports;
				var collection = new RoutingCollectionForTest((ITransportParent)transportCollection.Master);

				var transport1 = transportCollection.AddNew();
				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKDiscPort = "CATOR";
				var transport2 = transportCollection.AddNew();
				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_RL_NKDiscPort = ics2Zone;
				var transport3 = transportCollection.AddNew();
				transport3.JW_TransportMode = Constants.TransportModes.Air;
				transport3.JW_RL_NKDiscPort = "AUSYD";

				Assert(collection.HasIcs2ZoneAirDischarge);

				transport2.JW_RL_NKDiscPort = "USLAX";
				Assert(!collection.HasIcs2ZoneAirDischarge);
			}

			void SetupNorthernIrelandZone()
			{
				var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
				if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
				{
					var ni = Factory.New<RefCountryStates>();
					belfast.RL_RW = ni.PK;
					ni.RW_RegionName = "NORTHERN IRELAND";
				}
			}
		}

		public void TestHasLoadPortOutsideOfIcs2MemberCountry()
		{
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var collection = new RoutingCollectionForTest((ITransportParent)transportCollection.Master);

			var transport1 = transportCollection.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "ITSPE";
			Assert(!collection.HasLoadPortOutsideOfIcs2Zone);

			var transport2 = transportCollection.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "DEHAM";
			Assert(!collection.HasLoadPortOutsideOfIcs2Zone);

			var transport3 = transportCollection.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			Assert(collection.HasLoadPortOutsideOfIcs2Zone);
		}

		public void TestIsAirImportToICS2Member()
		{
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var collection = new RoutingCollectionForTest((ITransportParent)transportCollection.Master);

			var transport1 = transportCollection.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "ITSPE";
			Assert(!collection.IsAirImportOrTransitToICS2Zone);

			var transport2 = transportCollection.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "DEHAM";
			Assert(!collection.IsAirImportOrTransitToICS2Zone);

			var transport3 = transportCollection.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			Assert(!collection.IsAirImportOrTransitToICS2Zone);

			var transport4 = transportCollection.AddNew();
			transport4.JW_TransportMode = Constants.TransportModes.Air;
			transport4.JW_RL_NKLoadPort = "USLAX";
			transport4.JW_RL_NKDiscPort = "DEHAM";
			Assert(collection.IsAirImportOrTransitToICS2Zone);
		}

		#region RouteSets Tests

		public void TestCreateRouteSets_CarrierReferenceIsSameForAllLegs()
		{
			var legsData = new TransportLegDataForTest(new[] { "USCHI", "USLAX", "AUMEL", "AUSYD" },
													   new[] { "12345" },
													   null,
													   Factory);

			var collection = CreateTestTransportCollection(legsData);

			AssertEquals("Should be one RouteSet", 1, collection.RouteSets.Count);
			var routeSet = collection.RouteSets.First();
			AssertEquals("USCHI", routeSet.Origin.Code);
			AssertEquals("AUSYD", routeSet.Destination.Code);
			AssertEquals(1, routeSet.RouteSetNumber);
			AssertEquals("Via is not defined", null, routeSet.Via);
		}

		public void TestCreateRouteSets_CarrierReferencesAreEmptyForAllLegs()
		{
			var legsData = new TransportLegDataForTest(new[] { "USCHI", "USLAX", "AUMEL", "AUSYD" },
													   null,
													   null,
													   Factory);

			var collection = CreateTestTransportCollection(legsData);

			AssertEquals("Should be one RouteSet", 1, collection.RouteSets.Count);
			var routeSet = collection.RouteSets.First();
			AssertEquals("USCHI", routeSet.Origin.Code);
			AssertEquals("AUSYD", routeSet.Destination.Code);
			AssertEquals(1, routeSet.RouteSetNumber);
			AssertEquals("Via is not defined", null, routeSet.Via);
		}

		public void TestCreateRouteSets_DifferentCarrierReferences_InCorrectOrder()
		{
			var legsData = new TransportLegDataForTest(new[] { "USCHI", "USLAX", "AUMEL", "AUSYD" },
													   new[] { "12345", "12345", "7221" },
													   null,
													   Factory);

			var collection = CreateTestTransportCollection(legsData);

			AssertEquals("Should be two RouteSets", 2, collection.RouteSets.Count);
			var routeSet = collection.RouteSets[0];
			AssertEquals("USCHI", routeSet.Origin.Code);
			AssertEquals("AUMEL", routeSet.Destination.Code);
			AssertEquals(1, routeSet.RouteSetNumber);
			AssertEquals("USLAX", routeSet.Via.Code);

			var routeSet2 = collection.RouteSets[1];
			AssertEquals("AUMEL", routeSet2.Origin.Code);
			AssertEquals("AUSYD", routeSet2.Destination.Code);
			AssertEquals(2, routeSet2.RouteSetNumber);
			AssertEquals(null, routeSet2.Via);
		}

		public void TestCreateRouteSets_DifferentCarrierReferences_WithEmptyValues()
		{
			var legsData = new TransportLegDataForTest(new[] { "USCHI", "USLAX", "AUMEL", "AUSYD" },
													   new[] { "", "", "7221" },
													   null,
													   Factory);

			var collection = CreateTestTransportCollection(legsData);

			AssertEquals("Should be two RouteSets", 2, collection.RouteSets.Count);
			var routeSet = collection.RouteSets[0];
			AssertEquals("USCHI", routeSet.Origin.Code);
			AssertEquals("AUMEL", routeSet.Destination.Code);
			AssertEquals(1, routeSet.RouteSetNumber);
			AssertEquals("USLAX", routeSet.Via.Code);

			var routeSet2 = collection.RouteSets[1];
			AssertEquals("AUMEL", routeSet2.Origin.Code);
			AssertEquals("AUSYD", routeSet2.Destination.Code);
			AssertEquals(2, routeSet2.RouteSetNumber);
			AssertEquals(null, routeSet2.Via);
		}

		public void TestCreateRouteSets_CarrierReferenceInWrongOrder()
		{
			var legsData = new TransportLegDataForTest(new[] { "FRCDG", "FRPAR", "DEBRE", "AUMEL", "AUSYD" },
												   new[] { "1234", "1234", "5678", "5678" },
												   null,
												   Factory);

			var collection = CreateTestTransportCollection(legsData);
			AssertEquals("Should be 2 RouteSets", 2, collection.RouteSets.Count);

			legsData = new TransportLegDataForTest(new[] { "FRCDG", "FRPAR", "DEBRE", "AUMEL", "AUSYD" },
														   new[] { "1234", "5678", "1234", "5678" },
														   null,
														   Factory);
			collection = CreateTestTransportCollection(legsData);

			AssertEquals("Should be 4 RouteSets", 4, collection.RouteSets.Count);

			legsData = new TransportLegDataForTest(new[] { "FRCDG", "FRPAR", "DEBRE", "AUMEL", "AUSYD" },
														   new[] { "1234", "5678", "5678", "1234" },
														   null,
														   Factory);
			collection = CreateTestTransportCollection(legsData);

			AssertEquals("Should be 3 RouteSets, for the first leg, then two legs in between, and the last leg", 3, collection.RouteSets.Count);
		}

		public void TestCreateRouteSets_MatchByCountry()
		{
			var legsData = new TransportLegDataForTest(new[] { "AUMEL", "AUSYD", "AUBNE", "AUPER", "AUFRE" },
															   null,
															   null,
															   Factory);

			var collection = CreateTestTransportCollection(legsData);
			AssertEquals("Should be 1 RouteSet", 1, collection.RouteSets.Count);
			var routeSet = collection.RouteSets[0];
			AssertEquals("AUMEL", routeSet.Origin.Code);
			AssertEquals("AUFRE", routeSet.Destination.Code);
			AssertEquals(null, routeSet.Via);
		}

		public void TestCreateRouteSets_UnorderedLegs()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "FRCDG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transports = consol.Transports;

			var t2 = transports.AddNew();
			t2.JW_RL_NKLoadPort = "DEBRE";
			t2.JW_RL_NKDiscPort = "AUMEL";
			t2.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			var t3 = transports.AddNew();
			t3.JW_RL_NKLoadPort = "FRPAR";
			t3.JW_RL_NKDiscPort = "DEBRE";
			t3.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			var t4 = transports.AddNew();
			t4.JW_RL_NKLoadPort = "AUMEL";
			t4.JW_RL_NKDiscPort = "AUSYD";
			t4.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			transports[0].JW_RL_NKDiscPort = "FRPAR";
			transports[0].JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			AssertEquals("Prereq: order of transports is incorrect", (byte)1, transports[0].JW_LegOrder);
			AssertEquals("Prereq: order of transports is incorrect", (byte)2, t2.JW_LegOrder);
			AssertEquals("Prereq: order of transports is incorrect", (byte)3, t3.JW_LegOrder);
			AssertEquals("Prereq: order of transports is incorrect", (byte)4, t4.JW_LegOrder);

			var collection = (consol as IRoutingSupport).TransportsIncludingRelated;

			AssertEquals("Should be 1 RouteSets", 1, collection.RouteSets.Count);
			var routeSet = collection.RouteSets[0];
			AssertEquals(1, routeSet.RouteSetNumber);
			AssertEquals("FRCDG", routeSet.Origin.Code);
			AssertEquals("DEBRE", routeSet.Via.Code);
			AssertEquals("AUSYD", routeSet.Destination.Code);
		}

		public void TestCreateRouteSets_IncompleteLegs()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "FRCDG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transports = consol.Transports;

			var t2 = transports.AddNew();
			t2.JW_RL_NKLoadPort = "";
			t2.JW_RL_NKDiscPort = "AUMEL";
			t2.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			var t3 = transports.AddNew();
			t3.JW_RL_NKLoadPort = "FRPAR";
			t3.JW_RL_NKDiscPort = "DEBRE";
			t3.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			var t4 = transports.AddNew();
			t4.JW_RL_NKLoadPort = "AUMEL";
			t4.JW_RL_NKDiscPort = "";
			t4.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			transports[0].JW_RL_NKDiscPort = "FRPAR";
			transports[0].JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(Carrier1.PK);

			var collection = (consol as IRoutingSupport).TransportsIncludingRelated;

			AssertEquals("Should be 0 RouteSets as Route Legs are not complete", 0, collection.RouteSets.Count);
		}

		public void TestCreateRouteSets_RouteSetsRecalculatedWhenTransportChanges()
		{
			var legsData = new TransportLegDataForTest(new[] { "USCHI", "USLAX", "AUMEL", "AUSYD" },
												   null,
												   new[] { "12345" },
												   null,
												   new[] { Carrier1, Carrier1, Carrier1 });

			var collection = CreateTestTransportCollection(legsData);

			AssertEquals("Should be one RouteSet", 1, collection.RouteSets.Count);
			var routeSet = collection.RouteSets.First();
			AssertEquals("USCHI", routeSet.Origin.Code);
			AssertEquals("AUSYD", routeSet.Destination.Code);
			AssertEquals(1, routeSet.RouteSetNumber);
			AssertEquals("Via is not defined", null, routeSet.Via);

			var t1 = collection[0];
			var t2 = collection[1];
			var t3 = collection[2];

			//Changing Carrier Booking Reference
			t2.JW_CarrierBookingReference = "5664";
			AssertEquals("Should be 2 Route Sets", 3, collection.RouteSets.Count);

			t1.JW_CarrierBookingReference = "1234";
			t2.JW_CarrierBookingReference = "1234";
			t3.JW_CarrierBookingReference = "1234";

			AssertEquals("Should be one RouteSet", 1, collection.RouteSets.Count);

			//Changing Load/Discharge Port
			t1.JW_RL_NKDiscPort = "AUMEL";
			t2.JW_RL_NKLoadPort = "AUMEL";
			t2.JW_RL_NKDiscPort = "AUBNE";
			t3.JW_RL_NKLoadPort = "AUBNE";
			t3.JW_RL_NKDiscPort = "USLAX";

			AssertEquals("Should be 1 Route Sets", 1, collection.RouteSets.Count);
			routeSet = collection.RouteSets.First();
			AssertEquals("USCHI", routeSet.Origin.Code);
			AssertEquals("USLAX", routeSet.Destination.Code);
			AssertEquals(1, routeSet.RouteSetNumber);
			AssertEquals("Via is not defined", null, routeSet.Via);
		}

		public void TestCreateRouteSets_RouteSetsRecalculatedOnLegsCollectionChange()
		{
			var transports = Factory.New<CommonConsol>().Transports;
			var t1 = transports[0];
			t1.JW_RL_NKLoadPort = "FRPAR";
			t1.JW_RL_NKDiscPort = "AUSYD";

			var collection = new RoutingCollection((ITransportParent)transports.Master);

			AssertEquals("Should be 1 RouteSet for now", 1, collection.RouteSets.Count);

			var t2 = transports.AddNew();
			t2.JW_RL_NKLoadPort = "FRCDG";
			t2.JW_RL_NKDiscPort = "DEBRE";

			var t3 = transports.AddNew();
			t3.JW_RL_NKLoadPort = "DEBRE";
			t3.JW_RL_NKDiscPort = "AUMEL";

			AssertEquals("Should be 2 RouteSets now", 2, collection.RouteSets.Count);

			var t4 = transports.AddNew();
			t4.JW_RL_NKLoadPort = "AUMEL";
			t4.JW_RL_NKDiscPort = "AUSYD";

			transports.Remove(t1);

			AssertEquals("Should be 1 RouteSet", 1, collection.RouteSets.Count);
		}

		public void TestCreateRouteSets_CarrierModeAndDates()
		{
			var transports = Factory.New<CommonConsol>().Transports;
			var t1 = transports[0];
			t1.JW_RL_NKLoadPort = "UAIEV";
			t1.JW_RL_NKDiscPort = "UAODS";
			t1.JW_ETD = new ZDateTime(2016, 4, 5);
			t1.JW_ATD = new ZDateTime(2016, 4, 6);
			t1.JW_ETA = new ZDateTime(2016, 4, 7);
			t1.JW_ATA = new ZDateTime(2016, 4, 8);
			t1.JW_TransportMode = Constants.TransportModes.Rail;
			t1.JW_TransportType = Constants.TransportPlanningType.Other;
			t1.CarrierPK = Carrier1.PK;

			var t2 = transports.AddNew("UAODS", "DEBRE");
			t2.JW_ETD = new ZDateTime(2016, 4, 8);
			t2.JW_ATD = new ZDateTime(2016, 4, 9);
			t2.JW_ETA = new ZDateTime(2016, 5, 1);
			t2.JW_ATA = new ZDateTime(2016, 5, 2);
			t2.JW_TransportMode = Constants.TransportModes.Sea;
			t2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			t2.CarrierPK = Carrier1.PK;

			var collection = new RoutingCollection((ITransportParent)transports.Master);
			AssertEquals("Should be one route set", 1, collection.RouteSets.Count);

			var routeSet = collection.RouteSets.First();

			AssertEquals(new ZDateTime(2016, 4, 5), routeSet.ETD);
			AssertEquals(new ZDateTime(2016, 4, 6), routeSet.ATD);
			AssertEquals(new ZDateTime(2016, 5, 1), routeSet.ETA);
			AssertEquals(new ZDateTime(2016, 5, 2), routeSet.ATA);

			AssertEquals("UAIEV", routeSet.Origin.Code);
			AssertEquals("DEBRE", routeSet.Destination.Code);
			AssertEquals("UAODS", routeSet.Via.Code);
			AssertEquals(Constants.TransportModes.Sea, routeSet.TransportMode);
			AssertEquals(Carrier1, routeSet.Carrier);

			t2.JW_TransportType = Constants.TransportPlanningType.Other;
			AssertEquals("Should be two route sets as there is no Main Transport and different Transport Modes", 2, collection.RouteSets.Count);

			t2.JW_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Should be one route set as Carrier and Mode match", 1, collection.RouteSets.Count);

			t2.CarrierPK = Carrier2.PK;
			AssertEquals("Should be two route sets as there is no Main Transport and different Carriers", 2, collection.RouteSets.Count);
		}

		public void TestCreateRouteSets_CarrierCombinations()
		{
			var transports = Factory.New<CommonConsol>().Transports;
			var t1 = transports[0];
			t1.JW_RL_NKLoadPort = "ZAJNB";
			t1.JW_RL_NKDiscPort = "ZADUR";
			t1.JW_TransportMode = Constants.TransportModes.Road;
			t1.JW_TransportType = Constants.TransportPlanningType.Other;
			t1.JW_CarrierBookingReference = "1234";
			t1.CarrierPK = Carrier1.PK;

			var t2 = transports.AddNew("ZACAT", "ZACPT");
			t2.JW_TransportMode = Constants.TransportModes.Rail;
			t2.JW_TransportType = Constants.TransportPlanningType.Other;
			t2.JW_CarrierBookingReference = "5678";
			t2.CarrierPK = Carrier2.PK;

			var t3 = transports.AddNew("ZACPT", "SGSIN");
			t3.JW_TransportMode = Constants.TransportModes.Sea;
			t3.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			t3.JW_CarrierBookingReference = "5678";
			t3.CarrierPK = Carrier2.PK;

			var collection = new RoutingCollection((ITransportParent)transports.Master);
			AssertEquals("Should be two route sets", 2, collection.RouteSets.Count);

			t3.JW_CarrierBookingReference = "";
			t3.CarrierPK = Carrier3.PK;
			AssertEquals("Should be three route sets", 3, collection.RouteSets.Count);
		}

		public void TestCreateRouteSets_BookingReferenceAndVoyageFlightPrevail()
		{
			var transports = Factory.New<CommonConsol>().Transports;
			var t1 = transports[0];
			t1.JW_RL_NKLoadPort = "ZAJNB";
			t1.JW_RL_NKDiscPort = "ZADUR";
			t1.JW_TransportMode = Constants.TransportModes.Road;
			t1.JW_TransportType = Constants.TransportPlanningType.Other;
			t1.CarrierPK = Carrier1.PK;

			var t2 = transports.AddNew("ZADUR", "AEDXB");
			t2.JW_TransportMode = Constants.TransportModes.Sea;
			t2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			t2.CarrierPK = Carrier1.PK;

			var t3 = transports.AddNew("AEDXB", "SGSIN");
			t3.JW_TransportMode = Constants.TransportModes.Sea;
			t3.JW_TransportType = Constants.TransportPlanningType.Other;
			t3.CarrierPK = Carrier1.PK;

			var collection = new RoutingCollection((ITransportParent)transports.Master);
			AssertEquals("Should be one route set because of the same carrier and Main Vessel exists", 1, collection.RouteSets.Count);

			var routeSet = collection.RouteSets.First();
			AssertEquals("ZAJNB", routeSet.Origin.Code);
			AssertEquals("SGSIN", routeSet.Destination.Code);
			AssertEquals("AEDXB", routeSet.Via.Code);

			t1.JW_CarrierBookingReference = "0001";
			t2.JW_CarrierBookingReference = "0002";
			t3.JW_CarrierBookingReference = "0002";

			AssertEquals("Should be two route sets", 2, collection.RouteSets.Count);
			var routeSet2 = collection.RouteSets.OrderBy(x => x.RouteSetNumber).Last();

			AssertEquals("ZADUR", routeSet2.Origin.Code);
			AssertEquals("SGSIN", routeSet2.Destination.Code);
			AssertEquals("AEDXB", routeSet2.Via.Code);

			AssertEquals(Constants.TransportModes.Sea, routeSet2.TransportMode);
			AssertEquals(Carrier1, routeSet2.Carrier);

			t1.JW_CarrierBookingReference = "0001";
			t2.JW_CarrierBookingReference = "0001";
			t3.JW_CarrierBookingReference = "";

			t1.JW_VoyageFlight = "VF001";
			t2.JW_VoyageFlight = "VF001";
			t3.JW_VoyageFlight = "VF002";

			AssertEquals("Should be two route sets", 2, collection.RouteSets.Count);

			t1.JW_CarrierBookingReference = "";
			t2.JW_CarrierBookingReference = "0001";
			t3.JW_CarrierBookingReference = "";

			t2.JW_VoyageFlight = "VF002";
			t3.JW_VoyageFlight = "VF003";

			AssertEquals("Should be three route sets", 3, collection.RouteSets.Count);
		}

		public void TestRouteSets_AirTransportIsConsideredMain()
		{
			var transports = Factory.New<CommonConsol>().Transports;
			var t1 = transports[0];
			t1.JW_RL_NKLoadPort = "ZAJNB";
			t1.JW_RL_NKDiscPort = "ZADUR";
			t1.JW_TransportMode = Constants.TransportModes.Road;
			t1.JW_TransportType = Constants.TransportPlanningType.Other;
			t1.CarrierPK = Carrier1.PK;

			var t2 = transports.AddNew("ZADUR", "AEDXB");
			t2.JW_TransportMode = Constants.TransportModes.Air;
			t2.JW_TransportType = Constants.TransportPlanningType.Other;
			t2.CarrierPK = Carrier1.PK;

			var t3 = transports.AddNew("AEDXB", "SGSIN");
			t3.JW_TransportMode = Constants.TransportModes.Rail;
			t3.JW_TransportType = Constants.TransportPlanningType.Other;
			t3.CarrierPK = Carrier1.PK;

			var collection = new RoutingCollection((ITransportParent)transports.Master);
			AssertEquals("Should be one route set because of the same carrier and Air transport considered as Main", 1, collection.RouteSets.Count);
		}

		public void TestRouteSets_DontMixAirAndSeaInTheSameRouteSet()
		{
			var transports = Factory.New<CommonConsol>().Transports;
			var t1 = transports[0];
			t1.JW_RL_NKLoadPort = "ZAJNB";
			t1.JW_RL_NKDiscPort = "ZADUR";
			t1.JW_TransportMode = Constants.TransportModes.Air;
			t1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			t1.JW_CarrierBookingReference = "0001";
			t1.CarrierPK = Carrier1.PK;

			var t2 = transports.AddNew("ZADUR", "AEDXB");
			t2.JW_TransportMode = Constants.TransportModes.Sea;
			t2.JW_TransportType = Constants.TransportPlanningType.Other;
			t2.JW_CarrierBookingReference = "0001";
			t2.CarrierPK = Carrier1.PK;

			var t3 = transports.AddNew("AEDXB", "SGSIN");
			t3.JW_TransportMode = Constants.TransportModes.Sea;
			t3.JW_TransportType = Constants.TransportPlanningType.Other;
			t3.JW_CarrierBookingReference = "0001";
			t3.CarrierPK = Carrier1.PK;

			var collection = new RoutingCollection((ITransportParent)transports.Master);
			AssertEquals("Should be two route sets because we can't mix Air and Sea", 2, collection.RouteSets.Count);

			var routeSet1 = collection.RouteSets[0];

			AssertEquals("ZAJNB", routeSet1.Origin.Code);
			AssertEquals("ZADUR", routeSet1.Destination.Code);

			var routeSet2 = collection.RouteSets[1];

			AssertEquals("ZADUR", routeSet2.Origin.Code);
			AssertEquals("SGSIN", routeSet2.Destination.Code);
			AssertEquals("AEDXB", routeSet2.Via.Code);
		}

		#region Implementation

		RoutingCollection CreateTestTransportCollection(TransportLegDataForTest legsData)
		{
			var collection = Factory.New<CommonConsol>().Transports;

			for (int i = 0; i < legsData.UNLOCOs.Length - 1; i++)
			{
				var transport = i == 0 ? collection[0] : collection.AddNew();
				transport.JW_RL_NKLoadPort = legsData.UNLOCOs[i];
				transport.JW_RL_NKDiscPort = legsData.UNLOCOs[i + 1];
				transport.JW_CarrierBookingReference = legsData.CarrierReferences != null ? legsData.CarrierReferences.Length == 1 ? legsData.CarrierReferences[0] : legsData.CarrierReferences[i] : "";
				transport.JW_VoyageFlight = legsData.Voyages != null ? legsData.Voyages.Length == 1 ? legsData.Voyages[0] : legsData.Voyages[i] : "";
				transport.CarrierPK = legsData.Carriers.Length == 1 ? legsData.Carriers[0].PK : legsData.Carriers[i].PK;
			}

			return new RoutingCollection((ITransportParent)collection.Master);
		}

		OrgHeader carrier1;
		public OrgHeader Carrier1
		{
			get
			{
				if (carrier1 == null)
				{
					carrier1 = Factory.NewWithValidTestData<OrgHeader>();
					carrier1.OH_Code = "CARRIER1";
				}

				return carrier1;
			}
		}

		OrgHeader carrier2;
		public OrgHeader Carrier2
		{
			get
			{
				if (carrier2 == null)
				{
					carrier2 = Factory.NewWithValidTestData<OrgHeader>();
					carrier2.OH_Code = "CARRIER2";
				}

				return carrier2;
			}
		}

		OrgHeader carrier3;
		public OrgHeader Carrier3
		{
			get
			{
				if (carrier3 == null)
				{
					carrier3 = Factory.NewWithValidTestData<OrgHeader>();
					carrier3.OH_Code = "CARRIER3";
				}

				return carrier3;
			}
		}

		OrgHeader carrier4;
		public OrgHeader Carrier4
		{
			get
			{
				if (carrier4 == null)
				{
					carrier4 = Factory.NewWithValidTestData<OrgHeader>();
					carrier4.OH_Code = "CARRIER4";
				}

				return carrier4;
			}
		}

		class TransportLegDataForTest
		{
			public string[] UNLOCOs;
			public string[] CarrierReferences;
			public string[] Voyages;
			public OrgHeader[] Carriers;

			public TransportLegDataForTest(string[] uNLOCOs, string[] carrierReferences, string[] voyages, BusinessObjectFactory factory = null, OrgHeader[] carriers = null)
			{
				this.UNLOCOs = uNLOCOs;
				this.CarrierReferences = carrierReferences;
				this.Voyages = voyages;

				if (carriers == null)
				{
					var carrier = factory.NewWithValidTestData<OrgHeader>();
					carrier.OH_Code = "CARRIER";
					carriers = new OrgHeader[] { carrier };
				}

				this.Carriers = carriers;
			}
		}

		#endregion

		#endregion

	}
}
