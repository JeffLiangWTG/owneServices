namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class IATARecord
	{
		public string UnOrIdNumber { get; set; }
		public string ProperShippingName { get; set; }
		public bool NOS { get; set; }
		public bool HasTechnicalName { get; set; }
		public string QualifyingDescriptiveText { get; set; }
		public string OtherNames { get; set; }
		public string ClassOrDivision { get; set; }
		public string SubRisks { get; set; }
		public string HazardAndHandlingLabels { get; set; }
		public string PackingGroup { get; set; }
		public string PassengerAndCargoLQPackInstruction { get; set; }
		public string PassengerAndCargoLQPackMaxAmt { get; set; }
		public string PassengerAndCargoPackInstruction { get; set; }
		public string PassengerAndCargoPackMaxAmt { get; set; }
		public string CargoPackInstruction { get; set; }
		public string CargoPackMaxAmt { get; set; }
		public string SpecialProvisions { get; set; }
		public string ERGCode { get; set; }
		public string ExceptedQuantity { get; set; }
		public string UniqueRecordId { get; set; }
		public string SpecialHandlingCode1 { get; set; }
		public string SpecialHandlingCode2 { get; set; }
		public string SpecialHandlingCode3 { get; set; }
	}
}
