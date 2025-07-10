using System;

namespace Enterprise.Freight.OnlineSailingSchedules.Exceptions
{
	[Serializable]
	public class OnlineSailingSchedulesException : Exception
	{
		public OnlineSailingSchedulesException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public OnlineSailingSchedulesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
