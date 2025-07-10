using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class BusinessObjectExtensions
	{
		public static bool MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(this BusinessObject businessObject, ZString documentName)
		{
			var dialogs = (businessObject as IStmALogParent)?.GetDialogs(documentName, false);
			if (dialogs != null)
			{
				foreach (var dialog in dialogs.Reverse())
				{
					if (dialog.TransmissionCode == Events.MessageWithdrawCancelRequestCode && dialog.HasBeenAccepted())
					{
						return false;
					}

					if (dialog.HasBeenResetToOriginal())
					{
						return false;
					}

					if (dialog.TransmissionCode == Events.MessageSentCode)
					{
						return !dialog.HasBeenRejected();
					}
				}
			}

			return false;
		}

		public static bool MessageHasBeenSent(this BusinessObject businessObject, ZString documentName)
		{
			var dialogs = (businessObject as IStmALogParent)?.GetDialogs(documentName, false);
			if (dialogs != null)
			{
				foreach (var dialog in dialogs.Reverse())
				{
					if (dialog.HasBeenResetToOriginal())
					{
						return false;
					}

					if (dialog.TransmissionCode == Events.MessageSentCode)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool IsWaitingWithdrawConfirmation(this BusinessObject businessObject, ZString documentName)
		{
			var latestDialog = (businessObject as IStmALogParent)?.GetDialogs(documentName, false)?.LastOrDefault();
			return latestDialog != null
				&& latestDialog.TransmissionCode == Events.MessageWithdrawCancelRequestCode
				&& !latestDialog.HasReceivedResponse();
		}
	}
}
