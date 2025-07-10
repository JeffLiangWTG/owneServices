using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	class EnumerableExtensionsTest
	{
		[Test]
		public void ChunkFailsWithNegativeChunkSize()
		{
			var range = Enumerable.Range(1, 10);
			Assert.Throws<ArgumentException>(() =>
			{
				var y = EnumerableExtensions.Chunk(range, -1).ToList();
			});
		}

		[Test]
		public void ChunkCreatesCorrectChunks()
		{
			var range = Enumerable.Range(1, 10);
			var actual = EnumerableExtensions.Chunk(range, 3).ToList();
			Assert.Multiple(() =>
			{
				Assert.That(actual, Has.Count.EqualTo(4), "Actual");
				Assert.That(actual.Last(), Has.Length.EqualTo(1), "Actual Last element of the array");
			});
		}
	}
}
