using System;
using CargoWise.Billing.API;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CargoWise.Billing.Kafka.API
{
	public static class TransactionHelper
	{
		public static string ConvertBillingInfoToJson(BillingTransaction transaction, DateTime? submitToELKTime = null)
		{
			var billingTransactionForJSON = new BillingTransactionForJSON
			{
				BillableCount = transaction.BillableCount,
				Branch = transaction.Branch,
				Category = transaction.Category,
				ClientID = transaction.ClientID,
				ClientNumber = transaction.ClientNumber,
				ClientStaffCode = transaction.ClientStaffCode,
				PriceItemCode = transaction.PriceItemCode,
				ReportingSource = transaction.ReportingSource,
				ServiceOccuredUTC = DateTime.SpecifyKind(transaction.ServiceOccuredUTC, DateTimeKind.Utc),
				Reference1 = transaction.Reference1,
				Reference2 = transaction.Reference2,
				Reference3 = transaction.Reference3,
				Reference4 = transaction.Reference4,
				Reference5 = transaction.Reference5,
				Version = transaction.Version,
				MessageTrackingID = transaction.MessageTrackingID
			};

			return ConvertObjectToJson(billingTransactionForJSON, billingTransactionForJSON.ServiceOccuredUTC, submitToELKTime, transaction.AdditionalRefs);
		}

		public static string ConvertUsageInfoToJson(UsageTransaction transaction, DateTime? submitToELKTime = null)
		{
			var usageTransactionForJSON = new UsageTransactionForJSON
			{
				UsageCount = transaction.UsageCount,
				ServiceOccuredUTC = DateTime.SpecifyKind(transaction.ServiceOccuredUTC, DateTimeKind.Utc),
				EnterpriseCode = transaction.EnterpriseCode,
				ServerCode = transaction.ServerCode,
				Environment = transaction.Environment,
				CompanyCode = transaction.CompanyCode,
				CompanyName = transaction.CompanyName,
				BranchCode = transaction.BranchCode,
				UsageCode = transaction.UsageCode,
			};

			return ConvertObjectToJson(usageTransactionForJSON, usageTransactionForJSON.ServiceOccuredUTC, submitToELKTime, transaction.AdditionalRefs);
		}

		public  static BillingTransactionProtoBuf MapToProtoBuf(Billing.API.BillingTransaction transaction)
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

		public static CargoWise.Billing.API.BillingTransaction MapFromProtoBuf(
			BillingTransactionProtoBuf protoBufTransaction)
		{
			return new CargoWise.Billing.API.BillingTransaction
			{
				BillableCount = protoBufTransaction.BillableCount,
				Category = protoBufTransaction.Category,
				PriceItemCode = protoBufTransaction.PriceItemCode,
				ReportingSource = protoBufTransaction.ReportingSource,
				ClientID = protoBufTransaction.ClientId,
				ClientNumber = protoBufTransaction.ClientNumber,
				ClientStaffCode = protoBufTransaction.ClientStaffCode,
				Branch =  protoBufTransaction.Branch,
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

		static string ConvertObjectToJson(object baseObject, DateTime timestamp, DateTime? submitToELKTime, string additionalRefs)
		{
			try
			{
				var jsonTransaction = JObject.FromObject(baseObject, new JsonSerializer()
				{
					NullValueHandling = NullValueHandling.Ignore,
					DefaultValueHandling = DefaultValueHandling.Ignore,
					Formatting = Formatting.None
				});

				jsonTransaction.AddFirst(new JProperty("@timestamp", timestamp));
				if (submitToELKTime.HasValue)
					jsonTransaction.Add(new JProperty("SubmitToELKTime", submitToELKTime));

				if (!string.IsNullOrEmpty(additionalRefs))
				{
					var jsonUsage = JObject.Parse(additionalRefs);
					foreach (var property in jsonUsage?.Children())
					{
						AddOrMergeProperty(jsonTransaction, property);
					}
				}

				return jsonTransaction.ToString(Formatting.None);
			}
			catch (Exception ex)
			{
				throw new ValidationException("Transaction validation failed.", ex);
			}
		}

		static void AddOrMergeProperty(JObject source, JToken target)
		{
			if (target.Type == JTokenType.Object)
			{
				source.Merge(target, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
			}
			else
			{
				if (target is JProperty targetProperty && source.TryGetValue(targetProperty.Name, out var sourceToken))
				{
					if (string.Compare(targetProperty.Value.ToString(), sourceToken.ToString(), StringComparison.InvariantCultureIgnoreCase) != 0)
					{
						throw new ArgumentException($"Can not add or merge property {targetProperty.Name}. A property with the same name and a different value already exists.");
					}
				}
				else
				{
					source.Add(target);
				}
			}
		}
	}
}
