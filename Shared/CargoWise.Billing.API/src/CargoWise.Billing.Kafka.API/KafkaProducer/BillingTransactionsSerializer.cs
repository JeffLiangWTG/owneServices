using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Confluent.Kafka;

namespace CargoWise.Billing.Kafka.API
{
	public class BillingTransactionsSerializer<T> : ISerializer<T>
	{
		public byte[] Serialize(T data, SerializationContext context)
		{
			if (data != null)
			{
				if (data is Billing.API.BillingTransaction transaction)
				{
					var billingTransactionProtoBuf = TransactionHelper.MapToProtoBuf(transaction);
					using (var ms = new MemoryStream())
					{
						ProtoBuf.Serializer.Serialize(ms, billingTransactionProtoBuf);
						return ms.ToArray();
					}
				}

				if (data is string s)
				{
					return string.IsNullOrEmpty(s) ? null : Encoding.UTF8.GetBytes(s);
				}

				throw new InvalidOperationException($"Billing kafka process does not support type {data.GetType()}");
			}

			return null;
		}
	}
}
