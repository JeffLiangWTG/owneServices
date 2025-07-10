using System;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();

		public override Type GetAsycudaEDIMessageType()
		{
			return typeof(UYMessage);
		}
	}
}
