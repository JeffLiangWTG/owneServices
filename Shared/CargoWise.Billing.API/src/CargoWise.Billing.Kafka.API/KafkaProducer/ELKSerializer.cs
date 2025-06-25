using System;
using System.Text;
using CargoWise.Billing.API;
using Confluent.Kafka;

namespace CargoWise.Billing.Kafka.API
{
	public class ELKSerializer<T> : ISerializer<T> where T : ELKTransaction
	{
		public ELKSerializer() : this(new DateTimeProvider()) { }
		public ELKSerializer(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		public byte[] Serialize(T data, SerializationContext context)
		{
			if (data != null)
			{
				if (data is UsageTransaction transaction)
				{
					return Encoding.UTF8.GetBytes(TransactionHelper.ConvertUsageInfoToJson(transaction, dateTimeProvider.DateTimeNow));
				}

				throw new InvalidOperationException($"Unknown data type {data.GetType()}");
			}

			return null;
		}

		readonly IDateTimeProvider dateTimeProvider;
	}
}
