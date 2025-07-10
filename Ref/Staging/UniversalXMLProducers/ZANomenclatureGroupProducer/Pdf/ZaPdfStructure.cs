using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public class ZaPdfChunk
	{
		public ZaPdfChunk(int coordinate, string content)
		{
			Argument.NotNull(content, nameof(content));
			Coordinate = coordinate;
			Content = content;
		}

		public int Coordinate { get; set; }
		public string Content { get; set; }
	}

	public class ZaPdfColumnContent
	{
		public ZaPdfColumnContent(string columnName, string content)
		{
			Argument.NotNull(columnName, nameof(columnName));
			Argument.NotNull(content, nameof(content));

			ColumnName = columnName;
			Content = content;
		}
		public string ColumnName { get; set; }
		public string Content { get; set; }
	}

	public class ZaPdfCoordinateLine
	{
		public ZaPdfCoordinateLine(int startCoordinate, List<ZaPdfChunk> lines)
		{
			Argument.NotNull(lines, nameof(lines));
			StartCoordinate = startCoordinate;
			Chunks = lines;
		}
		public int StartCoordinate { get; set; }
		public List<ZaPdfChunk> Chunks { get; set; }

		public void AddChunk(int coordinate, string content)
		{
			Argument.NotNull(content, nameof(content));
			for (int x = 0; x < Chunks.Count; x++)
			{
				if (Chunks[x]?.Coordinate > coordinate)
				{
					Chunks.Insert(x, new ZaPdfChunk(coordinate, content));
					return;
				}
			}
			Chunks.Add(new ZaPdfChunk(coordinate, content));
		}
	}

	public class ZaPdfCoordinateColumnContent
	{
		public ZaPdfCoordinateColumnContent(int coordinate, List<ZaPdfColumnContent> columnContents)
		{
			Argument.NotNull(columnContents, nameof(columnContents));
			Coordinate = coordinate;
			ColumnsContents = columnContents;
		}

		public int Coordinate { get; set; }
		public List<ZaPdfColumnContent> ColumnsContents { get; set; }
	}

	public static class ZaPdfContentExtension
	{
		public static bool ContainsCoordinate(this List<ZaPdfCoordinateLine> dataCoordinateContent, int coordinate)
		{
			Argument.NotNull(dataCoordinateContent, nameof(dataCoordinateContent));
			return dataCoordinateContent.Any(o => o.StartCoordinate == coordinate);
		}

		public static bool ContainsCoordinate(this List<ZaPdfCoordinateColumnContent> dataColumnCoordinateContent, int coordinate)
		{
			Argument.NotNull(dataColumnCoordinateContent, nameof(dataColumnCoordinateContent));
			return dataColumnCoordinateContent.Any(o => o.Coordinate == coordinate);
		}

		public static bool ContainsColumn(this List<ZaPdfColumnContent> dataColumnContents, string columnName)
		{
			Argument.NotNull(dataColumnContents, nameof(dataColumnContents));
			return dataColumnContents.Any(o => o.ColumnName == columnName);
		}

		public static ZaPdfCoordinateLine GetByCoordinate(this List<ZaPdfCoordinateLine> dataCoordinateContent, int coordinate)
		{
			Argument.NotNull(dataCoordinateContent, nameof(dataCoordinateContent));
			return dataCoordinateContent.FirstOrDefault(o => o.StartCoordinate == coordinate);
		}

		public static ZaPdfCoordinateColumnContent GetByCoordinate(this List<ZaPdfCoordinateColumnContent> dataCoordinateColumnContent, int coordinate)
		{
			Argument.NotNull(dataCoordinateColumnContent, nameof(dataCoordinateColumnContent));
			return dataCoordinateColumnContent.FirstOrDefault(o => o.Coordinate == coordinate);
		}

		public static ZaPdfColumnContent GetByColumnName(this List<ZaPdfColumnContent> dataPdfColumnContent, string columnName)
		{
			Argument.NotNull(dataPdfColumnContent, nameof(dataPdfColumnContent));
			return dataPdfColumnContent.FirstOrDefault(o => o.ColumnName == columnName);
		}

		public static ZaPdfChunk GetByCoordinate(this List<ZaPdfChunk> dataPdfLine, int coordinate)
		{
			Argument.NotNull(dataPdfLine, nameof(dataPdfLine));
			return dataPdfLine.FirstOrDefault(o => o.Coordinate == coordinate);
		}
	}
}
