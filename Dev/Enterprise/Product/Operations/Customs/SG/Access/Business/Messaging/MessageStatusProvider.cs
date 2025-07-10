using IMessageParent = Enterprise.Customs.ASYCUDA.Business.IMessageParent;
using MessageStatusCodeList = Enterprise.Customs.ASYCUDA.Business.MessageStatusCodeList;

namespace Enterprise.Customs.SG.Access.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		const string CancelledBillStatus = "CN";

		public override bool AllowCancellationMessage(IMessageParent parent)
		{
			var packedItem = parent as AsycudaPackedItem;
			var isImportManifest = packedItem?.Header?.IsImport ?? false;
			var isInCanCacelStatus = (isImportManifest
										&& (parent.MessageStatus == MessageStatusCodeList.Codes.Awaiting || parent.MessageStatus == MessageStatusCodeList.Codes.Accepted)
										&& parent.CustomsStatus != Constants.CustomsStatusCode.Cancelled)
									|| (!isImportManifest
										&& HasManifestBeenAcceptedByCustoms(parent));

			return !HasBeenCancelled(parent) && isInCanCacelStatus;
		}

		public override bool AllowManifestCancellationMessage(IMessageParent parent) => AllowCancellationMessage(parent);

		public override bool AllowModificationMessage(IMessageParent parent)
		{
			var allowAmend = parent.ManifestType != SGManifestTypes.Codes.MGI;
			var packedItem = parent as AsycudaPackedItem;
			var lockedBills = packedItem?.Header?.LockedBills ?? false;
			return allowAmend && !HasBeenCancelled(parent) && HasManifestBeenAcceptedByCustoms(parent) && (!lockedBills || packedItem.API_MessageStatus == MessageStatusCodeList.Codes.Updated);
		}

		public override bool AllowOriginalMessage(IMessageParent parent)
		{
			return !HasBeenCancelled(parent) && !HasManifestBeenSubmittedToCustoms(parent);
		}

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => parent != null && !parent.RegistrationNumber.IsEmpty;

		public override bool MessageStatusCanBeReset(IMessageParent parent)
		{
			var result = false;
			if (parent is AsycudaPackedItem packedItem)
			{
				result |= IsCustomsStatusCancelled(packedItem?.Pack?.Bill);
			}

			return result || HasBeenCancelled(parent) || (IsLastMessageStatusSentToCustoms(parent) && !HasManifestBeenAcceptedByCustoms(parent));
		}

		public bool HasBeenCancelled(IMessageParent parent) => IsCustomsStatusCancelled(parent) && IsLastMessageStatusSentToCustoms(parent);

		bool IsCustomsStatusCancelled(IMessageParent parent)
		{
			if (parent != null)
			{
				if (parent is AsycudaBill)
				{
					return parent.CustomsStatus == CancelledBillStatus;
				}
				return parent.CustomsStatus == Constants.CustomsStatusCode.Cancelled;
			}
			return false;
		}
	}
}
