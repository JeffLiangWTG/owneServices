using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionCollection : ActiveBusinessObjectCollection<DtbBookingInstruction>
	{
		public DtbBookingInstructionCollection(DtbBooking booking)
		: base(booking.Factory, booking, null, DtbBookingInstructionSchema.KN_KM_BookingMovement)
		{
			Sequencer.SortBySequence(); // this does not sort until the elements are accessed
		}

		protected override bool AllowNew
		{
			get { return ConsolidationViewModeService.GetViewMode(Factory) != ConsolidationViewMode.MultiJob && base.AllowNew; }
		}

		public IEnumerable<DtbBookingInstruction> DeliveryInstructions
		{
			get { return this.Where(i => i.IsDelivery || i.IsMulti); }
		}

		public IEnumerable<DtbBookingInstruction> DeliveryInstructions_ExcludeEmpties
		{
			get { return this.Where(i => (i.IsDelivery || i.IsMulti) && i.OrganisationType != OrganisationTypesList.Codes.CYD); }
		}

		public IEnumerable<DtbBookingInstruction> DeliveryInstructions_EmptiesOnly
		{
			get { return this.Where(i => (i.IsDelivery || i.IsMulti) && i.OrganisationType == OrganisationTypesList.Codes.CYD); }
		}

		public IEnumerable<DtbBookingInstruction> PickUpInstructions
		{
			get { return this.Where(i => i.IsPickUp || i.IsMulti); }
		}

		public IEnumerable<DtbBookingInstruction> PickUpInstructionsExcludeMultis
		{
			get { return this.Where(i => i.IsPickUp); }
		}

		public DtbBookingInstruction AddNew(ZString instructionType)
		{
			var instruction = AddNew();
			instruction.KN_InstructionType = instructionType;

			return instruction;
		}

		public DtbBookingInstruction AddNew(ZString instructionType, ZString organisationType)
		{
			var instruction = AddNew(instructionType);
			instruction.OrganisationType = organisationType;

			return instruction;
		}

		protected override void SetDefaultsForNewElementCore(DtbBookingInstruction newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.KN_Sequence = Count + 1;
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { SkipResequencingOnInstructionDeleted };
		}

		public void MoveUp(DtbBookingInstruction instruction)
		{
			Sequencer.MoveUp(instruction);
		}

		public void MoveDown(DtbBookingInstruction instruction)
		{
			Sequencer.MoveDown(instruction);
		}

		public void DeleteAllInstructions()
		{
			using (EnterSkipResequencingOnInstructionDeletedMode())
			{
				DeleteAll();
			}
		}

		public void OnInstructionDeleted()
		{
			if (!SkipResequencingOnInstructionDeleted)
			{
				Sequence();
			}
		}

		bool SkipResequencingOnInstructionDeleted { get; set; }

#if DEBUG
		public
#endif
		DisposableAction EnterSkipResequencingOnInstructionDeletedMode()
		{
			return new DisposableAction(
				createAction: () =>
				{
					SkipResequencingOnInstructionDeleted = true;
				},
				disposeAction: () =>
				{
					SkipResequencingOnInstructionDeleted = false;
				});
		}

		public void Sequence()
		{
			Sequencer.Sequence();
		}

		CollectionSequencer<DtbBookingInstruction> Sequencer
		{
			get { return sequencer ?? (sequencer = new CollectionSequencer<DtbBookingInstruction>(this, DtbBookingInstructionSchema.KN_Sequence, i => i.Validation.ValidateKN_Sequence())); }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		CollectionSequencer<DtbBookingInstruction> sequencer;
	}
}
