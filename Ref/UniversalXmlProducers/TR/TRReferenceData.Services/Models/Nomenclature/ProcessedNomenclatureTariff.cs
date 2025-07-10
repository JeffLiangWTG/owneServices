namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class ProcessedNomenclatureTariff
	{
		public string Code { get; set; }
		public string Description { get; set; }
		public string UOM { get; set; }
		public int Level { get; set; }
		public int LevelOrder { get; set; }
		public string Rate { get; set; }
		public string RateDerivedFrom { get; set; }
		public string CompositeKey { get; set; }
		public bool IsTariff => Code.Length == 12;
	}
}
