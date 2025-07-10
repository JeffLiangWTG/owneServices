using Enterprise.Customs.ASYCUDA.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public sealed class AIMMessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => true;

		public override bool AllowModificationMessage(IMessageParent parent) => true;

		public override bool AllowOriginalMessage(IMessageParent parent) => true;

		public override bool AllowManifestCancellationMessage(IMessageParent parent) => false;

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;

		public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent) => false;

		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;

		public override string GetMessageFunctionSubTypeForSend(ASYCUDA.Business.AsycudaManifestHeader header) => GetIsExpressCourier(header) ? AIMMessageSubTypes.FXI : AIMMessageSubTypes.FRI;

		public override string GetMessageFunctionSubTypeForAmend(ASYCUDA.Business.AsycudaManifestHeader header) => GetIsExpressCourier(header) ? AIMMessageSubTypes.FXC : AIMMessageSubTypes.FRC;

		public override string GetMessageFunctionSubTypeForCancel(ASYCUDA.Business.AsycudaManifestHeader header) => GetIsExpressCourier(header) ? AIMMessageSubTypes.FXX : AIMMessageSubTypes.FRX;

		bool GetIsExpressCourier(ASYCUDA.Business.AsycudaManifestHeader header) => header is AsycudaManifestHeader newHeader && newHeader.IsExpressCourier;
	}
}
