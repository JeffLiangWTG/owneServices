using System;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	[Serializable]
	public class MessageProcessingException : ZException
	{
		public MessageProcessingException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public MessageProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
