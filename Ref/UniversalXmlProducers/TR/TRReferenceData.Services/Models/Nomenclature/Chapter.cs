using System.Linq;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class Chapter
	{
		public string Code { get; set; }
		public IOrderedEnumerable<RawNomenclatureTariff> Records { get; set; }
		public IOrderedEnumerable<RawNomenclatureTariff> Subchapters { get; set; }
	}
}
