using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	public class CartonisableItemDefinition
	{
		public CartonisableItemDefinition(Guid pk, decimal height, decimal length, decimal width, decimal volume, decimal weight, bool keepUpright = false)
		{
			PK = pk;
			Length = length;
			Width = width;
			Height = height;
			KeepUpright = keepUpright;
			Volume = volume;
			Weight = weight;
			SortedDims = Array.AsReadOnly(new[] { height, length, width }.OrderBy(d => d).ToArray());
		}

		public readonly Guid PK;
		public readonly decimal Length;
		public readonly decimal Width;
		public readonly decimal Height;
		public readonly bool KeepUpright;
		public readonly decimal Volume;
		public readonly decimal Weight;
		public readonly ReadOnlyCollection<decimal> SortedDims;
	}
}
