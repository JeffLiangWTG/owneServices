using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public static class ErrorMessageHelper
	{
		public static string ToLocalizedMessage(this ErrorInfo errorInfo, params string[] parameters)
		{
			switch (errorInfo.Code)
			{
				case ErrorCode.ERR000:
					return ResourceStringHelper.GeneralServiceException;
				case ErrorCode.ERR001:
					return ResourceStringHelper.ProductUnavailabilityDateCheckFailed(parameters);
				case ErrorCode.ERR002:
					return ResourceStringHelper.ProductUnavailabilityUnincorporated(parameters);
				case ErrorCode.ERR003:
					return ResourceStringHelper.NoMatchForSuppliedDUNS;
				case ErrorCode.ERR004:
					return ResourceStringHelper.UnmatchedUNLOCOAndDUNS;
				case ErrorCode.ERR005:
					return ResourceStringHelper.SharedKeyExpired;
				default:
					return ResourceStringHelper.GeneralError;
			}
		}
	}
}
