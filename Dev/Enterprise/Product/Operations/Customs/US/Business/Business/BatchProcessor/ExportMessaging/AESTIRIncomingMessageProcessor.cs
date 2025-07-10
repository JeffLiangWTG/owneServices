using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.Business
{
	public class AESTIRIncomingMessageProcessor : IncomingMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new AESMessageProcessorFactory(Logger));
			return result;
		}

		protected override void Execute(System.Threading.CancellationToken token)
		{
			((Enterprise.Messaging.Business.IInboundInterchangeProcessor)new AESInboundInterchangeProcessor(Logger)).Execute(token);
			base.Execute(token);
		}
	}
}
