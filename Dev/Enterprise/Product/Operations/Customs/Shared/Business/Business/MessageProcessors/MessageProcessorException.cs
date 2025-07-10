using System;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessageProcessors.ErrorReporting
{
	#region MessageProcessorException

	[Serializable]
	public class MessageProcessorException : Exception
	{
		public MessageProcessorException(string errorMessage, EDIMessage ediMessage, IErrorNotification messageProcessor)
			: this(errorMessage, ediMessage, messageProcessor, Res.GetString("206571c7-6ed4-476e-bc25-9996964c262b", "{0} Message Processor Error Report", messageProcessor.MessageProcessorName), false)
		{
		}

		public MessageProcessorException(string errorMessage, EDIMessage ediMessage, IErrorNotification messageProcessor, string subject, bool doNotReferenceMessage)
			: base(errorMessage)
		{
			EDIMessage = ediMessage;
			MessageProcessor = messageProcessor;
			Subject = subject;
			DoNotReferenceMessage = doNotReferenceMessage;
		}

#if NETFRAMEWORK
		public MessageProcessorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public IErrorNotification MessageProcessor { get; private set; }
		public EDIMessage EDIMessage { get; private set; }
		public string Subject { get; private set; }
		public bool DoNotReferenceMessage { get; private set; }
	}

	#endregion

	#region UnableToInterpretMessageException

	[Serializable]
	public class UnableToInterpretMessageException : MessageProcessorException
	{
		public UnableToInterpretMessageException(EDIMessage ediMessage, IErrorNotification messageProcessor)
			: base(Res.GetString("e03d76b9-be85-476f-a883-ff0bb878a18d", "The message processor was unable to interpret received message."), ediMessage, messageProcessor)
		{
		}

#if NETFRAMEWORK
		public UnableToInterpretMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion

	#region CouldNotFindLinkedObjectException

	[Serializable]
	public class CouldNotFindLinkedObjectException : MessageProcessorException
	{
		public CouldNotFindLinkedObjectException(string objectReference, EDIMessage ediMessage, IErrorNotification messageProcessor)
			: base(Res.GetString("252094ab-89c9-4dd1-b964-b550daff4aee", "Could not find an associated business object (Job) for document reference = '{0}'", objectReference), ediMessage, messageProcessor)
		{
		}

#if NETFRAMEWORK
		public CouldNotFindLinkedObjectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion

	#region CouldNotFindAssociatedTransmitMessageException

	[Serializable]
	public class CouldNotFindAssociatedTransmitMessageException : MessageProcessorException
	{
		public CouldNotFindAssociatedTransmitMessageException(EDIMessage ediMessage, IErrorNotification messageProcessor)
			: base(Res.GetString(
				"29e6c06d-ab8b-4eaa-b801-37ebef678042",
				"No associated transmit message has been found that matches the following details:\r\nApplication Code '{0}', Message Number '{1}'.",
				ediMessage.EM_ApplicationCode,
				ediMessage.EM_MessageNum),
				   ediMessage,
				   messageProcessor)
		{
		}

#if NETFRAMEWORK
		public CouldNotFindAssociatedTransmitMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion

	#region CriticalMessageProcessorException

	[Serializable]
	public class CriticalMessageProcessorException : MessageProcessorException
	{
		public CriticalMessageProcessorException(string errorType, string errorMessage, EDIMessage ediMessage, IErrorNotification messageProcessor)
			: base(errorMessage, ediMessage, messageProcessor)
		{
			ErrorType = errorType;
		}

#if NETFRAMEWORK
		public CriticalMessageProcessorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string ErrorType { get; private set; }
	}

	#endregion

	#region InvalidFormatMessageProcessorException

	[Serializable]
	public class InvalidFormatMessageProcessorException : CriticalMessageProcessorException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EDIFACT Internal Error")]
		public InvalidFormatMessageProcessorException(InvalidFormatException exception, EDIMessage ediMessage, IErrorNotification messageProcessor)
			: base("Invalid Format Exception", "The message processor has thrown an exception: " + exception.Message, ediMessage, messageProcessor)
		{
		}

#if NETFRAMEWORK
		public InvalidFormatMessageProcessorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion
}
