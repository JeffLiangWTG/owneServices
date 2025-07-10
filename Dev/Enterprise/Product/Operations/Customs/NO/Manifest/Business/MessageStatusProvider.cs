using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.NO.Manifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;

		public override bool AllowModificationMessage(IMessageParent parent) => false;

		public override bool AllowOriginalMessage(IMessageParent parent) => false;

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;
	}
}
