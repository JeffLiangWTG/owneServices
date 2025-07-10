using System;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	[Serializable]
	public class MessageGenerationException : ZException
	{
		public MessageGenerationException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public MessageGenerationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
