namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class Section
	{
		public int SectionNumber { get; set; }
		public int MinimumChapter { get; set; }
		public int MaximumChapter { get; set; }
		public string Description { get; set; }
	}
}
