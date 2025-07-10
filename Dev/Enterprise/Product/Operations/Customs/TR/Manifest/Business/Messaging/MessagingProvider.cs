using System;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();

		public override Type GetAsycudaEDIMessageType()
		{
			return typeof(TRManifestMessage);
		}
	}
}
