using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public static class PdfHelper
	{
		public static float GetLeftAlignCenter(List<TextChunk> list)
		{
			return ((3 * list.Min(x => x.GetStartXPosition())) + list.Max(x => x.GetEndXPosition())) / 4f;
		}
	}
}
