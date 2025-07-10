namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class DBKTariffIntermediateData
	{
		public DBKTariffIntermediateData() { }

		public string Tariff { get; set; }

		public string TariffDesc { get; set; }

		public string Unit { get; set; }

		public decimal Rate { get; set; }

		public decimal SpecificRate { get; set; }

		public string RateString { get; set; }
	}
}
