using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudBillingSolarwindsOct21toApr22
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudBillingSolarwindsOct21toApr22.Query.sql"; }
		}
		IEnumerable<TimeStampedTransaction> CreateBillingTransaction(IDataRecord record, Dictionary<string, List<string>> customerCodeIPs)
		{

			var transaction = CreateBilling(customerCodeIPs, record);
			if (transaction != null)
			{
				yield return new TimeStampedTransaction(transaction.ServiceOccuredUTC, transaction);
			}
		}

		WiseCloudSQLAccessRecord ProcessQueryResult(Dictionary<string, List<string>> customerCodeIPCollection, IDataRecord queryResults)
		{
			var sourceIP = GetValueAsString(queryResults, "SourceIP");
			var destinationIP = GetValueAsString(queryResults, "DestinationIP");
			var egressBytes = GetValueAsString(queryResults, "EgressBytes");
			var minTimeStamp = GetDateTime(queryResults, "MinTimeStamp");
			var maxTimeStamp = GetDateTime(queryResults, "MaxTimeStamp");

			var record = new WiseCloudSQLAccessRecord(IPAddress.Parse(sourceIP.ToString()).ToString(), IPAddress.Parse(destinationIP.ToString()).ToString(), egressBytes, minTimeStamp, maxTimeStamp.ToString());

			var customerIp = string.Empty;
			if (customerCodeIPCollection.TryGetValue(record.SourceIp, out var customerCodeCollection) && customerCodeCollection?.Count != 0)
			{
				customerIp = record.SourceIp;
			}
			else if (customerCodeIPCollection.TryGetValue(record.DestinationIp, out customerCodeCollection) && customerCodeCollection?.Count != 0)
			{
				customerIp = record.DestinationIp;
			}
			if (!string.IsNullOrEmpty(customerIp) && customerCodeCollection?.Count != 0)
			{
				var customerCode = customerCodeCollection[0];
				record.CustomerCode = customerCode.Insert(3, "???");
				record.CustomerIp = customerIp;
			}
			if (record.HasDataConsumption() && record.HasCustomer())
			{
				return record;
			}
			return null;
		}

		 BillingTransaction CreateBilling(Dictionary<string, List<string>> customerCodeIPCollection, IDataRecord queryResults)
		{
			var record = ProcessQueryResult(customerCodeIPCollection, queryResults);
			if (record != null)
			{
				return new BillingTransaction
				{
					BillableCount = record.TotalMegaBytesConsumed,
					Category = Category,
					PriceItemCode = PriceItemCode,
					ClientID = record.CustomerCode,
					Reference1 = record.SourceIp,
					Reference2 = record.DestinationIp,
					Reference3 = record.MaxTimeStampString,
					ReportingSource = ReportingSource,
					ServiceOccuredUTC = record.MinTimeStamp
				};
			}
			return null;
		}
		 protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		 {
			 throw new NotImplementedException();
		 }
		protected string PriceItemCode => "#HG";
		protected string Category => "HOS";
		protected string ReportingSource => "MSC";
		protected string ReferenceFilePath { get; set; }
		protected string ReferenceFileServerUsername { get; set; }
		protected string ReferenceFileServerPassword { get; set; }
		protected string ReferenceFileServerDomainName { get; set; }
		public virtual Impersonator GetImpersonator() => new Impersonator(Logger);

		public override void UpdateSettings(PluginSettings settings)
		{
			base.UpdateSettings(settings);
			ReferenceFilePath = settings.Parameters.SingleOrDefault(p => p.Name == "ReferenceFilePath")?.Value;
			ReferenceFileServerUsername = settings.Parameters.SingleOrDefault(p => p.Name == "ReferenceFileServerUsername")?.Value;
			ReferenceFileServerPassword = settings.Parameters.SingleOrDefault(p => p.Name == "ReferenceFileServerPassword")?.Value;
			ReferenceFileServerDomainName = settings.Parameters.SingleOrDefault(p => p.Name == "ReferenceFileServerDomainName")?.Value;
		}
		public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
		{
			var num = 0;
			var customerCodeIPs = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(ReferenceFilePath, ErrorReportingClient, Logger);
			using (var connection = OpenConnection())
			using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
			using (var command = CreateCommand(connection, transaction, start, end))
			using (var impersonator = GetImpersonator())
			using (var reader = command.ExecuteReader())
			{
				impersonator.Impersonate(ReferenceFileServerUsername, ReferenceFileServerPassword, ReferenceFileServerDomainName);
				if (customerCodeIPs.Count <= 0)
				{
					yield break;
				}
				Logger.LogInformation($"CustomerCodeIP collection has {customerCodeIPs.Count} records");

				while (reader.Read())
				{
					List<TimeStampedTransaction> recordTransactions;
					try
					{
						recordTransactions = CreateBillingTransaction(reader, customerCodeIPs).ToList();
					}
					catch (Exception e) 
					{
						ErrorReportingClient?.ReportToIssueManager($"Could not create billing transactions from the data record [{DataRecordToString(reader)}]", e, Logger);
						continue;
					}

					foreach (var recordTransaction in recordTransactions)
					{
						num++;
						yield return recordTransaction;
					}
				}
				Logger.LogInformation($"Finished processing for the current period, Number of record is  {num}");
			}
		}
	}
}
