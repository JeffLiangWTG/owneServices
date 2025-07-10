using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccPaymentApprovalValidation : AutoAccPaymentApprovalValidation
	{
		public AccPaymentApprovalValidation(AutoAccPaymentApproval parent) : base(parent)
		{
		}

		protected override void CheckAV_GS_NKApproval1st()
		{
			base.CheckAV_GS_NKApproval1st();
			ListValidation.ErrorIfInvalidCode(Parent.AV_GS_NKApproval1stInfo);
		}

		protected override void CheckAV_GS_NKApproval2nd()
		{
			base.CheckAV_GS_NKApproval2nd();
			ListValidation.ErrorIfInvalidCode(Parent.AV_GS_NKApproval2ndInfo);
		}

		protected override void CheckAV_GS_NKApproval3rd()
		{
			base.CheckAV_GS_NKApproval3rd();
			ListValidation.ErrorIfInvalidCode(Parent.AV_GS_NKApproval3rdInfo);
		}

		protected override void CheckAV_RX_NKPaymentCurrency()
		{
			base.CheckAV_RX_NKPaymentCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.AV_RX_NKPaymentCurrencyInfo);
		}
	}
}
