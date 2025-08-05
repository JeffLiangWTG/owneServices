using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccessNew
{
	public class CustomerCodeIP
	{
		public string CustomerIP { get; set; }
		public List<string> CustomerCodeCollection { get; set; } = new List<string>();
		public CustomerCodeIP(string customerIP, string customerCode)
		{
			CustomerIP = customerIP;
			CustomerCodeCollection.Add(customerCode);
		}
	}

	public class Plugin : AbstractPlugin // Actual class not matching file name infuriates me. This would be changed if the config xml for eservice plugins was readily available.
	{
		protected override ILogger CreateLogger() => string.IsNullOrEmpty(LoggerName) ? base.CreateLogger() : loggerFactory?.CreateLogger(LoggerName);
		public override void UpdateSettings(PluginSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			ReferenceFilePath = settings.Parameters.Single(p => p.Name == "ReferenceFilePath").Value;
			ReferenceFileServerUsername = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "ReferenceFileServerUsername"));
			ReferenceFileServerPassword = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "ReferenceFileServerPassword"));
			ReferenceFileServerDomainName = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "ReferenceFileServerDomainName"));
			SolarWindsServerUrl = settings.Parameters.Single(p => p.Name == "SolarWindsServerUrl").Value;
			SolarWindsFirewallDistinguishingMasks =  settings.Parameters.Where(p => p.Name == "SolarWindsFirewallDistinguishingMask" && !string.IsNullOrEmpty(p.Value));
			SolarWindsInterfaceDistinguishingMasks = settings.Parameters.Where(p => p.Name == "SolarWindsInterfaceDistinguishingMask" && !string.IsNullOrEmpty(p.Value));
			SolarWindsPortNumber = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "SolarWindsPortNumber"));
			SolarWindsPortNumberRange = settings.Parameters.Where(p => p.Name == "SolarWindsPortNumberRange" && !string.IsNullOrEmpty(p.Value));
			SolarWindsUserName = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "SolarWindsUserName"));
			SolarWindsPassword = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "SolarWindsPassword"));
			ExcludingIPWhiteList = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "ExcludingIPWhiteList"));
			ExcludingIPRegex = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "ExcludingIPRegex"));
			AryakaPrivateIPs = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "AryakaPrivateIPs"));
			LoggerName = ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "LoggerName"));

			BufferOffsetInHours = 0;
			if (int.TryParse(ValueOrEmptyString(settings.Parameters.FirstOrDefault(p => p.Name == "BufferOffsetInHours")), out int buffertOffset))
			{
				BufferOffsetInHours = buffertOffset;
			}
		}

		public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime utcStart, DateTime utcEnd)
		{
			Logger.LogInformation($"Start processing {WiseCloudSQLAccessBilling}.");
			Logger.LogInformation($"Input dates from '{utcStart:u}' to '{utcEnd:u}'.");

			using (var impersonator = GetImpersonator())
			{
				StartAndEndDateValidation(utcStart, utcEnd);
				impersonator.Impersonate(ReferenceFileServerUsername, ReferenceFileServerPassword, ReferenceFileServerDomainName);
				var customerCodeIPs = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(ReferenceFilePath, ErrorReportingClient, Logger);

				if (customerCodeIPs.Count <= 0)
				{
					yield break;
				}

				Logger.LogInformation($"CustomerCodeIP collection has {customerCodeIPs.Count} records");

				var billableTransactions = new List<BillingTransaction>();

				// offset the date time range by the config to cover a buffer if needed
				var startTimeWithOffset = utcStart.AddHours(BufferOffsetInHours);
				var endTimeWithOffset = utcEnd.AddHours(BufferOffsetInHours);

				var startHour = new DateTime(startTimeWithOffset.Year, startTimeWithOffset.Month, startTimeWithOffset.Day, startTimeWithOffset.Hour, 0, 0, DateTimeKind.Utc);
				var endHour = new DateTime(endTimeWithOffset.Year, endTimeWithOffset.Month, endTimeWithOffset.Day, endTimeWithOffset.Hour, 0, 0, DateTimeKind.Utc);
				Logger.LogInformation($"Query hours from '{startHour}' inclusive to '{endHour} exclusive'.");

				// loop through the hours to the end
				var fromHour = startHour;
				while (fromHour < endHour)
				{
					var toHour = fromHour.AddHours(1);
					var rows = QueryReadOnlySqlDataFromSolarwinds(fromHour, toHour).ToArray();

					Logger.LogInformation($"QueryReadOnlySqlDataFromSolarwinds returned {rows.Length} records in date range '{fromHour}' to '{toHour}'");

					if (rows.Any())
					{
						billableTransactions.AddRange(CreateBillingTransactions(customerCodeIPs, rows));
					}

					fromHour = toHour;
				};

				Logger.LogInformation($"Total number of billing transactions : {billableTransactions.Count}.");
				foreach (var transaction in billableTransactions)
				{
					yield return new TimeStampedTransaction(transaction.ServiceOccuredUTC, transaction);
				}

				Logger.LogInformation($"Finished processing {WiseCloudSQLAccessBilling}.");
			}
		}

		public bool StartAndEndDateValidation(DateTime utcStart, DateTime utcEnd)
		{
			if (utcStart >= utcEnd)
			{
				throw new ArgumentException($"The start time ({utcStart:u}) is later than the end time ({utcEnd:u}).");
			}

			return true;
		}

		IEnumerable<XElement> QueryReadOnlySqlDataFromSolarwinds(DateTime StartUtc, DateTime EndUtc)
		{
			// we have 1 day of corrupted data we need to ignore.

			var startOfBadData = new DateTime(2019, 1, 2, 13, 0, 0);
			var endOfBadData = new DateTime(2019, 1, 3, 13, 0, 0);

			if (EndUtc > startOfBadData && EndUtc < endOfBadData)
			{
				EndUtc = startOfBadData;
			}
			if (StartUtc > startOfBadData && StartUtc < endOfBadData)
			{
				StartUtc = endOfBadData;
			}

			var client = GetInformationServiceClient();
			client.ClientCredentials.UserName.UserName = SolarWindsUserName;
			client.ClientCredentials.UserName.Password = SolarWindsPassword;

			client.Open();
			var query = $@"SELECT
				F.SourceIP,
				F.DestinationIP,
				SUM(F.EgressBytes) AS[EgressBytes],
				MIN(F.TimeStamp) AS[TimeStamp],
				MAX(F.TimeStamp) AS[MaxTimeStamp]
			FROM
				Orion.Nodes AS N
				INNER JOIN Orion.NPM.Interfaces AS I ON I.NodeID = N.NodeID
				INNER JOIN Orion.Netflow.FlowsByConversation  AS F ON F.NodeID = N.NodeID
			WHERE
				{FormatFirwallDistinguishingMask()}
				{FormatInterfaceDistinguishingMask()}
				AND
				({FormatPortNumber()})
				AND
				F.TimeStamp > '{StartUtc:s}Z'
				AND
				F.TimeStamp <= '{EndUtc:s}Z'
			Group by F.SourceIP, F.DestinationIP";

			var result = client.QueryXml(query, null);
			client.Close();
			XNamespace solarWindsNamespace = Helper.SolarwindsNamespace;
			return XElement.Parse(result.OuterXml).Descendants(solarWindsNamespace + "row");
		}

		string FormatFirwallDistinguishingMask() => !SolarWindsFirewallDistinguishingMasks.Any() ? "" : $@"({string.Join(" OR ", SolarWindsFirewallDistinguishingMasks.Select(x => $"N.Caption like '%{x.Value}%'"))})";
		string FormatInterfaceDistinguishingMask() => !SolarWindsInterfaceDistinguishingMasks.Any() ? "" : $@"AND ({string.Join(" OR ", SolarWindsInterfaceDistinguishingMasks.Select(x => $"I.Caption like '%{x.Value}%'"))})";

		string FormatPortNumber() => string.Join(" OR ", new string[] { FormatPortNumberSingle(), FormatPortNumberRange() }.Where(s => !string.IsNullOrEmpty(s)));

		string FormatPortNumberSingle() => string.IsNullOrEmpty(SolarWindsPortNumber) ? "" : string.Join(" OR ", SolarWindsPortNumber.Split(';').Select(x => $"F.Port = {x}"));

		string FormatPortNumberRange()
		{
			return !SolarWindsPortNumberRange.Any() ? "" : string.Join(" OR ", SolarWindsPortNumberRange.Select(x => $"(F.Port >= {x.Value.Substring(0, x.Value.IndexOf("-"))} AND F.Port <= {x.Value.Substring(x.Value.IndexOf('-') + 1)})"));
		}

		// required to be both virtual and public for mock override
		public virtual IInformationServiceClient GetInformationServiceClient()
		{
			ServicePointManager.ServerCertificateValidationCallback = (We, Just, Return, True) => true;
			var swis = new BasicHttpBinding
			{
				Name = "BasicHttpBinding_InformationService",
				SendTimeout = new TimeSpan(0, 0, 0, 1800),
				OpenTimeout = new TimeSpan(0, 0, 0, 1800),
				MaxReceivedMessageSize = int.MaxValue,
				MaxBufferPoolSize = int.MaxValue,
				MaxBufferSize = int.MaxValue,
				ReaderQuotas =
				{
					MaxDepth = int.MaxValue,
					MaxStringContentLength = int.MaxValue,
					MaxArrayLength = int.MaxValue,
					MaxBytesPerRead = int.MaxValue,
					MaxNameTableCharCount = int.MaxValue
				},
				Security =
				{
					Mode = BasicHttpSecurityMode.Transport,
					Transport = {ClientCredentialType = HttpClientCredentialType.Basic}
				}
			};

			var address = new EndpointAddress(SolarWindsServerUrl);
			var client = new InformationServiceClient(swis, address);

			return client;
		}

		public virtual Impersonator GetImpersonator() => new Impersonator(Logger);

		public IEnumerable<BillingTransaction> CreateBillingTransactions(Dictionary<string, List<string>> customerCodeIPCollection, IEnumerable<XElement> rows)
		{
			var records = ProcessQueryResult(customerCodeIPCollection, rows);
			return from record in records
				   let billingTransaction = new BillingTransaction
				   {
					   BillableCount = record.TotalMegaBytesConsumed,
					   Category = Category,
					   PriceItemCode = PriceItemCode,
					   ClientID = record.CustomerCode,
					   Reference1 = record.SourceIp,
					   Reference2 = record.DestinationIp,
					   Reference3 = record.MaxTimeStampString,
					   ReportingSource = ReportingSource,
					   ServiceOccuredUTC = record.MinTimeStamp,
				   }
				   select billingTransaction;
		}

		IEnumerable<WiseCloudSQLAccessRecord> ProcessQueryResult(Dictionary<string, List<string>> customerCodeIPCollection, IEnumerable<XElement> rows)
		{
			var records = new List<WiseCloudSQLAccessRecord>();

			foreach (var item in rows)
			{
				var record = CreateRecord(item, customerCodeIPCollection);
				if (record.HasDataConsumption() && record.HasCustomer())
				{
					records.Add(record);
				}
			}

			return records.ToArray();
		}

		WiseCloudSQLAccessRecord CreateRecord(XElement item, Dictionary<string, List<string>> customerCodeIPCollection)
		{
			var record = new WiseCloudSQLAccessRecord(item);
			var customerIp = string.Empty;
			var isSourceExcluded = IsExcludedFilterIp(record.SourceIp);
			var isDestinationExcluded = IsExcludedFilterIp(record.DestinationIp);
			var isSourceInAryakaRange = IsIpInCidrRanges(record.SourceIp);
			var isDestinationInAryakaRange = IsIpInCidrRanges(record.DestinationIp);
			if (isSourceExcluded || isDestinationExcluded || isSourceInAryakaRange || isDestinationInAryakaRange)
			{
				return record;
			}
			if ( customerCodeIPCollection.TryGetValue(record.SourceIp, out var customerCodeCollection) && customerCodeCollection?.Count != 0)
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

			return record;
		}

		private  bool IsExcludedFilterIp(string ipAddress)
		{
			if (string.IsNullOrEmpty(ExcludingIPRegex) )
			{
				return false;
			}
			var regex = new Regex(ExcludingIPRegex);
			var whitelisted = ExcludingIPWhiteList?.Split(';').Any(p => p.Contains(ipAddress)) ?? false;
			return regex.IsMatch(ipAddress) && !whitelisted;

		}

		private bool IsIpInCidrRanges(string ip)
		{
			IPAddress ipAddress = IPAddress.Parse(ip);
			if (!string.IsNullOrEmpty(AryakaPrivateIPs))
			{
				var cidrRanges = AryakaPrivateIPs.Split(';');
				foreach (var cidr in cidrRanges)
				{
					if (IsIpInRange(ipAddress, cidr))
					{
						return true;
					}
				}
			}

			return false;
		}
		private bool IsIpInRange(IPAddress ip, string cidr)
		{
			var parts = cidr.Split('/');
			IPAddress cidrIp = IPAddress.Parse(parts[0]);
			int bits = Convert.ToInt32(parts[1]);

			byte[] ipBytes = ip.GetAddressBytes();
			byte[] cidrIpBytes = cidrIp.GetAddressBytes();
			byte[] maskBytes = new byte[ipBytes.Length];

			for (int i = 0; i < bits / 8; i++)
			{
				maskBytes[i] = 0xFF; // 255
			}
			if (bits % 8 != 0)
			{
				maskBytes[bits / 8] = (byte)(255 << (8 - (bits % 8)));
			}
			for (int i = 0; i < ipBytes.Length; i++)
			{
				if ((ipBytes[i] & maskBytes[i]) != (cidrIpBytes[i] & maskBytes[i]))
				{
					return false;
				}
			}
			return true;
		}
		const string WiseCloudSQLAccessBilling = "WiseCloudSQLAccessBilling";

                protected string PriceItemCode => "#HI";
		protected string Category => "HOS";
		protected string ReportingSource => "MSC";

		#region Settings
		string ReferenceFilePath { get; set; }
		string ReferenceFileServerUsername { get; set; }
		string ReferenceFileServerPassword { get; set; }
		string ReferenceFileServerDomainName { get; set; }
		string SolarWindsServerUrl { get; set; }
		string SolarWindsFirewallDistinguishingMask { get; set; }
		IEnumerable<PluginParameter> SolarWindsInterfaceDistinguishingMasks { get; set; }
		IEnumerable<PluginParameter> SolarWindsFirewallDistinguishingMasks { get; set; }
		string SolarWindsPortNumber { get; set; }
		IEnumerable<PluginParameter> SolarWindsPortNumberRange { get; set; }
		string SolarWindsUserName { get; set; }
		string SolarWindsPassword { get; set; }
		int BufferOffsetInHours { get; set; }
		string LoggerName { get; set; }
		string ExcludingIPWhiteList { get; set; }
		string ExcludingIPRegex { get; set; }
		string AryakaPrivateIPs { get; set; }
		#endregion
	}
}
