using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionCollection<T> : ActiveBusinessObjectCollection<T>, IDtbTransportInstructionCollection
		where T : DtbTransportInstruction
	{
		protected DtbTransportInstructionCollection(DtbTransport transport)
			: base(transport.Factory, transport, null, DtbBookingInstructionSchema.KN_KM_BookingMovement)
		{
			Sequencer.SortBySequence(); // this does not sort until the elements are accessed
		}

		protected DtbTransportInstructionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			Sequencer.SortBySequence(); // this does not sort until the elements are accessed
		}

		#region IDtbTransportInstructionCollection Members

		DtbTransportInstruction IDtbTransportInstructionCollection.this[int index] => this[index];

		#endregion

		#region AddNew

		public T AddNew(ZString instructionType)
		{
			var instruction = AddNew();
			instruction.KN_InstructionType = instructionType;

			return instruction;
		}

		public T AddNew(ZString instructionType, ZString organisationType)
		{
			var instruction = AddNew(instructionType);
			instruction.OrganisationType = organisationType;

			return instruction;
		}

		#endregion

		#region SetDefaultsForNewElement

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.KN_Sequence = Count + 1;
		}

		#endregion

		#region Move / Sequence

		public void MoveUp(T instruction)
		{
			Sequencer.MoveUp(instruction);
		}

		public void MoveDown(T instruction)
		{
			Sequencer.MoveDown(instruction);
		}

		public void Sequence()
		{
			Sequencer.Sequence();
		}

		CollectionSequencer<T> Sequencer
		{
			get { return sequencer ?? (sequencer = new CollectionSequencer<T>(this, DtbBookingInstructionSchema.KN_Sequence, i => i.Validation.ValidateKN_Sequence())); }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		CollectionSequencer<T> sequencer;

		#endregion
	}
}
