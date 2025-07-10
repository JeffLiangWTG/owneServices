using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolForwardingShipmentCollection))]
	sealed class ConsolForwardingShipmentCollectionTest : ConsolShipmentCollectionBOCollectionTest
	{
		public void TestReleaseType()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_ReleaseType = "ABC";

			ConsolForwardingShipmentCollection collection = new ConsolForwardingShipmentCollection(consol);

			ForwardingShipment shipment = collection.AddNew();
			AssertEquals("ABC", shipment.JS_ReleaseType);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			shipment = collection.AddNew();
			AssertEquals("", shipment.JS_ReleaseType);

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_ReleaseType = "";
			shipment = collection.AddNew();
			AssertEquals("", shipment.JS_ReleaseType);

			FreightDataRegistry.Instance.ReleaseType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EBL");
			consol.JK_ReleaseType = "";
			shipment = collection.AddNew();
			AssertEquals("EBL", shipment.JS_ReleaseType);
		}

		public void TestIndexerCasting()
		{
			var collection = GetCollectionToTest() as ConsolForwardingShipmentCollection;
			collection.AddNew();
			AssertEquals("Expecting ForwardingShipment", typeof(ForwardingShipment), collection[0].GetType());
		}

		public void TestAddNewCasting()
		{
			AssertEquals("Expecting ForwardingShipment", typeof(ForwardingShipment), GetCollectionToTest().AddNew().GetType());
		}

		public void TestOnAdded_SetShouldUpdateScreeningStatusCorrectly()
		{
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared, expectedParentConsolShouldUpdateScreeningStatus: false);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Clear, expectedParentConsolShouldUpdateScreeningStatus: false);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Matched, expectedParentConsolShouldUpdateScreeningStatus: true);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear, expectedParentConsolShouldUpdateScreeningStatus: true);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.JobCleared, expectedParentConsolShouldUpdateScreeningStatus: true);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, expectedParentConsolShouldUpdateScreeningStatus: true);

			void ParentConsolShouldUpdateScreeningStatusIsAsExpected(string parentConsolScreeningStatus,
				string shipmentScreeningStatus, bool expectedParentConsolShouldUpdateScreeningStatus)
			{
				var parentConsol = Factory.New<ForwardingConsol>();
				((IShouldUpdateScreeningStatus)parentConsol).ShouldUpdateScreeningStatus = false;
				parentConsol.JK_ScreeningStatus = parentConsolScreeningStatus;
				var collection = new ConsolForwardingShipmentCollection(parentConsol);
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ScreeningStatus = shipmentScreeningStatus;
				collection.Add(shipment);
				AssertEquals("ParentConsol's ShouldUpdateScreeningStatus changed unexpectedly.", expectedParentConsolShouldUpdateScreeningStatus, ((IShouldUpdateScreeningStatus)parentConsol).ShouldUpdateScreeningStatus);
			}
		}

		public void TestOnRemoved_SetShouldUpdateScreeningStatusCorrectly()
		{
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared, expectedParentConsolShouldUpdateScreeningStatus: false);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Clear, expectedParentConsolShouldUpdateScreeningStatus: false);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Matched, expectedParentConsolShouldUpdateScreeningStatus: false);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear, expectedParentConsolShouldUpdateScreeningStatus: true);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.JobCleared, expectedParentConsolShouldUpdateScreeningStatus: true);
			ParentConsolShouldUpdateScreeningStatusIsAsExpected(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, expectedParentConsolShouldUpdateScreeningStatus: true);

			void ParentConsolShouldUpdateScreeningStatusIsAsExpected(string parentConsolScreeningStatus,
			string shipmentScreeningStatus, bool expectedParentConsolShouldUpdateScreeningStatus)
			{
				var parentConsol = Factory.New<ForwardingConsol>();
				parentConsol.JK_ScreeningStatus = parentConsolScreeningStatus;
				var collection = new ConsolForwardingShipmentCollection(parentConsol);
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ScreeningStatus = shipmentScreeningStatus;
				collection.Add(shipment);
				((IShouldUpdateScreeningStatus)parentConsol).ShouldUpdateScreeningStatus = false;
				collection.Remove(shipment);
				AssertEquals("ParentConsol's ShouldUpdateScreeningStatus changed unexpectedly.", expectedParentConsolShouldUpdateScreeningStatus, ((IShouldUpdateScreeningStatus)parentConsol).ShouldUpdateScreeningStatus);
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ConsolForwardingShipmentCollection(Factory.New<ForwardingConsol>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingShipment>();
		}

		#endregion
	}
}
