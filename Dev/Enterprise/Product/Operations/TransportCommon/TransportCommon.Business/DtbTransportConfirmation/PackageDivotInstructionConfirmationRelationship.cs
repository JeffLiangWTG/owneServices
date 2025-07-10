using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public class PackageDivotInstructionConfirmationRelationship<T> : DependentRelationship
		where T : DtbTransportConfirmation
	{
		public PackageDivotInstructionConfirmationRelationship(DtbTransportInstructionPkgDivot packageDivot)
			: base(packageDivot, typeof(T), new ZQuery(), DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot)
		{
			HookDivot();
		}

		void HookDivot()
		{
			PackageDivot.KD_KN_BookingInstructionInfo.ValueChanged += KD_KN_BookingInstructionInfo_ValueChanged;
		}

		void KD_KN_BookingInstructionInfo_ValueChanged(object sender, EventArgs e)
		{
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override ZQuery RelationshipFilterCore
		{
			get
			{
				var result = base.RelationshipFilterCore;

				// + Instruction Confirmations
				var instructionConfirmationsQuery = new ZQuery(DtbBookingConfirmationSchema.KK_KN_BookingInstruction, PackageDivot.KD_KN_BookingInstruction);
				instructionConfirmationsQuery.AddToFilter(DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot, DBNull.Value);
				result.AddToFilter(instructionConfirmationsQuery, JoinCondition.Or);

				return result;
			}
		}

		DtbTransportInstructionPkgDivot PackageDivot
		{
			get { return (DtbTransportInstructionPkgDivot)Master; }
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			base.AddToRelationship(businessObject);

			var confirmation = (DtbTransportConfirmation)businessObject;
			confirmation.KK_KN_BookingInstruction = PackageDivot.KD_KN_BookingInstruction;
		}
	}
}
