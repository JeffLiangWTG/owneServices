using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingConfirmationCollection))]
	public class DtbBookingConfirmationCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbBookingConfirmationCollection>
	{
		public void TestDeliveries()
		{
			var collection = GetCollectionToTest();
			var pickUpconfirmation = collection.AddNew(ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = collection.AddNew(ConfirmationTypes.Codes.Delivery);

			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { deliveryConfirmation }, collection.Deliveries);
		}

		public void TestPickUps()
		{
			var collection = GetCollectionToTest();
			var pickUpconfirmation = collection.AddNew(ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = collection.AddNew(ConfirmationTypes.Codes.Delivery);

			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { pickUpconfirmation }, collection.PickUps);
		}

		public void TestIsDeliveryComplete()
		{
			var now = ZDateTime.Now;

			var collection = GetCollectionToTest();

			// 1 completed pickup confirmation

			var pickUpconfirmation = collection.AddNew(ConfirmationTypes.Codes.PickUp);
			pickUpconfirmation.KK_Actual = ZDateTime.Now;
			AssertEquals(false, collection.IsDeliveryComplete);

			// 1 completed delivery confirmation (quantity, actual date), 1 incomplete delivery confirmation (quantity, no actual date) ==> Incomplete

			var deliveryInstruction = Helper.CreateInstruction();
			var deliveryConfirmation1 = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);
			deliveryConfirmation1.KK_Actual = now;
			var deliveryConfirmation2 = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);
			AssertEquals(false, deliveryInstruction.Confirmations.IsDeliveryComplete);

			// 2 completed delivery confirmations ==> Complete

			deliveryConfirmation2.KK_Actual = now;
			AssertEquals(true, deliveryInstruction.Confirmations.IsDeliveryComplete);

			// 1 "actual dated" delivery confirmation without packages on the instruction ==> Complete

			var instruction = Helper.CreateInstruction();
			var deliveryConfirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.Delivery);
			deliveryConfirmation.KK_Actual = now;
			AssertEquals(true, instruction.Confirmations.IsDeliveryComplete);

			// 1 "actual dated" delivery confirmation on a package divot without full quantity ==> Incomplete

			instruction = Helper.CreateInstruction();
			var packageDivot = Helper.CreatePackageDivot(instruction, 2);
			deliveryConfirmation = Helper.CreateConfirmation(packageDivot, ConfirmationTypes.Codes.Delivery);
			deliveryConfirmation.KK_Actual = now;
			deliveryConfirmation.KK_Quantity = 1;
			AssertEquals(false, instruction.Confirmations.IsDeliveryComplete);

			// 1 "actual dated" delivery confirmation on package divot with full quantity ==> Complete

			deliveryConfirmation.KK_Quantity = 2;
			AssertEquals(true, instruction.Confirmations.IsDeliveryComplete);

			// 2 "actual dated" delivery confirmations on one package divot without full quantity ==> Incomplete

			instruction = Helper.CreateInstruction();
			packageDivot = Helper.CreatePackageDivot(instruction, 2);
			deliveryConfirmation = Helper.CreateConfirmation(packageDivot, ConfirmationTypes.Codes.Delivery);
			deliveryConfirmation.KK_Actual = now;
			deliveryConfirmation.KK_Quantity = 1;
			deliveryConfirmation2 = Helper.CreateConfirmation(packageDivot, ConfirmationTypes.Codes.Delivery);
			deliveryConfirmation2.KK_Actual = now;
			deliveryConfirmation2.KK_Quantity = 0;
			AssertEquals(false, instruction.Confirmations.IsDeliveryComplete);

			// 2 "actual dated" delivery confirmations on one package divot with full quantity ==> Complete

			deliveryConfirmation.KK_Quantity = 2;
			AssertEquals(true, instruction.Confirmations.IsDeliveryComplete);

			// 2 "actual dated" delivery confirmations on one package divot with more than full quantity ==> Complete

			deliveryConfirmation.KK_Quantity = 3;
			AssertEquals(true, instruction.Confirmations.IsDeliveryComplete);
		}

		public void TestIsPickUpComplete()
		{
			var now = ZDateTime.Now;

			var collection = GetCollectionToTest();

			// 1 completed delivery confirmation ==> Incomplete

			var deliveryConfirmation = collection.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryConfirmation.KK_Actual = now;
			AssertEquals(false, collection.IsPickUpComplete);

			// 1 completed pickup confirmation (quantity, actual date), 1 incomplete pickup confirmation (quantity, no actual date) ==> Incomplete

			var pickupInstruction = Helper.CreateInstruction();
			var pickUpConfirmation1 = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			pickUpConfirmation1.KK_Actual = now;
			var pickUpConfirmation2 = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			AssertEquals(false, pickupInstruction.Confirmations.IsPickUpComplete);

			// 2 completed pickup confirmations ==> Complete

			pickUpConfirmation2.KK_Actual = now;
			AssertEquals(true, pickupInstruction.Confirmations.IsPickUpComplete);

			// 1 "actual dated" pickup confirmation without packages on the instruction ==> Complete

			var instruction = Helper.CreateInstruction();
			var pickupConfirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			pickupConfirmation.KK_Actual = now;
			AssertEquals(true, instruction.Confirmations.IsPickUpComplete);

			// 1 "actual dated" pickup confirmation on a package divot without full quantity ==> Incomplete

			instruction = Helper.CreateInstruction();
			var packageDivot = Helper.CreatePackageDivot(instruction, 2);
			pickupConfirmation = Helper.CreateConfirmation(packageDivot, ConfirmationTypes.Codes.PickUp);
			pickupConfirmation.KK_Actual = now;
			pickupConfirmation.KK_Quantity = 1;
			AssertEquals(false, instruction.Confirmations.IsPickUpComplete);

			// 1 "actual dated" pickup confirmation on package divot with full quantity ==> Complete

			pickupConfirmation.KK_Quantity = 2;
			AssertEquals(true, instruction.Confirmations.IsPickUpComplete);

			// 2 "actual dated" pickup confirmations on one package divot without full quantity ==> Incomplete

			instruction = Helper.CreateInstruction();
			packageDivot = Helper.CreatePackageDivot(instruction, 2);
			pickupConfirmation = Helper.CreateConfirmation(packageDivot, ConfirmationTypes.Codes.PickUp);
			pickupConfirmation.KK_Actual = now;
			pickupConfirmation.KK_Quantity = 1;
			pickUpConfirmation2 = Helper.CreateConfirmation(packageDivot, ConfirmationTypes.Codes.PickUp);
			pickUpConfirmation2.KK_Actual = now;
			pickUpConfirmation2.KK_Quantity = 0;
			AssertEquals(false, instruction.Confirmations.IsPickUpComplete);

			// 2 "actual dated" pickup confirmations on one package divot with full quantity ==> Complete

			pickupConfirmation.KK_Quantity = 2;
			AssertEquals(true, instruction.Confirmations.IsPickUpComplete);

			// 2 "actual dated" pickup confirmations on one package divot with more than full quantity ==> Complete

			pickupConfirmation.KK_Quantity = 3;
			AssertEquals(true, instruction.Confirmations.IsPickUpComplete);
		}

		public void TestNullPackageDivot()
		{
			var now = ZDateTime.Now;
			var collection = GetCollectionToTest();
			var confirmation = collection.AddNew(ConfirmationTypes.Codes.Delivery);
			confirmation.KK_Actual = ZDateTime.Now;

			AssertEquals("Normal Case - no divots", true, collection.IsDeliveryComplete);
		}

		public void TestAddNewWithConfirmationType()
		{
			var collection = GetCollectionToTest();

			AssertEquals("", collection.AddNew("").KK_ConfirmationType);
			AssertEquals(ConfirmationTypes.Codes.PickUp, collection.AddNew(ConfirmationTypes.Codes.PickUp).KK_ConfirmationType);
			AssertEquals(ConfirmationTypes.Codes.Delivery, collection.AddNew(ConfirmationTypes.Codes.Delivery).KK_ConfirmationType);
		}

		public void TestHasDelivery()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.HasDelivery);

			collection.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals(false, collection.HasDelivery);

			collection.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals(true, collection.HasDelivery);
		}

		public void TestHasPickup()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.HasPickup);

			collection.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals(false, collection.HasPickup);

			collection.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals(true, collection.HasPickup);
		}

		protected override DtbBookingConfirmationCollection GetCollectionToTest()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();

			return new DtbBookingConfirmationCollection(instruction);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
