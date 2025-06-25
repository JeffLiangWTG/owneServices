using System;
using System.IO;
using CargoWise.Billing.API;
using Confluent.Kafka;
using ProtoBuf;
using SerializationContext = Confluent.Kafka.SerializationContext;

namespace CargoWise.Billing.Kafka.API
{
	public class BillingTransactionsDeserializer : IDeserializer<BillingTransaction>
	{
		public BillingTransaction Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
		{
			if (isNull)
				throw new NullReferenceException("data is null");
			try
			{
				using (var ms = new MemoryStream(data.ToArray()))
				{
					var protoBufTransaction = ProtoBuf.Serializer.Deserialize<Billing.Kafka.API.BillingTransactionProtoBuf>(ms);
					return TransactionHelper.MapFromProtoBuf(protoBufTransaction);
				}
			}
			catch (ProtoException ex)
			{
				throw new ProtoException("Deserialization error occurred during proto buffer processing.", ex);
			}
			catch (InvalidCastException ex)
			{
				throw new InvalidCastException("Error occurred while mapping from ProtoBuf transaction to API transaction.", ex);
			}
			catch (Exception ex)
			{
				throw new ProtoException("An error occurred during deserialization", ex);
			}
		}
	}
}
