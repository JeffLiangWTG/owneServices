using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors
{
	public abstract class EDIFACTMessageProcessor : ApplicationTypeMessageProcessor, IErrorNotification
	{
		protected EDIFACTMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			try
			{
				ProcessMessageInternal(ediMessage);
			}
			catch (MessageProcessorException ex)
			{
				MessageProcessorErrorReporter.ProcessException(ex, true);
			}
			catch (InvalidFormatException ex)
			{
				MessageProcessorErrorReporter.ProcessException(new InvalidFormatMessageProcessorException(ex, ediMessage, this), true);
			}
		}

		#region Implementation

		void ProcessMessageInternal(EDIMessage ediMessage)
		{
			var processor = GetMessageProcessor(ediMessage);
			if (processor != null)
			{
				processor.ProcessMessage(ediMessage);

				if (ediMessage.EM_Status == EDIMessage.Status.Queued)
				{
					throw new CriticalMessageProcessorException("Message status was not set", "The message processor has processed message but has not set its status", ediMessage, this);
				}
			}
			else
			{
				throw new CriticalMessageProcessorException("Supporting Processor Class", "Could not find a supporting Processor Class", ediMessage, this);
			}
		}

		protected abstract CustomsMessageProcessor GetMessageProcessor(EDIMessage ediMessage);

		#region Implementation of IErrorNotification

		void IErrorNotification.SendError(EmailDef email)
		{
			//Should send to Post Master Group only
		}

		void IErrorNotification.SendErrorToPostMaster(EmailDef email)
		{
			Env.OutgoingCustomsMailManager.CreateAndSave(email, Env.Registry.PostMasterGroup, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
		}

		void IErrorNotification.LogError(string errorMessage)
		{
			Logger.LogError(errorMessage);
		}

		string IErrorNotification.MessageProcessorName
		{
			get { return MessageFriendlyName; }
		}

		#endregion

		#endregion
	}
}
