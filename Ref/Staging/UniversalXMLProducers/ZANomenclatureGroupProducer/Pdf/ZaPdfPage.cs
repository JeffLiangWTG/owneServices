using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public class ZaPdfPage
	{
		readonly List<ZaPdfCoordinateLine> DataCoordinateContent = new List<ZaPdfCoordinateLine>();
		readonly IZaSchedule zaSchedule;
		internal float yStartCoordinates;
		internal List<ZaPdfChunk> PdfLines = new List<ZaPdfChunk>();

		public ZaPdfPage(List<ZaPdfCoordinateLine> dataCoordinateContent, IZaSchedule schedule)
		{
			Argument.NotNull(dataCoordinateContent, nameof(dataCoordinateContent));
			Argument.NotNull(schedule, nameof(schedule));

			DataCoordinateContent = dataCoordinateContent;
			zaSchedule = schedule;
		}

		public bool FindHeading()
		{
			foreach (var item in DataCoordinateContent)
			{
				PdfLines.Add(new ZaPdfChunk(item.StartCoordinate, ""));
				foreach (var subItem in item.Chunks)
				{
					var line = PdfLines.GetByCoordinate(item.StartCoordinate);
					line.Content += subItem.Content;
				}
			}
			var headers = PdfLines.Where(x => zaSchedule.GetHeaderTextForMatching().Contains(x.Content?.Trim())).Select(x => x.Coordinate);
			yStartCoordinates = headers.Any() ? headers.Max() : 0;
			if (yStartCoordinates != 0)
			{
				return true;
			}
			return false;
		}

		public List<ZaPdfCoordinateColumnContent> SplitDataToColumns()
		{
			var columns = zaSchedule.GetColumnXCoordinates();
			var result = new List<ZaPdfCoordinateColumnContent>();
			var currentRowKey = 0;

			for (var x = 0; x < DataCoordinateContent.Count; x++)
			{
				var currentRow = DataCoordinateContent[x];
				var nextRow = x + 1 < DataCoordinateContent.Count ? DataCoordinateContent[x + 1] : null;

				if (currentRow.StartCoordinate > yStartCoordinates)
				{
					if (currentRow.StartCoordinate < currentRowKey - 2 || currentRow.StartCoordinate > currentRowKey + 2)
					{
						result.Add(new ZaPdfCoordinateColumnContent(currentRow.StartCoordinate, new List<ZaPdfColumnContent>()));
						currentRowKey = currentRow.StartCoordinate;
					}
				}
				foreach (var column in columns)
				{
					foreach (var chunk in currentRow.Chunks)
					{
						if (column.columnName == "Description" && result.ContainsCoordinate(currentRowKey) && result.GetByCoordinate(currentRowKey).ColumnsContents.Count == 0
							&& (nextRow == null || Math.Abs(currentRowKey - nextRow.StartCoordinate) > 2))
						{
							var lastObject = result.Last(o => o.Coordinate != currentRowKey && o.ColumnsContents.Any(c => c.ColumnName == "HeadingSubHeading"));
							if (!lastObject.ColumnsContents.ContainsColumn(column.columnName))
							{
								lastObject.ColumnsContents.Add(new ZaPdfColumnContent(column.columnName, ""));
							}
							if (lastObject.ColumnsContents.ContainsColumn(column.columnName))
							{
								lastObject.ColumnsContents.GetByColumnName(column.columnName).Content += chunk.Content;
							}
							continue;
						}
						if (column.columnName == "Description" && result.ContainsCoordinate(currentRowKey) && !result.GetByCoordinate(currentRowKey).ColumnsContents.ContainsColumn("CD") && chunk.Coordinate >= column.xStartCoordinate)
						{
							SetValueIntoRowColumn(result, currentRowKey, column.columnName, chunk.Content);
							continue;
						}
						if (chunk.Coordinate > column.xEndCoordinate)
						{
							break;
						}
						if (chunk.Coordinate >= column.xStartCoordinate && chunk.Coordinate < column.xEndCoordinate)
						{
							SetValueIntoRowColumn(result, currentRowKey, column.columnName, chunk.Content);
						}
					}
				}
			}
			return result;
		}

		static void SetValueIntoRowColumn(List<ZaPdfCoordinateColumnContent> rows, int rowKey, string columnName, string value)
		{
			Argument.NotNull(rows, nameof(rows));
			Argument.NotNullOrEmpty(columnName, nameof(columnName));
			Argument.NotNull(value, nameof(value));

			if (rows.ContainsCoordinate(rowKey))
			{
				var row = rows.GetByCoordinate(rowKey);
				if (!row.ColumnsContents.ContainsColumn(columnName))
				{
					row.ColumnsContents.Add(new ZaPdfColumnContent(columnName, ""));
				}
				if (row.ColumnsContents.ContainsColumn(columnName))
				{
					var column = row.ColumnsContents.GetByColumnName(columnName);
					column.Content += value;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity")]
		public void MergeDescriptionsWithoutHeadingWithAscendingOrDescendingRows(List<ZaPdfCoordinateColumnContent> splittedData)
		{
			Argument.NotNull(splittedData, nameof(splittedData));
			for (var i = 0; i < splittedData.Count; i++)
			{
				if (i == 0)
				{
					continue;
				}
				var currentElement = splittedData.ElementAt(i);
				if (currentElement.ColumnsContents.ContainsColumn("CD"))
				{
					if (currentElement.ColumnsContents.ContainsColumn("HeadingSubHeading") && !string.IsNullOrEmpty(currentElement.ColumnsContents.GetByColumnName("HeadingSubHeading")?.Content?.Trim()) && !currentElement.ColumnsContents.ContainsColumn("Description"))
					{
						var lastElement = splittedData.ElementAt(i - 1);
						if (lastElement.ColumnsContents.ContainsColumn("Description") && string.IsNullOrEmpty(lastElement.ColumnsContents.GetByColumnName("HeadingSubHeading")?.Content?.Trim()))
						{
							var column = lastElement.ColumnsContents.GetByColumnName("Description");
							currentElement.ColumnsContents.Add(new ZaPdfColumnContent("Description", column.Content));
							continue;
						}

						if ((i + 1 >= splittedData.Count))
						{
							break;
						}
						var nextElement = splittedData.ElementAt(i + 1);
						if (nextElement.ColumnsContents.ContainsColumn("Description") && string.IsNullOrEmpty(nextElement.ColumnsContents.GetByColumnName("HeadingSubHeading")?.Content?.Trim()))
						{
							var column = nextElement.ColumnsContents.GetByColumnName("Description");
							currentElement.ColumnsContents.Add(new ZaPdfColumnContent("Description", column.Content));
						}
					}
					else if (currentElement.ColumnsContents.ContainsColumn("HeadingSubHeading") && !string.IsNullOrEmpty(currentElement.ColumnsContents.GetByColumnName("HeadingSubHeading")?.Content?.Trim()) && currentElement.ColumnsContents.ContainsColumn("Description"))
					{
						var currentElementColumn = currentElement.ColumnsContents.GetByColumnName("Description");
						if (currentElementColumn.Content != null)
						{
							var currentElementDescription = currentElementColumn.Content;
							var lastElement = splittedData.ElementAt(i - 1);

							if (lastElement.ColumnsContents.ContainsColumn("HeadingSubHeading"))
							{
								string lastElementContent = null;
								var lastElementColumn = lastElement.ColumnsContents.GetByColumnName("Description");
								if (lastElementColumn != null)
								{
									lastElementContent = lastElementColumn.Content;
								}

								if (lastElementContent != null && string.IsNullOrEmpty(lastElement.ColumnsContents.GetByColumnName("HeadingSubHeading")?.Content?.Trim()) && lastElementContent.Trim().StartsWith("-", StringComparison.Ordinal))
								{
									currentElementColumn.Content = lastElementColumn.Content;
									continue;
								}
								else if (!currentElementDescription.Trim().StartsWith("-", StringComparison.Ordinal) && !currentElementDescription.Trim().EndsWith("-", StringComparison.Ordinal))
								{
									var idx = currentElementDescription.LastIndexOf('-');
									if (idx == -1)
									{
										continue;
									}
									currentElementColumn.Content = currentElementDescription.Remove(idx + 1);
								}
							}
						}
					}
				}
			}
		}

		public List<RowData> GetTariffAndNomenclatureList(List<ZaPdfCoordinateColumnContent> data)
		{
			Argument.NotNull(data, nameof(data));
			return zaSchedule.GetTariffAndNomenclature(data);
		}
	}
}
