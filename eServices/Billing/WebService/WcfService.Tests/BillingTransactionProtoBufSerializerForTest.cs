using System;
using System.IO;
using CargoWise.Billing.API;
using CargoWise.Billing.Kafka.API;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	public static class BillingTransactionProtoBufSerializerForTest
	{
		public static byte[] SerializeTransaction(BillingTransaction transaction)
		{
			var billingTransactionProtoBuf = MapToProtoBuf(transaction);
			using (var ms = new MemoryStream())
			{
				ProtoBuf.Serializer.Serialize(ms, billingTransactionProtoBuf);
				return ms.ToArray();
			}
		}

		public static BillingTransaction DeserializeTransaction(byte[] data)
		{
			using (var ms = new MemoryStream(data))
			{
				var billingTransactionProtoBuf = ProtoBuf.Serializer.Deserialize<CargoWise.Billing.Kafka.API.BillingTransactionProtoBuf>(ms);
				return MapFromProtoBuf(billingTransactionProtoBuf);
			}
		}

		private static BillingTransactionProtoBuf MapToProtoBuf(BillingTransaction transaction)
		{
			return new BillingTransactionProtoBuf()
			{
				BillableCount = transaction.BillableCount,
				Category = transaction.Category,
				PriceItemCode = transaction.PriceItemCode,
				ReportingSource = transaction.ReportingSource,
				ClientId = transaction.ClientID,
				ClientNumber = transaction.ClientNumber,
				ClientStaffCode = transaction.ClientStaffCode,
				Branch = transaction.Branch,
				Reference1 = transaction.Reference1,
				Reference2 = transaction.Reference2,
				Reference3 = transaction.Reference3,
				Reference4 = transaction.Reference4,
				Reference5 = transaction.Reference5,
				MessageTrackingId = transaction.MessageTrackingID,
				AdditionalRefs = transaction.AdditionalRefs,
				Version = transaction.Version,
				ServiceOccuredUtc = transaction.ServiceOccuredUTC
			};
		}

		private static BillingTransaction MapFromProtoBuf(BillingTransactionProtoBuf protoBufTransaction)
		{
			return new BillingTransaction
			{
				BillableCount = protoBufTransaction.BillableCount,
				Category = protoBufTransaction.Category,
				PriceItemCode = protoBufTransaction.PriceItemCode,
				ReportingSource = protoBufTransaction.ReportingSource,
				ClientID = protoBufTransaction.ClientId,
				ClientNumber = protoBufTransaction.ClientNumber,
				ClientStaffCode = protoBufTransaction.ClientStaffCode,
				Branch = protoBufTransaction.Branch,
				Reference1 = protoBufTransaction.Reference1,
				Reference2 = protoBufTransaction.Reference2,
				Reference3 = protoBufTransaction.Reference3,
				Reference4 = protoBufTransaction.Reference4,
				Reference5 = protoBufTransaction.Reference5,
				MessageTrackingID = protoBufTransaction.MessageTrackingId,
				AdditionalRefs = protoBufTransaction.AdditionalRefs,
				Version = protoBufTransaction.Version,
				ServiceOccuredUTC = protoBufTransaction.ServiceOccuredUtc
			};
		}
	}

}

