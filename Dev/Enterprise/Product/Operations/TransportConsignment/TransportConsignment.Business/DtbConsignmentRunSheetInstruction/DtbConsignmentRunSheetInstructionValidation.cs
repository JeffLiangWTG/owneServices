//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentRunSheetInstructionValidation
//
//    This class should be used for overriding validation in AutoDtbConsignmentRunSheetInstructionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetInstructionValidation : AutoDtbConsignmentRunSheetInstructionValidation
	{
		public DtbConsignmentRunSheetInstructionValidation(AutoDtbConsignmentRunSheetInstruction parent)
			: base(parent)
		{
		}

		#region CheckK1_TimeIn

		protected override void CheckK1_TimeIn()
		{
			base.CheckK1_TimeIn();
			if (!Parent.K1_IsAcceptedByDriver && !Parent.K1_TimeIn.IsEmpty)
			{
				Parent.K1_TimeInInfo.AddError(Res.GetString("34a3428f-d780-454a-b6d1-af96cf9062dc", "Instruction must be accepted by driver before set Time In."));
			}
			else
			{
				TransportDateRangeValidation.ErrorOnStartIfAfterEnd((ZPropertyInfo<ZDateTimeOffset>)Parent.K1_TimeInInfo, (ZPropertyInfo<ZDateTimeOffset>)Parent.K1_TimeOutInfo);
			}
		}

		#endregion

		#region CheckK1_TimeOut

		protected override void CheckK1_TimeOut()
		{
			base.CheckK1_TimeOut();
			TransportDateRangeValidation.ErrorOnEndIfBeforeStart((ZPropertyInfo<ZDateTimeOffset>)Parent.K1_TimeInInfo, (ZPropertyInfo<ZDateTimeOffset>)Parent.K1_TimeOutInfo);
		}

		#endregion

		#region CheckK1_FailureReason

		protected override void CheckK1_FailureReason()
		{
			base.CheckK1_FailureReason();
			if (Parent.K1_FailureReason.IsEmpty && !Parent.K1_FailureNotes.IsEmpty)
			{
				Parent.K1_FailureReasonInfo.AddError(Res.GetString("710a68c2-7cb7-4327-beca-07669f925eb5", "Reason is required if there are notes."));
			}
		}

		#endregion

	}
}
