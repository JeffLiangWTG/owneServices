namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CFRRecord
	{
		public string UNNO { get; set; }
		public string Variant { get; set; }
		public string Prefix { get; set; }
		public string PSN { get; set; }
		public string Variation { get; set; }
		public string PrimaryClass { get; set; }
		public string SecondaryClass { get; set; }
		public string TertiaryClass { get; set; }
		public string MarinePollutant { get; set; }
		public string ExceptedQuantity { get; set; }
		public bool LimitedQuantityPermitted { get; set; }
		public string LimitedQuantity { get; set; }
		public string LimitedQuantityUnit { get; set; }
		public string ReportableQuantity { get; set; }
		public string ReportableQuantityUnit { get; set; }
		public string PassengerStowage { get; set; }
		public string StowageCategory { get; set; }
		public string StowageCodes { get; set; }
		public string GeneralStowage { get; set; }
		public string BulkPackingInstructions { get; set; }
		public string BulkPackingProvisions { get; set; }
		public string IBCInstructions { get; set; }
		public string IBCProvisions { get; set; }
		public string PackingExceptions { get; set; }
		public string PackingInstructions { get; set; }
		public string PackingProvisions { get; set; }
		public string PackingGroup { get; set; }
		public string SpecialProvisions { get; set; }
		public string TankInstructions { get; set; }
		public string TankProvisions { get; set; }
		public string PoisonInhalationHazard { get; set; }
		public string State { get; set; }
		public bool IsFixedPSN { get; set; }
		public bool AppliesForAirTransport { get; set; }
		public bool AppliesForDomesticTransport { get; set; }
		public bool AppliesForInternationalTransport { get; set; }
		public bool AppliesForVesselTransport { get; set; }
		public bool RequiresTechnicalNameInParenthesis { get; set; }
		public string EmergencyResponseGuide { get; set; }
		public string TechnicalName { get; set; }
		public string TreatAs { get; set; }
		public string PAXAirRailLimitType { get; set; }
		public string CargoAirRailLimitType { get; set; }
		public string PAXAirRailLimit { get; set; }
		public string PAXAirRailLimitUnit { get; set; }
		public string CargoAirRailLimit { get; set; }
		public string CargoAirRailLimitUnit { get; set; }
		public string SecondaryPAXAirRailLimit { get; set; }
		public string SecondaryPAXAirRailLimitUnit { get; set; }
		public string SecondaryCargoAirRailLimit { get; set; }
		public string SecondaryCargoAirRailLimitUnit { get; set; }
		public string Code => UNNO + Variant;
	}
}
