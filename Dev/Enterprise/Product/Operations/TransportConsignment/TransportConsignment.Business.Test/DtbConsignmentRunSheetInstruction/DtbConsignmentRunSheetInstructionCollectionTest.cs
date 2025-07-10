using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetInstructionCollection))]
	sealed class DtbConsignmentRunSheetInstructionCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentRunSheetInstructionCollection>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		#endregion

		#region TestAllowRemove

		public void TestAllowRemove()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowRemove);
		}

		#endregion

		#region TestAddingInstructionsIncrementsSequenceByOne

		public void TestAddingInstructionsIncrementsSequenceByOne()
		{
			var collection = GetCollectionToTest();
			AssertEquals(1, collection.AddNew().K1_Sequence);
			AssertEquals(2, collection.AddNew().K1_Sequence);
			AssertEquals(3, collection.AddNew().K1_Sequence);
		}

		#endregion

		#region TestMove

		public void TestMoveUp()
		{
			var collection = GetCollectionToTest();

			var i1 = collection.AddNew();
			var i2 = collection.AddNew();
			var i3 = collection.AddNew();
			var i4 = collection.AddNew();
			AssertInstructionSequence(i1, i2, i3, i4);

			collection.MoveUp(i4);
			AssertInstructionSequence(i1, i2, i4, i3);

			collection.MoveUp(i4);
			AssertInstructionSequence(i1, i4, i2, i3);

			collection.MoveUp(i4);
			AssertInstructionSequence(i4, i1, i2, i3);

			collection.MoveUp(i4);
			AssertInstructionSequence(i4, i1, i2, i3);
		}

		public void TestMoveDown()
		{
			var collection = GetCollectionToTest();

			var i1 = collection.AddNew();
			var i2 = collection.AddNew();
			var i3 = collection.AddNew();
			var i4 = collection.AddNew();
			AssertInstructionSequence(i1, i2, i3, i4);

			collection.MoveDown(i1);
			AssertInstructionSequence(i2, i1, i3, i4);

			collection.MoveDown(i1);
			AssertInstructionSequence(i2, i3, i1, i4);

			collection.MoveDown(i1);
			AssertInstructionSequence(i2, i3, i4, i1);

			collection.MoveDown(i1);
			AssertInstructionSequence(i2, i3, i4, i1);
		}

		void AssertInstructionSequence(DtbConsignmentRunSheetInstruction i1, DtbConsignmentRunSheetInstruction i2, DtbConsignmentRunSheetInstruction i3, DtbConsignmentRunSheetInstruction i4)
		{
			AssertInstructionSequence("", i1, i2, i3, i4);
		}

		void AssertInstructionSequence(ZString message, DtbConsignmentRunSheetInstruction i1, DtbConsignmentRunSheetInstruction i2, DtbConsignmentRunSheetInstruction i3, DtbConsignmentRunSheetInstruction i4)
		{
			AssertEquals(message, 1, i1.K1_Sequence);
			AssertEquals(message, 2, i2.K1_Sequence);
			AssertEquals(message, 3, i3.K1_Sequence);
			AssertEquals(message, 4, i4.K1_Sequence);
		}

		#region TestErrorMessage 

		public void TestMoveDownErrorMessage()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment1.KM_JobID = "CN1";

			var runsheet = Helper.CreateRunSheet();
			var pickupRSI1 = Helper.CreateRunSheetInstruction(runsheet, consignment1.PickupInstruction.PickupConfirmation);
			var deliveryRSI1 = Helper.CreateRunSheetInstruction(runsheet, consignment1.DeliveryInstruction.DeliveryConfirmation);
			pickupRSI1.K1_Sequence = 1;
			deliveryRSI1.K1_Sequence = 2;

			var expectedString = "Unable to modify pickup / delivery order as you cannot deliver the following consignment(s) before they get picked up:\r\nCN1. ";
			var notifications = new TestNotificationBuffer();
			runsheet.RunSheetInstructions.MoveUp(deliveryRSI1, notifications);

			AssertEquals(expectedString.TrimEnd(), notifications.AsString.TrimEnd());
		}

		public void TestMoveUpErrorMessage()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment1.KM_JobID = "CN1";

			var runsheet = Helper.CreateRunSheet();
			var pickupRSI1 = Helper.CreateRunSheetInstruction(runsheet, consignment1.PickupInstruction.PickupConfirmation);
			var deliveryRSI1 = Helper.CreateRunSheetInstruction(runsheet, consignment1.DeliveryInstruction.DeliveryConfirmation);
			pickupRSI1.K1_Sequence = 2;
			deliveryRSI1.K1_Sequence = 1;

			var expectedString = "Unable to modify pickup / delivery order as you cannot deliver the following consignment(s) before they get picked up:\r\nCN1. ";
			var notifications = new TestNotificationBuffer();
			runsheet.RunSheetInstructions.MoveDown(deliveryRSI1, notifications);

			AssertEquals(expectedString.TrimEnd(), notifications.AsString.TrimEnd());
		}

		#endregion

		#region TestMove_CanNotPutInstructionAboveOrBelowRelated

		public void TestMove_CanNotPutInstructionAboveOrBelowRelated()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runsheet = Helper.CreateRunSheet();
			var pickupRSI2 = Helper.CreateRunSheetInstruction(runsheet, consignment2.PickupInstruction.PickupConfirmation);
			var pickupRSI1 = Helper.CreateRunSheetInstruction(runsheet, consignment1.PickupInstruction.PickupConfirmation);
			var deliveryRSI1 = Helper.CreateRunSheetInstruction(runsheet, consignment1.DeliveryInstruction.DeliveryConfirmation);
			var deliveryRSI2 = Helper.CreateRunSheetInstruction(runsheet, consignment2.DeliveryInstruction.DeliveryConfirmation);
			AssertInstructionSequence(pickupRSI2, pickupRSI1, deliveryRSI1, deliveryRSI2); // Precondition

			runsheet.RunSheetInstructions.MoveUp(pickupRSI1);
			AssertInstructionSequence("There should be no issue moving a PIC above a PIC", pickupRSI1, pickupRSI2, deliveryRSI1, deliveryRSI2);

			runsheet.RunSheetInstructions.MoveDown(pickupRSI1);
			AssertInstructionSequence("There should be no issue moving a PIC below a PIC.", pickupRSI2, pickupRSI1, deliveryRSI1, deliveryRSI2);

			runsheet.RunSheetInstructions.MoveDown(deliveryRSI1);
			AssertInstructionSequence("There should be no issue moving a DLV below a DLV", pickupRSI2, pickupRSI1, deliveryRSI2, deliveryRSI1);

			runsheet.RunSheetInstructions.MoveUp(deliveryRSI1);
			AssertInstructionSequence("There should be no issue moving a DLV above a DLV.", pickupRSI2, pickupRSI1, deliveryRSI1, deliveryRSI2);

			runsheet.RunSheetInstructions.MoveDown(pickupRSI1);
			AssertInstructionSequence("The RSI sequence should not change if the PIC is is trying to be move below the related DLV.",
				pickupRSI2, pickupRSI1, deliveryRSI1, deliveryRSI2);

			runsheet.RunSheetInstructions.MoveUp(deliveryRSI1);
			AssertInstructionSequence("The RSI sequence should not change if the DLV is trying to be move above the related PIC.",
				 pickupRSI2, pickupRSI1, deliveryRSI1, deliveryRSI2);

			runsheet.RunSheetInstructions.MoveDown(pickupRSI2);
			AssertInstructionSequence("There should be no issue moving a PIC below a PIC.", pickupRSI1, pickupRSI2, deliveryRSI1, deliveryRSI2);

			runsheet.RunSheetInstructions.MoveDown(pickupRSI2);
			AssertInstructionSequence("There should be no issue moving a PIC below an unreleted DLV.", pickupRSI1, deliveryRSI1, pickupRSI2, deliveryRSI2);

			runsheet.RunSheetInstructions.MoveUp(pickupRSI2);
			runsheet.RunSheetInstructions.MoveUp(pickupRSI2);
			AssertInstructionSequence(pickupRSI2, pickupRSI1, deliveryRSI1, deliveryRSI2); // Resetting to start positions

			runsheet.RunSheetInstructions.MoveUp(deliveryRSI2);
			AssertInstructionSequence("There should be no issue moving a DLV above a DLV.", pickupRSI2, pickupRSI1, deliveryRSI2, deliveryRSI1);

			runsheet.RunSheetInstructions.MoveUp(deliveryRSI2);
			AssertInstructionSequence("There should be no issue moving a DLV above an unreleted PIC.", pickupRSI2, deliveryRSI2, pickupRSI1, deliveryRSI1);
		}

		#endregion

		#endregion

		#region TestSequence

		public void TestSequence()
		{
			var runSheet = Helper.CreateRunSheet();

			var i1 = runSheet.RunSheetInstructions.AddNew();
			var i2 = runSheet.RunSheetInstructions.AddNew();
			var i3 = runSheet.RunSheetInstructions.AddNew();
			var i4 = runSheet.RunSheetInstructions.AddNew();

			i1.K1_Sequence = 0;
			i2.K1_Sequence = 3;
			i3.K1_Sequence = 3;
			i4.K1_Sequence = 10;

			runSheet.RunSheetInstructions.Sequence();
			AssertEquals(1, i1.K1_Sequence);
			AssertNotEquals(i2.K1_Sequence, i3.K1_Sequence);
			Assert(i2.K1_Sequence == 2 || i2.K1_Sequence == 3);
			Assert(i3.K1_Sequence == 2 || i3.K1_Sequence == 3);
			AssertEquals(4, i4.K1_Sequence);
		}

		#endregion

		#region TestRunSheetInstructionsSortedBySequence

		public void TestRunSheetInstructionsSortedBySequence()
		{
			var runSheet = Helper.CreateRunSheet();
			var i3 = runSheet.RunSheetInstructions.AddNew();
			var i2 = runSheet.RunSheetInstructions.AddNew();
			var i4 = runSheet.RunSheetInstructions.AddNew();
			var i1 = runSheet.RunSheetInstructions.AddNew();
			i3.K1_Sequence = 3;
			i2.K1_Sequence = 2;
			i4.K1_Sequence = 4;
			i1.K1_Sequence = 1;

			CombineAssertions(() =>
			{
				AssertEquals("Instruction 1 should have sequence 1", 1, runSheet.RunSheetInstructions[0].K1_Sequence);
				AssertEquals("Instruction 2 should have sequence 2", 2, runSheet.RunSheetInstructions[1].K1_Sequence);
				AssertEquals("Instruction 3 should have sequence 3", 3, runSheet.RunSheetInstructions[2].K1_Sequence);
				AssertEquals("Instruction 4 should have sequence 4", 4, runSheet.RunSheetInstructions[3].K1_Sequence);
			});
		}

		#endregion

		#region TestHideDepotInstructions

		public void TestHideDepotInstructions()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet = Helper.CreateRunSheet();
			Helper.CreateRunSheetInstruction(runSheet, consignment.PickupInstruction.PickupConfirmation);
			var pickupInstruction = consignment.PickupInstruction;

			pickupInstruction.OrganisationType = "CFS";
			pickupInstruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			AssertEquals(true, pickupInstruction.IsOwnDepot);

			var confirmation = pickupInstruction.Confirmations[0];

			runSheet.RunSheetInstructions.AddNew();

			var runSheetInstructionsCollection = runSheet.RunSheetInstructions;

			AssertEquals(true, runSheetInstructionsCollection[0].IsOwnDepot);
			AssertEquals(false, runSheetInstructionsCollection[2].IsOwnDepot);

			AssertEquals(3, runSheetInstructionsCollection.Count);
			runSheetInstructionsCollection.HideDepotInstructions = true;
			AssertEquals(1, runSheetInstructionsCollection.Count);
		}

		#endregion

		#region Implementation

		protected override DtbConsignmentRunSheetInstructionCollection GetCollectionToTest()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			return new DtbConsignmentRunSheetInstructionCollection(runSheet);
		}

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}
