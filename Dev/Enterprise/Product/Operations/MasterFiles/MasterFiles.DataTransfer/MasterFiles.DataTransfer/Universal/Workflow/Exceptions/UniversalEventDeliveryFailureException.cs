using System;
using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer
{
	[Serializable]
	public sealed class UniversalEventDeliveryFailureException : Exception
	{
		public IEnumerable<Event> Events { get; }

		public UniversalEventDeliveryFailureException(IEnumerable<Event> events)
		{
			Events = events;
		}

		public UniversalEventDeliveryFailureException(IEnumerable<Event> events, string message) : base(message)
		{
			Events = events;
		}

		public UniversalEventDeliveryFailureException(IEnumerable<Event> events, string message, Exception innerException) : base(message, innerException)
		{
			Events = events;
		}

#if NETFRAMEWORK
		UniversalEventDeliveryFailureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
