//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCashAdvanceRequestLineValidation
//
//    This class should be used for overriding validation in AutoAccCashAdvanceRequestLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceRequestLineValidation : AutoAccCashAdvanceRequestLineValidation
	{
		public AccCashAdvanceRequestLineValidation(AutoAccCashAdvanceRequestLine parent) : base(parent)
		{
		}

		AccCashAdvanceRequestLine ParentBizO => base.Parent as AccCashAdvanceRequestLine;

		protected override void CheckCAL_Status()
		{
			base.CheckCAL_Status();

			if (ParentBizO.IsPaid && (ParentBizO.CAL_LocalAmount != ParentBizO.CAL_LocalPaidAmount || ParentBizO.CAL_OSAmount != ParentBizO.CAL_OSPaidAmount))
			{
				ParentBizO.CAL_StatusInfo.AddError(Res.GetString("d35a3892-f40f-48aa-bf8b-cdd1351c6501", "Status is updated to paid. But paid amount is not same as the request advance payment amount."));
			}
			if ((ParentBizO.IsCancelled || ParentBizO.IsRequested) && (ParentBizO.CAL_LocalPaidAmount != 0M || ParentBizO.CAL_OSPaidAmount != 0M))
			{
				ParentBizO.CAL_StatusInfo.AddError(Res.GetString("91621c55-1d23-4b9f-b8c4-4d1e37304891", "Line status is changed to {0} without resetting the paid amount to zero.", ParentBizO.CAL_Status));
			}
		}
	}
}
