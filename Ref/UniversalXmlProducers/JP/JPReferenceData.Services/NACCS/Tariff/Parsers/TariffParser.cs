using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public sealed class TariffParser : Parser
	{
		public TariffParser(IHttpClientHelper httpClientHelper, DateTime publishDate, bool isImport)
			: base(publishDate, isImport)
		{
			if(!isImport)
			{
				getOtherLawCodeResult = TryGetJPOtherLawCodes(httpClientHelper, out otherLawCodesDic);
				getExportTradeOrdinanceResult = TryGetJPExportTradeOrdinanceCodes(httpClientHelper, out exportTradeOrdinanceDic);
			}
		}

		readonly Dictionary<string, string> otherLawCodesDic;
		readonly Dictionary<string, Dictionary<string, string>> exportTradeOrdinanceDic;
		readonly bool getOtherLawCodeResult;
		readonly bool getExportTradeOrdinanceResult;
		const string ImportOutputXMLFileName = "JPImportTariff.xml";
		const string ExportOutputXMLFileName = "JPExportTariff.xml";
		const string ImportDataSource = "Japan Import Tariff";
		const string ExportDataSource = "Japan Export Tariff";
		const string GroupTypeImport = "IMP";
		const string GroupTypeExport = "EXP";

		protected override string DataSource { get => IsImport ? ImportDataSource : ExportDataSource; }
		protected override string OutputXMLFileName { get => IsImport ? ImportOutputXMLFileName : ExportOutputXMLFileName; }
		protected override string EntityName { get => "Tariff"; }

		protected override IEnumerable<RefDataRepoModelEntityType> CreateEntities(string filePath)
		{
			if (CsvReaderHelper.TryRead(filePath, out var records, "UTF-8"))
			{
				foreach (var row in records)
				{
					var columns = ParserHelper.SplitComma(row);
					var attributesToBeAdded = new HashSet<(string Name, string Value)>();

					if (columns[0].Trim().Equals(EntityName, StringComparison.OrdinalIgnoreCase))
					{
						var refCusTariff = new RefCusTariff()
						{
							ZZ1_TariffCode = columns[1].Trim().Substring(0, 9),
							ZZ1_Description = columns[3].Trim().TrimStart('-').TrimStart('－'),
							ZZ1_CompositeKeyOnZZ5 = columns[2].Trim(),
							RefCusTariffLanguages = new[]
							{
								new RefCusTariffLanguage() { ZX7_Description = columns[4].Trim().TrimStart('-').TrimStart('－') }
							}
						};

						var uom = columns[8].Trim();
						if (!string.IsNullOrEmpty(uom))
						{
							refCusTariff.RefCusTariffUOMs = new[]
							{
								new RefCusTariffUOM { ZZ8_Type = Constants.TariffUOMTypes.CustomsUnitII, ZZ8_UOM = uom }
							};
						}

						var originalAttrNames = columns[9].Trim();
						if (!string.IsNullOrEmpty(originalAttrNames))
						{
							var attrNames = ParserHelper.SplitComma(originalAttrNames);
							foreach (var attrName in attrNames)
							{
								if (getExportTradeOrdinanceResult && attrName.StartsWith("輸出貿易管理令", StringComparison.Ordinal))
								{
									var normalizedName = new Regex(@"(?=[XVI])(I[VX]|V?I{0,3})").Replace(attrName.Normalize(NormalizationForm.FormKC), m => RomanToInteger(m.Groups[1].Value).ToString(CultureInfo.InvariantCulture));
									var matches = new Regex(@"^輸出貿易管理令((\d{1,3}の?\d?)(-?)((\d{1,3}の?\d{0,1})?))").Matches(normalizedName);
									if (matches.Count > 0 && matches[0].Groups.Count > 1)
									{
										var keyCode = matches[0].Groups[1].Value;
										var sortedExportTradeOrdinanceDic = exportTradeOrdinanceDic.OrderByDescending(key => key.Key).ToDictionary((ki) => ki.Key, (vi) => vi.Value);
										foreach (var dic in sortedExportTradeOrdinanceDic)
										{
											if (dic.Value.TryGetValue(keyCode, out var value))
											{
												attributesToBeAdded.Add((Constants.TariffAttribute.ExportTradeOrdinance, value));
												break;
											}
										}
									}
								}
								else if (getOtherLawCodeResult)
								{
									if (otherLawCodesDic.TryGetValue(attrName, out var attValue))
									{
										attributesToBeAdded.Add((Constants.TariffAttribute.OtherLaw, attValue));
									}
									else
									{
										var findAppropriateKeys = otherLawCodesDic.Keys.Where(key => key.StartsWith(attrName, StringComparison.Ordinal)).ToList();
										foreach (var key in findAppropriateKeys)
										{
											attributesToBeAdded.Add((Constants.TariffAttribute.OtherLaw, otherLawCodesDic[key]));
										}
									}
								}

								attributesToBeAdded.Add((Constants.TariffAttribute.Reference, attrName));
							}

							if (attributesToBeAdded.Count > 0)
							{
								refCusTariff.RefCusTariffAttributes = attributesToBeAdded.Select(x => new RefCusTariffAttribute { ZZ3_Name = x.Name, ZZ3_Value = x.Value }).ToArray();
							}
						}

						yield return refCusTariff;
					}
				}
			}
		}

		static bool TryGetJPOtherLawCodes(IHttpClientHelper httpClientHelper, out Dictionary<string, string> otherLawCodesDic)
		{
			otherLawCodesDic = new Dictionary<string, string>();
			var downloadURL = AppConfig.NACCS.CodeLists.OtherLawCodeCsvFileDownloadUrl;
			var filePath = Path.GetTempFileName();
			var isDownloaded = FileDownloader.TryDownload(httpClientHelper, downloadURL, filePath).Result;
			if (isDownloaded)
			{
				if (CsvReaderHelper.TryRead(filePath, out var records))
				{
					foreach (var row in records)
					{
						var columnVals = ParserHelper.SplitComma(row);
						if (columnVals.Length > 2 && int.TryParse(columnVals[0], out int _))
						{
							var desc = columnVals[1];
							var code = columnVals[2];
							if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(desc))
							{
								otherLawCodesDic.Add(desc, code);

								if (desc == "覚醒剤取締法")
								{
									otherLawCodesDic.Add("覚せい剤取締法", code);
								}
							}
						}
					}
					return otherLawCodesDic.Count > 0;
				}
			}
			return false;
		}

		static bool TryGetJPExportTradeOrdinanceCodes(IHttpClientHelper httpClientHelper, out Dictionary<string, Dictionary<string, string>> exportTradeOrdinanceDic)
		{
			exportTradeOrdinanceDic = new Dictionary<string, Dictionary<string, string>>();
			var downloadURL = AppConfig.NACCS.CodeLists.ExportTradeControlOrdinanceAppendixCsvFileDownloadUrl;
			var filePath = Path.GetTempFileName();
			var isDownloaded = FileDownloader.TryDownload(httpClientHelper, downloadURL, filePath).Result;
			if (isDownloaded)
			{
				if (CsvReaderHelper.TryRead(filePath, out var records))
				{
					var tableDetails = new Dictionary<string, string>();
					var tableCode = string.Empty;
					foreach (var row in records)
					{
						var rowString = row.Normalize(NormalizationForm.FormKC);
						var match = new Regex(@"^別表第(\d{1}の?\d?)\S").Match(rowString);
						if (match.Success)
						{
							tableDetails = new Dictionary<string, string>();
							tableCode = match.Groups[1].Value;

							if (exportTradeOrdinanceDic.TryGetValue(tableCode, out var currentDic))
							{
								if (currentDic.Count == 0)
								{
									exportTradeOrdinanceDic[tableCode] = tableDetails;
								}
							}
							else
							{
								exportTradeOrdinanceDic.Add(tableCode, tableDetails);
							}
						}
						var columnVals = ParserHelper.SplitComma(rowString);
						var firstColumn = Regex.Replace(columnVals[0], @"\s+", "");

						var matches = new Regex(@"^(\d{1,3}の?\d?)(-?)(\(?(\d{1,3}の?\d?)\)?)").Matches(firstColumn);
						if (matches.Count > 0 && matches[0].Groups.Count == 5)
						{
							tableDetails.Add($"{matches[0].Groups[1].Value}{matches[0].Groups[2].Value}{matches[0].Groups[4].Value}", columnVals[1].Trim());
						}
						else if (int.TryParse(firstColumn, out var keyValue))
						{
							if (tableCode == "1")
							{
								tableDetails.Add(keyValue.ToString(CultureInfo.InvariantCulture), columnVals[1].Trim());
							}
							else
							{
								tableDetails.Add($"{tableCode}-{keyValue}", columnVals[1].Trim());
							}
						}
					}
					return true;
				}
			}
			return false;
		}

		protected override XmlWriterConfiguration GetXMLConfiguration()
		{
			var configuration = new XmlWriterConfiguration();

			var config = new EntityTypeConfiguration<RefCusTariff>(true);
			config.IncludeColumn(x => x.ZZ1_TariffCode, true);
			config.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, true);
			config.IncludeColumn(x => x.ZZ1_Description);
			config.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, IsImport ? GroupTypeImport : GroupTypeExport);
			config.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);
			config.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);
			config.IncludeColumnWithDefaultValue(x => x.ZZ1_StartDate, false, IsImport ? StaticResources.DefaultZZD_StartDate : new DateTime(2019, 09, 01, 00, 00, 00));
			config.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, StaticResources.DefaultZZD_EndDate);
			configuration.IncludeEntityTypeConfiguration(config);

			config.IncludeColumn(x => x.RefCusTariffLanguages, false);

			var languageConfig = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
			languageConfig.IncludeColumn(x => x.ZX7_Description);
			languageConfig.IncludeColumnWithConstantValue(x => x.ZX7_ZX6_NKLanguage, true, Constants.DataGrouping.JP);
			configuration.IncludeEntityTypeConfiguration(languageConfig);

			config.IncludeColumn(x => x.RefCusTariffUOMs, false);

			var uomsConfig = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			uomsConfig.IncludeColumn(x => x.ZZ8_Type, true);
			uomsConfig.IncludeColumn(x => x.ZZ8_UOM);
			configuration.IncludeEntityTypeConfiguration(uomsConfig);

			config.IncludeColumn(x => x.RefCusTariffAttributes, false);

			var attConfig = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			attConfig.IncludeColumn(x => x.ZZ3_Name, true);
			attConfig.IncludeColumn(x => x.ZZ3_Value, true);
			configuration.IncludeEntityTypeConfiguration(attConfig);

			return configuration;
		}
	}
}
