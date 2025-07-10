using System;

namespace Enterprise.Freight.OnlineSailingSchedules.Exceptions
{
	[Serializable]
	public class InternalServerErrorException : OnlineSailingSchedulesException
	{
		public InternalServerErrorException()
			: this("")
		{
		}

		public InternalServerErrorException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public InternalServerErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
