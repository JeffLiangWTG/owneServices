using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ColoadConsolsShipmentsCollection))]
	sealed class ColoadConsolsShipmentsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionCanNeverLoadOrAttemptToDoSo()
		{
			AssertType<ColoadConsolsShipmentsCollection>(Collection);
			Assert(Collection.IsLoaded);
			Assert(Collection.CompleteFilter.IsNoResultQuery);
			AssertExceptionThrown(typeof(NotSupportedException), () => Collection.Load());
		}

		public override void TestSuspendCountChanged()
		{
			var s13 = Factory.NewWithValidTestData<ForwardingShipment>();

			var clm = Factory.NewWithValidTestData<ForwardingConsol>();
			clm.JK_TransportMode = Constants.TransportModes.Air;
			clm.JK_RL_NKLoadPort = "AUSYD";
			clm.JK_RL_NKDischargePort = "NZAKL";
			clm.JK_AgentType = Constants.AgentType.AWBMaster;

			var cla1 = clm.ColoadConsols.AddNew();
			var s11 = cla1.Shipments.AddNew();
			var s12 = cla1.Shipments.AddNew();

			Action<IEnumerable<CollectionCountChangedEventArgs>> suspendedEventsProcessor = suspendedEventArgs =>
			{
				AssertEquals("All event args have been passed to processor", 2, suspendedEventArgs.Count());
				AssertEquals(true, suspendedEventArgs.First().ItemRemoved);
				AssertEquals(s12, suspendedEventArgs.First().BizObject);

				AssertEquals(true, suspendedEventArgs.Skip(1).First().ItemAdded);
				AssertEquals(s13, suspendedEventArgs.Skip(1).First().BizObject);
			};

			int countChangedEventHandlerCalled = 0;
			clm.ColoadConsolsShipments.CountChanged += (s, e) => { countChangedEventHandlerCalled++; };

			using (clm.ColoadConsolsShipments.SuspendCountChanged(suspendedEventsProcessor))
			{
				cla1.Shipments.Remove(s12);
				cla1.Shipments.Add(s13);
			}

			AssertEquals("CountChanged event was suspended", 0, countChangedEventHandlerCalled);

			AssertNoExceptionThrown("Null argument is allowed", () =>
			{
				using (clm.ColoadConsolsShipments.SuspendCountChanged(null))
				{
					cla1.Shipments.AddNew();
					cla1.Shipments.AddNew();
				}
			});

			AssertEquals("CountChanged event was suspended", 0, countChangedEventHandlerCalled);

			cla1.Shipments.Remove(s11);
			cla1.Shipments.AddNew();

			AssertEquals("CountChanged event was not suspended", 2, countChangedEventHandlerCalled);
		}

		public override void TestRemoveFromRelationship()
		{
			AssertType<ColoadConsolsShipmentsCollection>(Collection);
			var initialCount = Collection.Count;
			var icollection = Collection as IBusinessObjectCollection;
			icollection.RemoveFromRelationship(Collection.FirstOrDefault());
			AssertEquals("Collection count", initialCount, Collection.Count);
		}

		public override void TestDelete()
		{
			AssertType<ColoadConsolsShipmentsCollection>(Collection);
			var initialCount = Collection.Count;
			Collection.Remove(Collection.FirstOrDefault());
			AssertEquals("Precondition", initialCount, Collection.Count);
		}

		public override void TestAdd()
		{
			AssertType<ColoadConsolsShipmentsCollection>(Collection);
			var initialCount = Collection.Count;

			var bizO1 = GetNewElementToAddToTheCollection();
			var bizO2 = GetNewElementToAddToTheCollection();

			Collection.Add(bizO1);
			Collection.Add(bizO2);

			AssertEquals("Collection count", initialCount, Collection.Count);
			Assert(!Collection.Contains(bizO1));
			Assert(!Collection.Contains(bizO2));
		}

		public void TestCollectionCountAndElements()
		{
			var clm = Factory.NewWithValidTestData<ForwardingConsol>();
			clm.JK_TransportMode = Constants.TransportModes.Air;
			clm.JK_RL_NKLoadPort = "AUSYD";
			clm.JK_RL_NKDischargePort = "NZAKL";
			clm.JK_AgentType = Constants.AgentType.AWBMaster;

			var cla1 = clm.ColoadConsols.AddNew();
			var s11 = cla1.Shipments.AddNew();
			var s12 = cla1.Shipments.AddNew();

			var cla2 = clm.ColoadConsols.AddNew();
			var s21 = cla2.Shipments.AddNew();

			AssertEquals(3, clm.TopLevelShipments.Count);
			AssertEquals(3, clm.ShipmentsForTotalling.Count);

			var s13 = cla1.Shipments.AddNew();

			AssertEquals(4, clm.TopLevelShipments.Count);
			AssertEquals(4, clm.ShipmentsForTotalling.Count);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				"Collection recalculates correctly when reloaded",
				BusinessObjectEqualityComparer<ForwardingShipment>.PKOnlyComparer,
				x => x.JS_UniqueConsignRef,
				new[] { s11, s12, s13, s21 },
				new BusinessObjectFactory().Load<ForwardingConsol>(clm.PK).ColoadConsolsShipments.Cast<ForwardingShipment>());

			AssertContainsExactElementsInAnyOrder(
				"Shipments colection should be empty on CLM",
				BusinessObjectEqualityComparer<ForwardingShipment>.PKOnlyComparer,
				x => x.JS_UniqueConsignRef,
				Enumerable.Empty<ForwardingShipment>(),
				clm.Shipments.Cast<ForwardingShipment>());

			var cla3 = Factory.NewWithValidTestData<ForwardingConsol>();
			cla3.JK_TransportMode = Constants.TransportModes.Air;
			cla3.JK_RL_NKLoadPort = "AUSYD";
			cla3.JK_RL_NKDischargePort = "NZAKL";
			cla3.JK_AgentType = Constants.AgentType.AWBCoload;

			var s31 = cla3.Shipments.AddNew();
			var s32 = cla3.Shipments.AddNew();

			clm.ColoadConsols.Add(cla3);

			AssertContainsExactElementsInAnyOrder(
				"Collection recalculates when CLA is added",
				BusinessObjectEqualityComparer<ForwardingShipment>.PKOnlyComparer,
				x => x.JS_UniqueConsignRef,
				new[] { s11, s12, s13, s21, s31, s32 },
				clm.ColoadConsolsShipments.Cast<ForwardingShipment>());

			var cla4 = clm.ColoadConsols.AddNew();
			var s41 = cla4.Shipments.AddNew();
			var s42 = cla4.Shipments.AddNew();

			cla1.JK_JK_MasterConsol = ZGuid.Empty;
			cla2.JK_JK_MasterConsol = ZGuid.Empty;

			AssertContainsExactElementsInAnyOrder(
				"Collection recalculates when CLA gets master cleared",
				BusinessObjectEqualityComparer<ForwardingShipment>.PKOnlyComparer,
				x => x.JS_UniqueConsignRef,
				new[] { s31, s32, s41, s42 },
				clm.ColoadConsolsShipments.Cast<ForwardingShipment>());

			cla4.JK_JK_MasterConsol = ZGuid.Empty;
			cla2.JK_JK_MasterConsol = clm.PK;

			AssertContainsExactElementsInAnyOrder(
				"Collection recalculates when CLA gets master",
				BusinessObjectEqualityComparer<ForwardingShipment>.PKOnlyComparer,
				x => x.JS_UniqueConsignRef,
				new[] { s21, s31, s32 },
				clm.ColoadConsolsShipments.Cast<ForwardingShipment>());

			clm.ColoadConsolsShipments.RemoveAll();
			clm.ColoadConsolsShipments.RemoveRange(new[] { s21, s31, s32 });
			clm.ColoadConsolsShipments.AddRange(new[] { s41, s42 });

			AssertContainsExactElementsInAnyOrder(
				"Collection doesn't change when elements added or removed manually",
				BusinessObjectEqualityComparer<ForwardingShipment>.PKOnlyComparer,
				x => x.JS_UniqueConsignRef,
				new[] { s21, s31, s32 },
				clm.ColoadConsolsShipments.Cast<ForwardingShipment>());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var clm = Factory.NewWithValidTestData<ForwardingConsol>();
			clm.JK_TransportMode = Constants.TransportModes.Air;
			clm.JK_RL_NKLoadPort = "AUSYD";
			clm.JK_RL_NKDischargePort = "NZAKL";
			clm.JK_AgentType = Constants.AgentType.AWBMaster;

			var cla1 = clm.ColoadConsols.AddNew();
			cla1.Shipments.AddNew();
			cla1.Shipments.AddNew();

			Factory.Save();

			return clm.ColoadConsolsShipments;
		}
	}
}
