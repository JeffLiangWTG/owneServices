using System;

namespace Enterprise.Freight.OnlineSailingSchedules.Exceptions
{
	[Serializable]
	public class UnauthorizedException : OnlineSailingSchedulesException
	{
		public UnauthorizedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public UnauthorizedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
