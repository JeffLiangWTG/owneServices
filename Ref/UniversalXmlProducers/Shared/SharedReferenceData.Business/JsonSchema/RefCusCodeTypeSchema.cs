namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public class RefCusCodeTypeSchema
	{
		public string CodeType { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public bool IsReadonly { get; set; }
		public byte MaxLength { get; set; }
		public string Country { get; set; } = string.Empty;
	}
}
