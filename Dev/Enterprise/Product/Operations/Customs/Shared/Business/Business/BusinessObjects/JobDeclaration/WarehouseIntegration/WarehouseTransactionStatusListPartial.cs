using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	partial class WarehouseTransactionStatusList
	{
		public static bool IsChangeOfOwnershipCode(string code)
		{
			return code == Codes.ChangeOfOwnershipCanceled || code == Codes.ChangeOfOwnershipCreated || code == Codes.ChangeOfOwnershipCreatedPending
				|| code == Codes.ChangeOfOwnershipHolding || code == Codes.ChangeOfOwnershipUpdated || code == Codes.ChangeOfOwnershipUpdatedPending;
		}

		public static bool IsChangeOfRegimeCode(string code)
		{
			return code == Codes.ChangeOfRegimeCanceled || code == Codes.ChangeOfRegimeCreated || code == Codes.ChangeOfRegimeCreatedPending
				|| code == Codes.ChangeOfRegimeHolding || code == Codes.ChangeOfRegimeUpdated || code == Codes.ChangeOfRegimeUpdatedPending;
		}

		public static bool IsInwardCode(string code)
		{
			return code == Codes.InwardCanceled || code == Codes.InwardCanceledPendingWithdrawal || code == Codes.InwardCreated
				|| code == Codes.InwardCreatedPending || code == Codes.InwardCreationHeld || code == Codes.InwardUpdated
				|| code == Codes.InwardUpdatedPending;
		}

		public static bool IsOutwardCode(string code)
		{
			return code == Codes.OutwardCanceled || code == Codes.OutwardCanceledPendingWithdrawal || code == Codes.OutwardCreated
				|| code == Codes.OutwardCreatedPending || code == Codes.OutwardHolding || code == Codes.OutwardUpdated
				|| code == Codes.OutwardUpdatedPending;
		}

		public static bool IsPendingInwardOrOutward(ZString code)
		{
			return IsPendingInward(code) || IsPendingOutward(code);
		}

		public static bool IsPendingChangeOfOwnership(ZString code)
		{
			return code == Codes.ChangeOfOwnershipCreatedPending || code == Codes.ChangeOfOwnershipHolding || code == Codes.ChangeOfOwnershipUpdatedPending;
		}

		public static bool IsPendingChangeOfRegime(ZString code)
		{
			return code == Codes.ChangeOfRegimeCreatedPending || code == Codes.ChangeOfRegimeHolding || code == Codes.ChangeOfRegimeUpdatedPending;
		}

		public static bool IsPendingInward(ZString code)
		{
			return code == Codes.InwardCreatedPending || code == Codes.InwardUpdatedPending || code == Codes.InwardCanceledPendingWithdrawal || code == Codes.InwardCreationHeld;
		}

		public static bool IsPendingOutward(ZString code)
		{
			return code == Codes.OutwardCreatedPending || code == Codes.OutwardUpdatedPending || code == Codes.OutwardCanceledPendingWithdrawal || code == Codes.OutwardHolding;
		}

		public static bool HasWHSTransaction(ZString code)
		{
			return !code.IsEmpty && code != Codes.OutwardCanceled && code != Codes.InwardCanceled && code != Codes.AutomationIsDisabled && code != Codes.ChangeOfOwnershipCanceled && code != Codes.ChangeOfRegimeCanceled;
		}

		public static ZString GetErrorMessageIfPending(ZString code)
		{
			var result = ZString.Empty;
			if (IsPendingChangeOfRegime(code) || IsPendingChangeOfOwnership(code) || IsPendingInward(code) || IsPendingOutward(code))
			{
				result = Res.GetString("9ec15cae-a214-4af2-84dc-503349dff378", "There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.");
			}
			return result;
		}

		public static bool IsAutomationDisabled(string code)
		{
			return code == Codes.AutomationIsDisabled;
		}
	}
}
