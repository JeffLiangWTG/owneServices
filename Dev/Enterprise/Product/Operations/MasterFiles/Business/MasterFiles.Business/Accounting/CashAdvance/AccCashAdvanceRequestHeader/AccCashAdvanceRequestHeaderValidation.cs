//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCashAdvanceRequestHeaderValidation
//
//    This class should be used for overriding validation in AutoAccCashAdvanceRequestHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceRequestHeaderValidation : AutoAccCashAdvanceRequestHeaderValidation
	{
		public AccCashAdvanceRequestHeaderValidation(AutoAccCashAdvanceRequestHeader parent) : base(parent)
		{
		}

		AccCashAdvanceRequestHeader ParentBizO => base.Parent as AccCashAdvanceRequestHeader;

		protected override void CheckCAH_Status()
		{
			base.CheckCAH_Status();

			var lines = ParentBizO.Lines.OfType<AccCashAdvanceRequestLine>();
			if (ParentBizO.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Cancelled && !lines.All(l => l.IsCancelled))
			{
				ParentBizO.CAH_StatusInfo.AddError(Res.GetString("ae9c9e8b-a60b-4557-9739-c59adcc177f5", "Advance Payment request is canceled without canceling all request lines."));
			}
			else if (ParentBizO.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Invoiced && !lines.All(l => l.IsInvoiced))
			{
				ParentBizO.CAH_StatusInfo.AddError(Res.GetString("63bc6ded-9617-48f3-988c-6b292202b0fc", "Advance Payment request status is updated to invoiced. But all request lines are not in invoiced status."));
			}
			else if (ParentBizO.CAH_Status == CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced && !lines.Any(l => l.IsInvoiced))
			{
				ParentBizO.CAH_StatusInfo.AddError(Res.GetString("e9a53213-d02f-4b35-bf24-d0644894b9fe", "Advance Payment request status is marked as partially invoiced. But there is no request line in invoiced status."));
			}
			else if (ParentBizO.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Paid && !lines.All(l => l.IsPaid))
			{
				ParentBizO.CAH_StatusInfo.AddError(Res.GetString("926f436e-6992-4a47-beb6-9480e4f60cdf", "Advance Payment request status is updated to paid. But all request lines are not in paid status."));
			}
			else if (ParentBizO.CAH_Status == CashAdvanceStatusCodes.RequestHeader.PartiallyPaid && !lines.Any(l => l.IsPaid))
			{
				ParentBizO.CAH_StatusInfo.AddError(Res.GetString("6c4214d5-c56a-48dd-b1b0-6bbe6bb92bab", "Advance Payment request status is marked as partially paid. But there is no request line in paid status."));
			}
			else if (ParentBizO.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Requested && !lines.All(l => l.IsRequested))
			{
				ParentBizO.CAH_StatusInfo.AddError(Res.GetString("b420cca5-8fb3-4cb0-9378-5808385b1982", "Advance Payment request status is updated to requested. But all lines are not in requested status."));
			}
		}
	}
}
