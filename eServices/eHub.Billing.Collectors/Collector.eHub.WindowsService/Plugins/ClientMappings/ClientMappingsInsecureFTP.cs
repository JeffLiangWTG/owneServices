using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ClientMappings
{
	public class ClientMappingsInsecureFTP
	{
		public ClientMappingsInsecureFTP(string csvFilePath)
		{
			if (string.IsNullOrEmpty(csvFilePath)) throw new ArgumentException("File name cannot be null or empty", nameof(csvFilePath));
			Load(csvFilePath);
		}

		private List<(Regex Sender, Regex Recipient, string Interface)> InsecureFTPList = new List<(Regex, Regex, string)>();

		private void Load(string csvFilePath)
		{
			if (!File.Exists(csvFilePath))
				throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

			using (var reader = File.OpenText(csvFilePath))
			using (var csv = new CsvReader(reader))
			{
				foreach (var record in csv.GetRecords<InsecureFTPRecord>())
				{
					InsecureFTPList.Add((
						new Regex(record.Sender, RegexOptions.Compiled),
						new Regex(record.Recipient, RegexOptions.Compiled),
						record.Interface
					));
				}
			}
		}

		public bool IsMatch(string senderId, string recipientId, string interfaceName)
		{
			return InsecureFTPList.Any(i =>
				i.Sender.IsMatch(senderId) &&
				i.Recipient.IsMatch(recipientId) &&
				string.Equals(i.Interface, interfaceName, StringComparison.InvariantCultureIgnoreCase));
		}

		private class InsecureFTPRecord
		{
			public string Sender { get; set; }
			public string Recipient { get; set; }
			public string Interface { get; set; }
		}
	}
}
