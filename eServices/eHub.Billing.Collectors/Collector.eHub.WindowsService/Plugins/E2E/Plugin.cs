using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using System;
using System.IO;
using System.Linq;
using CargoWise.Billing.CollectorService.Plugin;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.E2E
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.E2E.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var typeKeysCollection = GetString(record, "TypeKeysCollection");
			var typeKeysArray = typeKeysCollection.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			var transactions = new HashSet<TimeStampedTransaction>();

			foreach (var typeKeys in typeKeysArray)
			{
				var typeKeyArray = typeKeys.Split(new char[] { ':' });

				if (!typeKeyArray.Any()) continue;

				var topLevelTypeKey = typeKeyArray.ElementAtOrDefault(0);

				if (!string.IsNullOrEmpty(topLevelTypeKey))
				{
					AddIfNotExisting(transactions, new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
					{
						Category = "E2E",
						PriceItemCode = GetPriceItemCode(GetString(record, "UniType")),
						BillableCount = 1,
						ReportingSource = "HUB",
						ServiceOccuredUTC = GetDateTime(record, "AM_SentToRecipientUTC"),
						ClientID = GetString(record, "RecipientID"),
						MessageTrackingID = messageTrackingID,
						Reference1 = GetString(record, "SenderID"),
						Reference2 = TruncateToFitReference(topLevelTypeKey),
					}));

					var level1SubShipmentTypeKey = typeKeyArray.ElementAtOrDefault(1);

					if (!string.IsNullOrEmpty(level1SubShipmentTypeKey))
					{
						AddIfNotExisting(transactions, new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
						{
							Category = "E2E",
							PriceItemCode = GetPriceItemCode(GetString(record, "UniType")),
							BillableCount = 1,
							ReportingSource = "HUB",
							ServiceOccuredUTC = GetDateTime(record, "AM_SentToRecipientUTC"),
							ClientID = GetString(record, "RecipientID"),
							MessageTrackingID = messageTrackingID,
							Reference1 = GetString(record, "SenderID"),
							Reference2 = TruncateToFitReference(topLevelTypeKey),
							Reference3 = TruncateToFitReference(level1SubShipmentTypeKey),
						}));

						var level2SubShipmentTypeKey = typeKeyArray.ElementAtOrDefault(2);

						if (!string.IsNullOrEmpty(level2SubShipmentTypeKey))
						{
							AddIfNotExisting(transactions, new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
							{
								Category = "E2E",
								PriceItemCode = GetPriceItemCode(GetString(record, "UniType")),
								BillableCount = 1,
								ReportingSource = "HUB",
								ServiceOccuredUTC = GetDateTime(record, "AM_SentToRecipientUTC"),
								ClientID = GetString(record, "RecipientID"),
								MessageTrackingID = messageTrackingID,
								Reference1 = GetString(record, "SenderID"),
								Reference2 = TruncateToFitReference(topLevelTypeKey),
								Reference3 = TruncateToFitReference(level1SubShipmentTypeKey),
								Reference4 = TruncateToFitReference(level2SubShipmentTypeKey),
							}));

							var level3SubShipmentTypeKey = typeKeyArray.ElementAtOrDefault(3);

							if (!string.IsNullOrEmpty(level3SubShipmentTypeKey))
							{
								AddIfNotExisting(transactions, new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
								{
									Category = "E2E",
									PriceItemCode = GetPriceItemCode(GetString(record, "UniType")),
									BillableCount = 1,
									ReportingSource = "HUB",
									ServiceOccuredUTC = GetDateTime(record, "AM_SentToRecipientUTC"),
									ClientID = GetString(record, "RecipientID"),
									MessageTrackingID = messageTrackingID,
									Reference1 = GetString(record, "SenderID"),
									Reference2 = TruncateToFitReference(topLevelTypeKey),
									Reference3 = TruncateToFitReference(level1SubShipmentTypeKey),
									Reference4 = TruncateToFitReference(level2SubShipmentTypeKey),
									Reference5 = TruncateToFitReference(level3SubShipmentTypeKey),
								}));
							}
						}
					}
				}
			}

			foreach (var transaction in transactions)
			{
				yield return transaction;
			}
		}

		void AddIfNotExisting(HashSet<TimeStampedTransaction> transactions, TimeStampedTransaction transaction)
		{
			if (!transactions.Contains(transaction))
			{
				transactions.Add(transaction);
			}
		}

        string TruncateToFitReference(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            return input.Length <= 50 ? input : input.Substring(0, 50);
        }

        string GetPriceItemCode(string universalType)
        {
	        switch (universalType)
	        {
				case "SHIPMENT": return "E2E";
				case "TRANSACTION": return "E2T";
				default: throw new InvalidDataException("Unrecognised universal type: " + universalType);
			}
        }
	}
}
