using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetInstructionCollection : ActiveBusinessObjectCollection<DtbConsignmentRunSheetInstruction>, IBindingList
	{
		public DtbConsignmentRunSheetInstructionCollection(DtbConsignmentRunSheet runSheet)
			: base(runSheet.Factory, runSheet, null, DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet)
		{
			Sequencer.SortBySequence(); // this does not sort until the elements are accessed
		}

		protected override bool MatchesFilterCore(DtbConsignmentRunSheetInstruction element, bool fetchOnlyFromLocalCache)
		{
			var isMatch = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
			if (hideDepotInstructions)
			{
				return isMatch && !element.IsOwnDepot;
			}
			else
			{
				return isMatch;
			}
		}

		#region HideDepotInstructions

		public ZBool HideDepotInstructions
		{
			get
			{
				return hideDepotInstructions;
			}
			set
			{
				hideDepotInstructions = value;
				this.RefreshFromDb();
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ZBool hideDepotInstructions = false;

		#endregion

		#region AllowNew

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region AllowRemove

		bool IBindingList.AllowRemove
		{
			get { return false; }
		}

		#endregion

		#region SetDefaultsForNewElement

		protected override void SetDefaultsForNewElementCore(DtbConsignmentRunSheetInstruction instruction)
		{
			base.SetDefaultsForNewElementCore(instruction);
			instruction.K1_Sequence = Count + 1;
		}

		#endregion

		#region Move / Sequence

		public void MoveUp(DtbConsignmentRunSheetInstruction instruction, INotifications notification = null)
		{
			var list = FindShipmentsThatNeedToBePickedUp(instruction, true);

			if (!list.Any())
			{
				Sequencer.MoveUp(instruction);
			}
			else if (notification != null)
			{
				ReportError(notification, list);
			}
		}

		public void MoveDown(DtbConsignmentRunSheetInstruction instruction, INotifications notification = null)
		{
			var list = FindShipmentsThatNeedToBePickedUp(instruction, false);

			if (!list.Any())
			{
				Sequencer.MoveDown(instruction);
			}
			else if (notification != null)
			{
				ReportError(notification, list);
			}
		}

		IEnumerable<ZString> FindShipmentsThatNeedToBePickedUp(DtbConsignmentRunSheetInstruction instruction, bool moveUp)
		{
			var newSequenceNumber = instruction.K1_Sequence + (moveUp ? -1 : 1);
			var instructionToSwap = instruction.RunSheet.RunSheetInstructions.FirstOrDefault(runSheet => runSheet.K1_Sequence == newSequenceNumber);
			if (instructionToSwap != null)
			{
				var instructionBookings = instruction.Confirmations.Select(c => c.Instruction.KN_KM_BookingMovement);
				var swapInstructionBookings = instructionToSwap.Confirmations.Select(c => c.Instruction.KN_KM_BookingMovement);
				var existingBookings = instructionBookings.Intersect(swapInstructionBookings);

				return instructionToSwap.Confirmations
					.Where(c => existingBookings.Contains(c.Instruction.KN_KM_BookingMovement))
					.Select(c => c.ConsignmentID);
			}

			return Enumerable.Empty<ZString>();
		}

		void ReportError(INotifications notification, IEnumerable<ZString> list)
		{
			var message = Res.GetString("e5cbf3b8-e29e-4acd-a286-b83c1e619e73", "Unable to modify pickup / delivery order as you cannot deliver the following consignment(s) before they get picked up:{0}{1}.", System.Environment.NewLine, string.Join(System.Environment.NewLine, list));
			notification.Notify(new Notification(ErrorType.Error, message));
		}

		public void Sequence()
		{
			Sequencer.Sequence();
		}

		CollectionSequencer<DtbConsignmentRunSheetInstruction> Sequencer
		{
			get { return sequencer ?? (sequencer = new CollectionSequencer<DtbConsignmentRunSheetInstruction>(this, DtbConsignmentRunSheetInstructionSchema.K1_Sequence, b => b.Validation.ValidateK1_Sequence())); }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		CollectionSequencer<DtbConsignmentRunSheetInstruction> sequencer;

		#endregion
	}
}
