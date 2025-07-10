using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.Business
{
	class PermitWithdrawRequestResponse : IPermitWithdrawRequestResponse
	{
		public PermitWithdrawRequestResponse(IPermitWithdrawRequest request)
		{
			this.request = Argument.NotNull(request, "request");
			SuccessOrFailure = SuccessOrFailure.Failure;
		}

		public IPermitWithdrawRequest Request => request;
		public SuccessOrFailure SuccessOrFailure { get; set; }

		public ZString FailureReason => failureReasons?.ToStringWithNewLineBetweenAppends();

		public void AddFailureReason(ZString reason)
		{
			if (!reason.IsEmpty)
			{
				failureReasons = failureReasons ?? new ZStringBuilder();
				failureReasons.Append(reason);
			}
		}
		ZStringBuilder failureReasons;

		public ZDecimal? AvailableQty { get; set; }

		public ZString? OutwardEntryNumber { get; set; }

		#region Implementation

		readonly IPermitWithdrawRequest request;

		#endregion
	}
}
