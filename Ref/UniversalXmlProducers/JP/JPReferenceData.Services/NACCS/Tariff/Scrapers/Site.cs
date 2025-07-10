using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class UrlInfo
	{
		public int Section { get; set; }

		public int Chapter { get; set; }

		public string ChapterUrl { get; set; }
	}

	public class Site : Scraper
	{
		public Site(IWebSourceProvider sourceProvider, string url)
			: base(sourceProvider, url)
		{
			PublishDate = GetPublishDate();
		}

		public bool AlreadyDownloaded { get => PublishDate.Equals(GetPublishDateFromCsv(), StringComparison.OrdinalIgnoreCase); }

		public override void Locate()
		{
			var nodes = Document.DocumentNode.SelectNodes("//td[@class='cd']");
			var lastSection = 0;

			foreach (var node in nodes)
			{
				var text = node.InnerText ?? string.Empty;
				var index = text.Split(' ').LastOrDefault();

				if (text.StartsWith("section ", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(index, out var section))
				{
					lastSection = section;
				}
				if (text.StartsWith("chapter ", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(index, out var chapter))
				{
					var match = ChapterRegex.Match(node.InnerHtml);
					if (match.Success)
					{
						UrlInfos.Add(new UrlInfo { Section = lastSection, Chapter = chapter, ChapterUrl = match.Value.Replace(ChapterIndexRoute, ChapterDetailsRoute) });
					}
				}
			}
		}

		public override void Decode()
		{
			if (SourceProvider.IsParallelMode)
			{
				var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 10) };
				Console.WriteLine($"Start Decode. MaxDegreeOfParallelism: {options.MaxDegreeOfParallelism}");

				Parallel.ForEach(UrlInfos, options, DecodeCore);
			}
			else
			{
				UrlInfos.ForEach(DecodeCore);
			}
		}

		void DecodeCore(UrlInfo urlInfo)
		{
			var chapter = new Chapter(SourceProvider, urlInfo.ChapterUrl, urlInfo.Chapter, urlInfo.Section);
			chapter.Translate();

			if (SourceProvider.IsParallelMode)
			{
				ChaptersParallel.Add(chapter);
			}
			else
			{
				Chapters.Add(chapter);
			}
		}

		public override void Pack()
		{
			Records.Add(new DynamicCsvRecord(nameof(PublishDate), PublishDate));
			Records.Add(Tariff.Header);

			var chapterGroups = SourceProvider.IsParallelMode
				? ChaptersParallel.GroupBy(c => c.Section)
				: Chapters.GroupBy(c => c.Section);

			foreach (var chapterGroup in chapterGroups.OrderBy(c => c.Key))
			{
				PackCore(chapterGroup);
			}

			if (IsImport)
			{
				foreach (var otherTariffScraper in GetOtherScrapers())
				{
					otherTariffScraper.Load();
					otherTariffScraper.Pack(Records);
					otherTariffScraper.Release();
				}
			}
		}

		IEnumerable<IOtherScraper> GetOtherScrapers()
		{
			yield return new TariffDecliningBalance(SourceProvider, AppConfig.NACCS.CodeLists.JPNACCS98TariffDownloadUrl);
			yield return new TariffSmallAmountImportedGoods(SourceProvider, AppConfig.NACCS.CodeLists.JPNACCS99TariffDownloadUrl);
		}

		void PackCore(IGrouping<int, Chapter> group)
		{
			if (GlobalSections.TryGetValue(group.Key, out var record))
			{
				Records.Add(record);
			}

			var records = SourceProvider.IsParallelMode
				? group.OrderBy(g => g.Index).SelectMany(g => g.Records)
				: group.SelectMany(g => g.Records);

			Records.AddRange(records);
		}

		public override void Release()
		{
			base.Release();

			UrlInfos.Clear();
			Chapters.Clear();
			ChaptersParallel.Clear();
			GlobalSections.Clear();
		}

		string GetPublishDate()
		{
			base.Load();
			var nodes = Document.DocumentNode.SelectNodes("//td");
			var regex = new Regex("[0-9]{4}-[0-9]{2}-[0-9]{2}");
			foreach (var node in nodes)
			{
				var match = regex.Match(node.InnerText);
				if (match.Success)
				{
					return match.Value;
				}
			}
			throw new UnhandledApplicationException("Cannot retrieve publish date.");
		}

		string GetPublishDateFromCsv()
		{
			string date = null;
			if (File.Exists(FullPath))
			{
				using (var reader = new StreamReader(FullPath))
				using (var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration { Encoding = Encoding.Unicode, HasHeaderRecord = false, IgnoreBlankLines = false }))
				{
					while (csv.Read())
					{
						csv.TryGetField(1, out date);
						break;
					}
				}
			}
			return date;
		}

		static readonly Regex ImportChapterRegex = new Regex(@"https://www.kanzei.or.jp/statistical/tariff/headline/hs2dig/e/[0-9]{2,3}", RegexOptions.IgnoreCase);
		static readonly Regex ExportChapterRegex = new Regex(@"https://www.kanzei.or.jp/statistical/expstatis/headline/hs2dig/e/[0-9]{2,3}", RegexOptions.IgnoreCase);
		Regex ChapterRegex => IsImport ? ImportChapterRegex : ExportChapterRegex;
		const string ChapterIndexRoute = "hs2dig";
		const string ChapterDetailsRoute = "hs4dig";

		List<UrlInfo> UrlInfos { get; } = new List<UrlInfo>();
		List<Chapter> Chapters { get; } = new List<Chapter>();
		ConcurrentBag<Chapter> ChaptersParallel { get; } = new ConcurrentBag<Chapter>();
		public string PublishDate { get; }
	}
}
