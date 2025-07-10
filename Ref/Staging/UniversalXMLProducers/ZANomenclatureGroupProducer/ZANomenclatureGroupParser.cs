using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public class ZANomenclatureGroupParser : IZANomenclatureGroupParser
	{
		#region Fields

		readonly IXmlWriter _tariffXmlWriter;
		readonly IXmlWriter _nomenclatureGroupXmlWriter;
		readonly List<ICompositeKeyNode> _compositeKeyStructure = new List<ICompositeKeyNode>();
		readonly CompositeKeyNode _rootNode = new CompositeKeyNode("root", "root", DateTime.MinValue, DateTime.MinValue, CompositeKeyNodeType.PlaceHolder, -1, null);

		public string ActualSection => Utils.ConvertRomanNumeralToInt(actualSection).ToString("00", CultureInfo.CurrentCulture);
		string actualSection;

		public string ActualSchedule => Convert.ToInt32(actualSchedule, CultureInfo.CurrentCulture).ToString("00", CultureInfo.CurrentCulture);
		string actualSchedule;

		public string ActualChapter => Convert.ToInt32(actualChapter, CultureInfo.CurrentCulture).ToString("00", CultureInfo.CurrentCulture);
		string actualChapter;

		public string ActualPart => Convert.ToInt32(actualPart, CultureInfo.CurrentCulture).ToString("00", CultureInfo.CurrentCulture);
		string actualPart;

		public string ActualSubchapter => actualSubchapter == null ? null : Utils.ConvertRomanNumeralToInt(actualSubchapter).ToString("00", CultureInfo.CurrentCulture);
		string actualSubchapter;

		public DateTime CreatedTime { get; private set; }

		#endregion

		#region Constants

		const string SectionRegex = @"SECTION ([A-Z]*)";
		const string SchedulePartRegex = @"SCHEDULE ([0-9].*) / PART ([0-9].*) / SECTION ([A-Z].*)";
		const string ChapterRegex = @"CHAPTER *([0-9]*)";
		const string SubchapterRegex = @"SUBCHAPTER ([A-Z]*)";

		const string TariffXmlDataSource = "ZA Tariff Composite Key";
		const string NomenclatureGroupXmlDataSource = "ZA Nomenclature Group";

		#endregion

		#region Constructor

		public ZANomenclatureGroupParser()
		{
			_tariffXmlWriter = new XmlWriter(XmlWriterHelper.GetRefCusTariffWriterConfiguration());
			_nomenclatureGroupXmlWriter = new XmlWriter(XmlWriterHelper.GetRefCusNomenclatureGroupConfiguration());

			_compositeKeyStructure.Add(_rootNode);
		}

		#endregion

		public void Parse(IFileDownloader fileDownloader)
		{
			using (var response = fileDownloader.GetFileStream())
			using (var stream = response.GetResponseStream())
			using (var pdfReader = new PdfReader(stream))
			{ 
				ParsePdf(pdfReader);
			}
		}

		void ParsePdf(PdfReader pdfReader)
		{
			Argument.NotNull(pdfReader, nameof(pdfReader));

			using (var pdfDocument = new PdfDocument(pdfReader))
			{
				var hasInitializedXmlWriters = false;

				for (int pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
				{
					var page = pdfDocument.GetPage(pageNumber);
					var strategyExtraction = PDFStrategyPicker.GetByPdfVersion(pdfDocument.GetPdfVersion());
					var processor = new PdfCanvasProcessor(strategyExtraction);
					processor.ProcessPageContent(page);

					if (strategyExtraction.PdfCoordinateContents != null)
					{
						var pdfPage = new ZaPdfPage(strategyExtraction.PdfCoordinateContents, new Schedule());

						if (pdfPage.FindHeading())
						{
							GetEffectiveDate(pdfPage.PdfLines);

							if (!hasInitializedXmlWriters)
							{
								InitializeXmlWriters();
								hasInitializedXmlWriters = true;
							}

							var splittedData = pdfPage.SplitDataToColumns();
							pdfPage.MergeDescriptionsWithoutHeadingWithAscendingOrDescendingRows(splittedData);

							var tariffNomenclatureList = pdfPage.GetTariffAndNomenclatureList(splittedData);
							tariffNomenclatureList = RemoveDuplicateData(tariffNomenclatureList);

							foreach (var item in tariffNomenclatureList)
							{
								PopulateTariffAndNomenclature(item);
							}
						}
						else
						{
							GetEffectiveDate(pdfPage.PdfLines);

							if (!hasInitializedXmlWriters)
							{
								InitializeXmlWriters();
								hasInitializedXmlWriters = true;
							}

							GetSchedulePartSection(pdfPage.PdfLines);
						}
					}
				}
				var result = ZANomenclatureGroupParserHelper.CompositeKeyGenerate(_rootNode);

				ZANomenclatureGroupParserHelper.PopulateTariffUXML(result.Tariffs, _tariffXmlWriter);

				ZANomenclatureGroupParserHelper.PopulateNomenclatureGroupUXML(result.NomenclatureGroups, _nomenclatureGroupXmlWriter);
			}
		}

		struct DataKey
		{
			public string CleanCode { get; set; }
			public int Level { get; set; }
		}

		static string GetHeading(RowData rowData)
		{
			return rowData.IsTariff ? rowData.Object.GetByColumnName("HeadingSubHeading")?.Content?.TrimEnd() ?? string.Empty :
				rowData.Object[0]?.ColumnName?.TrimEnd() ?? string.Empty;
		}

		static List<RowData> RemoveDuplicateData(List<RowData> data)
		{
			var result = new List<RowData>();
			var keys = new List<DataKey>();

			foreach (var item in data)
			{
				var heading = GetHeading(item);
				if (!string.IsNullOrEmpty(heading))
				{
					var cleanCode = Utils.RemoveTrailingZeros(heading).Replace(".", string.Empty);

					bool shouldAdd = true;

					if (heading.EndsWith(".00", StringComparison.Ordinal)) // Special case for some headings/subheadings/tariffs
					{
						if (keys.Any(x => x.CleanCode == cleanCode && x.Level == item.Level))
						{
							shouldAdd = false;
						}
					}

					if (shouldAdd)
					{
						keys.Add(new DataKey { CleanCode = cleanCode, Level = item.Level });
						result.Add(item);
					}
				}
				else
				{
					result.Add(item);
				}
			}

			return result;
		}

		void PopulateTariffAndNomenclature(RowData item)
		{
			Argument.NotNull(item, nameof(item));

			if (item.IsTariff)
			{
				PopulateTariff(item);
			}
			else
			{
				PopulateNomenclatureElement(item);
			}
		}

		void InitializeXmlWriters()
		{
			_tariffXmlWriter.SetDataSource(TariffXmlDataSource);
			_tariffXmlWriter.SetPublicationTime(CreatedTime.Date);
			_tariffXmlWriter.SetUpdateType(UpdateType.Full);
			_nomenclatureGroupXmlWriter.SetDataSource(NomenclatureGroupXmlDataSource);
			_nomenclatureGroupXmlWriter.SetPublicationTime(CreatedTime.Date);
			_nomenclatureGroupXmlWriter.SetUpdateType(UpdateType.Full);
		}

		void InitializeErrorXmlWriters()
		{
			_tariffXmlWriter.SetDataSource(TariffXmlDataSource);
			_tariffXmlWriter.SetPublicationTime(DateTime.UtcNow);
			_tariffXmlWriter.SetUpdateType(UpdateType.Full);
			_nomenclatureGroupXmlWriter.SetDataSource(NomenclatureGroupXmlDataSource);
			_nomenclatureGroupXmlWriter.SetPublicationTime(DateTime.UtcNow);
			_nomenclatureGroupXmlWriter.SetUpdateType(UpdateType.Full);
		}

		void GetSchedulePartSection(List<ZaPdfChunk> pageLines)
		{
			var hasRecentlyChangedChapter = false;
			var hasRecentlyChangedSection = false;
			var hasRecentlyChangedSubchapter = false;
			if (pageLines != null)
			{
				foreach (var line in pageLines)
				{
					if (string.IsNullOrEmpty(line.Content.Trim()))
					{
						continue;
					}

					if (hasRecentlyChangedChapter || hasRecentlyChangedSection || hasRecentlyChangedSubchapter)
					{
						if (Regex.IsMatch(line.Content, SchedulePartRegex))
						{
							continue;
						}
						var keyValueNomenclature = new ZaPdfColumnContent(string.Empty, line.Content);
						PopulateNomenclatureElement(keyValueNomenclature, hasRecentlyChangedSection, hasRecentlyChangedChapter, hasRecentlyChangedSubchapter, 0);

						hasRecentlyChangedChapter = false;
						hasRecentlyChangedSection = false;
						hasRecentlyChangedSubchapter = false;
						continue;
					}

					if (line.Content.ToUpper(CultureInfo.CurrentCulture).Contains("SCHEDULE") && !line.Content.ToUpper(CultureInfo.CurrentCulture).Contains("SECTION"))
					{
						var match = Regex.Match(line.Content, SchedulePartRegex);
						if (match.Success)
						{
							var schedule = match.Groups[1].Value;
							var part = match.Groups[2].Value;
							if (schedule != actualSchedule)
							{
								actualSchedule = schedule;
							}
							if (part != actualPart)
							{
								actualPart = part;
							}
						}
					}
					else if (line.Content.ToUpper(CultureInfo.CurrentCulture).Contains("SECTION") && !line.Content.ToUpper(CultureInfo.CurrentCulture).Contains("SCHEDULE"))
					{
						var match = Regex.Match(line.Content, SectionRegex);
						if (match.Success)
						{
							var section = match.Groups[1].Value;
							if (section != actualSection)
							{
								actualSection = section;
								hasRecentlyChangedSection = true;
							}
						}
					}
					else if (line.Content.ToUpper(CultureInfo.CurrentCulture).Contains("SUBCHAPTER"))
					{
						var match = Regex.Match(line.Content, SubchapterRegex);
						if (match.Success)
						{
							var subchapter = match.Groups[1].Value;
							if (subchapter != actualSubchapter)
							{
								actualSubchapter = match.Groups[1].Value;
								hasRecentlyChangedSubchapter = true;
							}
						}
					}
					else if (line.Content.ToUpper(CultureInfo.CurrentCulture).Contains("CHAPTER"))
					{
						var match = Regex.Match(line.Content, ChapterRegex);
						if (match.Success)
						{
							var chapter = match.Groups[1].Value;
							if (chapter != actualChapter)
							{
								actualChapter = match.Groups[1].Value;
								actualSubchapter = null;
								hasRecentlyChangedChapter = true;
							}
						}
					}
				}
			}
		}

		void PopulateTariff(RowData rowData)
		{
			Argument.NotNull(rowData, nameof(rowData));

			var value = rowData.Object.GetByColumnName("HeadingSubHeading")?.Content?.TrimEnd();

			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			value = value.Replace(".", string.Empty);
			var valueWithoutZeros = Utils.RemoveTrailingZeros(value);

			var level = rowData.Level;
			if (valueWithoutZeros.Length == 4)
			{
				var description = rowData.Object.GetByColumnName("Description")?.Content.Trim();
				PopulateNomenclatureElement(new ZaPdfColumnContent(value, description), false, false, false, level);
				level++;
			}

			var parent = ZANomenclatureGroupParserHelper.GetParent(_compositeKeyStructure, level);
			_ = new CompositeKeyNode(value, "notneeded", DateTime.MinValue, DateTime.MinValue, CompositeKeyNodeType.Tariff, level, parent);
		}

		void PopulateNomenclatureElement(ZaPdfColumnContent valueDescription, bool isSection, bool isChapter, bool isSubchapter, int level)
		{
			Argument.NotNull(valueDescription, nameof(valueDescription));

			var endDate = "2079-06-06 23:59:00";
			string value;

			var description = valueDescription.Content.Replace("-", "").Replace(":", "").Trim();
			// remove special character
			char toRemove = (char)0x1a;
			var pattern = @"\x" + ((int)toRemove).ToString("x", CultureInfo.CurrentCulture) + "+";
			description = Regex.Replace(description, pattern, string.Empty);

			if (string.IsNullOrEmpty(description))
			{
				return;
			}

			if (isSection)
			{
				_ = new CompositeKeyNode(ActualSection, description, CreatedTime, Convert.ToDateTime(endDate, CultureInfo.CurrentCulture), CompositeKeyNodeType.NomenclatureGroup, 0, _rootNode) { ZZ5Value = string.Empty };
			}
			else if (isChapter)
			{
				value = ActualChapter;
				var parent = ZANomenclatureGroupParserHelper.GetParent(_compositeKeyStructure, 1);
				_ = new CompositeKeyNode(value, description, CreatedTime, Convert.ToDateTime(endDate, CultureInfo.CurrentCulture), CompositeKeyNodeType.NomenclatureGroup, 1, parent) { ZZ5Value = string.Empty };
			}
			else if (isSubchapter)
			{
				value = ActualSubchapter;
				var parent = ZANomenclatureGroupParserHelper.GetParent(_compositeKeyStructure, 2);
				_ = new CompositeKeyNode(value, description, CreatedTime, Convert.ToDateTime(endDate, CultureInfo.CurrentCulture), CompositeKeyNodeType.NomenclatureGroup, 2, parent) { ZZ5Value = string.Empty };
			}
			else
			{
				if (actualSubchapter == null)
				{
					actualSubchapter = "00";
					var subChapterParent = ZANomenclatureGroupParserHelper.GetParent(_compositeKeyStructure, 2);

					_ = new CompositeKeyNode(actualSubchapter, description, CreatedTime, Convert.ToDateTime(endDate, CultureInfo.CurrentCulture), CompositeKeyNodeType.PlaceHolder, 2, subChapterParent);
				}

				if (string.IsNullOrEmpty(valueDescription.ColumnName.TrimEnd()))
				{
					return;
				}
				value = FormattableString.Invariant($"{valueDescription.ColumnName.TrimEnd().Replace(".", "")}");

				var parent = ZANomenclatureGroupParserHelper.GetParent(_compositeKeyStructure, level);
				_ = new CompositeKeyNode(Utils.RemoveTrailingZeros(value), description, CreatedTime, Convert.ToDateTime(endDate, CultureInfo.CurrentCulture), CompositeKeyNodeType.NomenclatureGroup, level, parent);
			}
		}

		void PopulateNomenclatureElement(RowData rowData)
		{
			Argument.NotNull(rowData, nameof(rowData));

			var pdfColumnContent = rowData.Object.FirstOrDefault();
			PopulateNomenclatureElement(pdfColumnContent, false, false, false, rowData.Level);
		}

		void GetEffectiveDate(List<ZaPdfChunk> pageLines)
		{
			if (pageLines != null && CreatedTime == DateTime.MinValue)
			{
				foreach (var line in pageLines)
				{
					if (line.Content.ToUpper(CultureInfo.CurrentCulture).Contains("DATE: "))
					{
						var idx = line.Content.ToUpper(CultureInfo.CurrentCulture).IndexOf("DATE: ", StringComparison.Ordinal);
						var extractedDate = line.Content.ToUpper(CultureInfo.CurrentCulture).Substring(idx, 16);
						if (DateTime.TryParse(extractedDate.Substring(6, 10), out var isValueADate))
						{
							CreatedTime = isValueADate;
							break;
						}
					}
				}
			}
		}

		public void ExportToXml(string filePathNomenclatureGroup, string filePathTariff)
		{
			Argument.NotNullOrEmpty(filePathNomenclatureGroup, nameof(filePathNomenclatureGroup));
			Argument.NotNullOrEmpty(filePathTariff, nameof(filePathTariff));

			_nomenclatureGroupXmlWriter.SaveXml(filePathNomenclatureGroup);
			_tariffXmlWriter.SaveXml(filePathTariff);
		}
	}
}
