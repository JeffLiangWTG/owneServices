using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ClientMappings
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ClientMappings.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();

			yield return CreateTransaction(record, messageTrackingID, "CMP");

			var insecure = InsecureFTP.IsMatch(GetString(record, "SenderID"), GetString(record, "RecipientID"), GetString(record, "TS_Name"));
			if (insecure)
			{
				yield return CreateTransaction(record, messageTrackingID, "CMF");
			}
		}

		private TimeStampedTransaction CreateTransaction(IDataRecord record, string messageTrackingID, string priceItemCode)
		{
			return new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
			{
				BillableCount = GetInt(record, "BillableCount"),
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = messageTrackingID,
				Reference2 = GetString(record, "FileName", true),
				Reference3 = GetString(record, "TS_BillingElement"),
				Reference4 = GetString(record, "TS_Name"),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "CMP",
				PriceItemCode = priceItemCode,
				MessageTrackingID = messageTrackingID,
			});
		}

		public virtual ClientMappingsInsecureFTP InsecureFTP
		{
			get
			{
				if (_insecureFTP == null)
				{
					var csvFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Plugins\\ClientMappings\\ClientMappingsInsecureFTP.csv");
					_insecureFTP = new ClientMappingsInsecureFTP(csvFilePath);
				}
				return _insecureFTP;
			}
		}

		private ClientMappingsInsecureFTP _insecureFTP;
	}
}
