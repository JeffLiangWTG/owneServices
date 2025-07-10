using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class NomenclatureTariffParser
	{
		protected IOrderedEnumerable<ProcessedNomenclatureTariff> GetProcessedData()
		{
			var errorMessages = new ConcurrentBag<string>();
			var processedRecords = new ConcurrentBag<IEnumerable<ProcessedNomenclatureTariff>>();

			var dataFileFullNames = Directory.GetFiles(DataFileDirectory);
			var fileCount = dataFileFullNames.Length;
			if (fileCount == 1)
			{
				GetProcessedRecordsFromTheMergedDataFile(dataFileFullNames[0], processedRecords, errorMessages);
			}
			else
			{
				GetProcessedRecordsFromDataFiles(dataFileFullNames, processedRecords, errorMessages);
			}

			foreach (var errorMessage in errorMessages)
			{
				ErrorBuilder.AppendLine(errorMessage);
			}

			processedRecords.Add(GetSectionRecords());
			processedRecords.Add(GetChapterRecords());

			var orderedProcessedRecords = processedRecords.SelectMany(x => x).OrderBy(x => x.CompositeKey);
			return orderedProcessedRecords;
		}

		void GetProcessedRecordsFromDataFiles(string[] dataFileFullNames, ConcurrentBag<IEnumerable<ProcessedNomenclatureTariff>> processedRecords, ConcurrentBag<string> errorMessages)
		{
			Parallel.ForEach(dataFileFullNames, fileFullName =>
			{
				try
				{
					var chapterCode = Path.GetFileName(fileFullName).Substring(0, 2);
					var chapter = NomenclatureTariffLoader.LoadData(fileFullName, chapterCode);
					if (chapter.Records.Any())
					{
						var processedData = Process(chapter);
						processedRecords.Add(processedData);
					}
				}
				catch (Exception ex)
				{
					var errorMessage = $"An error was encountered while processing file: {fileFullName}. Message: {ex.Message}";
					errorMessages.Add(errorMessage);
				}
			});
		}

		void GetProcessedRecordsFromTheMergedDataFile(string dataFileFullName, ConcurrentBag<IEnumerable<ProcessedNomenclatureTariff>> processedRecords, ConcurrentBag<string> errorMessages)
		{
			var records = NomenclatureTariffLoader.LoadData(dataFileFullName).ToArray();
			var tariffsByCode = new Dictionary<string, List<RawNomenclatureTariff>>();
			var currentCode = records[0].Code.Substring(0, 2);
			tariffsByCode[currentCode] = new List<RawNomenclatureTariff>();
			foreach (var record in records)
			{
				var code = record.Code;
				if (string.IsNullOrEmpty(code) || code.StartsWith(currentCode, StringComparison.Ordinal))
				{
					tariffsByCode[currentCode].Add(record);
				}
				else
				{
					currentCode = code.Substring(0, 2);
					tariffsByCode[currentCode] = new List<RawNomenclatureTariff> { record };
				}
			}

			var chapters = new List<Chapter>();
			foreach (var kvp in tariffsByCode)
			{
				chapters.Add(new Chapter() { Code = kvp.Key, Records = kvp.Value.OrderBy(x => x.SeqNum) });
			}

			Parallel.ForEach(chapters, chapter =>
			{
				try
				{
					var processedData = Process(chapter);
					processedRecords.Add(processedData);
				}
				catch (Exception ex)
				{
					var errorMessage = $"An error was encountered while processing chapter: {chapter.Code}. Message: {ex.Message}";
					errorMessages.Add(errorMessage);
				}
			});
		}

		IOrderedEnumerable<ProcessedNomenclatureTariff> ProcessedData
		{
			get
			{
				if (processedData == default)
				{
					processedData = GetProcessedData();
				}
				return processedData;
			}
		}
		IOrderedEnumerable<ProcessedNomenclatureTariff> processedData;

		protected virtual string DataFileDirectory => Path.Combine(ApplicationConfig.ResPath, @"NomenclatureTariff\fasil");

		public void BuildNomenclatureXMLFile(string outputFilePath)
		{
			var nomenclatureFilePath = Path.Combine(outputFilePath, "RefCusNomenclatureGroupZZ_TR.xml");
			var nomenclatureWriterConfiguration = GetNomenclatureWriterConfiguration();
			Helper.ExportToXMLFile("TR Nomenclature", nomenclatureFilePath, nomenclatureWriterConfiguration, PublicationDateTime, GetNomenclatures());
		}

		public void BuildTariffXMLFile(string outputFilePath, IEnumerable<RefCusTariff> tariffs)
		{
			var tariffFilePath = Path.Combine(outputFilePath, "RefCusTariffZZ_TR_HSN.xml");
			var tariffWriterConfiguration = GetTariffWriterConfiguration();

			var dependency = new Dependency(Constants.DataSources.TradeGroupAndCountries, PublicationDateTimeTariffHsn, DependencyType.Preferred);
			Helper.ExportToXMLFile(Constants.DataSources.HsnTariff, tariffFilePath, tariffWriterConfiguration, PublicationDateTimeTariffHsn, tariffs, dependency);
		}

		public IEnumerable<RefCusTariff> GetTariffs()
		{
			return GetTariffEntities(ClassifiedData.tariffs);
		}

		IEnumerable<RefCusNomenclatureGroup> GetNomenclatures()
		{
			return GetNomenclatureEntities(ClassifiedData.nomenclatures);
		}

		(IEnumerable<ProcessedNomenclatureTariff> nomenclatures, IEnumerable<ProcessedNomenclatureTariff> tariffs) ClassifiedData
		{
			get
			{
				if (classifiedData == default)
				{
					classifiedData = Classify(ProcessedData);
				}
				return classifiedData;
			}
		}
		(IEnumerable<ProcessedNomenclatureTariff> nomenclatures, IEnumerable<ProcessedNomenclatureTariff> tariffs) classifiedData;

		static (IEnumerable<ProcessedNomenclatureTariff> nomenclatures, IEnumerable<ProcessedNomenclatureTariff> tariffs) Classify(IOrderedEnumerable<ProcessedNomenclatureTariff> allProcessedData)
		{
			var nomenclatureList = new List<ProcessedNomenclatureTariff>();
			var tariffList = new List<ProcessedNomenclatureTariff>();
			foreach (var processedData in allProcessedData)
			{
				if (processedData.IsTariff)
				{
					tariffList.Add(processedData);
				}
				else
				{
					nomenclatureList.Add(processedData);
				}
			}
			return (nomenclatureList, tariffList);
		}

		protected static IEnumerable<RefCusNomenclatureGroup> GetNomenclatureEntities(IEnumerable<ProcessedNomenclatureTariff> nomenclatures)
		{
			var entities = new List<RefCusNomenclatureGroup>();

			foreach (var nomenclature in nomenclatures)
			{
				entities.Add(new RefCusNomenclatureGroup
				{
					ZZ5_Value = nomenclature.Code,
					ZZ5_Description = nomenclature.Description,
					ZZ5_StartDate = Constants.HsnTariffStartDate,
					ZZ5_CompositeKey = nomenclature.CompositeKey,
				});
			}

			return entities;
		}

		protected IEnumerable<RefCusTariff> GetTariffEntities(IEnumerable<ProcessedNomenclatureTariff> tariffs)
		{
			var entities = new List<RefCusTariff>();
			var vatCodes = GetVatCodes();

			var tariffStartDate = Constants.HsnTariffStartDate;

			foreach (var tariff in tariffs)
			{
				var entity = new RefCusTariff
				{
					ZZ1_TariffCode = tariff.Code,
					ZZ1_Description = tariff.Description,
					ZZ1_StartDate = tariffStartDate,
					ZZ1_CompositeKeyOnZZ5 = tariff.CompositeKey,
				};

				var uoms = new List<RefCusTariffUOM> { new RefCusTariffUOM { ZZ8_Type = Constants.TariffUOM.Type.CU1, ZZ8_UOM = Constants.TariffUOM.Unit.KGM } };
				if (!string.IsNullOrEmpty(tariff.UOM))
				{
					uoms.Add(new RefCusTariffUOM { ZZ8_Type = Constants.TariffUOM.Type.CU2, ZZ8_UOM = tariff.UOM });
				}
				entity.RefCusTariffUOMs = uoms.ToArray();

				if (!string.IsNullOrEmpty(tariff.Rate))
				{
					entity.RefCusRates = new RefCusRate[]
					{
						new RefCusRate
						{
							ZZ2_StartDate = tariffStartDate,
							ZZ2_RateFormula = tariff.Rate,
							ZZ2_RateFormulaDerivedFrom = tariff.RateDerivedFrom,
							ZZ2_ZY1_NKRateCode = Constants.TariffRateCode.Code._10,
							ZZ2_ZY1_ZZR_NKRateType = Constants.TariffRateType.Code.DTY,
							ZZ2_ZZS_NKPreference = Constants.Preference.Code.STD,
							ZZ2_ZZS_ZZZ_NKDataGrouping = Constants.CountryCodeTR,
						},
					};
				}

				if (vatCodes.TryGetValue(tariff.Code, out var validVatCodes) && validVatCodes.Any())
				{
					var vatapplics = validVatCodes.Select(item => new RefCusVATApplicability
					{
						ZX5_ZZF_NKTaxOrFeeCode = item.VatCode,
						ZX5_StartDate = tariffStartDate,
						ZX5_Description = item.Description,
					}).ToArray();
					entity.RefCusVATApplicabilities = vatapplics;
				}

				entities.Add(entity);
			}

			return entities;
		}

		protected virtual IDictionary<string, IEnumerable<TariffVatCode>> GetVatCodes()
		{
			IDictionary<string, IEnumerable<TariffVatCode>> result = null;
			try
			{
				var vatFile = Path.Combine(ApplicationConfig.ResPath, "TRTariffCodesVATCodes.xlsx");
				result = TariffVatCodeLoader.Load(vatFile).GroupBy(vat => vat.TariffCode).ToDictionary(group => group.Key, group => group.AsEnumerable());
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GetVatCodes failure, Exception: {ex.GetBaseException().Message}");
			}

			return result ?? new Dictionary<string, IEnumerable<TariffVatCode>>();
		}

		public virtual DateTime PublicationDateTime
		{
			get
			{
				if (publicationDateTime == default)
				{
					publicationDateTime = NomenclatureTariffLoader.LoadPublicationTime(Path.Combine(ApplicationConfig.ResPath, @"NomenclatureTariff\Tariff Publication Time.xlsx"));
				}
				return publicationDateTime;
			}
		}
		DateTime publicationDateTime;

		public virtual DateTime PublicationDateTimeTariffHsn => Constants.HsnTariffPublicationTime;

		protected static XmlWriterConfiguration GetNomenclatureWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var nomenclature = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			nomenclature.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, Constants.CountryCodeTR);
			nomenclature.IncludeColumn(x => x.ZZ5_Value);
			nomenclature.IncludeColumn(x => x.ZZ5_Description);
			nomenclature.IncludeColumn(x => x.ZZ5_StartDate);
			nomenclature.IncludeColumnWithConstantValue(x => x.ZZ5_EndDate, false, Constants.MaximumSmallDateTime);
			nomenclature.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			nomenclature.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(nomenclature);

			return xmlWriterConfiguration;
		}

		protected static XmlWriterConfiguration GetTariffWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var tariff = new EntityTypeConfiguration<RefCusTariff>(true);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffType.Code.HSN);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariff.IncludeColumn(x => x.ZZ1_Description);
			tariff.IncludeColumn(x => x.ZZ1_StartDate);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.MaximumDateTime);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false);
			tariff.IncludeColumn(x => x.RefCusTariffAttributes);
			tariff.IncludeColumn(x => x.RefCusTariffUOMs);
			tariff.IncludeColumn(x => x.RefCusRates);
			tariff.IncludeColumn(x => x.RefCusVATApplicabilities);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(tariff);

			var attribute = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			attribute.IncludeColumn(x => x.ZZ3_Name, true);
			attribute.IncludeColumn(x => x.ZZ3_Value, true);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(attribute);

			var uom = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			uom.IncludeColumn(x => x.ZZ8_Type, true);
			uom.IncludeColumn(x => x.ZZ8_UOM, true);
			uom.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(uom);

			var rate = new EntityTypeConfiguration<RefCusRate>(true);
			rate.IncludeColumn(x => x.ZZ2_StartDate);
			rate.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, Constants.MaximumDateTime);
			rate.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rate.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			rate.IncludeColumn(x => x.ZZ2_RateFormula);
			rate.IncludeColumn(x => x.ZZ2_ZZS_NKPreference);
			rate.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			rate.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom, true);
			rate.IncludeColumn(x => x.RefCusApplicabilities);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(rate);

			var applicability = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicability.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			applicability.IncludeColumn(x => x.ZZT_StartDate);
			applicability.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, Constants.MaximumSmallDateTime);
			applicability.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicability.IncludeColumn(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true);
			applicability.IncludeColumn(x => x.RefCusExcludedTradeGroups);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(applicability);

			var excludedTradeGroup = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroup.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedTradeGroup.IncludeColumn(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(excludedTradeGroup);

			var vatapplicability = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			vatapplicability.IncludeColumn(x => x.ZX5_StartDate);
			vatapplicability.IncludeColumnWithConstantValue(x => x.ZX5_EndDate, false, Constants.MaximumSmallDateTime);
			vatapplicability.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
			vatapplicability.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			vatapplicability.IncludeColumn(x => x.ZX5_Description);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(vatapplicability);

			return xmlWriterConfiguration;
		}

		IEnumerable<ProcessedNomenclatureTariff> GetChapterRecords()
		{
			var list = new List<ProcessedNomenclatureTariff>();

			foreach (var chapterCode in nomenclatureHelper.ChapterCodes)
			{
				var chapterRecord = new ProcessedNomenclatureTariff()
				{
					Code = chapterCode,
					Description = nomenclatureHelper.GetChapterTitle(chapterCode),
					CompositeKey = $"{nomenclatureHelper.GetSection(chapterCode)}.{chapterCode}",
				};

				list.Add(chapterRecord);
			}

			return list;
		}

		IEnumerable<ProcessedNomenclatureTariff> GetSectionRecords()
		{
			var list = new List<ProcessedNomenclatureTariff>();

			foreach (var section in nomenclatureHelper.Sections)
			{
				var sectionCode = section.Code;
				var sectionRecord = new ProcessedNomenclatureTariff()
				{
					Code = sectionCode,
					Description = section.Description,
					CompositeKey = sectionCode,
				};

				list.Add(sectionRecord);
			}

			return list;
		}

		IEnumerable<ProcessedNomenclatureTariff> Process(Chapter chapter)
		{
			SetParsedDescriptions(chapter);
			MergeData(chapter);
			return GetProcessedDataForChapter(chapter);
		}

		#region Parse Description

		protected static void SetParsedDescriptions(Chapter chapter)
		{
			foreach (var record in chapter.Records)
			{
				record.ParsedDescription = ParseDescription(record.Description);
			}
		}

		public static NomenclatureDescription ParseDescription(string description)
		{
			var result = new NomenclatureDescription();

			if (!string.IsNullOrWhiteSpace(description))
			{
				var firstLetter = description.First(x => !char.IsWhiteSpace(x) && x != '-');
				var indexOfFirstLetter = description.IndexOf(firstLetter);
				var content = description.Substring(indexOfFirstLetter);
				var prefix = description.Substring(0, indexOfFirstLetter);
				var dashCount = prefix.Count(x => x == '-');
				var indent = dashCount > 0 ? prefix.IndexOf('-') : indexOfFirstLetter;

				result.Content = content;
				result.DashCount = dashCount;
				result.Indent = indent;
				result.IndexOfFirstLetter = indexOfFirstLetter;
			}

			return result;
		}

		#endregion

		#region Merge data for Chapter
		readonly NomenclatureHelper nomenclatureHelper = new NomenclatureHelper();

		protected void MergeData(Chapter chapter)
		{
			SetSubchapterFirstLineFlag(chapter);

			RawNomenclatureTariff current = new RawNomenclatureTariff();
			var list = new List<RawNomenclatureTariff>();

			foreach (var record in chapter.Records)
			{
				var parsedDescription = record.ParsedDescription;

				var isPartOfCurrent = !record.IsSubchapter && string.IsNullOrEmpty(record.Code)
					&& (parsedDescription.DashCount == 0 || (parsedDescription.DashCount == 1 && parsedDescription.Indent > 2));
				if (isPartOfCurrent)
				{
					var leadingWhiteSpace = new string(' ', parsedDescription.IndexOfFirstLetter > current.ParsedDescription.IndexOfFirstLetter ?
						parsedDescription.IndexOfFirstLetter - current.ParsedDescription.IndexOfFirstLetter : 1);
					current.ParsedDescription.Content += leadingWhiteSpace + record.Description.TrimStart();

					if (string.IsNullOrEmpty(current.UOM) && !string.IsNullOrEmpty(record.UOM))
					{
						current.UOM = record.UOM;
					}

					if (string.IsNullOrEmpty(current.DutyRate) && !string.IsNullOrEmpty(record.DutyRate))
					{
						current.DutyRate = record.DutyRate;
					}
				}
				else
				{
					current = record;
					current.HSCodeInfo = new Nomenclature
					{
						Section = nomenclatureHelper.GetSection(chapter.Code),
						Chapter = chapter.Code,
					};
					list.Add(current);
				}
			}

			var last = list.Last();
			if (last.IsSubchapter)
			{
				list.Remove(last);
			}

			chapter.Records = list.OrderBy(x => x.SeqNum);
		}

		static void SetSubchapterFirstLineFlag(Chapter chapter)
		{
			var first = chapter.Records.First();
			if (string.IsNullOrEmpty(first.Code))
			{
				first.IsSubchapter = true;
			}

			var list = chapter.Records.ToList();
			var others = chapter.Records.Skip(1).Where(x =>
			{
				var result = false;

				if (string.IsNullOrEmpty(x.Code) && x.ParsedDescription.DashCount == 0)
				{
					var previous = list[list.IndexOf(x) - 1];
					var isLastRowEmpty = x.SeqNum - previous.SeqNum > 1;
					if (isLastRowEmpty || (x.IsHorizontalCenter && !previous.IsHorizontalCenter))
					{
						result = true;
					}
				}

				return result;
			});

			foreach (var record in others)
			{
				record.IsSubchapter = true;
			}
		}

		#endregion

		#region Process Merged Data

		protected static IEnumerable<ProcessedNomenclatureTariff> GetProcessedDataForChapter(Chapter chapter)
		{
			chapter.Subchapters = chapter.Records.Where(x => x.IsSubchapter).OrderBy(x => x.SeqNum);
			SetSubchapterCode(chapter);
			chapter.Records = chapter.Records.Except(chapter.Subchapters).OrderBy(x => x.SeqNum);

			IEnumerable<ProcessedNomenclatureTariff> result;

			if (chapter.Records.Any())
			{
				var sortedList = new SortedList<string, ProcessedNomenclatureTariff>();

				var dataByHeading = GroupDataByHeading(chapter);
				foreach (var entry in dataByHeading)
				{
					var orderByLevel = new Dictionary<int, int>();
					var parentCompositeKey = string.Empty;
					var sortedRecords = entry.Value.OrderBy(x => x.SeqNum);
					foreach (var record in sortedRecords)
					{
						var parsedDescription = record.ParsedDescription;
						int level = parsedDescription.DashCount;
						if (orderByLevel.TryGetValue(level, out var order))
						{
							orderByLevel[level] = order + 1;
						}
						else
						{
							orderByLevel.Add(level, 1);
						}

						var dashCount = parsedDescription.DashCount;
						if (dashCount == 0)
						{
							var hsCodeInfo = record.HSCodeInfo;
							parentCompositeKey = $"{hsCodeInfo.Section}.{hsCodeInfo.Chapter}.{hsCodeInfo.Subchapter}.{hsCodeInfo.Heading}";
						}
						else
						{
							var parent = sortedList.Last(x => x.Value.Level == dashCount - 1).Value;
							parentCompositeKey = parent.CompositeKey;
						}

						var entity = new ProcessedNomenclatureTariff
						{
							Level = level,
							LevelOrder = orderByLevel[level]
						};

						var code = record.Code;
						entity.Code = GetTariffCode(code);
						entity.Description = parsedDescription.Content.Trim().Replace("\r", string.Empty).Replace("\n", " ");
						entity.UOM = GetTariffUOM(record.UOM);

						var (formula, derivedFrom) = GetTariffRate(record.DutyRate);
						entity.Rate = formula;
						entity.RateDerivedFrom = derivedFrom;

						entity.CompositeKey = GetCompositeKey(parentCompositeKey, record, entity.Level, entity.LevelOrder);

						sortedList.Add(entity.CompositeKey, entity);
					}
				}

				result = sortedList.Values;
			}
			else
			{
				result = Enumerable.Empty<ProcessedNomenclatureTariff>();
			}

			return result;
		}

		static void SetSubchapterCode(Chapter chapter)
		{
			var subchapters = chapter.Subchapters;
			if (subchapters.Any())
			{
				var subchapterOrderedList = subchapters.ToList();
				foreach (var record in chapter.Records)
				{
					var subchapter = subchapterOrderedList.LastOrDefault(x => x.SeqNum < record.SeqNum);
					if (subchapter != null)
					{
						record.HSCodeInfo.Subchapter = (subchapterOrderedList.IndexOf(subchapter) + 1).ToString("D2", CultureInfo.InvariantCulture);
					}
				}
			}
		}

		static Dictionary<string, List<RawNomenclatureTariff>> GroupDataByHeading(Chapter chapter)
		{
			var sortedMergedData = chapter.Records;
			var heading = sortedMergedData.First().Code.Replace(".", string.Empty).Substring(0, 4);

			var dataByHeading = new Dictionary<string, List<RawNomenclatureTariff>>();

			foreach (var record in sortedMergedData)
			{
				var rawCode = record.Code;
				if (record.Code.Length == 7)
				{
					record.HSCodeInfo.SubHeading = rawCode.Substring(5, 2);
				}
				else if (record.Code.Length > 7)
				{
					record.HSCodeInfo.SubHeading = rawCode.Substring(5, 2);
					record.HSCodeInfo.LowLevelSubHeading = rawCode.Substring(8);
				}

				var code = record.Code.Replace(".", string.Empty);
				var isNewHeading = !string.IsNullOrEmpty(code) && !code.StartsWith(heading, StringComparison.Ordinal);
				if (isNewHeading)
				{
					heading = code.Substring(0, 4);
				}

				record.HSCodeInfo.Heading = heading.Substring(2, 2);

				if (dataByHeading.TryGetValue(heading, out var data))
				{
					data.Add(record);
				}
				else
				{
					dataByHeading.Add(heading, new List<RawNomenclatureTariff> { record });
				}
			}

			return dataByHeading;
		}

		public static string GetCompositeKey(string parentCompositeKey, RawNomenclatureTariff record, int level, int levelOrder)
		{
			var section = record.HSCodeInfo.Section;
			var chapter = record.HSCodeInfo.Chapter;
			var subchapter = record.HSCodeInfo.Subchapter;
			var heading = record.HSCodeInfo.Heading;
			var formattedLevel = level.ToString("D2", CultureInfo.InvariantCulture);
			var formattedLevelOrder = levelOrder.ToString("D2", CultureInfo.InvariantCulture);

			string result;

			if (level == 0)
			{
				result = $"{section}.{chapter}.{subchapter}.{heading}";
			}
			else
			{
				result = $"{parentCompositeKey}.{formattedLevel}.{formattedLevelOrder}";
			}

			return result;
		}

		#endregion

		public static (string formula, string derivedFrom) GetTariffRate(string dutyRate)
		{
			var formula = string.Empty;
			var derivedFrom = string.Empty;

			if (!string.IsNullOrEmpty(dutyRate) && decimal.TryParse(dutyRate, out var rateValue))
			{
				formula = $"VFD * {rateValue / 100:F2}";
				derivedFrom = $"{rateValue}%";
			}

			return (formula, derivedFrom);
		}

#pragma warning disable CA1502
		public static string GetTariffUOM(string rawUOM)
		{
			var result = string.Empty;

			switch (rawUOM)
			{
				case "-":
					result = "-";
					break;
				case "Ct/I":
					result = "CCT";
					break;
				case "TJ":
					result = "D30";
					break;
				case "giF/S":
				case "gi F/S":
					result = "GFI";
					break;
				case "Gr":
				case "Gram":
					result = "GRM";
					break;
				case "100 Adet":
					result = "H62";
					break;
				case "Kg/net eda":
				case "Kg-net eda":
					result = "K58";
					break;
				case "KgK2O":
				case "KgP2O5":
				case "KgH2O2":
					result = "KHO";
					break;
				case "kg met.am":
					result = "KMA";
					break;
				case "Kg N":
					result = "KNI";
					break;
				case "KgKOH":
					result = "KOH";
					break;
				case "Kg %90 sdt":
					result = "KSD";
					break;
				case "Baþ":
				case "Baş":
					result = "BAS";
					break;
				case "Adet":
				case "Adet (1)":
				case "adet":
					result = "C62";
					break;
				case "KgNaOH":
					result = "KSH";
					break;
				case "Kg U":
					result = "KUR";
					break;
				case "1000 Adet":
				case "1000 Ad.":
				case "1000 Ad":
					result = "T3";
					break;
				case "Litre":
					result = "LTR";
					break;
				case "Lt alk.%100":
				case "Lt alk. % 100":
					result = "LPA";
					break;
				case "1 000 m3":
					result = "R9";
					break;
				case "m³ (1)":
				case "m3 (1)":
				case "m3":
				case "m3   (1)":
					result = "MTQ";
					break;
				case "1000 kWs":
					result = "TWH";
					break;
				case "m2":
					result = "MTK";
					break;
				case "Çift":
					result = "PR";
					break;
				case "m":
				case "Metre":
					result = "MTR";
					break;
				case "Karat":
					result = "NCR";
					break;
				case "hücre adet":
					result = "NCL";
					break;
				case "Kilogram":
					result = "KGM";
					break;
			}

			return result;
		}

		public static string GetTariffCode(string code)
		{
			return code.Replace(".", string.Empty);
		}

		public string ErrorMessage => ErrorBuilder.ToString();

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
