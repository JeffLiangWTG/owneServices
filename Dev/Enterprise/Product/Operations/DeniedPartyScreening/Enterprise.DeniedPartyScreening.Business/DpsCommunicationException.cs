using System;
using System.ServiceModel;

namespace Enterprise.DeniedPartyScreening.Business
{
	[Serializable]
	public class DpsCommunicationException : CommunicationException
	{
		public DpsCommunicationException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DpsCommunicationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
