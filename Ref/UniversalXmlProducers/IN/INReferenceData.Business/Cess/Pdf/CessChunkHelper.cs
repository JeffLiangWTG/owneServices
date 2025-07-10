using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public static class CessChunkHelper
	{
		public static List<CessGroupedChunk> ChunkLine(List<CessLineChunk> line, decimal lineY)
		{
			var chunks = new List<CessGroupedChunk>();
			var currentChunk = new List<string>();
			decimal? startX = null;

			for (var i = 0; i < line.Count; i++)
			{
				if (currentChunk.Count == 0)
				{
					startX = line[i].StartX;
				}

				currentChunk.Add(line[i].Text);
				var endX = line[i].EndX;

				var isLast = i == line.Count - 1;
				var isGap = !isLast && (line[i + 1].StartX - line[i].EndX >= ChunkThreshold);

				if (isLast || isGap)
				{
					chunks.Add(new CessGroupedChunk
					{
						StartX = startX.Value,
						EndX = endX,
						Y = lineY,
						Words = new List<string>(currentChunk)
					});
					currentChunk.Clear();
				}
			}

			return chunks;
		}

		public static bool IsChunkInSingleColumn(decimal startX, decimal endX, List<decimal> topXs)
		{
			var overlapCount = topXs.Count(x => x >= startX && x <= endX);
			return overlapCount == 1;
		}

		const decimal ChunkThreshold = 5m;
	}
}
