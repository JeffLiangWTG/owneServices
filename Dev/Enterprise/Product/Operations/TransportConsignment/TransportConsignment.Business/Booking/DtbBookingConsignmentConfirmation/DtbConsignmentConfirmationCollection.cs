using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentConfirmationCollection : DtbTransportConfirmationCollection<DtbConsignmentConfirmation>, IBindingList
	{
		#region Construction

		public DtbConsignmentConfirmationCollection(DtbConsignmentInstruction instruction)
			: base(instruction)
		{
		}

		public DtbConsignmentConfirmationCollection(DtbConsignmentInstructionPkgDivot instructionPkgDivot)
			: base(instructionPkgDivot)
		{
		}

		public DtbConsignmentConfirmationCollection(DtbConsignmentRunSheetInstruction runSheetInstruction)
			: base(runSheetInstruction.Factory, runSheetInstruction, null, DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction)
		{
		}

		public DtbConsignmentConfirmationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public DtbConsignmentConfirmationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#endregion

		#region AllowNew

		protected override bool AllowNew
		{
			get { return false; } // user cannot add confirmations the route planner / runsheet confirmations
		}

		#endregion

		#region AllowRemove

		bool IBindingList.AllowRemove
		{
			get { return false; } // user cannot remove confirmations from the consignment
		}

		#endregion

		public new DtbConsignmentConfirmation AddNew(ZString confirmationType)
		{
			var confirmation = Factory.New<DtbConsignmentConfirmation>();
			confirmation.KK_ConfirmationType = confirmationType;
			Add(confirmation);
			return confirmation;
		}
	}
}
