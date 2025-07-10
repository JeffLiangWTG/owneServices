using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class TariffRate : Scraper
	{
		public TariffRate(string url) : base(url)
		{
			rates = new Dictionary<string, string>();
		}

		readonly Dictionary<string, string> rates;

		public override void Locate() => Node = Document.DocumentNode.SelectNodes("//table[@class='detail2']")[0];

		public override void Load() { }

		public override void Decode()
		{
			foreach (var rateHeader in RateHeaders)
			{
				rates.Add(rateHeader, string.Empty);
			}

			foreach (var node in Node.SelectNodes("tr").Skip(1))
			{
				var nodes = node.SelectNodes("th|td");
				var header = nodes[0].InnerText.Trim();

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

		public static readonly string[] RateHeaders = new[] { "General", "Temporary", "WTO", "GSP", "LDC" };
	}
}
