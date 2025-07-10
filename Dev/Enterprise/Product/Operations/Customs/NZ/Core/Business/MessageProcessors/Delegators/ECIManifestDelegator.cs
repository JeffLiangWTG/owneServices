using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	class ECIManifestDelegator : IProcessorDelegator
	{
		#region IProcessorDelegator Members

		public string MessageFriendlyName
		{
			get { return ECIWriteOff.Manifesting.MessageProcessor.MessageFriendlyName; }
		}

		public bool CanProcess(Declaration.NZCMessage message)
		{
			ZString sendersReference = message.MessageAsCUSRESD98A.UNH[0].CommonAccessReference;
			entryHeader = Declaration.ECIWriteOff.Manifesting.CusEntryHeader.Load(message.Factory, sendersReference);
			return entryHeader != null;
		}
		Declaration.ECIWriteOff.Manifesting.CusEntryHeader entryHeader;

		public void Process(LoggingInformation logger, Declaration.NZCMessage message)
		{
			entryHeader.Messages.Add(message);
			MessageProcessor messageProcessor = new ECIWriteOff.Manifesting.MessageProcessor(logger);

			logger.Log("Processing " + MessageFriendlyName + "...");
			message.EM_MessageType = messageProcessor.GetMessageTypeDelegate(message);
			message.EM_MessageSubType = messageProcessor.GetMessageTypeDelegate(message);

			messageProcessor.ProcessMessage(message);
		}

		#endregion
	}
}
