using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;
using System.Globalization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public class LinkFinder : ILinkFinder
	{
		public LinkFinder(IHttpClientHelper helper, IConfigProvider configProvider)
		{
			this.helper = helper;
			this.configProvider = configProvider;
		}
		readonly IHttpClientHelper helper;
		readonly IConfigProvider configProvider;

		HtmlDocument PageContent
		{
			get
			{
				if (pageContent == null)
				{
					pageContent = new HtmlDocument();
					pageContent.LoadHtml(helper.GetWebPageAsync(configProvider.XMLDistributionUrl).Result);
				}
				return pageContent;
			}
		}
		HtmlDocument pageContent;

		public DateTime GetPublicationTime()
		{
			if (!string.IsNullOrEmpty(configProvider.PublicationTime_alternative))
			{
				return DateTime.Parse(configProvider.PublicationTime_alternative, CultureInfo.InvariantCulture);
			}
			var publicationTime = GetLinks("Measure_").Max(x => x.PublishDate);
			Console.WriteLine($"PublicationTime for EUN Monthly Import Tariff is {publicationTime}");
			return publicationTime;
		}

		public string GetMeasureTypeLink()
		{
			return !string.IsNullOrEmpty(configProvider.MeasureTypeLink_alternative) ? configProvider.MeasureTypeLink_alternative : GetLink("MeasureType_");
		}

		public string GetRegulationLink()
		{
			return !string.IsNullOrEmpty(configProvider.RegulationLink_alternative) ? configProvider.RegulationLink_alternative : GetLink("Regulation_");
		}

		public string GetMeasureLink()
		{
			return !string.IsNullOrEmpty(configProvider.MeasureLink_alternative) ? configProvider.MeasureLink_alternative : GetLink("Measure_");
		}

		public string GetMeasureConditionCodeLink()
		{
			return !string.IsNullOrEmpty(configProvider.MeasureConditionCode_alternative) ? configProvider.MeasureConditionCode_alternative : GetLink("MeasureConditionCode_");
		}

		public string GetGoodsNomenclatureLink()
		{
			return !string.IsNullOrEmpty(configProvider.GoodsNomenclatureLink_alternative) ? configProvider.GoodsNomenclatureLink_alternative : GetLink("GoodsNomenclature_");
		}

		public string GetDeclarableGoodsNomenclatureLink()
		{
			return !string.IsNullOrEmpty(configProvider.DeclarableGoodsNomenclatureLink_alternative) ? configProvider.DeclarableGoodsNomenclatureLink_alternative : GetLink("DeclarableGoodsNomenclature_");
		}

		string GetLink(string prefix)
		{
			var link = GetLinks(prefix).OrderByDescending(x => x.PublishDate).FirstOrDefault().DownloadLink;
			Console.WriteLine($"File for {prefix.Trim('_')} will be downloaded from {link}");
			return link;
		}

		IEnumerable<IDownloadEntity> GetLinks(string prefix)
		{
			var rootUrl = configProvider.XMLDistributionUrl.Trim('/');
			var links = PageContent.DocumentNode.SelectNodes($"//a[starts-with(@href, '{prefix}')]")
				.Select(link =>
				{
					var lastModified = link.NextSibling.InnerText;
					var regex = new Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}");
					var dateStr = regex.Match(lastModified).Value;

					var relativeUrl = link.GetAttributeValue("href", string.Empty);
					var downloadLink = rootUrl + '/' + relativeUrl;

					return new DownloadEntity(DateTime.Parse(dateStr, CultureInfo.InvariantCulture), downloadLink);
				});
			return links;
		}
	}
}
