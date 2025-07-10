using System;
using System.Collections.Generic;

namespace Enterprise.Freight.OnlineSailingSchedules.Exceptions
{
	[Serializable]
	public class BadRequestException : OnlineSailingSchedulesException
	{
		public string TraceId { get; }

		public IReadOnlyList<string> ProblemMessages { get; }

		public BadRequestException(string message, IReadOnlyList<string> problemMessages, string traceId) : base(message)
		{
			ProblemMessages = problemMessages ?? throw new ArgumentNullException(nameof(ProblemMessages));
			TraceId = traceId;
		}

#if NETFRAMEWORK
		public BadRequestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			if (info == null)
			{ throw new ArgumentNullException(nameof(info)); }

			info.AddValue(nameof(TraceId), TraceId);

			base.GetObjectData(info, context);
		}
#endif
	}
}
