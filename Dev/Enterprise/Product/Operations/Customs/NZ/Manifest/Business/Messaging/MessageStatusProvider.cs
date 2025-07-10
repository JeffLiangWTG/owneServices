using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => HasManifestBeenSubmittedToCustoms(parent);
		public override bool AllowModificationMessage(IMessageParent parent) => HasManifestBeenAcceptedByCustoms(parent);
		public override bool AllowOriginalMessage(IMessageParent parent) => !HasManifestBeenSubmittedToCustoms(parent);

		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => parent != null && (parent.HasCustomsNumbers || parent.IsCustomsCleared);
	}
}
