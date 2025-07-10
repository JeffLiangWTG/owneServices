using System.Collections.Generic;
using System.Reflection;
using CargoWise.RefDbRepo.INReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Tests
{
	public class CessChunkHelperTest
	{
		[Test]
		public void TestChunkLineReturnsCorrectChunks()
		{
			var input = new List<CessLineChunk>
			{
				new CessLineChunk(0, 10, "A"),
				new CessLineChunk(11, 20, "B"),
				new CessLineChunk(30, 40, "C")
			};

			var result = CessChunkHelper.ChunkLine(input, 100m);

			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("A B", string.Join(" ", result[0].Words));
			Assert.AreEqual("C", string.Join(" ", result[1].Words));
		}

		[Test]
		public void TestIsChunkInSingleColumnReturnsTrueWhenChunkInOneColumn()
		{
			var result = CessChunkHelper.IsChunkInSingleColumn(10m, 12m, new List<decimal> { 10m, 30m, 50m, 60m });
			Assert.IsTrue(result);

			result = CessChunkHelper.IsChunkInSingleColumn(10m, 40m, new List<decimal> { 10m, 30m, 50m, 60m });
			Assert.IsFalse(result);
		}

		[Test]
		public void TestMapChunksToItemCreatesCorrectItem()
		{
			var method = typeof(CessPdfPigParser).GetMethod("MapChunksToItem", BindingFlags.NonPublic | BindingFlags.Static);

			var chunks = new List<CessGroupedChunk>
			{
				new CessGroupedChunk { StartX = 0m, EndX = 10m, Y = 100m, Words = new List<string> { "1" } },
				new CessGroupedChunk { StartX = 20m, EndX = 30m, Y = 100m, Words = new List<string> { "0101" } },
				new CessGroupedChunk { StartX = 40m, EndX = 50m, Y = 100m, Words = new List<string> { "Desc" } },
				new CessGroupedChunk { StartX = 60m, EndX = 70m, Y = 100m, Words = new List<string> { "5%" } }
			};

			var topXs = new List<decimal> { 0m, 20m, 40m, 60m };

			var item = (CessDataItem)method.Invoke(null, new object[] { chunks, topXs });

			Assert.AreEqual("1", item.SerialNumber);
			Assert.AreEqual("0101", item.HsCode);
			Assert.AreEqual("Desc", item.Description);
			Assert.AreEqual("5%", item.Rate);
		}

		[Test]
		public void TestAppendChunksToItemAppendsTextCorrectly()
		{
			var method = typeof(CessPdfPigParser).GetMethod("AppendChunksToItem", BindingFlags.NonPublic | BindingFlags.Static);

			var chunks = new List<CessGroupedChunk>
			{
				new CessGroupedChunk { StartX = 0m, EndX = 10m, Y = 100m, Words = new List<string> { "A" } },
				new CessGroupedChunk { StartX = 20m, EndX = 30m, Y = 100m, Words = new List<string> { "B" } }
			};

			var topXs = new List<decimal> { 0m, 20m, 40m, 60m };

			var item = new CessDataItem
			{
				SerialNumber = "1",
				HsCode = "0101",
				Description = "",
				Rate = ""
			};

			method.Invoke(null, new object[] { chunks, topXs, item });

			Assert.AreEqual("1 A", item.SerialNumber);
			Assert.AreEqual("0101 B", item.HsCode);
		}
	}
}
