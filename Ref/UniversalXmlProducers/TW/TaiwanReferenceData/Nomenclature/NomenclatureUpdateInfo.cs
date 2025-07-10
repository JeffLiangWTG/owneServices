using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class NomenclatureUpdateInfo : RefDataUpdateInfo
	{
		public NomenclatureUpdateInfo(updateInfoBean info, string filename) : base(info, filename) { }

		protected virtual IWebClient GetWebClientWrapper()
		{
			return new WebClientWrapper();
		}

		protected override void Execute()
		{
			var hsChapterAndSection = new Dictionary<int, int>();
			var hsSectionDescription = new Dictionary<int, string>();
			var hs8Code = new Dictionary<string, string>();
			var hsSectionDescriptionEn = new Dictionary<int, string>();
			var hs8CodeEn = new Dictionary<string, string>();
			var urls = Info.downloadURL.Split(';');

			using (var webClient = GetWebClientWrapper())
			{
				using (var file = new MemoryStream(webClient.DownloadData(urls[0])))
				{
					Utility.CompileHsSectionsAndChaptersFromWord(file, hsChapterAndSection);
				}
			}
			using (var webClient = GetWebClientWrapper())
			{
				using (var file = new MemoryStream(webClient.DownloadData(urls[1])))
				{
					ParseDescriptionChinese(file, hsSectionDescription, hs8Code);
				}
			}
			using (var webClient = GetWebClientWrapper())
			{
				using (var file = new MemoryStream(webClient.DownloadData(urls[2])))
				{
					ParseDescriptionEnglish(file, hsSectionDescriptionEn, hs8CodeEn);
				}
			}

			if (hsChapterAndSection.Count > 0 && hs8Code.Count > 0 && hs8CodeEn.Count > 0)
			{
				List<RefCusNomenclatureGroup> nomenclatureGroupList = new List<RefCusNomenclatureGroup>();
				var hsSectionNomenclatureGroupList =
				from section in hsSectionDescription
				join sectionen in hsSectionDescriptionEn on section.Key equals sectionen.Key
				select new RefCusNomenclatureGroup()
				{
					ZZ5_Value = section.Key.ToString("00", CultureInfo.InvariantCulture),
					ZZ5_Description = sectionen.Value,
					ZZ5_CompositeKey = section.Key.ToString("00", CultureInfo.InvariantCulture),
					RefCusNomenclatureLanguages = new RefCusNomenclatureLanguage[] { new RefCusNomenclatureLanguage() { ZX8_Description = section.Value } }
				};
				nomenclatureGroupList.AddRange(hsSectionNomenclatureGroupList);

				var hs8CodeEnNomenclatureGroupList =
				from hs in hs8Code
				join hsen in hs8CodeEn on hs.Key equals hsen.Key
				from section in hsSectionDescription
				join sectionen in hsSectionDescriptionEn on section.Key equals sectionen.Key
				join chapterAndSection in hsChapterAndSection on section.Key equals chapterAndSection.Value
				where Convert.ToInt32(hs.Key.Substring(0, 2)) == chapterAndSection.Key
				where Convert.ToInt32(hsen.Key.Substring(0, 2)) == chapterAndSection.Key
				select new RefCusNomenclatureGroup()
				{
					ZZ5_Value = hsen.Key,
					ZZ5_Description = hsen.Value,
					ZZ5_CompositeKey = string.Format(CultureInfo.InvariantCulture, "{0}.{1}{2}{3}{4}",
					sectionen.Key.ToString("00", CultureInfo.InvariantCulture),
					hsen.Key.SafeSubstring(0, 2),
					string.IsNullOrEmpty(hsen.Key.SafeSubstring(2, 2)) ? "" : string.Concat("..", hsen.Key.SafeSubstring(2, 2)),
					Utility.GetSubheadings(hsen.Key.SafeSubstring(4, 2)),
								string.IsNullOrEmpty(hsen.Key.SafeSubstring(6, 2)) ? "" : GetTens(hs8CodeEn, hsen)),
					RefCusNomenclatureLanguages = new RefCusNomenclatureLanguage[] { new RefCusNomenclatureLanguage() { ZX8_Description = hs.Value } }
				};
				nomenclatureGroupList.AddRange(hs8CodeEnNomenclatureGroupList);

				WriteXmlHelper.ExportToXMLFile(Filename, "TW Nomenclature", SystemContext.Now(), nomenclatureGroupList, GetWriterConfiguration());
			}
		}

		XmlWriterConfiguration GetWriterConfiguration()
		{
			var minSmallDateTime = new DateTime(1900, 01, 01, 00, 00, 00);
			var maxSmallDateTime = new DateTime(2079, 06, 06, 23, 59, 00);

			var writerConfiguration = new XmlWriterConfiguration();
			var cusNomenclatureGroupConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			cusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true);
			cusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Value, false);
			cusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Description, false);
			cusNomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_StartDate, false, minSmallDateTime.ToString("s", CultureInfo.InvariantCulture));
			cusNomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_EndDate, false, maxSmallDateTime.ToString("s", CultureInfo.InvariantCulture));
			cusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			cusNomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			writerConfiguration.IncludeEntityTypeConfiguration(cusNomenclatureGroupConfiguration);
			cusNomenclatureGroupConfiguration.IncludeColumn(x => x.RefCusNomenclatureLanguages, false);

			var cusNomenclatureLanguageConfiguration = new EntityTypeConfiguration<RefCusNomenclatureLanguage>(true);
			cusNomenclatureLanguageConfiguration.IncludeColumnWithConstantValue(x => x.ZX8_ZX6_NKLanguage, true, Constants.Languages.ChineseTraditional);
			cusNomenclatureLanguageConfiguration.IncludeColumn(x => x.ZX8_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(cusNomenclatureLanguageConfiguration);
			return writerConfiguration;
		}

		protected string GetTens(Dictionary<string, string> hs8Codes, KeyValuePair<string, string> hs)
		{
			var hsGroup = hs8Codes.Where(hs8Code => hs8Code.Key.SafeSubstring(0, 6) == hs.Key.SafeSubstring(0, 6) && hs8Code.Key.Length == 8);

			return string.Concat(".", ((hsGroup.ToList().IndexOf(hs) + 1) * 10).ToString(new string('0', (int)Math.Log10(hsGroup.Count()) + 2)), CultureInfo.InvariantCulture);
		}

		protected void ParseDescriptionChinese(MemoryStream file, Dictionary<int, string> hsSectionDescription, Dictionary<string, string> hs8Code)
		{
			using (var reader = new StreamReader(file))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (!string.IsNullOrEmpty(line))
					{
						var hsCode = line.Substring(0, 8).Trim();
						string description = line.Substring(9);
						if (Utility.TryParseValueAsInt(hsCode, out int hsNumber))
						{
							if (hsCode.Length == 2)
							{
								description = description.Split()[1];
							}
							if (hsCode.Length == 5)
							{
								description = description.Trim('︰', '─', '－');
							}
							hs8Code.Add(hsCode, description);
						}
						else if (line.StartsWith("S") && Utility.TryParseValueAsInt(line.Substring(1, 2), out int sectionNumber))
						{
							hsSectionDescription.Add(sectionNumber, description.Split()[1]);
						}
					}
				}
			}
		}

		protected void ParseDescriptionEnglish(MemoryStream file, Dictionary<int, string> hsSectionDescription, Dictionary<string, string> hs8Code)
		{
			using (var reader = new StreamReader(file))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (!string.IsNullOrEmpty(line))
					{
						var hsCode = line.Substring(0, 8).Trim();
						string description = line.Substring(9);
						if (Utility.TryParseValueAsInt(hsCode, out int hsNumber))
						{
							if (hsCode.Length == 2)
							{
								var match = Regex.Match(description, @"Chapter \d+ ([\s\S]+)");

								if (match.Success)
								{
									description = match.Groups[1].Value;
								}
							}
							if (hsCode.Length == 5)
							{
								description = description.Trim(':', '-').Trim();
							}
							hs8Code.Add(hsCode, description);
						}
						else if (line.StartsWith("S") && Utility.TryParseValueAsInt(line.Substring(1, 2), out int sectionNumber))
						{
							var match = Regex.Match(description, @"SECTION \S+ ([\s\S]+)");

							if (match.Success)
							{
								description = match.Groups[1].Value;
							}
							hsSectionDescription.Add(sectionNumber, description);
						}
					}
				}
			}
		}
	}
}
