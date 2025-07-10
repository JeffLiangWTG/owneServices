using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class SGInboundEDIMessageProcessor : ApplicationTypeMessageProcessor
	{
		public SGInboundEDIMessageProcessor(LoggingInformation logger, ZString applicationCode)
			: base(logger)
		{
			this.applicationCode = applicationCode;
		}

		readonly ZString applicationCode;

		protected override string MessageFriendlyNameCore
		{
			get { return "SG Customs Message Processor"; }
		}

		protected override string ApplicationCodeCore
		{
			get { return applicationCode; }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			try
			{
				var edifactMessage = ((SGEDIMessage)message).TradeNetMessage;
				Logger.DebugLog(edifactMessage.GetType().ToString());

				var processor = GetMessageProcessor(edifactMessage);
				if (processor == null)
				{
					Logger.Log("Unrecognised message.");
					message.EM_Status = EDIMessage.Status.Error;
				}
				else
				{
					processor.ProcessMessage(message);
				}
			}
			catch (Edifact.InvalidFormatException)
			{
				Logger.Log("Invalid Edifact format.");
				message.EM_Status = EDIMessage.Status.Failed;
			}
		}

		MessageProcessor GetMessageProcessor(Edifact.Auto.SegmentGroup edifactMessage)
		{
			return CheckForD09BMessages(edifactMessage);
		}

		/// <summary>
		/// TradeNet 4.1 Messages
		/// <returns></returns>
		MessageProcessor CheckForD09BMessages(Edifact.Auto.SegmentGroup edifactMessage)
		{
			MessageProcessor result = null;

			if (edifactMessage is Edifact.D09B.Messages.CUSPMT.CUSPMTMessage)
			{
				result = new Cuspmt09bMessageProcessor(Logger);
			}
			else if (edifactMessage is Edifact.D09B.Messages.TCODEC.TCODECMessage)
			{
				result = new Tcodec09bMessageProcessor(Logger);
			}
			else if (edifactMessage is Edifact.D09B.Messages.CUSRES.CUSRESMessage)
			{
				result = new Cusres09bMessageProcessor(Logger);
			}
			else if (edifactMessage is Edifact.D09B.Messages.APERAK.APERAKMessage)
			{
				result = new Aperak09bMessageProcessor(Logger);
			}
			else if (edifactMessage is Edifact.D09B.Messages.IFTRIN.IFTRINMessage)
			{
				result = new Iftrin09bMessageProcessor(Logger);
			}
			else if (edifactMessage is Edifact.D09B.Messages.DEBADV.DEBADVMessage)
			{
				result = new Debadv09bMessageProcessor(Logger);
			}
			else if (edifactMessage is Edifact.D09B.Messages.CLASET.CLASETMessage)
			{
				result = new Claset09bMessageProcessor(Logger);
			}

			return result;
		}
	}
}
