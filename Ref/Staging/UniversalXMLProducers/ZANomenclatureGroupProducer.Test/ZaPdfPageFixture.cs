using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.Test
{
	[TestFixture]
	public class ZaPdfPageFixture
	{
		[Test]
		public void FindHeadingV7()
		{
			var page = new ZaPdfPage(BuildPdfCoordinateContentV7(), new Schedule());
			Assert.True(page.FindHeading());
		}

		[Test]
		public void SplitDataToColumnsV7()
		{
			var page = new ZaPdfPage(BuildPdfCoordinateContentV7(), new Schedule());
			page.FindHeading();
			var columnsContent = page.SplitDataToColumns();

			Assert.True(columnsContent.Any());
			var reg = columnsContent.First();
			Assert.True(reg.ColumnsContents.Any());
			var heading = reg.ColumnsContents.GetByColumnName("HeadingSubHeading");
			Assert.True(heading != null);
			Assert.True(heading.Content.Trim() == "01.01");
		}

		[Test]
		public void MergingDataWhenStartingTheLineInDescriptionCoordinateV7()
		{
			var page = new ZaPdfPage(BuildPdfCoordinateContentV7(), new Schedule());
			page.FindHeading();
			var columnsContent = page.SplitDataToColumns();
			page.MergeDescriptionsWithoutHeadingWithAscendingOrDescendingRows(columnsContent);

			Assert.True(columnsContent.Any());
			var reg = columnsContent.GetByCoordinate(308);
			Assert.True(reg != null);
			Assert.True(reg.ColumnsContents.Any());
			var heading = reg.ColumnsContents.GetByColumnName("HeadingSubHeading");
			Assert.True(heading != null);
			Assert.True(heading.Content.Trim() == "0208.40");
			var description = reg.ColumnsContents.GetByColumnName("Description");
			Assert.True(description != null);
			Assert.True(description.Content.Trim() == "-Of whales,(mammals of the suborder)");
		}

		[Test]
		public void MergingDataV7_CloseLinesMeansTheSameLine()
		{
			var page = new ZaPdfPage(BuildPdfCoordinateContentV7_CloseLinesMeansTheSameLine(), new Schedule());
			page.FindHeading();
			var columnsContent = page.SplitDataToColumns();
			page.MergeDescriptionsWithoutHeadingWithAscendingOrDescendingRows(columnsContent);

			Assert.True(columnsContent.Any());
			var reg = columnsContent.GetByCoordinate(308);
			Assert.True(reg != null);
			Assert.True(reg.ColumnsContents.Any());
			var heading = reg.ColumnsContents.GetByColumnName("HeadingSubHeading");
			Assert.True(heading != null);
			Assert.True(heading.Content.Trim() == "0208.40");
			var description = reg.ColumnsContents.GetByColumnName("Description");
			Assert.True(description != null);
			Assert.True(description.Content.Trim() == "Of whales,(mammals of the suborder)- - -");
		}

		List<ZaPdfCoordinateLine> BuildPdfCoordinateContentV7()
		{
			var result = new List<ZaPdfCoordinateLine>();
			var coordinateContentHeading = new ZaPdfCoordinateLine(72, new List<ZaPdfChunk>());
			coordinateContentHeading.Chunks.AddRange(GetHeadingLinesV7());
			result.AddNew(coordinateContentHeading);
			var coordinateSubHeading = new ZaPdfCoordinateLine(87, new List<ZaPdfChunk>());
			coordinateSubHeading.Chunks.AddRange(GetSubHeadingLinesV7());
			result.AddNew(coordinateSubHeading);
			var coordinateContentRegister = new ZaPdfCoordinateLine(117, GetRegisterV7());
			result.AddNew(coordinateContentRegister);
			result.AddNew(new ZaPdfCoordinateLine(308, GetLine1OfTwoLinesDescriptionV7()));
			result.AddNew(new ZaPdfCoordinateLine(317, GetLine2OfTwoLinesDescriptionV7()));
			return result;
		}

		List<ZaPdfCoordinateLine> BuildPdfCoordinateContentV7_CloseLinesMeansTheSameLine()
		{
			var result = new List<ZaPdfCoordinateLine>();
			var coordinateContentHeading = new ZaPdfCoordinateLine(72, new List<ZaPdfChunk>());
			coordinateContentHeading.Chunks.AddRange(GetHeadingLinesV7());
			result.AddNew(coordinateContentHeading);
			var coordinateSubHeading = new ZaPdfCoordinateLine(87, new List<ZaPdfChunk>());
			coordinateSubHeading.Chunks.AddRange(GetSubHeadingLinesV7());
			result.AddNew(coordinateSubHeading);
			var coordinateContentRegister = new ZaPdfCoordinateLine(117, GetRegisterV7());
			result.AddNew(coordinateContentRegister);
			result.AddNew(new ZaPdfCoordinateLine(308, GetLine1OfCloseLinesMeansTheSameDescriptionV7()));
			result.AddNew(new ZaPdfCoordinateLine(309, GetLine2OfCloseLinesMeansTheSameDescriptionV7()));
			return result;
		}

		List<ZaPdfChunk> GetHeadingLinesV7()
		{
			return new List<ZaPdfChunk>()
			{
				new ZaPdfChunk(39, "Heading/"),
				new ZaPdfChunk(114, "CD"),
				new ZaPdfChunk(141, "Article Description"),
				new ZaPdfChunk(435, "Statistical"),
				new ZaPdfChunk(618, "Rate of Duty")
			};
		}

		List<ZaPdfChunk> GetSubHeadingLinesV7()
		{
			return new List<ZaPdfChunk>()
			{
				new ZaPdfChunk(39, "Subheading"),
				new ZaPdfChunk(446, "Unit"),
				new ZaPdfChunk(490, "General"),
				new ZaPdfChunk(542, "EU / UK"),
				new ZaPdfChunk(597, "EFTA"),
				new ZaPdfChunk(648, "SADC"),
				new ZaPdfChunk(696, "MERCOSUR"),
				new ZaPdfChunk(764, "AfCFTA")
			};
		}

		List<ZaPdfChunk> GetRegisterV7()
		{
			return new List<ZaPdfChunk>()
			{
				new ZaPdfChunk(39, "01.01"),
				new ZaPdfChunk(142, "Live horses, asses, mules and hinnies:")
			};
		}

		List<ZaPdfChunk> GetLine1OfTwoLinesDescriptionV7()
		{
			return new List<ZaPdfChunk>()
			{
				new ZaPdfChunk(39, "0208.40"),
				new ZaPdfChunk(142, "-"),
				new ZaPdfChunk(172, "Of whales,")
			};
		}

		List<ZaPdfChunk> GetLine2OfTwoLinesDescriptionV7()
		{
			return new List<ZaPdfChunk>()
			{
				new ZaPdfChunk(172, "(mammals of the suborder)")
			};
		}

		List<ZaPdfChunk> GetLine1OfCloseLinesMeansTheSameDescriptionV7()
		{
			return new List<ZaPdfChunk>()
			{
				new ZaPdfChunk(172, "Of whales,(mammals of the suborder)")
			};
		}

		List<ZaPdfChunk> GetLine2OfCloseLinesMeansTheSameDescriptionV7()
		{
			return new List<ZaPdfChunk>()
			{
				new ZaPdfChunk(39, "0208.40"),
				new ZaPdfChunk(142, "- - -"),
			};
		}
	}
}
