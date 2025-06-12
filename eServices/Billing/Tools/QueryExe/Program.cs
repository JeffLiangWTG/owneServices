using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess;

namespace CargoWise.eServices.Billing.QueryExe
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length < 3)
			{
				Console.WriteLine("Please specify the plugin suffix name, start and end time in UTC format");
				Console.WriteLine("Example: WiseCloudSQLAccess 2019-08-22T00:01:20Z 2019-08-23T23:02:38Z");

				Environment.Exit(0);
			}

			var pluginKey = args[0];
			var utcStart = Helper.ISO8601StringToDateTimeUtc(args[1]);
			var utcEnd = Helper.ISO8601StringToDateTimeUtc(args[2]);

			var pluginType = $"CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.{pluginKey}.Plugin";
			var plugin = (IBillingTransactionsPlugin)Activator.CreateInstance("CargoWise.eServices.Billing.Collector.Misc.WindowsService", pluginType).Unwrap();

			if (plugin == null)
			{
				Environment.Exit(0);
			}

			try
			{
				var serviceSettings = ServiceSettings.ReadFromFile("plugins_settings.xml");
				var settings = serviceSettings.Plugins.FirstOrDefault(x => x.Key == pluginKey);
				if (settings == null)
				{
					Console.WriteLine($"No plugin settings found for {pluginKey}");
					Environment.Exit(0);
				}

				plugin.UpdateSettings(settings.PluginSettings);

				var billingTransactions = plugin.GetTransactions(utcStart, utcEnd).ToArray();
				Console.WriteLine($"Total: {billingTransactions.Count()} records retrieved for date range from: {utcStart:s}Z to {utcEnd:s}Z");

				if (billingTransactions.Count() > 0)
				{
					var headerRecord = billingTransactions[0];
					var headers = new List<string>
					{
						$"{nameof(headerRecord.TimeStamp)}",
						$"{nameof(headerRecord.BillingTransaction.Version)}",
						$"{nameof(headerRecord.BillingTransaction.Category)}",
						$"{nameof(headerRecord.BillingTransaction.PriceItemCode)}",
						$"{nameof(headerRecord.BillingTransaction.BillableCount)}",
						$"{nameof(headerRecord.BillingTransaction.ReportingSource)}",
						$"{nameof(headerRecord.BillingTransaction.ServiceOccuredUTC)}",
						$"{nameof(headerRecord.BillingTransaction.ClientID)}",
						$"{nameof(headerRecord.BillingTransaction.ClientNumber)}",
						$"{nameof(headerRecord.BillingTransaction.ClientStaffCode)}",
						$"{nameof(headerRecord.BillingTransaction.Branch)}",
						$"{nameof(headerRecord.BillingTransaction.Reference1)}",
						$"{nameof(headerRecord.BillingTransaction.Reference2)}",
						$"{nameof(headerRecord.BillingTransaction.Reference3)}",
						$"{nameof(headerRecord.BillingTransaction.Reference4)}",
						$"{nameof(headerRecord.BillingTransaction.Reference5)}",
						$"{nameof(headerRecord.BillingTransaction.MessageTrackingID)}"
					};

					Console.WriteLine(string.Join(",", headers));

					foreach (var transaction in billingTransactions)
					{
						var values = new List<string>
						{
							$"{transaction.TimeStamp:s}Z",
							$"{transaction.BillingTransaction.Version}",
							$"{transaction.BillingTransaction.Category}",
							$"{transaction.BillingTransaction.PriceItemCode}",
							$"{transaction.BillingTransaction.BillableCount}",
							$"{transaction.BillingTransaction.ReportingSource}",
							$"{transaction.BillingTransaction.ServiceOccuredUTC:s}Z",
							$"{transaction.BillingTransaction.ClientID}",
							$"{transaction.BillingTransaction.ClientNumber}",
							$"{transaction.BillingTransaction.ClientStaffCode}",
							$"{transaction.BillingTransaction.Branch}",
							$"{transaction.BillingTransaction.Reference1}",
							$"{transaction.BillingTransaction.Reference2}",
							$"{transaction.BillingTransaction.Reference3}",
							$"{transaction.BillingTransaction.Reference4}",
							$"{transaction.BillingTransaction.Reference5}",
							$"{transaction.BillingTransaction.MessageTrackingID}"
						};

						Console.WriteLine(string.Join(",", values));
					}
				}
			}
			catch (FormatException)
			{
				Console.WriteLine("No plugins found in the settings file.");
				Environment.Exit(0);
			}
		}
	}
}
