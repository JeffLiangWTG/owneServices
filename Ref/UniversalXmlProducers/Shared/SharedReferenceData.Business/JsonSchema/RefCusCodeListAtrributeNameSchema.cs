namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public class RefCusCodeListAtrributeNameSchema
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string CodeType { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
		public bool IsMandatory { get; set; }
		public bool AllowDuplicates { get; set; }
		public bool IsValueMandatory { get; set; }
		public string CodeTypeForValueList { get; set; } = string.Empty;
		public string ValueDataType { get; set; } = string.Empty;
		public short MinLengthOrValue { get; set; }
		public decimal MaxLengthOrValue { get; set; }
		public byte DecimalPlaces { get; set; }
		public string ColumnCaption { get; set; } = string.Empty;
	}
}
