namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();
	}
}
