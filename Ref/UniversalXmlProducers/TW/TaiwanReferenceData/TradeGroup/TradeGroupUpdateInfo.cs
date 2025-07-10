using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TradeGroupUpdateInfo : RefDataUpdateInfo
	{
		protected virtual IWebClient GetWebClientWrapper()
		{
			return new WebClientWrapper();
		}

		protected Dictionary<string, string> IsoCountries
		{
			get
			{
				if (isoCountries == null)
				{
					isoCountries = new Dictionary<string, string>();
				}
				return isoCountries;
			}
		}

		protected HashSet<string> EUCountryList
		{
			get
			{
				if (euCountryList == null)
				{
					euCountryList = new HashSet<string>();
				}
				return euCountryList;
			}
		}

		protected List<RefCusTradeGroup> TradeGroupList
		{
			get
			{
				if (tradeGroupList == null)
				{
					tradeGroupList = new List<RefCusTradeGroup>();
				}
				return tradeGroupList;
			}
		}

		private Dictionary<string, string> isoCountries;
		private HashSet<string> euCountryList;
		private List<RefCusTradeGroup> tradeGroupList;


		public TradeGroupUpdateInfo(updateInfoBean info, string filename) : base(info, filename) { }

		protected override void Execute()
		{
			string[] urlArray = Info.downloadURL.Split(';');
			GetAllCountries(urlArray[0]);
			GetWtoFtaCountries(urlArray[1]);
			GetLdcsAndSpecialCountries(urlArray[2]);
			TradeGroupList.ForEach(tradeGroup => tradeGroup.RefCusTradeGroupCountries = tradeGroup.Countries.Select(country => new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = country }).ToArray());
			WriteXmlHelper.ExportToXMLFile(Filename, "TW Trade Group", SystemContext.Now(), TradeGroupList, GetWriterConfiguration());
		}

		XmlWriterConfiguration GetWriterConfiguration()
		{
			var minSmallDateTime = new DateTime(1900, 01, 01, 00, 00, 00);
			var maxSmallDateTime = new DateTime(2079, 06, 06, 23, 59, 00);

			var writerConfiguration = new XmlWriterConfiguration();
			var cusTradeGroupConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			cusTradeGroupConfiguration.IncludeColumn(x => x.ZZA_TradeGroup, true);
			cusTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusTradeGroupConfiguration.IncludeColumn(x => x.ZZA_Description, false);
			cusTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_StartDate, false, minSmallDateTime.ToString("s", CultureInfo.InvariantCulture));
			cusTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_EndDate, false, maxSmallDateTime.ToString("s", CultureInfo.InvariantCulture));
			writerConfiguration.IncludeEntityTypeConfiguration(cusTradeGroupConfiguration);
			cusTradeGroupConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries, false);
			var cusTradeGroupCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			cusTradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			cusTradeGroupCountryConfiguration.IncludeColumnWithConstantValue(x => x.ZZB_StartDate, false, minSmallDateTime.ToString("s", CultureInfo.InvariantCulture));
			cusTradeGroupCountryConfiguration.IncludeColumnWithConstantValue(x => x.ZZB_EndDate, false, maxSmallDateTime.ToString("s", CultureInfo.InvariantCulture));
			writerConfiguration.IncludeEntityTypeConfiguration(cusTradeGroupCountryConfiguration);
			return writerConfiguration;
		}

		private string GetWebSource(string url)
		{
			using (var webClient = GetWebClientWrapper())
			using (var file = new MemoryStream(webClient.DownloadData(url)))
			using (var reader = new StreamReader(file))
			{
				return reader.ReadToEnd();
			}
		}

		private string ConvertSpecialCharacter(string originStr)
		{
			return string.Join("", originStr.Normalize(NormalizationForm.FormD).Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
		}

		protected void GetLdcsAndSpecialCountries(string url)
		{
			var ldcCountries = new RefCusTradeGroup()
			{
				ZZA_TradeGroup = TradeGroup.LDC.ToString(),
				ZZA_Description = "Least Developed Countries"
			};

			var specialCountries = new RefCusTradeGroup()
			{
				ZZA_TradeGroup = TradeGroup.SPE.ToString(),
				ZZA_Description = "Special Countries (Column II)"
			};
			var singleCountryList = new List<RefCusTradeGroup>();

			var column2DataRows = Utility.GetDataFromFile<TariffColumn2DataRow>(url, GetWebClientWrapper(), line => new TariffColumn2DataRow(line));
			var ldcCountryGroups = column2DataRows.Where(row => row.IsLdcs).GroupBy(row => row.CountryCode);
			var specialCountryGroups = column2DataRows.Where(row => !row.IsLdcs).GroupBy(row => row.CountryCode);

			foreach (var countryGroup in ldcCountryGroups)
			{
				ldcCountries.Countries.Add(countryGroup.Key);
			}

			foreach (var countryGroup in specialCountryGroups)
			{
				var countryCode = countryGroup.Key;
				specialCountries.Countries.Add(countryCode);

				singleCountryList.Add(new RefCusTradeGroup(countryCode)
				{
					ZZA_Description = IsoCountries.FirstOrDefault(pair => pair.Value == countryGroup.Key).Key,
				});
			}

			TradeGroupList.Add(ldcCountries);
			TradeGroupList.Add(specialCountries);
			TradeGroupList.AddRange(singleCountryList);
		}

		protected void GetAllCountries(string url)
		{
			var allCountries = new RefCusTradeGroup()
			{
				ZZA_TradeGroup = TradeGroup.ALL.ToString(),
				ZZA_Description = "All Countries"
			};

			foreach (var record in XDocument.Load(url).Root.Elements("record").Where(r => r.Attribute("deprecated").Value == "false" && r.Element("code-3166-1-alpha-2") != null && !string.IsNullOrEmpty(r.Element("code-3166-1-alpha-2").Value)))
			{
				string countryCode = record.Element("code-3166-1-alpha-2").Value,
					longName = ConvertSpecialCharacter(record.Element("long.label")?.Elements("lg.version").Where(l => l.Attribute("lg").Value == "eng").First().Value ?? string.Empty),
					shortName = ConvertSpecialCharacter(record.Element("label").Elements("lg.version").Where(l => l.Attribute("lg").Value == "eng").First().Value);

				allCountries.Countries.Add(countryCode);

				IsoCountries.Add(shortName, countryCode);
				if (!IsoCountries.TryGetValue(longName, out string code))
				{
					IsoCountries.Add(longName, countryCode);
				}

				if (record.Attribute("protocol.order") != null && !string.IsNullOrEmpty(record.Attribute("protocol.order").Value))
				{
					EUCountryList.Add(countryCode);
				}
			}

			TradeGroupList.Add(allCountries);
		}

		protected void GetWtoFtaCountries(string url)
		{
			var wtoCountries = new RefCusTradeGroup()
			{
				ZZA_TradeGroup = TradeGroup.WTO.ToString(),
				ZZA_Description = "WTO Countries (Column I)"
			};
			var ftaCountries = new RefCusTradeGroup()
			{
				ZZA_TradeGroup = TradeGroup.FTA.ToString(),
				ZZA_Description = "FTA Countries (Column I)"
			};

			string table = ProcessWtoFtaTable(GetWebSource(url));
			var wtoList = new List<TradeGroupCountry>();
			var ftaList = new List<TradeGroupCountry>();
			ExtractTradeGroupCountries(table, wtoList, ftaList);
			AddTradeGroupCountriesToGroup(wtoList, wtoCountries);
			if (!wtoCountries.Countries.Contains(Taiwan))
			{
				wtoCountries.Countries.Add(Taiwan);
			}
			AddTradeGroupCountriesToGroup(ftaList, ftaCountries);

			TradeGroupList.Add(wtoCountries);
			TradeGroupList.Add(ftaCountries);
		}

		const string Taiwan = "TW";

		public static string ProcessWtoFtaTable(string htmlSource)
		{
			return Regex.Replace(Regex.Match(htmlSource, "<table.*summary=\"資料表格\"(.|\n)*?</table>").Value, @"<!--[\s\S]*?-->", string.Empty, RegexOptions.IgnoreCase);
		}

		IEnumerable<string> GetRows(string table)
		{
			foreach (Match match in Regex.Matches(table, @"(<tr[\s\S]*?</tr>)"))
			{
				yield return match.Value;
			}
		}

		List<string> GetDataList(string table)
		{
			var list = new List<string>();
			foreach (Match match in Regex.Matches(table, @"(<td[\s\S]*?</td>)"))
			{
				list.Add(Regex.Replace(match.Value, @"<[\s\S]*?>", string.Empty, RegexOptions.IgnoreCase));
			}
			return list;
		}

		void ExtractTradeGroupCountries(string table, List<TradeGroupCountry> wtoList, List<TradeGroupCountry> ftaList)
		{
			int headerCount = 0;
			foreach (var row in GetRows(table))
			{
				if (IsNonDataRow(row))
				{
					continue;
				}

				if (IsRowHeader(row))
				{
					headerCount++;
					continue;
				}

				var dataList = GetDataList(row);
				if (dataList.Count != 4)
				{
					continue;
				}

				if (headerCount == 1)
				{
					wtoList.Add(new TradeGroupCountry { ID = dataList[0], CountryCode = dataList[1], EnglishName = dataList[2], Remarks = dataList[3] });
				}
				else if (headerCount == 2)
				{
					ftaList.Add(new TradeGroupCountry { ID = dataList[0], CountryCode = dataList[1], EnglishName = dataList[2], Remarks = dataList[3] });
				}
			}
		}

		bool IsRowHeader(string row)
		{
			return Regex.Match(row, @"(<tr[\s\S]*?<th[\s\S]*?/tr>)").Success;
		}

		bool IsNonDataRow(string row)
		{
			return Regex.Match(row, @"(<tr[\s\S]*?<td[\s\S]*?colspan[\s\S]*?/tr>)").Success;
		}

		void ExpandEUCountries(RefCusTradeGroup group)
		{
			foreach (string member in EUCountryList)
			{
				group.Countries.Add(member);
			}
		}

		void ExpandRemarks(RefCusTradeGroup group, string remarks)
		{
			var match = Regex.Match(remarks, @"(?<=includ.*?\s).*", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				foreach (string countryName in match.Value.Split(new string[] { "&", ",", ";", " and " }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList())
				{
					if (IsoCountries.TryGetValue(countryName, out var countryCode))
					{
						group.Countries.Add(countryCode);
					}
				}
			}
		}

		bool IsValidCountryCode(string code)
		{
			return IsoCountries.ContainsValue(code) || Regex.IsMatch(code, "(AA|Q[M-Z]|X[A-Z]|ZZ)");
		}

		void AddTradeGroupCountriesToGroup(List<TradeGroupCountry> countries, RefCusTradeGroup group)
		{
			foreach (var country in countries)
			{
				var countryCode = country.CountryCode;
				if (countryCode == Constants.DataGroupings.EuropeanUnion)
				{
					ExpandEUCountries(group);
				}

				if (IsValidCountryCode(countryCode))
				{
					group.Countries.Add(countryCode);
				}

				ExpandRemarks(group, country.Remarks);
			}
		}

		class TradeGroupCountry
		{
			public string ID { get; set; }

			public string CountryCode { get; set; }

			public string EnglishName { get; set; }

			public string Remarks { get; set; }
		}
	}

	public class RefCusTradeGroup : CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusTradeGroup
	{
		public RefCusTradeGroup() { }

		public RefCusTradeGroup(string countryCode)
		{
			ZZA_TradeGroup = countryCode;
			Countries.Add(countryCode);
		}

		public HashSet<string> Countries = new HashSet<string>();
	}
}
