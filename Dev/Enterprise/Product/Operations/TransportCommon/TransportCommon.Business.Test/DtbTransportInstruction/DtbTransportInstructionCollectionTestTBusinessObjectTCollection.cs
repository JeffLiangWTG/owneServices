using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionCollectionTest<TBusinessObject, TCollection> : ActiveBusinessObjectCollectionTestCase<TCollection>
			where TBusinessObject : DtbTransportInstruction
			where TCollection : DtbTransportInstructionCollection<TBusinessObject>
	{
		#region TestAddingInstructionsIncrementsSequenceByOne

		public void TestAddingInstructionsIncrementsSequenceByOne()
		{
			var booking = GetNewTransport();
			var instructionsCollection = (DtbTransportInstructionCollection<TBusinessObject>)booking.Instructions;
			AssertEquals(1, instructionsCollection.AddNew().KN_Sequence);
			AssertEquals(2, instructionsCollection.AddNew().KN_Sequence);
			AssertEquals(3, instructionsCollection.AddNew().KN_Sequence);
		}

		#endregion

		#region TestAddNew_WithInstructionType

		public void TestAddNew_WithInstructionType()
		{
			var collection = GetCollectionToTest();

			AssertEquals("", collection.AddNew("").KN_InstructionType);
			AssertEquals(InstructionTypes.Codes.PickUp, collection.AddNew(InstructionTypes.Codes.PickUp).KN_InstructionType);
			AssertEquals(InstructionTypes.Codes.Delivery, collection.AddNew(InstructionTypes.Codes.Delivery).KN_InstructionType);
		}

		#endregion

		#region TestAddNew_WithInstructionAndOrgType

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

		#endregion

		#region TestSequence

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

		#endregion

		#region TestDelete_SequencesInstructions

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

		#endregion

		#region TestMove

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

		void AssertInstructionSequence(TBusinessObject i1, TBusinessObject i2, TBusinessObject i3, TBusinessObject i4)
		{
			AssertEquals(1, i1.KN_Sequence);
			AssertEquals(2, i2.KN_Sequence);
			AssertEquals(3, i3.KN_Sequence);
			AssertEquals(4, i4.KN_Sequence);
		}

		#endregion

		#region Implementation

		protected abstract DtbTransport GetNewTransport();

		#endregion
	}
}
