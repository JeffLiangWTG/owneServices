using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public abstract class SGMessageProcessor : CustomsMessageProcessor
	{
		public SGMessageProcessor(LoggingInformation logger, string msgType3CharCode, string messageName)
			: base(logger, msgType3CharCode, messageName)
		{
		}

		#region Overrides

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			return "subclasses do this processing";
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return SGCustomsDataRegistry.Instance.SendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return SGCustomsDataRegistry.Instance.SendAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return SGCustomsDataRegistry.Instance.SendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return SGCustomsDataRegistry.Instance.SendImpediments.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return SGCustomsDataRegistry.Instance.SendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return SGCustomsDataRegistry.Instance.SendErrors.Value; }
		}

		#endregion

		protected bool ProcessMessage()
		{
			bool result = false;

			var matchingMessage = OutgoingMessage;
			if (matchingMessage != null)
			{
				entry = LoadLinkedEntry(matchingMessage);
				if (entry != null)
				{
					entry.Messages.Add(IncomingMessage);
					IncomingMessage.EM_ApplicationReference = matchingMessage.EM_ApplicationReference;
					SetEntryStatus();
					result = true;
				}
			}

			return result;
		}

		protected internal EmailDef responseEmail = new EmailDef();
		protected CusEntryHeader entry;
		protected abstract void SetEntryStatus();
		protected abstract string URN { get; }
		protected abstract EDIMessage IncomingMessage { get; }
		protected EDIMessage OutgoingMessage
		{
			get
			{
				if (outgoingMessage == null || outgoingMessage.EM_ApplicationReference != IncomingMessage.EM_ApplicationReference || outgoingMessage.EM_ApplicationCode != IncomingMessage.EM_ApplicationCode)
				{
					return outgoingMessage = FindMatchingMessage();
				}
				else
				{
					return outgoingMessage;
				}
			}
		}
		EDIMessage outgoingMessage;

		protected BusinessObjectFactory Factory
		{
			get { return IncomingMessage.Factory; }
		}

		CusEntryHeader LoadLinkedEntry(EDIMessage matchingMessage)
		{
			return Factory.Load<CusEntryHeader>(matchingMessage.EM_LinkUniqueID);
		}

		EDIMessage FindMatchingMessage()
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, URN);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, IncomingMessage.EM_ApplicationCode);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.OrderBy = $"{EDIMessageSchema.EM_SystemCreateTimeUtc.Name} desc";

			return Factory.LoadTop1<EDIMessage>(query);
		}
	}
}
