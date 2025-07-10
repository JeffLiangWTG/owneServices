using System;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	[Serializable]
	public class MessageProcessingException : Messaging.Business.MessageProcessingException
	{
		public MessageProcessingException(ZString exceptionMesssage, NZMMessage message, bool shouldSendEmailToUsers, bool shouldSendDeveloperInformation)
			: base(exceptionMesssage + "\r\n\r\n" + message.EM_MessageText + "\r\n", ZString.Empty, shouldSendEmailToUsers, shouldSendDeveloperInformation)
		{
		}

#if NETFRAMEWORK
		protected MessageProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
