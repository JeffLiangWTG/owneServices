using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class Chapter : Scraper
	{
		public Chapter(IWebSourceProvider sourceProvider, string url, int index, int section) : base(sourceProvider, url)
		{
			Index = index;
			Section = section;
		}

		public int Index { get; }

		public int Section { get; }

		public List<Tariff> Tariffs { get; } = new List<Tariff>();

		public override void Locate()
		{
			var urls = new HashSet<string>();
			var matches = TariffDetailsRegex.Matches(Document.DocumentNode.InnerHtml);
			foreach (var match in matches.Cast<Match>())
			{
				if (match.Success)
				{
					var url = match.Value.Trim();
					if (urls.Add(url))
					{
						var tariff = new Tariff(SourceProvider, url) { Section = Section, ChapterCompositeKeys = ChapterCompositeKeys };
						Tariffs.Add(tariff);
					}
				}
			}
		}

		public override void Decode() => Tariffs.ForEach(DecodeCore);

		async void DecodeCore(Tariff tariff)
		{
			var retry = 0;

			do
			{
				try
				{
					tariff.Release();
					tariff.Translate();

					return;
				}
				catch (UnhandledApplicationException e)
				{
					Console.WriteLine($"Decode Error: [{e.Message}]. Retry: [{retry}]");
					await Task.Delay(TimeSpan.FromSeconds(5));
				}
			} while (retry++ < 10);
		}

		public override void Pack() => Tariffs.ForEach(t => Records.AddRange(t.Records));

		public override void Release()
		{
			base.Release();
			ChapterCompositeKeys.Clear();
		}

		Dictionary<string, string> ChapterCompositeKeys { get; } = new Dictionary<string, string>();

		public static readonly Regex ImportTariffDetailsRegex = new Regex(@"https://www.kanzei.or.jp/statistical/tariff/detail/index/e/[0-9]{9,10}†?[0-9]*", RegexOptions.IgnoreCase);

		public static readonly Regex ExportTariffDetailsRegex = new Regex(@"https://www.kanzei.or.jp/statistical/expstatis/detail/index/e/[0-9]{9,10}†?[0-9]*", RegexOptions.IgnoreCase);

		public Regex TariffDetailsRegex => IsImport ? ImportTariffDetailsRegex : ExportTariffDetailsRegex;
	}
}
