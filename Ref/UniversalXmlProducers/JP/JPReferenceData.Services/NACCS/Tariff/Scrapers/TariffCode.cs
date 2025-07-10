using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class TariffCode : Scraper
	{
		public TariffCode(IWebSourceProvider sourceProvider, string url) : base(sourceProvider, url)
		{
		}

		#region Properties

		HtmlDocument documentJapanese;
		HtmlNode classificationNode;
		HtmlNode classificationNodeJapanese;
		TariffAdditional tariffAdditional;

		readonly List<string[]> tempCompositeKeys = new List<string[]>();

		bool HasAdditional { get; set; }

		string Synopsis
		{
			get => synopsis;
			set => synopsis = Decode(value);
		}
		string synopsis;

		string SynopsisJapanese
		{
			get => synopsisJapanese;
			set => synopsisJapanese = Decode(value);
		}
		string synopsisJapanese;

		string CompositeKey { get; set; }

		public string Description
		{
			get => description;
			set
			{
				description = Decode(value);
				if (string.IsNullOrWhiteSpace(description))
				{
					throw new ArgumentException("Description should not be empty. ");
				}
			}
		}
		string description;

		public string HSCode { get; internal set; }

		public string NaccsCode { get; internal set; }

		public string UnitI { get; internal set; }

		public string UnitII { get; internal set; }

		public string Reference { get; internal set; }

		public string DescriptionJapanese
		{
			get => descriptionJapanese;
			set
			{
				descriptionJapanese = Decode(value);
				if (string.IsNullOrWhiteSpace(descriptionJapanese))
				{
					throw new ArgumentException("Japanese Description should not be empty.");
				}
			}
		}
		string descriptionJapanese;

		public List<string[]> CompositeKeys { get; } = new List<string[]>();

		public Dictionary<string, string> ChapterCompositeKeys { get; internal set; }

		public static DynamicCsvRecord Header { get => new DynamicCsvRecord("Type", "Code", "Composite Key", "Description", "DescriptionJapanese", "Synopsis", "SynopsisJapanese"); }

		#endregion

		#region Override

		public override void Load()
		{
			var url = Url.Replace("/e/", "/j/");
			documentJapanese = new HtmlDocument();
			documentJapanese.LoadHtml(GetWebPageAsyncWithRetry(url).GetAwaiter().GetResult());
		}

		public override void Locate()
		{
			Node = Document.DocumentNode.SelectNodes("//table[@class='detail1']")[0];
			classificationNode = Document.DocumentNode.SelectNodes("//table[@class='classification']")[0];
			classificationNodeJapanese = documentJapanese.DocumentNode.SelectNodes("//table[@class='classification']")[0];
		}

		public override void Decode()
		{
			var contentNodes = Node.SelectNodes("tr")[0].SelectNodes("td");
			HSCode = contentNodes[0].InnerText.Trim();
			NaccsCode = contentNodes[1].InnerText.Trim();

			var rows = classificationNode.SelectNodes("tr");
			var rowsJapanese = classificationNodeJapanese.SelectNodes("tr");

			for (var i = 0; i < rows.Count; i++)
			{
				var tds = rows[i].SelectNodes("td");
				var tdsJapanese = rowsJapanese[i].SelectNodes("td");
				var key = new[] { ExtractCode(tds[0].InnerText.Trim()), tds[1].InnerText.Trim(), tdsJapanese[1].InnerText.Trim() };
				tempCompositeKeys.Add(key);
			}

			Synopsis = Document.DocumentNode.SelectNodes("//table[@class='detail1 detail1_width']")[0].SelectNodes("tr")[0].SelectNodes("td")[0].InnerText.Trim();
			foreach (var (item, index) in documentJapanese.DocumentNode.SelectNodes("//table[@class='detail1 detail1_width']")[0].SelectNodes("tr").Select((value, i) => (value, i)))
			{
				switch (index)
				{
					case 0:
						SynopsisJapanese = item.SelectNodes("td")[0].InnerText.Trim();
						break;
					case 2:
						UnitI = item.SelectNodes("td")[0].InnerText.Trim();
						break;
					case 3:
						UnitII = item.SelectNodes("td")[0].InnerText.Trim();
						break;
					case 4:
						var uls = item.SelectNodes("td")[0].SelectNodes("ul");
						if (uls != null)
						{
							var lis = uls[0].SelectNodes("li");
							Reference = string.Join(", ", lis.Select(t => t.InnerText.Trim()));
						}
						break;
				}
			}

			HasAdditional = NaccsCode.Contains('†', StringComparison.OrdinalIgnoreCase);

			CalculateCompositeKeys();

			if (HasAdditional)
			{
				LoadInnerPage(contentNodes[1]);
				tariffAdditional.Decode();

				tariffAdditional.AdditionalTariff.ForEach(t =>
				{
					t.CompositeKey = CompositeKey;
					t.Synopsis = Synopsis;
					t.SynopsisJapanese = SynopsisJapanese;
				});
			}
			else
			{
				Description = tempCompositeKeys.Last()[1];
				DescriptionJapanese = tempCompositeKeys.Last()[2];
			}
		}

		public override void Pack()
		{
			for (int i = 0; i < CompositeKeys.Count; i++)
			{
				Records.Add(PackNomenclature(i));
			}

			if (HasAdditional)
			{
				tariffAdditional.Pack();
				tariffAdditional.Records.ForEach(r => Records.Add(r));
			}
			else
			{
				Records.Add(PackTariff());
			}
		}

		public override void Release()
		{
			base.Release();
			documentJapanese = null;
			classificationNode = null;
			classificationNodeJapanese = null;
		}

		void CalculateCompositeKeys()
		{
			void SetGlobalKeys(string key, int index)
			{
				if (!ChapterCompositeKeys.ContainsKey(key))
				{
					ChapterCompositeKeys.TryAdd(key, tempCompositeKeys[index][1]);
					CompositeKeys.Add(new[] { key, tempCompositeKeys[index][1], tempCompositeKeys[index][2] });
				}

				CompositeKey = key;
			}

			bool ArrivedEnd(int index)
			{
				return HasAdditional ? tempCompositeKeys.Count == index : tempCompositeKeys.Count == index + 1;
			}

			var tempKey = string.Empty;
			var hasHeading = tempCompositeKeys[2][0].Length == 5;

			var section = tempCompositeKeys[0][0];
			tempKey += section;
			SetGlobalKeys(tempKey, 0);

			var chapter = tempCompositeKeys[1][0];
			tempKey += '.' + chapter;
			SetGlobalKeys(tempKey, 1);

			var heading = hasHeading ? tempCompositeKeys[2][0].Split('.').Last() : HSCode.Split('.').First().Substring(2, 2);
			tempKey += ".." + heading;
			SetGlobalKeys(tempKey, 2);

			var subHeadingIndex = hasHeading ? 3 : 2;

			if (ArrivedEnd(subHeadingIndex))
			{ return; }

			if (tempCompositeKeys[subHeadingIndex][0].Length == 0)
			{
				subHeadingIndex++;
				var middleHeading = tempCompositeKeys[subHeadingIndex][0].Split(".").Last().Substring(0, 1);
				tempKey += "." + middleHeading;
				SetGlobalKeys(tempKey, 3);
			}

			if (ArrivedEnd(subHeadingIndex))
			{ return; }

			var subHeading = tempCompositeKeys[subHeadingIndex][0].Split(".").Last();
			tempKey += "." + subHeading;
			SetGlobalKeys(tempKey, subHeadingIndex);

			var end = HasAdditional ? tempCompositeKeys.Count : tempCompositeKeys.Count - 1;
			for (var i = subHeadingIndex + 1; i < end; i++)
			{
				string key;
				var k = 1;
				do
				{ key = tempKey + $".{k++}"; }
				while (ChapterCompositeKeys.ContainsKey(key) && !ChapterCompositeKeys[key].Equals(tempCompositeKeys[i][1], StringComparison.OrdinalIgnoreCase));

				tempKey = key;
				SetGlobalKeys(tempKey, i);
			}
		}

		void LoadInnerPage(HtmlNode naccsNode)
		{
			var matches = AdditionalUrlRegex.Matches(naccsNode.InnerHtml);

			tariffAdditional = new TariffAdditional(SourceProvider, matches[0].Value.Trim());
			tariffAdditional.Load();
		}

		Regex AdditionalUrlRegex => IsImport ? ImportAdditionalUrlRegex : ExportAdditionalUrlRegex;
		static readonly Regex ImportAdditionalUrlRegex = new Regex(@"https://www.kanzei.or.jp/statistical/popcontent/naccs/tariff/[0-9]{9,10}†[0-9]*", RegexOptions.IgnoreCase);
		static readonly Regex ExportAdditionalUrlRegex = new Regex(@"https://www.kanzei.or.jp/statistical/popcontent/naccs/expstatis/[0-9]{9,10}†[0-9]*", RegexOptions.IgnoreCase);

		#endregion

		public DynamicCsvRecord PackTariff() => new DynamicCsvRecord(GetType(true), $"{ExtractCode(HSCode, false)}{NaccsCode}", CompositeKey + ".X", Description, DescriptionJapanese, Synopsis, SynopsisJapanese, UnitI, UnitII, Reference);

		DynamicCsvRecord PackNomenclature(int i) => new DynamicCsvRecord(GetType(false), string.Empty, CompositeKeys[i][0], CompositeKeys[i][1], CompositeKeys[i][2], string.Empty, string.Empty);

		static string GetType(bool isTariff) => isTariff ? "Tariff" : "Nomenclature";

		static string ExtractCode(string input, bool hasDot = true) => Regex.Replace(input, hasDot ? @"[^\d.\d]" : @"[^\d\d]", string.Empty);
	}
}
