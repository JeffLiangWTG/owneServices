using System;
using Common.Logging;
using Confluent.Kafka;

namespace CargoWise.eServices.Billing.WcfService.Hangfire
{
	public sealed class IssuerReporterDeserializerProxy<T> : IDeserializer<T>
	{
		private readonly IDeserializer<T> innerDeserializer;
		internal static ILog Logger = LogManager.GetLogger(typeof(IssuerReporterDeserializerProxy<T>));

		public IssuerReporterDeserializerProxy(IDeserializer<T> deserializer)
		{
			innerDeserializer = deserializer;
		}

		public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
		{
			try
			{
				return innerDeserializer.Deserialize(data, isNull, context);
			}
			catch (Exception ex)
			{
				var content = string.Empty;
				if (!isNull)
				{
					content = BitConverter.ToString(data.ToArray());
				}

				var exception = new AggregateException($"Invalid message data: {content}", ex);
				IssueReporter.ReportToIssueManager($"{innerDeserializer.GetType()}", exception, Logger);
				throw;
			}
		}
	}
}
