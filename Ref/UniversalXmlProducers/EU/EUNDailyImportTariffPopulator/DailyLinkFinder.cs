using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public class DailyLinkFinder : IDailyLinkFinder
	{
		readonly IHttpClientHelper helper;
		readonly IDailyConfigProvider configProvider;

		public DailyLinkFinder(IHttpClientHelper helper, IDailyConfigProvider configProvider)
		{
			this.helper = helper;
			this.configProvider = configProvider;
		}

		HtmlDocument PageContent
		{
			get
			{
				if (pageContent == null)
				{
					pageContent = new HtmlDocument();
					pageContent.LoadHtml(helper.GetWebPageAsync(configProvider.DailyXMLDistributionUrl).Result);
				}
				return pageContent;
			}
		}
		HtmlDocument pageContent;

		public IEnumerable<string> GetIncrementalObjectTraderExportLinkOrderByPublishDateAscending()
		{
			var links = GetLinksFromDay1OnCurrentMonth();
			var orderedLinks = links.OrderBy(x => x.PublishDate).Select(x => x.IncrementalObjectTraderExportLink);
			return orderedLinks;
		}

		public IEnumerable<string> GetIncrementalObjectTraderExport_DeclarableGoodsNomenclatureLinkOrderByPublishDateAscending()
		{
			var links = GetLinksFromDay1OnCurrentMonth();
			var orderedLinks = links.OrderBy(x => x.PublishDate).Select(x => x.IncrementalObjectTraderExportDeclarableGoodsNomenclatureLink);
			return orderedLinks;
		}

		public DateTime GetPublicationTime()
		{
			if (!string.IsNullOrEmpty(configProvider.DailyPublicationTime_alternative))
			{
				return DateTime.Parse(configProvider.DailyPublicationTime_alternative, CultureInfo.InvariantCulture);
			}
			if (publicationTime != DateTime.MinValue)
			{
				return publicationTime;
			}
			var links = GetLinksFromDay1OnCurrentMonth();
			return links.Max(x => x.PublishDate);
		}

		readonly DateTime publicationTime = DateTime.MinValue;
		readonly string defaultLinkPrefix = "IncrementalObjectTraderExport";

		IDownloadEntity GetOrCreateDownloadEntity(DateTime publishDate)
		{
			if (_downloadEntities == null)
			{
				_downloadEntities = new List<IDownloadEntity>();
			}
			if (_downloadEntities.Any(x => x.PublishDate == publishDate))
			{
				return _downloadEntities.FirstOrDefault(x => x.PublishDate == publishDate);
			}
			var newDownloadEntity = new DownloadEntity(publishDate);
			_downloadEntities.Add(newDownloadEntity);

			return newDownloadEntity;
		}

		IEnumerable<IDownloadEntity> GetLinksFromDay1OnCurrentMonth()
		{
			if (_downloadEntities != null)
			{
				return _downloadEntities;
			}
			_downloadEntities = new List<IDownloadEntity>();
			var links = PageContent.DocumentNode.SelectNodes($"//a[contains(@href, '{defaultLinkPrefix}')]").AsEnumerable();
			var rootUrl = configProvider.DailyXMLDistributionUrl.Trim('/');
			foreach (var link in links)
			{
				var lastModified = link.NextSibling.InnerText;
				var regex = new Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}");
				var dateStr = regex.Match(lastModified).Value;

				var relativeUrl = link.GetAttributeValue("href", string.Empty);
				var downloadEntity = GetOrCreateDownloadEntity(DateTime.Parse(dateStr, CultureInfo.InvariantCulture));
				if (relativeUrl.Contains("DeclarableGoodsNomenclature"))
				{
					downloadEntity.IncrementalObjectTraderExportDeclarableGoodsNomenclatureLink = $"{rootUrl}/{relativeUrl}";
				}
				else
				{
					downloadEntity.IncrementalObjectTraderExportLink = $"{rootUrl}/{relativeUrl}";
				}
			}
			//remove all records that don't belong to the current month.
			var publicationTime = GetPublicationTime();
			var publicationTimeMonth = publicationTime.Month;
			var publicationTimeDay = publicationTime.Day;
			_downloadEntities.RemoveAll(x => x.PublishDate.Month != publicationTimeMonth || x.PublishDate.Day > publicationTimeDay);
			return _downloadEntities;
		}
		List<IDownloadEntity> _downloadEntities;
	}
}
