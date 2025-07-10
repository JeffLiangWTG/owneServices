using System;
using System.Runtime.Serialization;

namespace Enterprise.Freight.AIS
{
	[Serializable]
	public class AisWebApiException : Exception
	{
		public AisWebApiException()
		{
		}

		public AisWebApiException(string message) : base(message)
		{
		}

#if NET
		[Obsolete]
#endif
		public AisWebApiException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
