using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	class ColumnPositionProvider
	{
		readonly Queue<Dictionary<int, List<TextChunk>>> queue = new Queue<Dictionary<int, List<TextChunk>>>();
		readonly int capacity;

		public ColumnPositionProvider(int capacity)
		{
			this.capacity = capacity;
		}

		public void AddAndBuild(Dictionary<int, List<TextChunk>> textChunks)
		{
			if (textChunks.Count != 6)
			{
				return;
			}

			queue.Enqueue(textChunks);
			if (queue.Count > capacity)
			{
				queue.Dequeue();
			}
			Build();
		}

		public bool IsReady => queue.Count == capacity;

		public float DescriptionCenter { get; private set; }
		public float UnitCenter { get; private set; }
		public float RateCenter { get; private set; }
		public float PreferentialRateCenter { get; private set; }

		void Build()
		{
			DescriptionCenter = GetColumnCenter(2);
			UnitCenter = GetColumnCenter(3);
			RateCenter = GetColumnCenter(4);
			PreferentialRateCenter = GetColumnCenter(5);
		}

		float GetColumnCenter(int index)
		{
			var centers = queue.Select(x => PdfHelper.GetLeftAlignCenter(x[index])).ToList();
			var weightedAvg = 0f;
			var centersCount = centers.Count;
			for (var i = 0; i < centersCount; i++)
			{
				weightedAvg += centers[i] * (i + 1);
			}
			weightedAvg = (2 * weightedAvg) / (centersCount * (centersCount + 1));

			return weightedAvg;
		}
	}
}
