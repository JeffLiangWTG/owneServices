using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using System.Data;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TariffUpdateInfo : RefDataUpdateInfo
	{
		DateTime CurrentTaipeiDate { get; }
		DateTime PublicationDateTime { get; }

		public TariffUpdateInfo(updateInfoBean info, string filename, DateTime publicationDateTime) : base(info, filename)
		{
			CurrentTaipeiDate = TimeZoneInfo.ConvertTimeFromUtc(SystemContext.UtcNow(), TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"));
			PublicationDateTime = publicationDateTime;
		}

		protected virtual IWebClient GetWebClientWrapper()
		{
			return new WebClientWrapper();
		}

		protected List<TariffColumn2DataRow> ApplyOverrides(List<TariffColumn2DataRow> rows, List<TariffColumn2OverrideDataRow> overrideList)
		{
			var result = new List<TariffColumn2DataRow>();

			foreach (var row in rows)
			{
				var overrides = overrideList.Where(item => item.TariffCode == row.TariffCode && item.CountryCode == row.CountryCode);
				if (overrides.Count() > 0)
				{
					foreach (var match in overrides)
					{
						var clone = new TariffColumn2DataRow(row.LineData);
						clone.SetField(TariffColumn2DataRow.Schema.SpecificRate.Name, string.Empty);
						clone.SetField(TariffColumn2DataRow.Schema.AdValoremRate.Name, string.Empty);
						clone.SetField(TariffColumn2DataRow.Schema.ProvisionalEndDate.Name, match.GetField(TariffColumn2OverrideDataRow.Schema.ProvisionalEndDate.Name));
						clone.SetField(TariffColumn2DataRow.Schema.ProvisionalStartDate.Name, match.GetField(TariffColumn2OverrideDataRow.Schema.ProvisionalStartDate.Name));
						clone.SetField(TariffColumn2DataRow.Schema.ProvisionalSpecificRate.Name, match.GetField(TariffColumn2OverrideDataRow.Schema.ProvisionalSpecificRate.Name));
						clone.SetField(TariffColumn2DataRow.Schema.ProvisionalAdValoremRate.Name, match.GetField(TariffColumn2OverrideDataRow.Schema.ProvisionalAdValoremRate.Name));
						clone.OrderNumber = match.OrderNumber;
						clone.IsOverridden = true;

						result.Add(clone);
					}
				}
				else
				{
					result.Add(row);
				}
			}
			return result;
		}

		protected List<TariffColumn2DataRowGroup> GroupTariffColumn2DataRows(List<TariffColumn2DataRow> rows)
		{
			var result = new List<TariffColumn2DataRowGroup>();
			if (rows.Count() == 0)
			{
				return result;
			}

			var specialCountryList = new List<string>();
			foreach (var country in rows.Where(row => !row.IsLdcs).GroupBy(row => row.CountryCode))
			{
				specialCountryList.Add(country.Key);
			}

			var ldcsCountryList = new List<string>();
			foreach (var country in rows.Where(row => row.IsLdcs).GroupBy(row => row.CountryCode))
			{
				ldcsCountryList.Add(country.Key);
			}

			foreach (var tariffGroup in rows.GroupBy(row => row.TariffCode))
			{
				var specailTariffRows = rows.Where(row => row.TariffCode == tariffGroup.Key && !row.IsLdcs).ToList();
				var ldcTariffRows = rows.Where(row => row.TariffCode == tariffGroup.Key && row.IsLdcs).ToList();

				var specialGroups = specailTariffRows.GroupBy(row => new { row.EndDate, row.StartDate, row.SpecificRate, row.AdValoremRate, row.ProvisionalEndDate, row.ProvisionalStartDate, row.ProvisionalSpecificRate, row.ProvisionalAdValoremRate });
				var ldcGroups = ldcTariffRows.GroupBy(row => new { row.EndDate, row.StartDate, row.SpecificRate, row.AdValoremRate, row.ProvisionalEndDate, row.ProvisionalStartDate, row.ProvisionalSpecificRate, row.ProvisionalAdValoremRate });

				bool isOverrideDetected = false;
				foreach (var row in specailTariffRows)
				{
					if (row.IsOverridden)
					{
						isOverrideDetected = true;
						break;
					}
				}

				foreach (var row in ldcTariffRows)
				{
					if (row.IsOverridden)
					{
						isOverrideDetected = true;
						break;
					}
				}

				if (specialGroups.Count() == 1 && !isOverrideDetected)
				{
					var dataRowGroup = new TariffColumn2DataRowGroup();
					dataRowGroup.tariffColumn2DataRows.Add(specialGroups.First().Select(group =>
					{
						group.TradeGroup = TradeGroup.SPE.ToString();
						group.ExcludeTradeGroup = specialCountryList.Except(specailTariffRows.Select(row => row.CountryCode)).ToList();
						return group;
					}).First());
					result.Add(dataRowGroup);
				}
				else
				{
					foreach (var specialGroup in specialGroups)
					{
						var dataRowGroup = new TariffColumn2DataRowGroup();
						foreach (var country in specialGroup)
						{
							dataRowGroup.tariffColumn2DataRows.Add(country);
						}
						result.Add(dataRowGroup);
					}
				}

				if (ldcGroups.Count() == 1 && !isOverrideDetected)
				{
					var dataRowGroup = new TariffColumn2DataRowGroup();
					dataRowGroup.tariffColumn2DataRows.Add(ldcGroups.First().Select(group =>
					{
						group.TradeGroup = TradeGroup.LDC.ToString();
						group.ExcludeTradeGroup = ldcsCountryList.Except(ldcTariffRows.Select(row => row.CountryCode)).ToList();
						return group;
					}).First());
					result.Add(dataRowGroup);
				}
				else
				{
					foreach (var ldcGroup in ldcGroups)
					{
						var dataRowGroup = new TariffColumn2DataRowGroup();
						foreach (var country in ldcGroup)
						{
							dataRowGroup.tariffColumn2DataRows.Add(country);
						}
						result.Add(dataRowGroup);
					}
				}
			}

			return result;
		}

		protected IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> CreateRefCusRates(IEnumerable<RefCusRate> refCusRates)
		{
			return refCusRates.Where(refCusRate => DateTime.Compare(refCusRate.ZZ2_EndDate, CurrentTaipeiDate) >= 0 && !string.IsNullOrEmpty(refCusRate.ZZ2_RateFormula)).
				Select(refCusRate => new Common.UniversalXmlWriter.EntityType.RefCusRate()
				{
					ZZ2_StartDate = refCusRate.ZZ2_StartDate,
					ZZ2_EndDate = refCusRate.ZZ2_EndDate,
					ZZ2_RateFormula = refCusRate.ZZ2_RateFormula,
					ZZ2_RateFormulaDerivedFrom = refCusRate.ZZ2_RateFormulaDerivedFrom,
					ZZ2_ZZS_NKPreference = refCusRate.ZZ2_ZZS_NKPreference,
					ZZ2_ZY1_NKRateCode = refCusRate.ZZ2_ZY1_NKRateCode,
					RefCusApplicabilities = CreateApplicabilities(refCusRate).ToArray()
				});
		}

		IEnumerable<RefCusApplicability>  CreateApplicabilities(RefCusRate refCusRate)
		{
			foreach(var item in refCusRate.RefCusApplicabilities)
			{
				yield return new RefCusApplicability()
				{
					ZZT_StartDate = item.ZZT_StartDate,
					ZZT_EndDate = item.ZZT_EndDate,
					ZZT_ZZA_NKTradeGroup = item.ZZT_ZZA_NKTradeGroup,
					ZZT_OrderNumber = item.ZZT_OrderNumber ?? string.Empty,
					RefCusExcludedTradeGroups = item.RefCusExcludedTradeGroup == null ? null : CreateRefCusExcludedTradeGroup(item.RefCusExcludedTradeGroup).ToArray()
				};
			}
		}

		IEnumerable<RefCusExcludedTradeGroup> CreateRefCusExcludedTradeGroup(List<string> refCusApplicabilities)
		{
			foreach (var exclusion in refCusApplicabilities)
			{
				yield return new RefCusExcludedTradeGroup() { ZZC_ZZA_NKTradeGroup = exclusion };
			}
		}

		protected bool IsQuotaTariffCode(string code)
		{
			if (code == "98990000006")
			{
				return false;
			}

			if (code.StartsWith("98"))
			{
				return true;
			}

			return false;
		}

		const string QUOTA = "QUOTA";
		protected IEnumerable<RefCusRate> CreateRefCusRateElement_Column1and3(bool isQuotaTariffCode, DateTime startDate, DateTime endDate,
			(string RateFormula, string ZZ2_RateFormulaDerivedFrom)? column3SpecificRateInfo,
			(string RateFormula, string ZZ2_RateFormulaDerivedFrom)? column3AdValoremRateInfo,
			(string RateFormula, string ZZ2_RateFormulaDerivedFrom)? column1SpecificRateInfo,
			(string RateFormula, string ZZ2_RateFormulaDerivedFrom)? column1AdValoremRateInfo,
			bool forProvisional = false)
		{
			var column3ZZS = (forProvisional ? Preference.PT3 : Preference.STD).ToString();
			var column1ZZS = (forProvisional ? Preference.PT1 : Preference.PR1).ToString();

			IRefCusApplicability app;
			if (!isQuotaTariffCode && column3SpecificRateInfo != null)
			{
				var column3SpecificRate = new RefCusRate()
				{
					ZZ2_StartDate = startDate,
					ZZ2_EndDate = endDate,
					ZZ2_RateFormula = column3SpecificRateInfo?.RateFormula,
					ZZ2_RateFormulaDerivedFrom = column3SpecificRateInfo?.ZZ2_RateFormulaDerivedFrom,
					ZZ2_ZY1_NKRateCode = RateCodeTypes.DTS,
					ZZ2_ZZS_NKPreference = column3ZZS,
				};
				app = column3SpecificRate.AddNewRefCusApplicability();
				app.ZZT_ZZA_NKTradeGroup = TradeGroup.ALL.ToString();

				yield return column3SpecificRate;
			}

			if (!isQuotaTariffCode && column3AdValoremRateInfo != null)
			{
				var column3AdValoremRate = new RefCusRate()
				{
					ZZ2_StartDate = startDate,
					ZZ2_EndDate = endDate,
					ZZ2_RateFormula = column3AdValoremRateInfo?.RateFormula,
					ZZ2_RateFormulaDerivedFrom = column3AdValoremRateInfo?.ZZ2_RateFormulaDerivedFrom,
					ZZ2_ZY1_NKRateCode = RateCodeTypes.DTA,
					ZZ2_ZZS_NKPreference = column3ZZS,
				};
				app = column3AdValoremRate.AddNewRefCusApplicability();
				app.ZZT_ZZA_NKTradeGroup = TradeGroup.ALL.ToString();

				yield return column3AdValoremRate;
			}

			if (column1SpecificRateInfo != null)
			{
				var column1SpecificRate = new RefCusRate()
				{
					ZZ2_StartDate = startDate,
					ZZ2_EndDate = endDate,
					ZZ2_RateFormula = column1SpecificRateInfo?.RateFormula,
					ZZ2_RateFormulaDerivedFrom = column1SpecificRateInfo?.ZZ2_RateFormulaDerivedFrom,
					ZZ2_ZY1_NKRateCode = RateCodeTypes.DTS,
					ZZ2_ZZS_NKPreference = column1ZZS,
				};
				if (!isQuotaTariffCode)
				{
					app = column1SpecificRate.AddNewRefCusApplicability();
					app.ZZT_ZZA_NKTradeGroup = TradeGroup.FTA.ToString();
				}
				app = column1SpecificRate.AddNewRefCusApplicability();
				app.ZZT_ZZA_NKTradeGroup = TradeGroup.WTO.ToString();
				app.ZZT_OrderNumber = isQuotaTariffCode ? QUOTA : string.Empty;

				yield return column1SpecificRate;
			}

			if (column1AdValoremRateInfo != null)
			{
				var column1AdValoremRate = new RefCusRate()
				{
					ZZ2_StartDate = startDate,
					ZZ2_EndDate = endDate,
					ZZ2_RateFormula = column1AdValoremRateInfo?.RateFormula,
					ZZ2_RateFormulaDerivedFrom = column1AdValoremRateInfo?.ZZ2_RateFormulaDerivedFrom,
					ZZ2_ZY1_NKRateCode = RateCodeTypes.DTA,
					ZZ2_ZZS_NKPreference = column1ZZS,
				};
				if (!isQuotaTariffCode)
				{
					app = column1AdValoremRate.AddNewRefCusApplicability();
					app.ZZT_ZZA_NKTradeGroup = TradeGroup.FTA.ToString();
				}
				app = column1AdValoremRate.AddNewRefCusApplicability();
				app.ZZT_ZZA_NKTradeGroup = TradeGroup.WTO.ToString();
				app.ZZT_OrderNumber = isQuotaTariffCode ? QUOTA : string.Empty;

				yield return column1AdValoremRate;
			}
		}

		protected IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> CreateRefCusRates(TariffColumn1And3DataRow tariffData)
		{
			var isQuotaTariffCode = IsQuotaTariffCode(tariffData.TariffCode);
			var refCusRates = new List<RefCusRate>();

			refCusRates.AddRange(CreateRefCusRateElement_Column1and3(
				isQuotaTariffCode, tariffData.StartDate, tariffData.EndDate,
				(tariffData.Column3SpecificRateFormula, tariffData.Column3SpecificRateFormulaDerivedFrom),
				(tariffData.Column3AdValoremRateFormula, tariffData.Column3AdValoremRateFormulaDerivedFrom),
				(tariffData.Column1SpecificRateFormula, tariffData.Column1SpecificRateFormulaDerivedFrom),
				(tariffData.Column1AdValoremRateFormula, tariffData.Column1AdValoremRateFormulaDerivedFrom)
			));

			if (tariffData.ProvisionalEndDate > DateTime.MinValue)
			{
				refCusRates.AddRange(CreateRefCusRateElement_Column1and3(
					isQuotaTariffCode, tariffData.ProvisionalStartDate, tariffData.ProvisionalEndDate,
					(tariffData.Column3ProvisionalSpecificRateFormula, tariffData.Column3ProvisionalSpecificRateFormulaDerivedFrom),
					(tariffData.Column3ProvisionalAdValoremRateFormula, tariffData.Column3ProvisionalAdValoremRateFormulaDerivedFrom),
					(tariffData.Column1ProvisionalSpecificRateFormula, tariffData.Column1ProvisionalSpecificRateFormulaDerivedFrom),
					(tariffData.Column1ProvisionalAdValoremRateFormula, tariffData.Column1ProvisionalAdValoremRateFormulaDerivedFrom),
					forProvisional: true
				));
			}
		
			return CreateRefCusRates(refCusRates);
		}

		protected IEnumerable<RefCusRate> CreateRefCusRateElement_Column2(IEnumerable<TariffColumn2DataRow> tariffColumn2DataRows, DateTime startDate, DateTime endDate,
			(string RateFormula, string ZZ2_RateFormulaDerivedFrom)? specificRateInfo,
			(string RateFormula, string ZZ2_RateFormulaDerivedFrom)? adValoremRateInfo,
			bool forProvisional = false)
		{
			var column2ZZS = (forProvisional ? Preference.PT2 : Preference.PR2).ToString();

			IRefCusApplicability app;
			if (specificRateInfo != null)
			{
				var column2SpecificRate = new RefCusRate()
				{
					ZZ2_StartDate = startDate,
					ZZ2_EndDate = endDate,
					ZZ2_RateFormula = specificRateInfo?.RateFormula,
					ZZ2_RateFormulaDerivedFrom = specificRateInfo?.ZZ2_RateFormulaDerivedFrom,
					ZZ2_ZY1_NKRateCode = RateCodeTypes.DTS,
					ZZ2_ZZS_NKPreference = column2ZZS,
				};
				foreach (var row in tariffColumn2DataRows)
				{
					app = column2SpecificRate.AddNewRefCusApplicability();
					app.ZZT_ZZA_NKTradeGroup = row.TradeGroup;
					app.RefCusExcludedTradeGroup = row.ExcludeTradeGroup;
					app.ZZT_OrderNumber = row.OrderNumber;
				}

				yield return column2SpecificRate;
			}

			if (adValoremRateInfo != null)
			{
				var column2AdValoremRate = new RefCusRate()
				{
					ZZ2_StartDate = startDate,
					ZZ2_EndDate = endDate,
					ZZ2_RateFormula = adValoremRateInfo?.RateFormula,
					ZZ2_RateFormulaDerivedFrom = adValoremRateInfo?.ZZ2_RateFormulaDerivedFrom,
					ZZ2_ZY1_NKRateCode = RateCodeTypes.DTA,
					ZZ2_ZZS_NKPreference = column2ZZS,
				};
				foreach (var row in tariffColumn2DataRows)
				{
					app = column2AdValoremRate.AddNewRefCusApplicability();
					app.ZZT_ZZA_NKTradeGroup = row.TradeGroup;
					app.RefCusExcludedTradeGroup = row.ExcludeTradeGroup;
					app.ZZT_OrderNumber = row.OrderNumber;
				}
				yield return column2AdValoremRate;
			}
		}

		protected IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> CreateRefCusRateElement(string specificRateUnit, TariffColumn2DataRowGroup col2TariffData)
		{
			var refCusRates = new List<RefCusRate>();
			refCusRates.AddRange(CreateRefCusRateElement_Column2(
				col2TariffData.tariffColumn2DataRows, col2TariffData.StartDate, col2TariffData.EndDate,
				(col2TariffData.SpecificRateFormula(specificRateUnit), col2TariffData.SpecificRateFormulaDerivedFrom(specificRateUnit)),
				(col2TariffData.AdValoremRateFormula(), col2TariffData.AdValoremRateFormulaDerivedFrom())
			));

			if (col2TariffData.ProvisionalEndDate > DateTime.MinValue)
			{
				refCusRates.AddRange(CreateRefCusRateElement_Column2(
					col2TariffData.tariffColumn2DataRows, col2TariffData.ProvisionalStartDate, col2TariffData.ProvisionalEndDate,
					(col2TariffData.ProvisionalSpecificRateFormula(specificRateUnit), col2TariffData.ProvisionalSpecificRateFormulaDerivedFrom(specificRateUnit)),
					(col2TariffData.ProvisionalAdValoremRateFormula(), col2TariffData.ProvisionalAdValoremRateFormulaDerivedFrom()),
					forProvisional: true
				));
			}

			return CreateRefCusRates(refCusRates);
		}

		protected Func<List<TariffColumn1And3DataRow>, TariffColumn1And3DataRow, KeyValuePair<int, int>, string> CompositeKeyOnZZ5 = (tariffDataRowList, tariffRow, chapterAndSection) => string.Format(CultureInfo.InvariantCulture, "{0}.{1}..{2}{3}.{4}",
									   chapterAndSection.Value.ToString("00", CultureInfo.InvariantCulture),
									   tariffRow.TariffCode.SafeSubstring(0, 2),
									   tariffRow.TariffCode.SafeSubstring(2, 2),
									   Utility.GetSubheadings(tariffRow.TariffCode.SafeSubstring(4, 2)),
									   GetTens(tariffDataRowList, tariffRow));

		protected static string GetTens(List<TariffColumn1And3DataRow> tariffDataRowList, TariffColumn1And3DataRow tariffRow)
		{
			var groupBy8 = tariffDataRowList.Where(tariff => tariff.TariffCode.SafeSubstring(0, 6) == tariffRow.TariffCode.SafeSubstring(0, 6)).GroupBy(tariff => tariff.TariffCode.SafeSubstring(0, 8));

			var currentGroup = groupBy8.Single(group => group.Key == tariffRow.TariffCode.SafeSubstring(0, 8));
			var indexOfCurrentGroup = 0;
			foreach (var group in groupBy8)
			{
				indexOfCurrentGroup++;
				if (group.Key == currentGroup.Key)
					break;
			}

			return string.Concat((indexOfCurrentGroup * 10).ToString(new string('0', (int)Math.Log10(groupBy8.Count()) + 2), CultureInfo.InvariantCulture), ".",
				((currentGroup.ToList().IndexOf(tariffRow) + 1) * 10).ToString(new string('0', (int)Math.Log10(currentGroup.Count()) + 2), CultureInfo.InvariantCulture));
		}

		protected override void Execute()
		{
			string[] urlArray = Info.downloadURL.Split(';');

			var tariffDataRowList = Utility.GetDataFromFile<TariffColumn1And3DataRow>(urlArray[0], GetWebClientWrapper(), line => new TariffColumn1And3DataRow(line));
			if (tariffDataRowList.Count() > 0)
			{
				var overrideFilePath = new Uri(Path.GetFullPath("Tariff/Tariff_2_override.csv"));
				var tariffDataRowsColumn2OverrideList = Utility.GetDataFromFile<TariffColumn2OverrideDataRow>(overrideFilePath.AbsoluteUri, GetWebClientWrapper(), line => new TariffColumn2OverrideDataRow(line));
				var tariffDataRowsColumn2List = GroupTariffColumn2DataRows(ApplyOverrides(Utility.GetDataFromFile<TariffColumn2DataRow>(urlArray[1], GetWebClientWrapper(), line => new TariffColumn2DataRow(line)), tariffDataRowsColumn2OverrideList));
				var tariffDescriptionDataRowList = Utility.GetDataFromFile<TariffDescriptionDataRow>(urlArray[2], GetWebClientWrapper(), line => new TariffDescriptionDataRow(line));
				var tariffEnglishDescriptionDataRowList = Utility.GetDataFromFile<TariffDescriptionDataRow>(urlArray[5], GetWebClientWrapper(), line => new TariffDescriptionDataRow(line));
				var environmentProtectionTariffs = Utility.GetEnvironmentalProtectionTariffsFromExcel(urlArray[4], GetWebClientWrapper());
				var tariffF5FTZDestinationAttributeDataRowList = TryGetTariffAttributeDataRowsFromExcel();

				var hsChapterAndSection = new Dictionary<int, int>();
				using (var webClient = GetWebClientWrapper())
				{
					var array = webClient.DownloadData(urlArray[3]);
					using (var file = new MemoryStream(array))
					{
						Utility.CompileHsSectionsAndChaptersFromWord(file, hsChapterAndSection);
					}
				}

				var tariffList =
				from row in tariffDataRowList
				join chapterAndSection in hsChapterAndSection on Convert.ToInt32(row.TariffCode.SafeSubstring(0, 2)) equals chapterAndSection.Key
				join tariffDescRow in tariffDescriptionDataRowList on row.TariffCode.SafeSubstring(0, 10) equals tariffDescRow.TariffCode
				join tariffEngDescRow in tariffEnglishDescriptionDataRowList on row.TariffCode.SafeSubstring(0, 10) equals tariffEngDescRow.TariffCode
				select new RefCusTariff()
				{
					ZZ1_TariffCode = row.TariffCode,
					ZZ1_Description = tariffEngDescRow.TariffDescription,
					ZZ1_StartDate = row.StartDate,
					ZZ1_EndDate = row.EndDate,
					ZZ1_CompositeKeyOnZZ5 = CompositeKeyOnZZ5(tariffDataRowList, row, chapterAndSection),
					RefCusTariffUOMs = row.GetTariffUOM().ToArray(),
					RefCusTariffAttributes = row.GetRefCusTariffAttribute().Union(CreateAttributeForEnvironmentalProtectionTariff(row.TariffCode, environmentProtectionTariffs)).Union(CreateAttributeFromTariffAttributeDataRow(row.TariffCode, tariffF5FTZDestinationAttributeDataRowList)).ToArray(),
					RefCusTariffLanguages = CreateRefCusTariffLanguage(tariffDescRow).ToArray(),
					RefCusRates = CreateRefCusRates(row).Union(CreateRefCusRates(tariffDataRowsColumn2List, row)).ToArray()
				};
				WriteXmlHelper.ExportToXMLFile(Filename, "TW Tariff", PublicationDateTime, tariffList.ToList(), GetWriterConfiguration());
			}
		}

		List<TariffAttributeDataRow> TryGetTariffAttributeDataRowsFromExcel()
		{
			var tariffF5FTZDestinationAttributeDataRowList = new List<TariffAttributeDataRow>();
			try
			{
				var tariffF5FTZDestinationAttributeFilePath = Path.Combine(FolderHelper.GetBinFolder(), AppConfig.Tariff.TWTariffF5ValidationFileName);
				tariffF5FTZDestinationAttributeDataRowList = Utility.GetDataFromFile<TariffAttributeDataRow>(tariffF5FTZDestinationAttributeFilePath, GetWebClientWrapper(), line => new TariffAttributeDataRow(line));
			}
			catch (System.Exception ex)
			{
				ErrorWriter.WriteException(ex);
			}
			return tariffF5FTZDestinationAttributeDataRowList;
		}

		IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> CreateRefCusRates(List<TariffColumn2DataRowGroup> tariffDataRowsColumn2List, TariffColumn1And3DataRow row)
		{
			return tariffDataRowsColumn2List.Where(col2 => col2.TariffCode == row.TariffCode.SafeSubstring(0, 8))
				.SelectMany(col2 => CreateRefCusRateElement(row.SpecificRateUnit, col2));
		}

		XmlWriterConfiguration GetWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			#region RefCusTariff
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, "HSN");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_EndDate, false);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false);
			tariffConfiguration.IncludeColumn(x => x.RefCusRates, false);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs, false);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes, false);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffLanguages, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			#endregion
			#region RefCusTariffAttribute
			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value, true);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffAttributeConfiguration);
			#endregion
			#region RefCusTariffLanguage
			var tariffLanguageConfiguration = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
			tariffLanguageConfiguration.IncludeColumnWithConstantValue(x => x.ZX7_ZX6_NKLanguage, true, Constants.Languages.ChineseTraditional);
			tariffLanguageConfiguration.IncludeColumn(x => x.ZX7_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffLanguageConfiguration);
			#endregion
			#region RefCusTariffUOM
			var tariffUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			tariffUOMConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
			tariffUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffUOMConfiguration);
			#endregion
			#region RefCusRate
			var cusRateConfiguration = new EntityTypeConfiguration<Common.UniversalXmlWriter.EntityType.RefCusRate>(true);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_EndDate, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, "DTY");
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusRateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			writerConfiguration.IncludeEntityTypeConfiguration(cusRateConfiguration);
			#endregion
			#region RefCusApplicability
			var cusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, false);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_OrderNumber, true);
			cusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusApplicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups, false);
			writerConfiguration.IncludeEntityTypeConfiguration(cusApplicabilityConfiguration);
			#endregion
			#region RefCusExcludedTradeGroup
			var cusExcludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			cusExcludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			cusExcludedTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			writerConfiguration.IncludeEntityTypeConfiguration(cusExcludedTradeGroupConfiguration);
			#endregion
			return writerConfiguration;
		}

		protected IEnumerable<RefCusTariffAttribute> CreateAttributeForEnvironmentalProtectionTariff(string tariffCode, List<string> referenceList)
		{
			if (referenceList.Any(code => code == tariffCode))
			{
				yield return new RefCusTariffAttribute() { ZZ3_Name = "EnvironmentalProtectionTariff", ZZ3_Value = "TRUE" };
			}
		}

		protected IEnumerable<RefCusTariffAttribute> CreateAttributeFromTariffAttributeDataRow(string tariffCode, List<TariffAttributeDataRow> tariffAttributeDataRowList)
		{
			var tariffAttributeDataRow = tariffAttributeDataRowList.FirstOrDefault(t => t.TariffCode == tariffCode);
			if (tariffAttributeDataRow != null)
			{
				yield return new RefCusTariffAttribute() { ZZ3_Name = tariffAttributeDataRow.TariffAttributeName, ZZ3_Value = tariffAttributeDataRow.TariffAttributeValue };
			}
		}

		protected IEnumerable<RefCusTariffLanguage> CreateRefCusTariffLanguage(TariffDescriptionDataRow datarow)
		{
			yield return new RefCusTariffLanguage() { ZX7_Description = datarow.TariffDescription };
		}
	}

	class RateCodeTypes
	{
		public const string DTS = "DTS";

		public const string DTA = "DTA";
	}
}
