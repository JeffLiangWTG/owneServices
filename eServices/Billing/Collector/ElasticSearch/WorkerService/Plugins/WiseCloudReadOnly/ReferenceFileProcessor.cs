using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Billing.CollectorService.Plugin;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly
{
public static class ReferenceFileProcessor
	{
		public static Dictionary<string, List<string>> ProcessReferenceFile(string refFilePath, IErrorReportingClient errorReportingClient, ILogger log)
		{

			if (Path.GetExtension(refFilePath)?.ToLower() != ".csv")
			{
				throw new NotSupportedException("The extension of the reference file is incorrect. Please provide a valid .csv file.");
			}

			if (!File.Exists(refFilePath))
			{
				throw new FileNotFoundException("Reference file path does not exist.", refFilePath);
			}
			var hasErrors = false;
			using (var fileStream = new FileStream(refFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(fileStream))
            {
                if (reader.EndOfStream)
                {
					return new Dictionary<string, List<string>>();
				}
                CheckReferenceFileHeader(reader);
				var customerCodeIPs = ReadandProcessRawContentFromReferenceFile(reader, log, ref hasErrors);
				if (hasErrors)
				{
					var error = "Reference file has invalid lines. Check log for which lines are invalid.";
					errorReportingClient?.ReportToIssueManager(error, new FormatException(), log);
				}

				return customerCodeIPs;
			}
		}

		static Dictionary<string, List<string>> ReadandProcessRawContentFromReferenceFile(StreamReader reader, ILogger log, ref bool hasErrors)
		{
			var customerCodeIPCollection = new Dictionary<string, List<string>>();
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (ValidateLineFormat(line.Replace(" ", "")))
					{
						var values = line.Split(',');
						var item = new CustomerCodeIP(values[0], values[1]);
						ProcessRawContent(item, log, customerCodeIPCollection, ref hasErrors);
					}
					else
					{
						hasErrors = true;
						log.LogError($"Reference file has incorrect customer code or IP format: {line}");
					}
				}
			}

			return RemoveAmbiguousRecords(customerCodeIPCollection, log);
		}

		static bool ValidateLineFormat(string line)
		{
			return Regex.IsMatch(line, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(\s*-\s*\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}|/\d{1,2})?,Customer-\w{6}-\d{2,3}$");
		}

		static void CheckReferenceFileHeader(StreamReader reader)
		{
			var header = reader.ReadLine();
			var values = header?.Split(',');
			if (values?.Length != 2)
			{
				throw new InvalidDataException("Reference file has incorrect column count");
			}
			if (values[0] != "Customer IP Address" || values[1] != "Firewall Object Name")
			{
				throw new InvalidDataException("Reference file has incorrect column names");
			}
		}

		public static Dictionary<string, List<string>> ProcessRawContent(CustomerCodeIP item, ILogger log, Dictionary<string, List<string>> customerCodeIPCollection, ref bool hasErrors)
		{
				var customerCodeValues = item.CustomerCodeCollection[0].Split('-');
				if (item.CustomerIP.Contains("/"))
				{
					hasErrors = true;
					customerCodeIPCollection.Add(item.CustomerIP, new List<string>() { "XXXXXX" });
					log.LogError($"Please replace subnet in {item.CustomerIP} with IP range. Customer code: {item.CustomerCodeCollection[0]}");
				}
				else if (item.CustomerIP.Contains("-"))
				{
					var ipRange = item.CustomerIP.Split('-');
					var startIp = Ip4ToLong(ipRange[0].Trim());
					var endIp = Ip4ToLong(ipRange[1].Trim());
					for (var current = startIp; current <= endIp; ++current)
					{
						var ipAddress = LongToIP4(current);
						if (customerCodeIPCollection.TryGetValue(ipAddress, out var customers))
						{
							customers.Add(customerCodeValues[1]);
						}
						else
						{
							customerCodeIPCollection.Add(ipAddress, new List<string>() { customerCodeValues[1] });
						}
					}
				}
				else
				{
					if (customerCodeIPCollection.TryGetValue(item.CustomerIP, out var customers))
					{
						customers.Add(customerCodeValues[1]);
					}
					else
					{
						customerCodeIPCollection.Add(item.CustomerIP, new List<string>() { customerCodeValues[1] } );
					}
				}
				return customerCodeIPCollection;
		}

		static Dictionary<string, List<string>> RemoveAmbiguousRecords(Dictionary<string, List<string>> customerCodeIPCollection, ILogger log)
		{
			var codeIPsWithNoAmbiguousItem = new Dictionary<string, List<string>>();
			var combinedList = new Dictionary<string, List<string>>();

			foreach (var kvp in customerCodeIPCollection)
			{
				if(kvp.Value.Count() == 1)
				{
					codeIPsWithNoAmbiguousItem.Add(kvp.Key, kvp.Value);
				}
				else if (kvp.Value.Count() > 1)
				{
					var customersWithCommonIP = string.Join(", ", kvp.Value);
					if (!combinedList.ContainsKey(customersWithCommonIP))
					{
						combinedList.Add(customersWithCommonIP, new List<string>());
					}

					combinedList[customersWithCommonIP].Add(kvp.Key);
				}
			}
			foreach (var item in combinedList)
			{
				var items = item.Value
					.Select(Ip4ToLong)
					.OrderBy(i => i)
					.Select((n, i) => new { number = n, group = n - i })
					.GroupBy(n => n.group)
					.Select(g => g.Count() >= 3
						? LongToIP4(g.First().number) + "-" + LongToIP4(g.Last().number)
						: string.Join(", ", g.Select(x => LongToIP4(x.number)))
					)
					.ToList();
				log.LogInformation($"Ambiguous records found: Customers {item.Key} share same IP {string.Join("\r\n\t", items)}");
			}
			return codeIPsWithNoAmbiguousItem;
		}

		static bool IsValidIP(string ip)
		{
			var parts = ip.Trim().Split('.').Select(long.Parse).ToArray();
			return parts.Length == 4 && parts.All(i => (i >= 0) && (i <= 255));
		}

		public static long Ip4ToLong(string ip)
		{
			if (!IsValidIP(ip))
			{
				throw new ArgumentException("Invalid IP : " + ip);
			}
			var parts = ip.Trim().Split('.').Select(long.Parse).ToArray();
			return (parts[0] << 24) | (parts[1] << 16) | (parts[2] << 8) | parts[3];
		}

		public static string LongToIP4(long longIP)
		{
			var ip = string.Empty;
			for (var i = 0; i < 4; i++)
			{
				var num = (int)(longIP / Math.Pow(256, (3 - i)));
				longIP = longIP - (long)(num * Math.Pow(256, (3 - i)));
				if (i == 0)
				{
					ip = num.ToString();
				}
				else
				{
					ip = ip + "." + num.ToString();
				}
			}
			if (!IsValidIP(ip))
			{
				throw new ArgumentException("LongToIP4 returns invalidIP " + ip);
			}
			return ip;
		}
	}
}
