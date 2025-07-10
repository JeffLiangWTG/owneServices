using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(AllMasterConsolCollection))]
	sealed class AllMasterConsolCollectionTest : ActiveBusinessObjectCollectionTestCase<AllMasterConsolCollection>
	{
		public void TestRebuild()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var collection = new AllMasterConsolCollection(shipment);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, collection);

			var consol3 = shipment.Consols.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, consol3 }, collection);

			shipment.Consols.Remove(consol3);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, collection);

			var masterConsol = Factory.New<ForwardingConsol>();

			var coloadConsol1 = Factory.New<ForwardingConsol>();
			coloadConsol1.JK_JK_MasterConsol = masterConsol.PK;

			var coloadConsol2 = Factory.New<ForwardingConsol>();
			coloadConsol2.JK_JK_MasterConsol = masterConsol.PK;

			shipment.Consols.Add(coloadConsol1);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, coloadConsol1, masterConsol }, collection);

			shipment.Consols.Add(coloadConsol2);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, coloadConsol1, masterConsol, coloadConsol2 }, collection);
		}

		public void TestHookConsols()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var collection = new AllMasterConsolCollection(shipment);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, collection);

			var masterConsol1 = Factory.New<ForwardingConsol>();
			consol2.JK_JK_MasterConsol = masterConsol1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, masterConsol1 }, collection);

			var consol3 = Factory.New<ForwardingConsol>();
			var masterConsol2 = Factory.New<ForwardingConsol>();

			shipment.Consols.Add(consol3);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, consol3, masterConsol1 }, collection);

			consol3.JK_JK_MasterConsol = masterConsol2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, consol3, masterConsol1, masterConsol2 }, collection);

			shipment.Consols.Remove(consol3);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, masterConsol1 }, collection);
		}

		#region Implementation

		protected override AllMasterConsolCollection GetCollectionToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new AllMasterConsolCollection(shipment);
		}

		#endregion
	}
}
