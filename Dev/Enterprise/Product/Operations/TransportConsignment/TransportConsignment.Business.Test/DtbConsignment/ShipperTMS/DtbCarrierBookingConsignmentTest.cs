using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbCarrierBookingConsignment))]
	sealed class DtbCarrierBookingConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		#region TestDeliveryAddress

		public void TestDeliveryAddress()
		{
			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var address2 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			AssertEquals("Precondition", 2, consignment.Addresses.Count);

			var deliveryAddress = consignment.DeliveryAddress;
			AssertEquals(ConsignmentAddressTypes.Codes.Delivery, deliveryAddress.LTS_InstructionType); 
			AssertCollectionContains(deliveryAddress, consignment.Addresses);

			var dodgyAddress = consignment.Addresses.AddNew(ConsignmentAddressTypes.Codes.Delivery);
			AssertExceptionThrown(typeof(InvalidOperationException), () => { var poke = consignment.DeliveryAddress; });
		}

		#endregion

		#region TestPickupAddress

		public void TestPickupAddress()
		{
			var badConsignment = Factory.New<DtbConsignment>();
			var poke = badConsignment.PickupAddress;
			AssertEquals("While we expect one pickup address, this is managed by validation. The error report will only come to us now if there are 2 or more pickup addresses.",
				string.Empty, ErrorReporter.LastMessageReported);

			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var address2 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			AssertEquals("Precondition", 2, consignment.Addresses.Count);

			var pickupAddress = consignment.PickupAddress;
			AssertEquals(ConsignmentAddressTypes.Codes.PickUp, pickupAddress.LTS_InstructionType);
			AssertCollectionContains(pickupAddress, consignment.Addresses);

			consignment.Addresses.AddNew(ConsignmentAddressTypes.Codes.PickUp);
			poke = consignment.PickupAddress;
			AssertEquals("Consignment contains 2 Pickup Address.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Implementations

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Helper.CreateConsignment();
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new CarrierBookingTestHelper(Factory);
		}

		#endregion

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.DtbConsignment);
			}
		}

		#endregion

		#region Helper

		CarrierBookingTestHelper Helper
		{
			get { return helper ?? (helper = new CarrierBookingTestHelper(Factory)); }
		}

		CarrierBookingTestHelper helper;

		#endregion
	}
}
