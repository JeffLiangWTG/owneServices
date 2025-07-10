using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public class Schedule : IZaSchedule
	{
		public List<string> GetHeaderTextForMatching()
		{
			return new List<string>()
			{
				"Heading /CDArticle DescriptionStatisticalRate of Duty",
				"SubheadingUnitGeneralEU / UKEFTASADCMERCOSURAfCFTA"
			};
		}

		public List<ZaColumn> GetColumnXCoordinates()
		{
			var columns = new List<ZaColumn>
			{
				new ZaColumn(@"HeadingSubHeading", 39f, 113f),
				new ZaColumn(@"CD", 114f, 140f),
				new ZaColumn(@"Description", 141f, 449f),
				new ZaColumn(@"StatiscicalUnit", 450f, 480f),
				new ZaColumn(@"GeneralRate", 481f, 534f),
				new ZaColumn(@"EURate", 535f, 585f),
				new ZaColumn(@"EFTARate", 586f, 636f),
				new ZaColumn(@"SADCRate", 637f, 688f),
				new ZaColumn(@"MercosurRate", 689f, 756f),
				new ZaColumn(@"AfCFTA", 757f, 815f)
			};
			return columns;
		}

		static bool IsTariff(List<ZaPdfColumnContent> columns)
		{
			if (columns == null)
			{
				return false;
			}
			if (columns.ContainsColumn("HeadingSubHeading") && columns.ContainsColumn("CD"))
			{
				if (!string.IsNullOrWhiteSpace(columns.GetByColumnName("HeadingSubHeading")?.Content) && !string.IsNullOrWhiteSpace(columns.GetByColumnName("CD")?.Content?.Trim()) && int.TryParse(columns.GetByColumnName("CD")?.Content?.Trim(), out int _))
				{
					return true;
				}
			}
			return false;
		}

		static bool IsHeading(List<ZaPdfColumnContent> columnContent)
		{
			if (columnContent == null)
			{
				return false;
			}
			if (columnContent.ContainsColumn("HeadingSubHeading"))
			{
				if (!string.IsNullOrWhiteSpace(columnContent.GetByColumnName("HeadingSubHeading")?.Content))
				{
					if (columnContent.ContainsColumn("CD"))
					{
						if (string.IsNullOrWhiteSpace(columnContent.GetByColumnName("CD")?.Content?.Trim()))
						{
							return true;
						}
						else if (!string.IsNullOrWhiteSpace(columnContent.GetByColumnName("CD")?.Content?.Trim()) && !int.TryParse(columnContent.GetByColumnName("CD")?.Content?.Trim(), out int _))
						{
							return true;
						}
					}
					return true;
				}
			}
			return false;
		}

		List<ZaPdfColumnContent> PopulateTariff(List<ZaPdfColumnContent> currentTariff, List<ZaPdfColumnContent> newTariffData)
		{
			Argument.NotNull(currentTariff, nameof(currentTariff));
			Argument.NotNull(newTariffData, nameof(newTariffData));
			foreach (var column in GetColumnXCoordinates())
			{
				if (newTariffData.ContainsColumn(column.columnName) && currentTariff.ContainsColumn(column.columnName))
				{
					var pdfColumn = currentTariff.GetByColumnName(column.columnName);
					pdfColumn.Content += newTariffData.GetByColumnName(column.columnName)?.Content;
				}
				if (!currentTariff.ContainsColumn(column.columnName) && newTariffData.ContainsColumn(column.columnName))
				{
					var pdfColumnNewTariff = newTariffData.GetByColumnName(column.columnName);
					currentTariff.Add(new ZaPdfColumnContent(column.columnName, pdfColumnNewTariff.Content));
				}
			}
			return currentTariff;
		}

		static int GetLevel(ZaPdfCoordinateColumnContent columnContent)
		{
			Argument.NotNull(columnContent, nameof(columnContent));

			if (columnContent.ColumnsContents.ContainsColumn("Description"))
			{
				var description = columnContent.ColumnsContents.GetByColumnName("Description")?.Content;
				if (!string.IsNullOrEmpty(description))
				{
					var charArray = description.ToCharArray();
					var result = 0;
					foreach (var charValue in charArray)
					{
						if (char.IsWhiteSpace(charValue))
						{
							continue;
						}
						else if (charValue != '-')
						{
							break;
						}
						else if (charValue == '-')
						{
							result++;
						}
					}
					if (result > 0)
					{
						return result + 3;
					}
					foreach (var charValue in charArray.Reverse())
					{
						if (char.IsWhiteSpace(charValue))
						{
							continue;
						}
						else if (charValue != '-')
						{
							break;
						}
						else if (charValue == '-')
						{
							result++;
						}
					}
					if (columnContent.ColumnsContents.ContainsColumn("HeadingSubHeading"))
					{
						var headingSubHeading = columnContent.ColumnsContents.GetByColumnName("HeadingSubHeading")?.Content;
						var splitHeader = headingSubHeading.Split('.');
						if (splitHeader.Length == 2)
						{
							if (splitHeader[1].Trim() == "00")
							{
								return 3;
							}
						}
						else if (splitHeader.Length == 3 && result == 0)
						{
							if (splitHeader[1].Trim() == "00")
							{
								return 4;
							}
						}
					}

					return result + 3;
				}
			}
			return 3;
		}

		public List<RowData> GetTariffAndNomenclature(List<ZaPdfCoordinateColumnContent> pdfCoordinateColumns)
		{
			Argument.NotNull(pdfCoordinateColumns, nameof(pdfCoordinateColumns));
			var result = new List<RowData>();
			var currentTariff = new List<ZaPdfColumnContent>();
			var nomenclature = new List<ZaPdfColumnContent>();
			foreach (var line in pdfCoordinateColumns)
			{
				if (IsTariff(line.ColumnsContents))
				{
					if (currentTariff.Count != 0)
					{
						result.Add(new RowData() { IsTariff = true, Level = GetLevel(line), Object = currentTariff });
					}
					currentTariff = new List<ZaPdfColumnContent>();
					currentTariff = PopulateTariff(currentTariff, line.ColumnsContents);
				}
				else
				{
					if (line.ColumnsContents.ContainsColumn("HeadingSubHeading"))
					{
						var value = line.ColumnsContents.GetByColumnName("HeadingSubHeading")?.Content;
						if (!line.ColumnsContents.ContainsColumn("Description"))
						{
							continue;
						}
						var column = line.ColumnsContents.GetByColumnName("Description");
						var obj = new List<ZaPdfColumnContent>()
						{
							new ZaPdfColumnContent(value, column.Content)
						};
						result.Add(new RowData() { IsTariff = false, Object = obj, Level = GetLevel(line) });
					}
				}
				if (!IsHeading(line.ColumnsContents) && currentTariff.Count != 0)
				{
					currentTariff = PopulateTariff(currentTariff, line.ColumnsContents);
				}
				else
				{
					if (currentTariff.Count != 0)
					{
						result.Add(new RowData() { IsTariff = true, Level = GetLevel(line), Object = currentTariff });
					}
					currentTariff = new List<ZaPdfColumnContent>();
				}
			}
			return result;
		}
	}

	public class RowData
	{
		public int Level { get; set; }
		public List<ZaPdfColumnContent> Object { get; set; }
		public bool IsTariff { get; set; }
	}
}
