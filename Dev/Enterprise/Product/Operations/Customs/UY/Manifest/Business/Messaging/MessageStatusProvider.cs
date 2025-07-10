using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => HasManifestBeenAcceptedByCustoms(parent);

		public override bool AllowModificationMessage(IMessageParent parent) => HasManifestBeenAcceptedByCustoms(parent);

		public override bool AllowOriginalMessage(IMessageParent parent)
		{
			var result = false;
			if (parent is AsycudaManifestHeader header)
			{
				result = header.Bills.Cast<AsycudaBill>().Any(x => x.CanSendOriginalMessage());
			}
			return result;
		}

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => parent != null && parent.MessageStatus == MessageStatusCodeList.Codes.Accepted;

		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;

		public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent) => parent != null && (parent.MessageStatus == MessageStatusCodeList.Codes.Awaiting || parent.MessageStatus == MessageStatusCodeList.Codes.Cancel || parent.MessageStatus == MessageStatusCodeList.Codes.Accepted || parent.MessageStatus == MessageStatusCodeList.Codes.Error);
	}
}
