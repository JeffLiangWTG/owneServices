using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.Business
{
	public sealed class EBondIncomingMessageProcessor : IncomingMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new EBondMssageProcessorFactory(Logger));
			return result;
		}

		protected override void Execute(System.Threading.CancellationToken token)
		{
			((Enterprise.Messaging.Business.IInboundInterchangeProcessor)new EBondInboundInterchangeProcessor(Logger)).Execute(token);
			base.Execute(token);
		}
	}
}
