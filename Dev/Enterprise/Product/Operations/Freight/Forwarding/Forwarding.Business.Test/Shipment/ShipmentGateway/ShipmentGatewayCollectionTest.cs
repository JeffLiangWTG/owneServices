using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ShipmentGatewayCollection))]
	sealed class ShipmentGatewayCollectionTest : ActiveBusinessObjectCollectionTestCase<ShipmentGatewayCollection>
	{
		#region TestSequence

		public void TestIncrementSequenceForAddNewElement()
		{
			var collection = GetCollectionToTest();

			for (var expectedSequence = 1; expectedSequence <= 10; expectedSequence++)
			{
				var shipmentGateway = collection.AddNew();
				AssertEquals(expectedSequence, shipmentGateway.JSG_Sequence);
			}
		}

		public void TestSequenceIsAdjustedOnDelete()
		{
			var collection = GetCollectionToTest();
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();
			var element3 = collection.AddNew();
			var element4 = collection.AddNew();
			var element5 = collection.AddNew();

			collection.Delete(element3);

			AssertEquals((byte)1, element1.JSG_Sequence);
			AssertEquals((byte)2, element2.JSG_Sequence);
			AssertEquals((byte)3, element4.JSG_Sequence);
			AssertEquals((byte)4, element5.JSG_Sequence);

			collection.Delete(element2);

			AssertEquals((byte)1, element1.JSG_Sequence);
			AssertEquals((byte)2, element4.JSG_Sequence);
			AssertEquals((byte)3, element5.JSG_Sequence);
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			var collection = new ShipmentGatewayCollectionForTest(Factory.New<ForwardingShipment>());

			for (var i = 1; i < 256; i++)
			{
				Assert(collection.AllowNew);
				collection.AddNew();
			}

			Assert("Collection allows only 256 elements", !collection.AllowNew);
		}

		#endregion

		#region TestShipment

		public void TestSetJSG_JS_ShipmentForNewElement()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gateway1 = shipment.Gateways.AddNew();
			AssertEquals("Shipment is set for new element", shipment.PK, gateway1.JSG_JS_Shipment);
		}

		#endregion

		#region TestSwapGateways

		public void TestSwapGateways()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var collection = shipment.Gateways;
			var gateway1 = collection.AddNew();
			var gateway2 = collection.AddNew();
			var gateway3 = collection.AddNew();
			var gateway4 = collection.AddNew();

			var address1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A")).MainAddress.PK;
			var address2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B")).MainAddress.PK;
			var address3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C")).MainAddress.PK;
			var address4 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D")).MainAddress.PK;

			gateway1.JSG_OA_ForwarderAddress = address1;
			gateway2.JSG_OA_ForwarderAddress = address2;
			gateway3.JSG_OA_ForwarderAddress = address3;
			gateway4.JSG_OA_ForwarderAddress = address4;

			Factory.Save();

			AssertAddressOrder("Pre-condition", shipment, address1, address2, address3, address4);

			Assert("Sequence 0 is not valid", !collection.SwapGateways(1, 0));
			AssertAddressOrder("No change", shipment, address1, address2, address3, address4);

			Assert("Sequence 5 is not valid", !collection.SwapGateways(4, 5));
			AssertAddressOrder("Still no change", shipment, address1, address2, address3, address4);

			Assert("Can swap 1 and 2", collection.SwapGateways(1, 2));
			AssertAddressOrder("1 and 2 have been swapped", shipment, address2, address1, address3, address4);

			Assert("Can swap 2 and 3", collection.SwapGateways(2, 3));
			AssertAddressOrder("Gateways have been swapped", shipment, address2, address3, address1, address4);

			AssertNoExceptionThrown("Can save OK", () => Factory.Save());
		}

		void AssertAddressOrder(string message, ForwardingShipment shipment, params ZGuid[] expectedAddressPKs)
		{
			var sequence = 1;
			foreach (var addressPK in expectedAddressPKs)
			{
				AssertEquals(message, addressPK, shipment.Gateways.Single(x => x.JSG_Sequence == sequence).JSG_OA_ForwarderAddress);
				sequence++;
			}
		}

		#endregion

		#region Implementation

		protected override ShipmentGatewayCollection GetCollectionToTest()
		{
			return Factory.New<ForwardingShipment>().Gateways;
		}

		class ShipmentGatewayCollectionForTest : ShipmentGatewayCollection
		{
			public ShipmentGatewayCollectionForTest(ForwardingShipment shipment)
				: base(shipment)
			{ }

			public new bool AllowNew => base.AllowNew;
		}

		#endregion
	}
}
