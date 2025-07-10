using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CsvHelper;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class Scraper
	{
		protected Scraper(string url)
		{
			Url = url;
		}

		protected Scraper(IWebSourceProvider sourceProvider, string url)
		{
			SourceProvider = sourceProvider;
			Url = url;
		}

		public bool IsImport => Url.StartsWith("https://www.kanzei.or.jp/statistical/tariff", StringComparison.OrdinalIgnoreCase);

		public bool IsExport => Url.StartsWith("https://www.kanzei.or.jp/statistical/expstatis", StringComparison.OrdinalIgnoreCase);

		public static ConcurrentDictionary<int, DynamicCsvRecord> GlobalSections { get; } = new ConcurrentDictionary<int, DynamicCsvRecord>();

		public string Url { get; }

		public string FullPath { get; set; }

		public HtmlDocument Document { get; set; }

		public HtmlNode Node { get; set; }

		public IWebSourceProvider SourceProvider { get; }

		public List<DynamicCsvRecord> Records { get; } = new List<DynamicCsvRecord>();

		public DynamicCsvRecord Record { get; } = new DynamicCsvRecord();

		public virtual void Load()
		{
			Records.Clear();

			if (Document == null)
			{
				Document = new HtmlDocument();
				Document.LoadHtml(GetWebPageAsyncWithRetry(Url).GetAwaiter().GetResult());
			}
		}

		public virtual void Locate() { }

		public virtual void Decode() { }

		public virtual void Pack() { }

		public virtual void Release()
		{
			Document = null;
			Node = null;
		}

		public virtual void Translate()
		{
			Load();
			Locate();
			Decode();
			Pack();
			Release();
		}

		public virtual void Export()
		{
			using (var writer = new StreamWriter(FullPath))
			using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.Configuration { Encoding = Encoding.Unicode, HasHeaderRecord = false, IgnoreBlankLines = false }))
			{
				csv.WriteRecords(Records);
			}
		}

		protected static string Decode(string source)
		{
			return JapaneseLocalHelper.ConvertFullWidthToHalfWidth(HttpUtility.HtmlDecode(source));
		}

		protected async Task<string> GetWebPageAsyncWithRetry(string url)
		{
			var retry = 0;
			string content;

			do
			{
				try
				{
					content = await SourceProvider.GetPageAsync(url);
				}
				catch (Exception e) when (e is HttpRequestException || e is SocketException)
				{
					content = string.Empty;
					Console.WriteLine($"Error: [{e.Message}]. Retry: [{retry}]");
					await Task.Delay(TimeSpan.FromSeconds(1));
				}
			} while (retry++ < 5 && (string.IsNullOrEmpty(content) || content.Contains("504 Gateway Time-out")));

			return content;
		}
	}
}
