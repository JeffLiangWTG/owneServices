using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class TariffAdditional : Scraper
	{
		public TariffAdditional(IWebSourceProvider sourceProvider, string url) : base(sourceProvider, url)
		{
		}

		public List<TariffCode> AdditionalTariff { get; } = new List<TariffCode>();

		public override void Decode()
		{
			var hsCodePre = Document.DocumentNode.SelectNodes("//h2")[0].InnerText.Split('-').First().Trim();
			Node = Document.DocumentNode.SelectNodes("//table[@class='naccs']")[0];

			foreach (var row in Node.SelectNodes("tr").Skip(1))
			{
				var tds = row.SelectNodes("td");

				var tariffCode = new TariffCode(SourceProvider, Url)
				{
					HSCode = hsCodePre + '-' + tds[0].InnerText.Trim(),
					NaccsCode = tds[1].InnerText.Trim(),
					DescriptionJapanese = tds[2].InnerText.Trim(),
					Description = tds[2].InnerText.Trim()
				};

				AdditionalTariff.Add(tariffCode);
			}
		}

		public override void Pack() => AdditionalTariff.ForEach(t => Records.Add(t.PackTariff()));
	}
}
