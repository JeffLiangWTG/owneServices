using System.Collections.Generic;
using System.Linq;
using System;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class TariffRateEpa : Scraper
	{
		public TariffRateEpa(string url) : base(url)
		{
			rates = new Dictionary<string, string>();
		}

		readonly Dictionary<string, string> rates;

		public override void Load() { }

		public override void Locate() => Node = Document.DocumentNode.SelectNodes("//table[@class='detail2']")[2];

		public override void Decode()
		{
			foreach (var rateHeader in RateHeaders)
			{
				rates.Add(rateHeader, string.Empty);
			}

			foreach (var node in Node.SelectNodes("tr").Skip(1))
			{
				var nodes = node.SelectNodes("th|td");
				var header = PurgeInvalidChar(nodes[0].InnerText.Trim());

				if (RateHeaders.Any(c => c.Equals(header, StringComparison.OrdinalIgnoreCase)))
				{
					rates[header] = Decode(nodes[1].InnerText.Trim());
				}
				else
				{
					Console.WriteLine($"Missing {header} in {Url}.");
				}
			}
		}

		public override void Pack() => Record.AddProperties(rates.Values.ToArray());

		public static DynamicCsvRecord Header { get => new DynamicCsvRecord(RateHeaders); }

		static string PurgeInvalidChar(string sourceString)
		{
			return new string(sourceString.Where(c => char.IsLetterOrDigit(c) || c == '(' || c == ')').ToArray());
		}

		public readonly static string[] RateHeaders = new[]
		{
			"Singapore",
			"Mexico",
			"Malaysia",
			"Chile",
			"Thailand",
			"Indonesia",
			"Brunei",
			"ASEAN",
			"Philippines",
			"Switzerland",
			"VietNam",
			"India",
			"Peru",
			"Australia",
			"Mongolia",
			"CPTPP",
			"EU",
			"UK",
			"ASEANAustraliaNewZealand(RCEP)",
			"China(RCEP)",
			"Korea(RCEP)",
			"JapanUSTradeAgreement"
		};
	}
}
