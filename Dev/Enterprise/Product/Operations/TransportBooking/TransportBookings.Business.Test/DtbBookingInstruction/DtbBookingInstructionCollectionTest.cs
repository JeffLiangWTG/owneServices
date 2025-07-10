using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingInstructionCollection))]
	public class DtbBookingInstructionCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbBookingInstructionCollection>
	{
		public void TestDeliveryInstructions()
		{
			var collection = GetCollectionToTest();
			var pickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);
			var multiInstruction = collection.AddNew(InstructionTypes.Codes.Multi);

			AssertContainsExactElementsInAnyOrder(new DtbBookingInstruction[] { deliveryInstruction, multiInstruction }, collection.DeliveryInstructions);
		}

		public void TestDeliveryInstructions_ExcludeEmpties()
		{
			var collection = GetCollectionToTest();
			var pickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var cydPickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = collection.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);
			var cydDeliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);
			cydPickupInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			cydDeliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			AssertContainsExactElementsInAnyOrder(new DtbBookingInstruction[] { deliveryInstruction, multiInstruction }, collection.DeliveryInstructions_ExcludeEmpties);
		}

		public void TestDeliveryInstructions_EmptiesOnly()
		{
			var collection = GetCollectionToTest();
			var pickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var cydPickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = collection.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);
			var cydDeliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);
			cydPickupInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			cydDeliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			AssertContainsExactElementsInAnyOrder(new DtbBookingInstruction[] { cydDeliveryInstruction }, collection.DeliveryInstructions_EmptiesOnly);
		}

		public void TestPickUpInstructions()
		{
			var collection = GetCollectionToTest();
			var pickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var cydPickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = collection.AddNew(InstructionTypes.Codes.Multi);
			var cydMultiInstruction = collection.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);
			var cydDeliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);
			cydPickupInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			cydMultiInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			cydDeliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			AssertContainsExactElementsInAnyOrder(new DtbBookingInstruction[] { pickupInstruction, cydPickupInstruction, multiInstruction, cydMultiInstruction }, collection.PickUpInstructions);
		}

		public void TestPickUpInstructionsExcludeMultis()
		{
			var collection = GetCollectionToTest();
			var pickupInstruction = collection.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = collection.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = collection.AddNew(InstructionTypes.Codes.Delivery);

			AssertContainsExactElementsInAnyOrder("PickUpInstructionsExcludeMultis should only have instructions of type PickUp, not of type Multi", new DtbBookingInstruction[] { pickupInstruction }, collection.PickUpInstructionsExcludeMultis);
		}

		public void TestAllowNew()
		{
			var booking = Helper.CreateBooking();
			var instructions = (IBindingList)new DtbBookingInstructionCollection(booking);

			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.SingleJob);
			AssertEquals(true, instructions.AllowNew);

			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.MultiJob);
			AssertEquals(false, instructions.AllowNew);
		}

		public void TestAddingInstructionsIncrementsSequenceByOne()
		{
			var booking = GetNewTransport();
			var instructionsCollection = booking.Instructions;
			AssertEquals(1, instructionsCollection.AddNew().KN_Sequence);
			AssertEquals(2, instructionsCollection.AddNew().KN_Sequence);
			AssertEquals(3, instructionsCollection.AddNew().KN_Sequence);
		}

		public void TestAddNew_WithInstructionType()
		{
			var collection = GetCollectionToTest();

			AssertEquals("", collection.AddNew("").KN_InstructionType);
			AssertEquals(InstructionTypes.Codes.PickUp, collection.AddNew(InstructionTypes.Codes.PickUp).KN_InstructionType);
			AssertEquals(InstructionTypes.Codes.Delivery, collection.AddNew(InstructionTypes.Codes.Delivery).KN_InstructionType);
		}

		public void TestAddNew_WithInstructionAndOrgType()
		{
			var collection = GetCollectionToTest();

			var instruction1 = collection.AddNew("", "");
			AssertEquals("", instruction1.KN_InstructionType);
			AssertEquals("", instruction1.OrganisationType);

			var instruction2 = collection.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			AssertEquals(InstructionTypes.Codes.PickUp, instruction2.KN_InstructionType);
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction2.OrganisationType);

			var instruction3 = collection.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			AssertEquals(InstructionTypes.Codes.Delivery, instruction3.KN_InstructionType);
			AssertEquals(OrganisationTypesList.Codes.CNE, instruction3.OrganisationType);
		}

		public void TestSequence()
		{
			var instructions = GetCollectionToTest();

			var i1 = instructions.AddNew();
			var i2 = instructions.AddNew();
			var i3 = instructions.AddNew();
			var i4 = instructions.AddNew();

			i1.KN_Sequence = 0;
			i2.KN_Sequence = 3;
			i3.KN_Sequence = 3;
			i4.KN_Sequence = 10;

			instructions.Sequence();

			AssertEquals(1, i1.KN_Sequence);
			AssertNotEquals(i2.KN_Sequence, i3.KN_Sequence);
			Assert(i2.KN_Sequence == 2 || i2.KN_Sequence == 3);
			Assert(i3.KN_Sequence == 2 || i3.KN_Sequence == 3);
			AssertEquals(4, i4.KN_Sequence);
		}

		public void TestDelete_SequencesInstructions()
		{
			var instructions = GetCollectionToTest();

			var instruction1 = instructions.AddNew();
			var instruction2 = instructions.AddNew();
			var instruction3 = instructions.AddNew();
			var instruction4 = instructions.AddNew();

			instruction1.KN_Sequence = 1;
			instruction2.KN_Sequence = 2;
			instruction3.KN_Sequence = 3;
			instruction4.KN_Sequence = 4;

			using (((IBusinessObjectCollection)instructions).SuspendListChanged())
			{
				instruction3.Delete();
				AssertEquals(1, instruction1.KN_Sequence);
				AssertEquals(2, instruction2.KN_Sequence);
				AssertEquals(3, instruction4.KN_Sequence);

				instruction2.Delete();
				AssertEquals(1, instruction1.KN_Sequence);
				AssertEquals(2, instruction4.KN_Sequence);
			}
		}

		public void TestDeleteAllInstructionsDoesNotSequenceInstructions()
		{
			var instructions = GetCollectionToTest();

			var instruction1 = instructions.AddNew();
			var instruction2 = instructions.AddNew();
			var instruction3 = instructions.AddNew();
			var instruction4 = instructions.AddNew();

			instruction1.KN_Sequence = 1;
			instruction2.KN_Sequence = 2;
			instruction3.KN_Sequence = 3;
			instruction4.KN_Sequence = 4;

			var instructionsListChangedOccurrences = new List<ListChangedType>();
			((IBindingList)instructions).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				instructionsListChangedOccurrences.Add(e.ListChangedType);
			};

			instructions.DeleteAllInstructions();
			var expectedInstructionsListChangedOccurrences = new[] { ListChangedType.Reset };

			CombineAssertions("Check that instructions are deleted and there was no re-sequencing done", () =>
			{
				Assert("All instructions should have been deleted", !instructions.Any());
				AssertContainsExactElementsInExactOrder("instructions should only have had one occurrence of the Reset ListChanged event (from the call to DeleteAll() - see ActiveBusinessObjectCollectionIndex.DeleteAll()), no re-sequencing should occur after each delete (indicated by extra Reset ListChanged events)", expectedInstructionsListChangedOccurrences, instructionsListChangedOccurrences);
			});
		}

		public void TestOnInstructionDeleted()
		{
			var instructions = GetCollectionToTest();

			var instruction1 = instructions.AddNew();
			var instruction2 = instructions.AddNew();
			var instruction3 = instructions.AddNew();

			instruction1.KN_Sequence = 2;
			instruction2.KN_Sequence = 3;
			instruction3.KN_Sequence = 4;

			var instructionsListChangedOccurrences = new List<ListChangedType>();
			((IBindingList)instructions).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				instructionsListChangedOccurrences.Add(e.ListChangedType);
			};

			instructions.OnInstructionDeleted();

			CombineAssertions("Check that instructions were re-sequenced", () =>
			{
				Assert("instructions should have had ListChanged events due to the re-sequencing", instructionsListChangedOccurrences.Any());
				var firstInstructionsListChangedOccurrence = instructionsListChangedOccurrences.FirstOrDefault();
				AssertEquals("instructions should have had Reset as its first ListChanged event due to re-sequencing", ListChangedType.Reset, firstInstructionsListChangedOccurrence);
				AssertEquals($"{nameof(instruction1)}.KN_Sequence should have been re-sequenced to 1", 1, instruction1.KN_Sequence);
				AssertEquals($"{nameof(instruction2)}.KN_Sequence should have been re-sequenced to 2", 2, instruction2.KN_Sequence);
				AssertEquals($"{nameof(instruction3)}.KN_Sequence should have been re-sequenced to 3", 3, instruction3.KN_Sequence);
			});
		}

		public void TestOnInstructionDeletedInsideSkipResequencingMode()
		{
			var instructions = GetCollectionToTest();

			var instruction1 = instructions.AddNew();
			var instruction2 = instructions.AddNew();
			var instruction3 = instructions.AddNew();

			instruction1.KN_Sequence = 2;
			instruction2.KN_Sequence = 3;
			instruction3.KN_Sequence = 4;

			var instructionsListChangedOccurrences = new List<ListChangedType>();
			((IBindingList)instructions).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				instructionsListChangedOccurrences.Add(e.ListChangedType);
			};

			using (instructions.EnterSkipResequencingOnInstructionDeletedMode())
			{
				instructions.OnInstructionDeleted();
			}

			CombineAssertions("Check that instructions were not re-sequenced", () =>
			{
				Assert("instructions should have had no ListChanged events due to the re-sequencing being skipped", !instructionsListChangedOccurrences.Any());
				AssertEquals($"{nameof(instruction1)}.KN_Sequence should have been remained at 2", 2, instruction1.KN_Sequence);
				AssertEquals($"{nameof(instruction2)}.KN_Sequence should have been remained at 3", 3, instruction2.KN_Sequence);
				AssertEquals($"{nameof(instruction3)}.KN_Sequence should have been remained at 4", 4, instruction3.KN_Sequence);
			});
		}

		public void TestMoveUp()
		{
			var instructions = GetCollectionToTest();

			var i1 = instructions.AddNew();
			var i2 = instructions.AddNew();
			var i3 = instructions.AddNew();
			var i4 = instructions.AddNew();
			AssertInstructionSequence(i1, i2, i3, i4);

			instructions.MoveUp(i4);
			AssertInstructionSequence(i1, i2, i4, i3);

			instructions.MoveUp(i4);
			AssertInstructionSequence(i1, i4, i2, i3);

			instructions.MoveUp(i4);
			AssertInstructionSequence(i4, i1, i2, i3);

			instructions.MoveUp(i4);
			AssertInstructionSequence(i4, i1, i2, i3);
		}

		public void TestMoveDown()
		{
			var instructions = GetCollectionToTest();

			var i1 = instructions.AddNew();
			var i2 = instructions.AddNew();
			var i3 = instructions.AddNew();
			var i4 = instructions.AddNew();
			AssertInstructionSequence(i1, i2, i3, i4);

			instructions.MoveDown(i1);
			AssertInstructionSequence(i2, i1, i3, i4);

			instructions.MoveDown(i1);
			AssertInstructionSequence(i2, i3, i1, i4);

			instructions.MoveDown(i1);
			AssertInstructionSequence(i2, i3, i4, i1);

			instructions.MoveDown(i1);
			AssertInstructionSequence(i2, i3, i4, i1);
		}

		void AssertInstructionSequence(DtbBookingInstruction i1, DtbBookingInstruction i2, DtbBookingInstruction i3, DtbBookingInstruction i4)
		{
			AssertEquals(1, i1.KN_Sequence);
			AssertEquals(2, i2.KN_Sequence);
			AssertEquals(3, i3.KN_Sequence);
			AssertEquals(4, i4.KN_Sequence);
		}

		DtbBooking GetNewTransport()
		{
			return Helper.CreateBooking();
		}

		protected override DtbBookingInstructionCollection GetCollectionToTest()
		{
			return new DtbBookingInstructionCollection(Factory.New<DtbBooking>());
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
