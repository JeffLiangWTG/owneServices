using NUnit.Framework;
using ProtoBuf;
using CargoWise.Billing.API;
using CargoWise.Billing.Kafka.API;
using System;

namespace CargoWise.Billing.Kafka.API.Tests
{
	[TestFixture]
	public class BillingTransactionTests
	{
		[Test]
		public void TestProtobufSerializationAndDeserializationSuccess()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ",
				ClientNumber = "222222222222222222222222222222222222222222222222222",
				ClientStaffCode = "ABC",
				PriceItemCode = "DEF",
				Reference2 = "REFERENCE 2",
				Reference3 = "REFERENCE 3",
				Reference4 = "REFERENCE 4",
				Reference5 = "REFERENCE 5",
				MessageTrackingID = "MessageTrackingID",
				ReportingSource = "XYZ",
				ServiceOccuredUTC = DateTime.UtcNow
			};

			var serializer = new BillingTransactionsSerializer<BillingTransaction>();
			byte[] serializedData = serializer.Serialize(transaction, new Confluent.Kafka.SerializationContext());

			var deserializer = new BillingTransactionsDeserializer();
			var deserializedTransaction = deserializer.Deserialize(serializedData, false, new Confluent.Kafka.SerializationContext());

			Assert.AreEqual(transaction.BillableCount, deserializedTransaction.BillableCount);
			Assert.AreEqual(transaction.ClientID, deserializedTransaction.ClientID);
			Assert.AreEqual(transaction.ClientNumber, deserializedTransaction.ClientNumber);
			Assert.AreEqual(transaction.ClientStaffCode, deserializedTransaction.ClientStaffCode);
			Assert.AreEqual(transaction.PriceItemCode, deserializedTransaction.PriceItemCode);
			Assert.AreEqual(transaction.Reference2, deserializedTransaction.Reference2);
			Assert.AreEqual(transaction.Reference3, deserializedTransaction.Reference3);
			Assert.AreEqual(transaction.Reference4, deserializedTransaction.Reference4);
			Assert.AreEqual(transaction.Reference5, deserializedTransaction.Reference5);
			Assert.AreEqual(transaction.MessageTrackingID, deserializedTransaction.MessageTrackingID);
			Assert.AreEqual(transaction.ReportingSource, deserializedTransaction.ReportingSource);
			Assert.AreEqual(transaction.ServiceOccuredUTC, deserializedTransaction.ServiceOccuredUTC);
		}

		[Test]
		public void TestProtobufDeserializationThrowsProtoException()
		{
			var serializer = new BillingTransactionsSerializer<string>();
			byte[] serializedData = serializer.Serialize("invalid", new Confluent.Kafka.SerializationContext());

			var deserializer = new BillingTransactionsDeserializer();

			var ex = Assert.Throws<ProtoException>(() =>
			{
				deserializer.Deserialize(serializedData, false, new Confluent.Kafka.SerializationContext());
			});
			Assert.AreEqual("Deserialization error occurred during proto buffer processing.", ex.Message);
		}
	}
}
