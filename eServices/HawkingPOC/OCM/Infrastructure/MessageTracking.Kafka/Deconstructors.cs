using Confluent.Kafka;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka
{
	static class Deconstructors
	{
		public static void Deconstruct(this Error error, out ErrorCode code, out string source, out string reason)
		{
			code = error.Code;
			reason = error.Reason;
			source = error.IsBrokerError ? "Broker" :
					 error.IsLocalError ? "Local" :
					 "Unknown";
		}
	}
}
