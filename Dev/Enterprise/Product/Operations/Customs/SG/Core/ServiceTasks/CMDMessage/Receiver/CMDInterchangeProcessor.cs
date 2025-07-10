using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	public class CMDInterchangeProcessor : BaseInboundInterchangeProcessor
	{
		protected override string[] ApplicationCodes => new[] { EDIInterchange.ApplicationCodes.SingaporeCMD };

		protected override bool ProcessInterchange(EDIInterchange interchange)
		{
			var helper = new CMDInboundMessageProcesserHelper(Logger);
			var inboundMessage = new CMDInbound(interchange.EI_BodyText);
			return helper.CreateMessageThrouthInterchangeAndSendEmail(interchange, inboundMessage);
		}
	}
}
