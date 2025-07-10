using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	class ConsolDelegator : IProcessorDelegator
	{
		#region IProcessorDelegator Members

		public string MessageFriendlyName
		{
			get { return "Outward Report"; }
		}

		public bool CanProcess(Declaration.NZCMessage message)
		{
			//			ZString sendersReference = message.MessageAsCUSRESD98A.UNH[0].CommonAccessReference;
			//			return sendersReference.StartsWith("C");
			ZString sendersReference = message.MessageAsCUSRESD98A.UNH[0].CommonAccessReference;
			consol = ForwardingConsol.LoadFromRef(message.Factory, sendersReference);
			return consol != null;
		}

		ForwardingConsol consol;

		public void Process(LoggingInformation logger, Declaration.NZCMessage message)
		{
			consol.Messages.Add(message);
			logger.Log("Processing Outward Report ...");
			OutwardReport.MessageProcessor messageProcessor = new OutwardReport.MessageProcessor(logger);
			messageProcessor.ProcessMessage(message);
		}

		#endregion
	}
}
