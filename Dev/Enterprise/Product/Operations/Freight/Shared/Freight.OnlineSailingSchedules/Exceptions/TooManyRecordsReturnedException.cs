using System;

namespace Enterprise.Freight.OnlineSailingSchedules.Exceptions
{
	[Serializable]
	public class TooManyRecordsReturnedException : OnlineSailingSchedulesException
	{
		public TooManyRecordsReturnedException()
			: this("")
		{
		}

		public TooManyRecordsReturnedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public TooManyRecordsReturnedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
